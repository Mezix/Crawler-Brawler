using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class DamagePrefabScript : NetworkBehaviour
{
    private NetworkObject netObj;
    public Rigidbody2D _rb;
    public Text _damageText;
    public float _damage;
    public float _currentLifeTime;
    public float _maxLifetime;
    public AEnemy _currentEnemy;

    public GameObject _DamageTextTextPrefabReference;
    private void Awake()
    {
        netObj = GetComponentInChildren<NetworkObject>();
        _DamageTextTextPrefabReference = Resources.Load(GS.Prefabs("DamageTextPrefab"), typeof(GameObject)) as GameObject;
    }
    public void InitDamage(float damage, AEnemy enemy)
    {
        transform.position = enemy.transform.position + new Vector3(UnityEngine.Random.Range(0.2f, -0.2f), 0.2f, 0);
        _damage = damage;
        _damageText.text = HM.FloatToString(damage);
        _damageText.color = Color.red;
        _currentLifeTime = 0;
        _maxLifetime = 1;
        _currentEnemy = enemy;
    }
    public virtual void FixedUpdate()
    {
        _currentLifeTime += Time.deltaTime;
        _currentLifeTime = Mathf.Min(_maxLifetime, _currentLifeTime);
        if (_currentLifeTime < _maxLifetime) MoveDamageText();
        else RemoveEnemy(); 
    }

    private void RemoveEnemy()
    {
        if (_currentEnemy)
        {
            _currentEnemy._damagePrefab = null;
            _currentEnemy = null;
            _damage = 0;

            //netObj.Despawn();
            //DespawnDamageTextServerRpc();
            if (IsServer)
            {
            }
            else
            {

            }
            NetworkObjectPool.Singleton.ReturnNetworkObject(netObj, _DamageTextTextPrefabReference);
        }
    }

    private void MoveDamageText()
    {
        _rb.MovePosition(_rb.transform.position += Vector3.up * 0.02f);
        _damageText.color = new Color(_damageText.color.r, _damageText.color.g, _damageText.color.b, 1 - _currentLifeTime/_maxLifetime);
    }
}
