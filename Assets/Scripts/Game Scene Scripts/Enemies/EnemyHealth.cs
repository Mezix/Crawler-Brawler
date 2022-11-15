using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float _maxHealth;
    public float _currentHealth;
    public bool _unkillable;

    public void InitHealth(float h)
    {
        if (h == -1)
        {
            _unkillable = true;
            _currentHealth = _maxHealth = 100;
            return;
        }
        _currentHealth = _maxHealth = h;
    }

    public bool TakeDamage(float damage)
    {
        if(!_unkillable) _currentHealth -= damage;
        if (_currentHealth <= 0) return true;
        return false;
    }
}