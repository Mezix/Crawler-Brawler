using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PhysicsPackage : AThrowable, IPunchable
{
    public float _punchForce;
    private GameObject _packagePrefabReference;
    public override void Awake()
    {
        base.Awake();
        _packagePrefabReference = Resources.Load(GS.Prefabs("PhysicsObject"), typeof(GameObject)) as GameObject;
    }
    public void Punch(Vector3 dir)
    {
        rb.AddForce(dir * _punchForce, ForceMode2D.Impulse);
    }
    public override void Throw(Vector3 dir)
    {
        rb.AddForce(dir * _throwForce, ForceMode2D.Impulse);
    }
    public void DestroyPackage()
    {
        NetworkObjectPool.Singleton.ReturnNetworkObject(_netObj, _packagePrefabReference);
    }
}