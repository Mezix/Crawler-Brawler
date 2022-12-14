using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController2D : NetworkBehaviour
{
    //  Movement
    private float horizontalInput, verticalInput;
    private float moveSpeed;

    [HideInInspector] public Rigidbody2D _playerRB;
    [HideInInspector] public Collider2D _playerCol;

    public Transform _armTransform;
    public Transform _weaponProjectileSpot;
    public SpriteRenderer _characterSpriteRend;

    [Header("Controllers")]
    public bool _controllerOn;
    public PlayerUI _pUI;
    private PlayerControls _controls;
    private Vector2 moveVec;
    public GameObject _controllerCrosshair;
    private void Awake()
    {
        REF._PCons.Add(this);
        _playerRB = GetComponentInChildren<Rigidbody2D>();
        _playerCol = GetComponentInChildren<Collider2D>();

        //  InitControls

        _controls = new PlayerControls();
        _controls.Gameplay.LightAttack.performed += ctx => { if (_controllerOn) LightAttack(); };
        _controls.Gameplay.HeavyAttack.performed += ctx => { if (_controllerOn) HeavyAttack(); };

        _controls.Gameplay.Move.performed += ctx => moveVec = ctx.ReadValue<Vector2>();
        _controls.Gameplay.Move.canceled += ctx => moveVec = Vector2.zero;

        _controls.Gameplay.Aim.performed += ctx => ControllerAim(ctx.ReadValue<Vector2>(), true);
        _controls.Gameplay.Aim.canceled += ctx => ControllerAim(Vector2.zero, false); 
    }

    private void OnEnable()
    {
        _controls.Gameplay.Enable();
    }
    private void Start()
    {
        moveSpeed = 0.1f;
    }
    private void Update()
    {
        horizontalInput = 0;
        verticalInput = 0;

        if(_controllerOn)
        {
            horizontalInput = moveVec.x;
            verticalInput = moveVec.y;
        }
        else
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) verticalInput = 1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) verticalInput = -1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1;
        }

        if(!_controllerOn)
        {
            HandleMouseInput();
            HandleMousePos();
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }
    public override void OnNetworkSpawn()
    {
        if (REF.CurrentScene == Loader.Scene.MenuScene) Destroy(gameObject);
        if (IsOwner) SetCharacter();
        else ThisCharacterIsNotMyPlayerCharacter();
        base.OnNetworkSpawn();
    }

    private void ThisCharacterIsNotMyPlayerCharacter()
    {
        this.enabled = false;
        _pUI.enabled = false;
    }
    private void SetCharacter()
    {
        if (REF.CharIndex == 0) _characterSpriteRend.sprite = Resources.Load("Graphics/WizardWheelsGraphics/Wizards/TechWizard", typeof(Sprite)) as Sprite;
        else if (REF.CharIndex == 1) _characterSpriteRend.sprite = Resources.Load("Graphics/WizardWheelsGraphics/Wizards/FirstAidFiddler", typeof(Sprite)) as Sprite;
        else if (REF.CharIndex == 2) _characterSpriteRend.sprite = Resources.Load("Graphics/WizardWheelsGraphics/Wizards/ImmolatingImp", typeof(Sprite)) as Sprite;
        else if (REF.CharIndex == 3) _characterSpriteRend.sprite = Resources.Load("Graphics/WizardWheelsGraphics/Wizards/PortalPriest", typeof(Sprite)) as Sprite;
        else if (REF.CharIndex == 4) _characterSpriteRend.sprite = Resources.Load("Graphics/WizardWheelsGraphics/Wizards/PotionPeddler", typeof(Sprite)) as Sprite;
        else if (REF.CharIndex == 5) _characterSpriteRend.sprite = Resources.Load("Graphics/WizardWheelsGraphics/Wizards/WoodlandWanderer", typeof(Sprite)) as Sprite;
        else Debug.Log("Index wrong!");
    }

    private void HandleMouseInput()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            LightAttack();
        }
    }

    private void LightAttack()
    {
        GameObject p = ProjectilePool.Instance.GetProjectileFromPool("DefaultProjectile");
        p.GetComponent<AProjectile>().SetBulletStatsAndTransformToWeaponStats(_weaponProjectileSpot);
    }
    private void HeavyAttack()
    {
        GameObject p = ProjectilePool.Instance.GetProjectileFromPool("DefaultProjectile");
        p.GetComponent<AProjectile>().SetBulletStatsAndTransformToWeaponStats(_weaponProjectileSpot);
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
        _playerRB.MovePosition(transform.position + horizontalInput * moveSpeed * Vector3.right + verticalInput * moveSpeed * Vector3.up);
    }

    //  Controller
    private void ControllerAim(Vector2 vec, bool show)
    {
        _controllerCrosshair.gameObject.SetActive(show);
        _controllerCrosshair.transform.localPosition = vec * 2;
        HM.RotateLocalTransformToAngle(_armTransform, new Vector3(0, 0, HM.GetAngle2DBetween(Vector3.zero, vec)));
        Cursor.visible = false;
    }
}
