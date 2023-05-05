using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PhysicsPackage : MonoBehaviour, Punchable
{
    private NetworkObject _netObj;
    public float _punchForce;
    public Rigidbody2D _rb;

    public bool brittle;

    public bool _wasDispensed;
    public Vector3 dispensedVector;
    private void Awake()
    {
        _netObj = GetComponentInChildren<NetworkObject>();
    }
    public void Punched(Vector3 dir)
    {
        if (brittle)
        {
            DestroyPackage();
        }
        _wasDispensed = false;
        _rb.AddForce(dir * _punchForce, ForceMode2D.Impulse);
    }
    private void FixedUpdate()
    {
        if (_wasDispensed)
        {
            MoveContinuously();
        }
    }

    public void InitDispense(Vector3 vec)
    {
        _wasDispensed = true;
        dispensedVector = vec;
    }
    private void MoveContinuously()
    {
        _rb.velocity = dispensedVector;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _wasDispensed = false;
    }
    public void DestroyPackage()
    {
       REF.ObjPool.AddToPool(_netObj);
    }
}
