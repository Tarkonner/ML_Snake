using System;
using UnityEngine;

public class Food : MonoBehaviour
{
    EnviormentManager enviormentManager;

    public void Setup(EnviormentManager enviorment)
    {
        enviormentManager = enviorment;
    }

    public void Eaten()
    {
        enviormentManager.MoveFood();
    }
}
