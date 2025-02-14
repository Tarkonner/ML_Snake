using System;
using UnityEngine;

public class Food : MonoBehaviour
{
    EnviormentManager enviormentManager;

    private void Awake()
    {
        enviormentManager = GetComponentInParent<EnviormentManager>();
    }

    public void Eaten()
    {
        enviormentManager.MoveFood(gameObject);
    }
}
