using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class ShipController : MonoBehaviour
{
    
    public static event System.Action<int, int> OnAmmoUpdate;
    public static event System.Action<bool> OnAmmoReload;
    [SerializeField] Projectile projectilePrefab;
    [SerializeField] float moveSpeed = 6f;
    [SerializeField] float projectileSpeed = 12f;
    [SerializeField] float fireRate = 0.3f;
    [SerializeField] int maxAmmo = 5;
    [SerializeField] float reloadDuration = 1.5f;

    // HUD hud;

    bool isActive;
    bool isReloading;
    int bulletsRemaining;
    float nextFireTime;
    float horizontalLimit;

    IObjectPool<Projectile> projectilePool;

    void Awake()
    {
        // hud = Object.FindFirstObjectByType<HUD>();

        projectilePool = new ObjectPool<Projectile>(
            createFunc: () => Instantiate(projectilePrefab),
            actionOnGet: (projectile) => projectile.gameObject.SetActive(true),
            actionOnRelease: (projectile) => projectile.gameObject.SetActive(false),
            actionOnDestroy: (projectile) => Destroy(projectile.gameObject),
            defaultCapacity: maxAmmo,
            maxSize: maxAmmo
        );
    }

    Projectile CreateProjectile()
    {
        return Instantiate(projectilePrefab);
    }

    void OnGetProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(true);
    }

    void OnReleaseProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
    }

    void OnDestroyProjectile(Projectile projectile)
    {
        Destroy(projectile.gameObject);
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
        OnAmmoUpdate?.Invoke(bulletsRemaining, maxAmmo); // hud.UpdateAmmo(bulletsRemaining, maxAmmo);
        OnAmmoReload?.Invoke(false); // hud.SetReloading(false);
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
        OnAmmoUpdate?.Invoke(bulletsRemaining, maxAmmo); // hud.UpdateAmmo(bulletsRemaining, maxAmmo);

        Vector3 spawnPos = transform.position + Vector3.up * 0.6f;

        Projectile projectile = projectilePool.Get();

        projectile.transform.position = spawnPos;
        projectile.Launch(projectileSpeed, projectilePool);

        if (bulletsRemaining <= 0)
            StartCoroutine(AutoReloadRoutine());
    }

    IEnumerator AutoReloadRoutine()
    {
        isReloading = true;
        OnAmmoReload?.Invoke(true); // hud.SetReloading(true);
        yield return new WaitForSeconds(reloadDuration);
        bulletsRemaining = maxAmmo;
        isReloading = false;
        OnAmmoReload?.Invoke(false); // hud.SetReloading(false);
        OnAmmoUpdate?.Invoke(bulletsRemaining, maxAmmo); // hud.UpdateAmmo(bulletsRemaining, maxAmmo);
    }
}
