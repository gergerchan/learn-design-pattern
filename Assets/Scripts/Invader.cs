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
        
        Projectile proj = other.GetComponent<Projectile>();
        proj.ReleaseToPool();

        parentFleet.OnInvaderDestroyed(gameObject);
        Destroy(gameObject);
    }
}
