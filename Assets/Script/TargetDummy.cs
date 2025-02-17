using UnityEngine;

public class TargetDummy : MonoBehaviour
{
    [Tooltip("Reference to the Target Dummy Manager.")]
    public TargetDummyManager manager;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            // When hit by a bullet, simply notify the manager to reposition this dummy.
            if (manager != null)
            {
                manager.ResetTargetDummy();
            }
        }
    }
}
