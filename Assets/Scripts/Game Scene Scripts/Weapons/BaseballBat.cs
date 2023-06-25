using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseballBat : AWeapon
{
    public float _punchForce;

    public override void Throw(Vector3 dir)
    {
        rb.AddForce(dir * _throwForce, ForceMode2D.Impulse);
    }
}