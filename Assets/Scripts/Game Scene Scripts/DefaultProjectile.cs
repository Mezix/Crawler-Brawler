using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultProjectile : AProjectile
{
    public override void Awake()
    {
        base.Awake();
        Damage = 3;
    }
}
