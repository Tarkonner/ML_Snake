using System.ComponentModel;
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
            for (int i = 0; i < collision.gameObject.transform.childCount; i++)
            {
                GameObject child = collision.gameObject.transform.GetChild(i).gameObject;
                if (child.CompareTag("Enemy"))
                {
                    SnakeMovement sm = child.GetComponent<SnakeMovement>();
                    if (sm == null)
                        sm = child.GetComponentInParent<SnakeMovement>();
                    if (sm == null)
                        sm = child.GetComponentInChildren<SnakeMovement>();
                    sm.ShrinkSnake();

                    // if (agent != null)
                    // {
                    //     agent.AddReward(rewardOnHit);
                    // }
                    // else
                    //     agent.AddReward((rewardOnHit / 4) * -1);
                }

                hasRegisteredHit = true;
            }
        }

        // Schedule destruction after 2 seconds.
        Destroy(gameObject, 2f);
    }
}
