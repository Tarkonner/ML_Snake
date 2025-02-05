using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeGameManager : MonoBehaviour
{
    public static SnakeGameManager instance;

    // Public field setting the time interval (in seconds) between game ticks (each snake move).
    [SerializeField] private float TickInterval = 0.2f;

    [SerializeField] private GameObject snakePrefab;
    
    // Private flag to track whether the game is over.
    private bool _isGameOver = false;

    public Action GameStep;
    public Action GameOver;

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    /// <summary>
    /// Called before the first frame update; initializes game state, positions, and starts the game loop.
    /// </summary>
    public void StartGame()
    {
        // Set the game over flag to false to indicate an active game.
        _isGameOver = false;

        // Start the game loop coroutine which controls the game ticks.
        StartCoroutine(GameLoop());
        
        FoodSpawner.Instance.SpawnFood();
        Instantiate(snakePrefab, Vector2.zero, Quaternion.identity);
        
    }

    /// <summary>
    /// Coroutine that serves as the main game loop, updating the game state at fixed intervals.
    /// </summary>
    /// <returns>An IEnumerator required for coroutine handling.</returns>
    private IEnumerator GameLoop()
    {
        // Continue running the loop as long as the game is not over.
        while (!_isGameOver)
        {
            // Wait for the defined tick interval before performing the next update.
            yield return new WaitForSeconds(TickInterval);
            GameStep?.Invoke();
        }
    }

}
