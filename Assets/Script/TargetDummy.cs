using System.Collections.Generic;
using UnityEngine;

public class TargetDummy : MonoBehaviour
{
    [Tooltip("Reference to the Target Dummy Manager.")]
    public TargetDummyManager manager;

    // Track the instance IDs of bullets that have already triggered a reset.
    private HashSet<int> bulletsThatTriggeredReset = new HashSet<int>();

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            //Debug.Log("DummyHitRegister");
            int bulletID = collision.gameObject.GetInstanceID();
            // Only reset if this bullet hasn't already triggered a reset.
            if (!bulletsThatTriggeredReset.Contains(bulletID))
            {
                bulletsThatTriggeredReset.Add(bulletID);
                if (manager != null)
                {
                    manager.ResetTargetDummy();
                }
            }
            else
            {
                Debug.Log("Bullet already triggered reset; dummy not reset again.");
            }
        }
    }

    /// <summary>
    /// Clears the record of bullets that have triggered resets.
    /// Call this whenever the dummy is repositioned.
    /// </summary>
    public void ClearTriggeredBullets()
    {
        bulletsThatTriggeredReset.Clear();
    }
}
