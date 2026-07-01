using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    float travelSpeed;
    float topBoundary;

    private IObjectPool<GameObject> _projectilePool;

    public void Launch(float speed)
    {
        travelSpeed = speed;
        topBoundary = Camera.main.orthographicSize + 1f;
    }

    void Update()
    {
        transform.position += Vector3.up * travelSpeed * Time.deltaTime;

        if (transform.position.y >= topBoundary)
            DeactiveAndReturn();
    }

    public void SetPoolOwner(IObjectPool<GameObject> poolOwner)
    {
        _projectilePool = poolOwner;
    }

    private void DeactiveAndReturn()
    {
        if (_projectilePool != null)
        {
            _projectilePool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Invader"))
        {
            DeactiveAndReturn();
            collision.GetComponent<Invader>().DestroyInvader();
        }
    }

}
