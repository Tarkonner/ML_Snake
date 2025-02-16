using UnityEngine;

public class GunPositionFollower : MonoBehaviour
{
    // The target whose position we want to follow (e.g., snake's head)
    public Transform target;

    // Offset from the target's position (e.g., a 0.5 y offset)
    public Vector3 offset = new Vector3(0, 0.5f, 0);

    private GunAgent gunAgent;

    void Awake()
    {
        // Try to get the GunAgent component from the same GameObject.
        gunAgent = GetComponent<GunAgent>();
    }

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }

        // Apply the desired rotation computed by the GunAgent.
        if (gunAgent != null)
        {
            transform.rotation = gunAgent.DesiredRotation;
        }
    }
}
