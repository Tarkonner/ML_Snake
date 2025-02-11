using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;

    private Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        ObjectPool[] pools = GetComponents<ObjectPool>();

        RegisterPool("SnakeBodypart", pools[0]);
        RegisterPool("Food", pools[1]);
    }


    /// <summary>
    /// Registers an object pool.
    /// </summary>
    public void RegisterPool(string key, ObjectPool pool)
    {
        if (!pools.ContainsKey(key))
        {
            pools[key] = pool;
        }
    }

    /// <summary>
    /// Retrieves an object from the specified pool.
    /// </summary>
    public GameObject GetObject(string key)
    {
        if (pools.TryGetValue(key, out ObjectPool pool))
        {
            return pool.GetObject();
        }
        Debug.LogError($"No pool found for key: {key}");
        return null;
    }

    /// <summary>
    /// Returns an object to its pool.
    /// </summary>
    public void ReturnObject(string key, GameObject obj)
    {
        if (pools.TryGetValue(key, out ObjectPool pool))
        {
            pool.ReturnObject(obj);
        }
        else
        {
            Debug.LogError($"No pool found for key: {key}");
        }
    }
}
