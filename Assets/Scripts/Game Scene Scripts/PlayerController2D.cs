using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

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
    public List<AThrowable> _nearbyThrowables = new List<AThrowable>();
    public AThrowable _currentThrowable;
    public float _throwingTime;
    public float _maxTimeThrowing;

    public CombatState _combatState = CombatState.Default;
    public enum CombatState
    {
        Default,
        ThrowableInHand
    }

    [Header("Controllers")]
    public bool _controllerOn;
    public PlayerUI _pUI;
    private PlayerControls _controls;
    private Vector2 controllerMoveInputVector;
    public GameObject _controllerCrosshair;
    public GameObject _leftClickProjectilePrefab;

    //  Network Stuff
    //private readonly NetworkVariable<DirectionCursorData> _cursorData = new NetworkVariable<DirectionCursorData>(writePerm: NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<PlayerMovementData> _playerNetworkData = new NetworkVariable<PlayerMovementData>(writePerm: NetworkVariableWritePermission.Owner);
    private Vector3 _vel;
    private float _rotVel;
    [SerializeField] private float cheapInterpolationTime = 0.1f;

    private void Awake()
    {
        REF.PCon = this;

        _playerRB = GetComponentInChildren<Rigidbody2D>();
        _playerCol = GetComponentInChildren<Collider2D>();
        _playerAnim = GetComponentInChildren<Animator>();
        _leftClickProjectilePrefab = Resources.Load(GS.Prefabs("Projectile"), typeof(GameObject)) as GameObject;

        InitControllerControls();
    }
    private void OnEnable()
    {
        _controls.Gameplay.Enable();
    }
    private void Start()
    {
        maxSpeed = 0.1f;
        _maxTimeThrowing = 1f;
        _throwingTime = 0;
        if (IsOwner) REF.MainCam.SetObjectToTrack(transform);
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
        if (IsOwner)
        {
            moveVector = horizontalInput * maxSpeed * Vector3.right + verticalInput * maxSpeed * Vector3.up;
            currentSpeed = Vector3.Magnitude(moveVector);
            _playerRB.MovePosition(transform.position + moveVector);

            _playerAnim.SetFloat(_speedParameter, currentSpeed);
            _playerAnim.SetFloat(_horizontalParameter, horizontalInput);
            _playerAnim.SetFloat(_verticalParameter, verticalInput);
        }
        else
        {
            //currentSpeed = 1;

            moveVector = _playerNetworkData.Value.XDir * maxSpeed * Vector3.right + _playerNetworkData.Value.YDir * maxSpeed * Vector3.up;
            currentSpeed = Vector3.Magnitude(moveVector);
            transform.position = Vector3.SmoothDamp(transform.position, _playerNetworkData.Value.Position, ref _vel, cheapInterpolationTime);

            _playerAnim.SetFloat(_speedParameter, currentSpeed);
            _playerAnim.SetFloat(_horizontalParameter, _playerNetworkData.Value.XDir);
            _playerAnim.SetFloat(_verticalParameter, _playerNetworkData.Value.YDir);
        }
    }

    private void InitControllerControls()
    {
        _controls = new PlayerControls();
        _controls.Gameplay.LightAttack.performed += ctx => { if (_controllerOn) ShootInput(); };
        _controls.Gameplay.HeavyAttack.performed += ctx => { if (_controllerOn) PickUpInput(); };

        _controls.Gameplay.Move.performed += ctx => controllerMoveInputVector = ctx.ReadValue<Vector2>();
        _controls.Gameplay.Move.canceled += ctx => controllerMoveInputVector = Vector2.zero;

        _controls.Gameplay.Aim.performed += ctx => ControllerAim(ctx.ReadValue<Vector2>(), true);
        _controls.Gameplay.Aim.canceled += ctx => ControllerAim(Vector2.zero, false);
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

        _playerNetworkData.Value = new PlayerMovementData()
        {
            Position = transform.position,
            Rotation = transform.rotation.eulerAngles,
            XDir = horizontalInput,
            YDir = verticalInput
        };

    }
    private void NotOwnerBehaviour()
    {
        HM.RotateLocalTransformToAngle(transform, new Vector3(0, 0, Mathf.SmoothDampAngle(transform.rotation.eulerAngles.z, _playerNetworkData.Value.Rotation.z, ref _rotVel, cheapInterpolationTime)));
    }
    private void HandleMouseInput()
    {
        if (_combatState == CombatState.Default)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                ShootInput();
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                PickUpInput();
            }
        }
        else if (_combatState == CombatState.ThrowableInHand)
        {
            HandleThrowInput();
        }
    }

    // Left Click Interaction
    private void ShootInput()
    {
        LeftClickAttackServerRPC();
    }

    [ServerRpc]
    public void LeftClickAttackServerRPC()
    {
        NetworkObject netObj = NetworkObjectPool.Singleton.GetNetworkObject(_leftClickProjectilePrefab);
        netObj.GetComponent<AProjectile>().SetTransform(_weaponProjectileSpot.transform);

        if (!netObj.IsSpawned)
        {
            netObj.Spawn();
        }
    }
    //  Keyboard and Mouse
    private void HandleMousePos()
    {
        _controllerCrosshair.gameObject.SetActive(false);
        Cursor.visible = true;
        HM.RotateLocalTransformToAngle(transform, new Vector3(0, 0, HM.GetAngle2DBetween(Camera.main.WorldToScreenPoint(transform.position), Input.mousePosition)));
    }

    //  Controller
    private void ControllerAim(Vector2 vec, bool show)
    {
        _controllerCrosshair.gameObject.SetActive(show);
        _controllerCrosshair.transform.localPosition = vec * 2;
        HM.RotateLocalTransformToAngle(_armTransform.transform, new Vector3(0, 0, HM.GetAngle2DBetween(Vector3.zero, vec)));
        Cursor.visible = false;
    }

    //  Throwables
    private void PickUpInput()
    {
        PickupNearestThrowableServerRpc();
    }

    [ServerRpc]
    public void PickupNearestThrowableServerRpc()
    {
        if (_nearbyThrowables.Count > 0)
        {
            AThrowable throwable = _nearbyThrowables[0];
            float minDistance = Mathf.Infinity;

            //  Select the closest throwable
            foreach (AThrowable t in _nearbyThrowables)
            {
                float distance = Vector3.Distance(transform.position, t.transform.position);
                if (distance < minDistance)
                {
                    throwable = t;
                    minDistance = distance;
                }
                _currentThrowable = throwable;
            }
            _currentThrowable.PickUp(_weaponProjectileSpot, true);
            _currentThrowable.transform.SetParent(transform, false);
            _combatState = CombatState.ThrowableInHand;
        }
    }

    private void HandleThrowInput()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            _throwingTime += Time.deltaTime;
        }
        if(Input.GetKeyUp(KeyCode.Mouse1))
        {
            ThrowNearestThrowableServerRpc();
        }
    }

    [ServerRpc]
    public void ThrowNearestThrowableServerRpc()
    {
        _currentThrowable.transform.SetParent(null);
        _currentThrowable.PickUp(null, false);
        if (_currentThrowable != null)
        {
            Vector3 throwDir = HM.PolarToVector(180 + _playerNetworkData.Value.Rotation.z);
            _currentThrowable.Throw(throwDir);
        }
        _currentThrowable = null;
        _combatState = CombatState.Default;
        _throwingTime = 0;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out AThrowable throwable))
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
        if (collision.TryGetComponent(out AThrowable throwable))
        {
            if (_nearbyThrowables.Contains(throwable))
            {
                _nearbyThrowables.Remove(throwable);
                // Debug.Log(throwable + " exited.");
            }
        }
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
        _pUI.TurnOnMenu(false);
        _pUI.enabled = false;
    }

    //  NETWORK STRUCTS

    struct PlayerMovementData : INetworkSerializable
    {
        private float _xPosition;
        private float _yPosition;
        private float _horizontalValue;
        private float _verticalValue;

        private float _zrot;

        internal Vector3 Position
        {
            get => new Vector3(_xPosition, _yPosition, 0);
            set
            {
                _xPosition = value.x;
                _yPosition = value.y;
            }
        }

        internal Vector3 Rotation
        {
            get => new Vector3(0, 0, _zrot);
            set => _zrot = value.z;
        }
        internal float XDir
        {
            get => _horizontalValue;
            set => _horizontalValue = value;
        }
        internal float YDir
        {
            get => _verticalValue;
            set => _verticalValue = value;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _xPosition);
            serializer.SerializeValue(ref _yPosition);
            serializer.SerializeValue(ref _horizontalValue);
            serializer.SerializeValue(ref _verticalValue);
            serializer.SerializeValue(ref _zrot);
        }
    }

}
