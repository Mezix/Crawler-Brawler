using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController2D : NetworkBehaviour
{
    //  Misc
    [HideInInspector] public Rigidbody2D _playerRB;
    [HideInInspector] public Collider2D _playerCol;
    public SpriteRenderer _characterSpriteRend;

    //  Movement
    private float horizontalInput, verticalInput;
    private Vector3 moveVector;
    private float maxSpeed;
    private float currentSpeed;

    //  Animation
    [HideInInspector] public Animator _playerAnim;
    public readonly string _speedParameter = "Speed";
    public readonly string _horizontalParameter = "Horizontal";
    public readonly string _verticalParameter = "Vertical";

    //  Combat
    public Transform _armTransform;
    public Transform _weaponProjectileSpot;
    public List<IThrowable> _nearbyThrowables = new List<IThrowable>();

    [Header("Controllers")]
    public bool _controllerOn;
    public PlayerUI _pUI;
    private PlayerControls _controls;
    private Vector2 controllerMoveInputVector;
    public GameObject _controllerCrosshair;
    public GameObject _leftClickProjectilePrefab;

    //  Network Stuff
    private readonly NetworkVariable<DirectionCursorData> _cursorData = new NetworkVariable<DirectionCursorData>(writePerm: NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        REF.PCon = this;
        _playerRB = GetComponentInChildren<Rigidbody2D>();
        _playerCol = GetComponentInChildren<Collider2D>();
        _playerAnim = GetComponentInChildren<Animator>();

        //  InitControls

        _controls = new PlayerControls();
        _controls.Gameplay.LightAttack.performed += ctx => { if (_controllerOn) LightAttackInput(); };
        _controls.Gameplay.HeavyAttack.performed += ctx => { if (_controllerOn) PickupNearestThrowableInput(); };

        _controls.Gameplay.Move.performed += ctx => controllerMoveInputVector = ctx.ReadValue<Vector2>();
        _controls.Gameplay.Move.canceled += ctx => controllerMoveInputVector = Vector2.zero;

        _controls.Gameplay.Aim.performed += ctx => ControllerAim(ctx.ReadValue<Vector2>(), true);
        _controls.Gameplay.Aim.canceled += ctx => ControllerAim(Vector2.zero, false);

        _leftClickProjectilePrefab = Resources.Load(GS.Prefabs("Projectile"), typeof(GameObject)) as GameObject;
    }

    private void OnEnable()
    {
        _controls.Gameplay.Enable();
    }
    private void Start()
    {
        maxSpeed = 0.1f;
        REF.MainCam.SetObjectToTrack(transform);
    }

    private void Update()
    {
        if (IsOwner)
        {
            OwnerBehaviour();
        }
        else
        {
            NotOwnerBehaviour();
        }
    }
    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void OwnerBehaviour()
    {
        horizontalInput = 0;
        verticalInput = 0;

        if (_controllerOn)
        {
            horizontalInput = controllerMoveInputVector.x;
            verticalInput = controllerMoveInputVector.y;
        }
        else
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) verticalInput = 1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) verticalInput = -1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1;
        }
        if (!_controllerOn)
        {
            HandleMouseInput();
            HandleMousePos();
        }

        //  Network Stuff

        _cursorData.Value = new DirectionCursorData()
        {
            DirectionalCursorRotation = _armTransform.rotation.eulerAngles
        };

    }
    private void NotOwnerBehaviour()
    {
        //transform.position = Vector3.SmoothDamp(transform.position, _netState.Value.Position, ref _vel, cheapInterpolationTime);
        HM.RotateLocalTransformToAngle(_armTransform, _cursorData.Value.DirectionalCursorRotation);
    }
    private void HandleMouseInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            LightAttackInput();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            PickupNearestThrowableInput();
        }
    }

    // Left Click Interaction
    private void LightAttackInput()
    {
        LightAttackServerRPC();
    }

    [ServerRpc]
    public void LightAttackServerRPC()
    {
        NetworkObject netObj = NetworkObjectPool.Singleton.GetNetworkObject(_leftClickProjectilePrefab);
        netObj.GetComponent<AProjectile>().SetTransform(_weaponProjectileSpot);

        if (!netObj.IsSpawned)
        {
            netObj.Spawn();
        }
    }

    //  Right Click Interaction
    private void PickupNearestThrowableInput()
    {

        if (_nearbyThrowables.Count > 0)
        {
            IThrowable throwable = _nearbyThrowables[0];
            if(throwable != null)
            {
                Vector3 throwDir = HM.PolarToVector(180 + _cursorData.Value.DirectionalCursorRotation.z);
                //Vector3 throwDir = HM.PolarToVector(180 + HM.GetAngle2DBetween(transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition))).normalized;
                throwable.Throw(throwDir);
                Debug.Log(throwDir);

                // choose nearest throwable
                /*foreach(IThrowable throwable in _nearbyThrowables)
                {

                }*/
                //_nearbyThrowables.Remove(_nearbyThrowables[0]);
                //Debug.Log("Throwing!");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IThrowable throwable))
        {
            if (!_nearbyThrowables.Contains(throwable))
            {
                _nearbyThrowables.Add(throwable);
               // Debug.Log(throwable + " entered.");
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IThrowable throwable))
        {
            if (_nearbyThrowables.Contains(throwable))
            {
                _nearbyThrowables.Remove(throwable);
               // Debug.Log(throwable + " exited.");
            }
        }
    }
    //  Keyboard and Mouse
    private void HandleMousePos()
    {
        _controllerCrosshair.gameObject.SetActive(false);
        Cursor.visible = true;
        HM.RotateLocalTransformToAngle(_armTransform, new Vector3(0, 0, HM.GetAngle2DBetween(Camera.main.WorldToScreenPoint(transform.position), Input.mousePosition)));
    }

    private void HandleMovement()
    {
        moveVector = horizontalInput * maxSpeed * Vector3.right + verticalInput * maxSpeed * Vector3.up;
        currentSpeed = Vector3.Magnitude(moveVector);
        _playerRB.MovePosition(transform.position + moveVector);

        _playerAnim.SetFloat(_speedParameter, currentSpeed);
        _playerAnim.SetFloat(_horizontalParameter, horizontalInput);
        _playerAnim.SetFloat(_verticalParameter, verticalInput);
    }

    //  Controller
    private void ControllerAim(Vector2 vec, bool show)
    {
        _controllerCrosshair.gameObject.SetActive(show);
        _controllerCrosshair.transform.localPosition = vec * 2;
        HM.RotateLocalTransformToAngle(_armTransform, new Vector3(0, 0, HM.GetAngle2DBetween(Vector3.zero, vec)));
        Cursor.visible = false;
    }

    //  Network Stuff

    public override void OnNetworkSpawn()
    {
        if (REF.CurrentScene == Loader.Scene.MenuScene) Destroy(gameObject);
        if (IsOwner) IsMyPlayerCharacter();
        else ThisCharacterIsNotMyPlayerCharacter();
        base.OnNetworkSpawn();
    }

    private void IsMyPlayerCharacter()
    {
        /*
        if (REF.CharIndex == 0) _characterSpriteRend.sprite = Resources.Load("Graphics/WizardWheelsGraphics/Wizards/TechWizard", typeof(Sprite)) as Sprite;
        else if (REF.CharIndex == 1) _characterSpriteRend.sprite = Resources.Load("Graphics/WizardWheelsGraphics/Wizards/FirstAidFiddler", typeof(Sprite)) as Sprite;
        else if (REF.CharIndex == 2) _characterSpriteRend.sprite = Resources.Load("Graphics/WizardWheelsGraphics/Wizards/ImmolatingImp", typeof(Sprite)) as Sprite;
        else if (REF.CharIndex == 3) _characterSpriteRend.sprite = Resources.Load("Graphics/WizardWheelsGraphics/Wizards/PortalPriest", typeof(Sprite)) as Sprite;
        else if (REF.CharIndex == 4) _characterSpriteRend.sprite = Resources.Load("Graphics/WizardWheelsGraphics/Wizards/PotionPeddler", typeof(Sprite)) as Sprite;
        else if (REF.CharIndex == 5) _characterSpriteRend.sprite = Resources.Load("Graphics/WizardWheelsGraphics/Wizards/WoodlandWanderer", typeof(Sprite)) as Sprite;
        else Debug.Log("Index wrong!");
        */
    }

    private void ThisCharacterIsNotMyPlayerCharacter()
    {
        _controllerCrosshair.SetActive(false);
        _pUI.enabled = false;
    }

    struct DirectionCursorData : INetworkSerializable
    {
        private float _directionCursorRotation;
        internal Vector3 DirectionalCursorRotation
        {
            get => new Vector3(0, 0, _directionCursorRotation);
            set => _directionCursorRotation = value.z;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _directionCursorRotation);
        }
    }
}
