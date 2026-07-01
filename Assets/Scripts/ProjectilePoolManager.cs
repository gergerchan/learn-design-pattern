using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePoolManager : MonoBehaviour
{
    public static ProjectilePoolManager Instance;

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 50;
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
            defaultCapacity: initialPoolSize,
            maxSize: maxPoolSize
        );
        
        var temp = new List<GameObject>();

        for (int i = 0; i < initialPoolSize; i++)
        {
            temp.Add(_pool.Get());
        }

        foreach (var obj in temp)
        {
            _pool.Release(obj);
        }
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