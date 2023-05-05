using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AEnemy : MonoBehaviour
{
    protected Rigidbody2D enemyRB;
    protected Collider2D enemyCol;
    protected EnemyHealth enemyHealth;
    public EnemyStats _enemyStats;

    [Header("Damage Indicators")]
    public DamagePrefabScript _damagePrefab;
    public Transform _damagePos;
    public virtual void Awake()
    {
        enemyRB = GetComponentInChildren<Rigidbody2D>();
        enemyCol = GetComponentInChildren<Collider2D>();
        enemyHealth = GetComponentInChildren<EnemyHealth>();
    }
    public virtual void Start()
    {
        enemyHealth.InitHealth(_enemyStats._enemyHealth);
    }
    public void TakeDamage(float damage)
    {
        if (enemyHealth.TakeDamage(damage)) KillEnemy();

        if (!_damagePrefab)
        {
            GameObject g = REF.ObjPool.GetNetworkObjectFromPool("DamageText").gameObject;
            _damagePrefab = g.GetComponent<DamagePrefabScript>();
        }
        _damagePrefab.InitDamage(_damagePrefab._damage += damage, this);
    }
    private void KillEnemy()
    {
        Destroy(gameObject);
    }
}
