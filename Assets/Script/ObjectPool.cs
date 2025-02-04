using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;   // Prefab to pool
    [SerializeField] private int initialSize = 10; // Initial number of objects

    private Queue<GameObject> pool = new Queue<GameObject>(); // Queue for managing available objects

    void Awake()
    {
        // Pre-instantiate objects in the pool
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject();
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// Retrieves an object from the pool or creates a new one if empty.
    /// </summary>
    public GameObject GetObject()
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue(); // Get from pool
            obj.SetActive(true);
        }
        else
        {
            obj = CreateNewObject(); // Create new if empty
        }
        return obj;
    }

    /// <summary>
    /// Returns an object back to the pool.
    /// </summary>
    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    /// <summary>
    /// Creates a new pooled object.
    /// </summary>
    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        return obj;
    }
}
