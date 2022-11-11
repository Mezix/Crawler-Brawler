using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    //  Movement
    private float horizontalInput, verticalInput;
    private float moveSpeed;

    [HideInInspector] public Rigidbody2D playerRB;
    [HideInInspector] public Collider2D _playerCol;

    public Transform _armTransform;
    public Transform _weaponProjectileSpot;

    private void Awake()
    {
        REF._PCon = this;
        playerRB = GetComponentInChildren<Rigidbody2D>();
        _playerCol = GetComponentInChildren<Collider2D>();
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
        playerRB.MovePosition(transform.position + horizontalInput * moveSpeed * Vector3.right + verticalInput * moveSpeed * Vector3.up);
    }

    private void HandleMouse()
    {
        HM.RotateLocalTransformToAngle(_armTransform, new Vector3(0, 0, HM.GetAngle2DBetween(Camera.main.WorldToScreenPoint(transform.position), Input.mousePosition)));
    }
}
