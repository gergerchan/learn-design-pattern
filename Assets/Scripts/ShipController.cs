using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipController : MonoBehaviour
{
    // [SerializeField] GameObject projectilePrefab;
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float projectileSpeed = 12f;
    [SerializeField] float fireRate = 0.3f;
    [SerializeField] int maxAmmo = 5;
    [SerializeField] float reloadDuration = 1.5f;

    HUD hud;

    bool isActive;
    bool isReloading;
    int bulletsRemaining;
    float nextFireTime;
    float horizontalLimit;

    void Awake()
    {
        hud = Object.FindFirstObjectByType<HUD>();
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
        hud.UpdateAmmo(bulletsRemaining, maxAmmo);
        hud.SetReloading(false);
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
        if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;
        if (Time.time < nextFireTime) return;
        if (bulletsRemaining <= 0) return;

        FireProjectile();
    }

    void FireProjectile()
    {
        nextFireTime = Time.time + fireRate;
        bulletsRemaining--;
        hud.UpdateAmmo(bulletsRemaining, maxAmmo);

        Vector3 spawnPos = transform.position + Vector3.up * 0.6f;
        // GameObject go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        GameObject go = ProjectilePoolManager.Instance.SpawnProjectile(spawnPos, Quaternion.identity);
        go.GetComponent<Projectile>().Launch(projectileSpeed);

        if (bulletsRemaining <= 0)
            StartCoroutine(AutoReloadRoutine());
    }

    IEnumerator AutoReloadRoutine()
    {
        isReloading = true;
        hud.SetReloading(true);
        yield return new WaitForSeconds(reloadDuration);
        bulletsRemaining = maxAmmo;
        isReloading = false;
        hud.SetReloading(false);
        hud.UpdateAmmo(bulletsRemaining, maxAmmo);
    }
}
