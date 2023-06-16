using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class AProjectile : NetworkBehaviour
{
    private NetworkObject netObj;
    public float CurrentLifeTime { get; protected set; } //Check IProjectile for explanations
    public float MaxLifetime { get; set; }
    public int Damage { get; set; }
    public float ProjectileSpeed { get; set; }
    public bool HitPlayer { get; set; }

    public bool HasDoneDamage { get; set; }
    [HideInInspector] public bool _despawnAnimationPlaying;

    [HideInInspector] public Rigidbody2D _projectileRB;
    [HideInInspector] public Collider2D _projectileCollider;
    [HideInInspector] public TrailRenderer _trailRenderer;

    public GameObject _projectilePrefabReference;

    public virtual void Awake()
    {
        netObj = GetComponentInChildren<NetworkObject>();
        _projectileRB = GetComponentInChildren<Rigidbody2D>();
        _projectileCollider = GetComponentInChildren<Collider2D>();
        _trailRenderer = GetComponentInChildren<TrailRenderer>();
        MaxLifetime = 3;
        _projectilePrefabReference = Resources.Load(GS.Prefabs("Projectile"), typeof(GameObject)) as GameObject;
    }

    public void Update()
    {
        CurrentLifeTime += Time.deltaTime;
        MoveProjectile();
        CheckLifetime();
    }
    public virtual void FixedUpdate()
    {
        if (!_despawnAnimationPlaying)
            MoveProjectile();
    }
    public virtual void OnEnable()
    {
        CurrentLifeTime = 0;
        _despawnAnimationPlaying = false;
        HasDoneDamage = false;
    }
    public virtual void MoveProjectile()
    {
        _projectileRB.MovePosition(transform.position + transform.right * ProjectileSpeed * Time.deltaTime);
    }
    protected void CheckLifetime() //a function that checks if our projectile has reached the end of its lifespan, and then decides what to do now
    {
        if (CurrentLifeTime >= MaxLifetime && !_despawnAnimationPlaying)
        {
            StartCoroutine(DespawnAnimation());
        }
    }

    //  Bullet Spawning
    public virtual void SetTransform(Transform t)
    {
        if (_trailRenderer) _trailRenderer.Clear();
        transform.position = t.transform.position;
        transform.rotation = t.transform.rotation;
        ProjectileSpeed = 15;
        Damage = 3;
        HitPlayer = false;
        if (!HitPlayer)
        {
            Physics2D.IgnoreCollision(_projectileCollider, REF.PCon._playerCol);
        }
        if (_trailRenderer) _trailRenderer.Clear();
    }
    public virtual IEnumerator DespawnAnimation()
    {
        _despawnAnimationPlaying = true;
        yield return new WaitForSeconds(0f);
        if(IsServer) DespawnProjectileServerRpc();
    }
    [ServerRpc]
    protected void DespawnProjectileServerRpc()
    {
        _trailRenderer.Clear();
        //NetworkObjectPool.Singleton.ReturnNetworkObject(netObj, _projectilePrefabReference);
        if (IsSpawned) netObj.Despawn();
    }
    public virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col)
        {
            if (!HasDoneDamage)
            {
                if (HitPlayer)
                {
                    if (col.GetComponent<PlayerController2D>())
                    {
                        PlayerController2D player = col.GetComponentInChildren<PlayerController2D>();
                        //player.TakeDamage(Damage);
                        HasDoneDamage = true;
                        StartCoroutine(DespawnAnimation());
                    }
                }
                else
                {
                    if (col.GetComponentInChildren<AEnemy>())
                    {
                        AEnemy enemy = col.GetComponentInChildren<AEnemy>();
                        enemy.TakeDamage(Damage);
                        HasDoneDamage = true;
                    }
                }
            }
            StartCoroutine(DespawnAnimation());
        }
    }
}
