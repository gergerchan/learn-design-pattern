using UnityEngine;

public class Invader : MonoBehaviour
{
    InvaderFleet parentFleet;

    public void Initialize(InvaderFleet fleet)
    {
        parentFleet = fleet;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Projectile")) return;
        ObjectPoolManager.Return(other.gameObject);
        parentFleet.OnInvaderDestroyed(gameObject);
        ObjectPoolManager.Return(gameObject);
        
        VfxManager.Instance?.SpawnVfx("vfx-explosion", other.transform.position, Quaternion.identity);
    }
}
