using UnityEngine;

public class BulletReward : MonoBehaviour
{
    [Tooltip("Reward for hitting an enemy.")]
    public float rewardOnHit = 1.0f;

    [HideInInspector]
    public GunAgent agent;

    // This flag indicates whether this bullet has already registered a hit.
    // Make sure this is NOT declared as static.
    private bool hasRegisteredHit = false;

    public bool HasRegisteredHit { get { return hasRegisteredHit; } }

    // If you're pooling bullets, this ensures the flag is reset every time the bullet is reused.
    private void OnEnable()
    {
        hasRegisteredHit = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Bullet collided with: " + collision.gameObject.name);

        // Process the collision only if we haven't registered a hit yet.
        if (!hasRegisteredHit)
        {
            // Check if the collision is with an enemy.
            if (collision.gameObject.CompareTag("Enemy"))
            {
                Debug.Log("Enemy Hit!");
                if (agent != null)
                {
                    agent.AddReward(rewardOnHit);
                }
                hasRegisteredHit = true;
            }
        }

        // Schedule destruction after 2 seconds.
        Destroy(gameObject, 2f);
    }
}
