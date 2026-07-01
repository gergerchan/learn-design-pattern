using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipController : MonoBehaviour
{
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float projectileSpeed = 12f;
    [SerializeField] float fireRate = 0.3f;
    [SerializeField] int maxAmmo = 5;
    [SerializeField] float reloadDuration = 1.5f;

    bool isActive;
    bool isReloading;
    int bulletsRemaining;
    float nextFireTime;
    float horizontalLimit;

    void Awake()
    {
        ObjectPoolManager.RegisterPrefab(projectilePrefab, initialSize: 5, maxSize: 20);
    }

    void Start()
    {
        float halfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        horizontalLimit = halfWidth - 0.6f;
    }

    public void Activate()
    {
        isActive = true;
        isReloading = false;
        bulletsRemaining = maxAmmo;
        nextFireTime = 0f;
        EventBus.PublishAmmoChanged(bulletsRemaining, maxAmmo);
        EventBus.PublishReloadingChanged(false);
    }

    public void Deactivate()
    {
        isActive = false;
    }

    void Update()
    {
        if (!isActive) return;
        MoveShip();
        HandleFire();
    }

    void MoveShip()
    {
        float input = 0f;
        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) input = -1f;
        if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) input = 1f;
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x + input * moveSpeed * Time.deltaTime, -horizontalLimit, horizontalLimit);
        transform.position = pos;
    }

    void HandleFire()
    {
        if (isReloading) return;
        if (!Keyboard.current.spaceKey.isPressed) return;
        if (Time.time < nextFireTime) return;
        if (bulletsRemaining <= 0) return;

        FireProjectile();
    }

    void FireProjectile()
    {
        nextFireTime = Time.time + fireRate;
        bulletsRemaining--;
        EventBus.PublishAmmoChanged(bulletsRemaining, maxAmmo);
        AudioManager.Instance?.PlaySfx("sfx-shoot");

        Vector3 spawnPos = transform.position + Vector3.up * 0.6f;
        GameObject go = ObjectPoolManager.Get(projectilePrefab, spawnPos, Quaternion.identity);
        go.GetComponent<Projectile>().Launch(projectileSpeed);

        if (bulletsRemaining <= 0)
            StartCoroutine(AutoReloadRoutine());
    }

    IEnumerator AutoReloadRoutine()
    {
        isReloading = true;
        EventBus.PublishReloadingChanged(true);
        yield return new WaitForSeconds(reloadDuration);
        bulletsRemaining = maxAmmo;
        isReloading = false;
        EventBus.PublishReloadingChanged(false);
        EventBus.PublishAmmoChanged(bulletsRemaining, maxAmmo);
    }
}
