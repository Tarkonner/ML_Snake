using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeGameManager : MonoBehaviour
{
    public static SnakeGameManager instance;

    // Public field setting the time interval (in seconds) between game ticks (each snake move).
    [SerializeField] private float TickInterval = 0.2f;

    // Private flag to track whether the game is over.
    private bool _isGameOver = false;

    public Action GameStep;

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }

    /// <summary>
    /// Called before the first frame update; initializes game state, positions, and starts the game loop.
    /// </summary>
    private void Start()
    {
        // Set the game over flag to false to indicate an active game.
        _isGameOver = false;

        // Start the game loop coroutine which controls the game ticks.
        StartCoroutine(GameLoop());
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

    /// <summary>
    /// Transitions the game into a game over state.
    /// </summary>
    private void GameOver()
    {
        // Set the game over flag so the game loop will cease.
        _isGameOver = true;

        // Output a message to the Unity Console indicating that the game has ended.
        Debug.Log("Game Over");

        // Additional game over handling (such as UI feedback or a restart mechanism) can be added here.
    }
}
