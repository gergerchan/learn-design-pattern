using UnityEngine;

public class Projectile : MonoBehaviour
{
    float travelSpeed;
    float topBoundary;

    public void Launch(float speed)
    {
        travelSpeed = speed;
        topBoundary = Camera.main.orthographicSize + 1f;
    }

    void Update()
    {
        transform.position += Vector3.up * travelSpeed * Time.deltaTime;

        if (transform.position.y >= topBoundary)
            ObjectPoolManager.Return(gameObject);
    }
}
