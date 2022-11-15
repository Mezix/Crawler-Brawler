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

    public PlayerUI _pUI;

    private void Awake()
    {
        REF._PCons.Add(this);
        _playerRB = GetComponentInChildren<Rigidbody2D>();
        _playerCol = GetComponentInChildren<Collider2D>();
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

    private void Start()
    {
        moveSpeed = 0.1f;
    }
    private void Update()
    {
        horizontalInput = 0;
        verticalInput = 0;
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        HandleMouse();
        HandleInput();
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

    private void HandleInput()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            Fire();
        }
    }

    private void Fire()
    {
        GameObject p = ProjectilePool.Instance.GetProjectileFromPool("DefaultProjectile");
        p.GetComponent<AProjectile>().SetBulletStatsAndTransformToWeaponStats(_weaponProjectileSpot);
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        _playerRB.MovePosition(transform.position + horizontalInput * moveSpeed * Vector3.right + verticalInput * moveSpeed * Vector3.up);
    }

    private void HandleMouse()
    {
        HM.RotateLocalTransformToAngle(_armTransform, new Vector3(0, 0, HM.GetAngle2DBetween(Camera.main.WorldToScreenPoint(transform.position), Input.mousePosition)));
    }
}
