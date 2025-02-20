using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public class GunAgent : Agent
{
    [Header("Projectile Settings")]
    [Tooltip("Projectile prefab to be fired.")]
    public GameObject projectilePrefab;
    public int projectileCount = 0;
    public float shootPenalty = -0.01f;
    
    [Tooltip("Speed applied to the projectile.")]
    public float projectileForce = 5f;

    [Header("Gun Settings")]
    [Tooltip("Time in seconds between shots.")]
    public float cooldownDuration = 0.5f;

    [Tooltip("Rotation speed in degrees per second (used for ML training).")]
    public float rotationSpeed = 180f;

    // Time when the gun last fired
    private float lastShotTime = -Mathf.Infinity;
    private bool bulletWaiting = false; // Flag for manual input

    private BehaviorParameters behaviorParameters;

    [SerializeField] Transform bulletSpawnPoint;

    // This property will store the desired rotation computed in Update.
    public Quaternion DesiredRotation { get; private set; }

    public override void Initialize()
    {
        //Time.timeScale = 1000f;
        behaviorParameters = GetComponent<BehaviorParameters>();
        // Initialize DesiredRotation to the current rotation.
        DesiredRotation = transform.rotation;
    }

    public void PlayerControl()
    {
        GetComponent<BehaviorParameters>().BehaviorType = BehaviorType.HeuristicOnly;
    }

    public override void OnEpisodeBegin()
    {
        lastShotTime = -Mathf.Infinity;
        bulletWaiting = false;
        projectileCount = 0;

        // Destroy all bullets in the scene.
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (GameObject bullet in bullets)
        {
            Destroy(bullet);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observation 1: Normalized cooldown status.
        float cooldownNormalized = Mathf.Clamp01((lastShotTime + cooldownDuration - Time.time) / cooldownDuration);
        sensor.AddObservation(cooldownNormalized);

        // (Additional enemy observations can be added here if needed.)
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // For ML-controlled behavior.
        if (behaviorParameters.BehaviorType != BehaviorType.HeuristicOnly)
        {
            // Use ML actions for rotation.
            float rotationInput = actionBuffers.ContinuousActions[0];
            // Update DesiredRotation based on ML input.
            DesiredRotation = Quaternion.AngleAxis(rotationInput * rotationSpeed * Time.deltaTime, Vector3.up) * DesiredRotation;

            int shootAction = actionBuffers.DiscreteActions[0];
            if (shootAction == 1 && Time.time >= lastShotTime + cooldownDuration)
            {
                FireProjectile();
                lastShotTime = Time.time;
            }
        }
        // In heuristic mode, shooting is handled in Update() for responsiveness.
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Fill these buffers even if we don't use them directly.
        var continuousActionsOut = actionsOut.ContinuousActions;
        if (continuousActionsOut.Length > 0)
            continuousActionsOut[0] = 0f;
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (discreteActionsOut.Length > 0)
            discreteActionsOut[0] = 0;
    }

    void Update()
    {
        // In heuristic mode, update the desired rotation based on the mouse position.
        if (behaviorParameters.BehaviorType == BehaviorType.HeuristicOnly)
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3 direction = (mouseWorldPos - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                DesiredRotation = Quaternion.LookRotation(direction, Vector3.up);
            }

            // Only set bulletWaiting if Space is pressed AND the cooldown is ready.
            if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastShotTime + cooldownDuration)
            {
                bulletWaiting = true;
            }

            // If bullet is waiting and the cooldown has passed, fire immediately.
            if (bulletWaiting && Time.time >= lastShotTime + cooldownDuration)
            {
                FireProjectile();
                lastShotTime = Time.time;
                bulletWaiting = false;
            }
        }
    }

    void FireProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Projectile prefab is not assigned!");
            return;
        }

        // Spawn the projectile with a +0.5 offset in the Y axis.
        Vector3 spawnPosition = transform.position + new Vector3(0, 0.5f, 0);
        GameObject projectile = Instantiate(projectilePrefab, bulletSpawnPoint.position, transform.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * projectileForce;
        }

        // Add a small penalty for shooting.
        AddReward(shootPenalty * projectileCount);

        // Assign this agent to the bullet so it can reward hits.
        BulletReward bulletReward = projectile.GetComponent<BulletReward>();
        if (bulletReward != null)
        {
            bulletReward.agent = this;
        }
    }

    /// <summary>
    /// Returns the world position corresponding to the mouse cursor on a horizontal plane at the gun's height.
    /// </summary>
    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return transform.position;
    }
}
