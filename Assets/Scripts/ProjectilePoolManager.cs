using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePoolManager : MonoBehaviour
{
    public static ProjectilePoolManager Instance;

    [SerializeField] private GameObject projectilePrefab;
    private ObjectPool<GameObject> _pool;

    void Awake()
    {
        Instance = this;

        _pool = new ObjectPool<GameObject>(
            createFunc: CreateProjectile,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnFromPool,
            actionOnDestroy: OnDestroyPoolObject,
            collectionCheck: true,
            defaultCapacity: 2,
            maxSize: 50

    );
    }


    private GameObject CreateProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab);
        projectile.GetComponent<Projectile>().SetPoolOwner(_pool);

        return projectile;
    }

    private void OnTakeFromPool(GameObject projectile)
    {
        projectile.SetActive(true);
    }

    private void OnReturnFromPool(GameObject projectile)
    {
        projectile.SetActive(false);
    }

    private void OnDestroyPoolObject (GameObject projectile)
    {
        Destroy(projectile);
    }

    public GameObject SpawnProjectile(Vector3 position, Quaternion rotation)
    {
        GameObject projectile = _pool.Get();
        projectile.transform.position = position;
        projectile.transform.rotation = rotation;
        return projectile;
    }

}