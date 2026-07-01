using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    float travelSpeed;
    float topBoundary;
    IObjectPool<Projectile> pool;

    bool isReleased;

    public void Launch(float speed, IObjectPool<Projectile> pool)
    {
        this.travelSpeed = speed;
        this.pool = pool;
        topBoundary = Camera.main.orthographicSize + 1f;
        isReleased = false;
    }

    void Update()
    {
        if (pool == null) return;

        transform.position += Vector3.up * travelSpeed * Time.deltaTime;

        if (transform.position.y >= topBoundary)
            ReleaseToPool();
    }

    public void ReleaseToPool()
    {
        if (isReleased) return;
        isReleased = true;

        if (pool != null)
            pool.Release(this);
        else
            Destroy(gameObject);
    }
}
