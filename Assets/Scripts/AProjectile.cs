using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AProjectile : MonoBehaviour
{
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

    public virtual void Awake()
    {
        _projectileRB = GetComponentInChildren<Rigidbody2D>();
        _projectileCollider = GetComponentInChildren<Collider2D>();
        _trailRenderer = GetComponentInChildren<TrailRenderer>();
        MaxLifetime = 3;
    }
    public void Update()
    {
        CurrentLifeTime += Time.deltaTime;
        MoveProjectile();
        CheckLifetime();
    }
    public virtual void FixedUpdate()
    {
        //if (!_despawnAnimationPlaying)
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
    public virtual void SetBulletStatsAndTransformToWeaponStats(Transform t)
    {
        transform.position = t.transform.position;
        transform.rotation = t.transform.rotation;
        ProjectileSpeed = 15;
        Damage = 3;
        HitPlayer = false;
        if (!HitPlayer)
        {
            foreach(PlayerController2D pcon in REF._PCons)
            {
                if (!pcon) continue;
                Physics2D.IgnoreCollision(_projectileCollider, pcon._playerCol);
            }
        }
        if (_trailRenderer) _trailRenderer.Clear();
    }
    protected void DespawnBullet()
    {
        _trailRenderer.Clear();
        ProjectilePool.Instance.AddToPool(gameObject);
    }
    public virtual IEnumerator DespawnAnimation()
    {
        _despawnAnimationPlaying = true;
        yield return new WaitForSeconds(0f);
        DespawnBullet();
    }
    public virtual void OnTriggerEnter2D(Collider2D col)
    {
        if (col)
        {
            if (!HasDoneDamage)
            {
                if (HitPlayer)
                {
                    //if (col.GetComponent<PlayerController>())
                    {
                        //REF.PCon._pHealth.TakeDamage(Damage);
                        HasDoneDamage = true;
                        StartCoroutine(DespawnAnimation());
                    }
                }
                else
                {
                    //if (col.GetComponentInChildren<AEnemy>())
                    {
                        //AEnemy enemy = col.GetComponentInChildren<AEnemy>();
                        //enemy.TakeDamage(Damage);
                        HasDoneDamage = true;
                    }
                }
            }
            StartCoroutine(DespawnAnimation());
        }
    }
}
