using UnityEngine;
using Unity.MLAgents;

public class BulletReward : MonoBehaviour
{
    [Tooltip("Reward for hitting an enemy.")]
    public float rewardOnHit = 1.0f;

    [HideInInspector]
    public GunAgent agent;

    // This flag indicates whether the bullet is still lethal.
    private bool isLethal = true;

    void OnCollisionEnter(Collision collision)
    {
        // If the bullet is no longer lethal, ignore further collisions.
        if (!isLethal)
            return;

        // If the bullet hits an enemy, award the reward.
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (agent != null)
            {
                Debug.Log("Enemy Hit!");
                agent.AddReward(rewardOnHit);
            }
        }

        // Mark the bullet as non-lethal after the initial collision.
        isLethal = false;

        // Instead of destroying immediately, delay destruction by 2 seconds.
        Destroy(gameObject, 5f);
    }
}
