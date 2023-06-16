using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PhysicsPackage : MonoBehaviour, IPunchable, IThrowable
{
    private Rigidbody2D rb;
    private NetworkObject _netObj;
    private GameObject _packagePrefabReference;

    public float _punchForce;
    public float _throwForce;
    public bool _pickedUp;
    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        _netObj = GetComponentInChildren<NetworkObject>();
        _packagePrefabReference = Resources.Load(GS.Prefabs("PhysicsObject"), typeof(GameObject)) as GameObject;
    }
    public void Punch(Vector3 dir)
    {
        rb.AddForce(dir * _punchForce, ForceMode2D.Impulse);
    }
    public void Throw(Vector3 dir)
    {
        rb.AddForce(dir * _throwForce, ForceMode2D.Impulse);
    }
    public void DestroyPackage()
    {
        NetworkObjectPool.Singleton.ReturnNetworkObject(_netObj, _packagePrefabReference);
    }
}