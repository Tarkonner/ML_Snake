using System.Collections.Generic;
using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    //Movement
    private List<GameObject> snakesBody = new List<GameObject>();
    private Vector2 movementDirection = Vector2.up;
    [SerializeField] int startBodySize = 3;

    [Header("Color")]
    [SerializeField] Color mainColor;
    [SerializeField] Color seconddaryColor;
    private bool useMainColor = true;

    bool snakeGrowing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Move with game tick
        SnakeGameManager.instance.GameStep += Move;

        // //Make body
        // for (int i = 1; i < startBodySize + 1; i++)
        // {
        //     SpawnBodyInEnd((Vector2)transform.localPosition - (movementDirection * i)); 
        // }
    }

    #region Movement
    /// <summary>
    /// Moves the snake based on its current direction, handles collisions, and updates the snake's body.
    /// </summary>
    public void Move()
    {
        // Store the current head position so that the body parts can follow.
        Vector3 origonPoint = transform.localPosition;
    
        // Calculate the new head position by moving in the current movement direction.
        Vector2 moveToPoint = transform.localPosition + (Vector3)(movementDirection * Grid.gridSize);

        // Define a small box area at the new position to detect any collisions.
        Vector2 boxSize = new Vector2(0.1f, 0.1f);
        Collider2D hit = Physics2D.OverlapBox(moveToPoint, boxSize, 0f);

        // Check for collisions at the target position.
        if (hit != null)
        {
            GameObject go = hit.gameObject;
            // If the snake collides with food, grow the snake and remove the food object.
            if (go.CompareTag("Edible"))
            {
                GetComponent<SnakeAgent>().RewardForEating();
                Grow();
                Destroy(go);
            }
            else
            {
                // Any other collision causes the snake to die.
                Dead();
            }
        }

        // Move the snake's head to the new position.
        transform.localPosition = moveToPoint;

        // If the snake is not growing, move the last body part to the previous head position.
        if (!snakeGrowing)
        {
            // Check if there are any body parts before attempting to reposition.
            if (snakesBody.Count > 0)
            {
                // Get the last body part from the list.
                GameObject lastBodypart = snakesBody[snakesBody.Count - 1].gameObject;
                // Remove the last body part from the list.
                snakesBody.RemoveAt(snakesBody.Count - 1);
                // Reposition it to where the head was.
                lastBodypart.transform.localPosition = origonPoint;
                // Insert it at the beginning of the list.
                snakesBody.Insert(0, lastBodypart);
            }
            else
            {
                // If no body parts exist (which might occur after a reset), spawn one to maintain consistency.
                SpawnBodyUnderHead(origonPoint);
            }
        }
        else
        {
            // If the snake is growing, spawn a new body part under the head without removing any parts.
            SpawnBodyUnderHead(origonPoint);
            // Reset the growth flag.
            snakeGrowing = false;
        }
    }


    public void MoveLeft() => movementDirection = Vector2.left;
    public void MoveRight() => movementDirection = Vector2.right;
    public void MoveUp() => movementDirection = Vector2.up;
    public void MoveDown() => movementDirection = Vector2.down;
    #endregion

    public void Grow()
    {
        snakeGrowing = true;
        FoodSpawner.Instance.SpawnFood();
    }

    GameObject SpawnBody(Vector2 position)
    {
        //Setup
        GameObject bodypart = PoolManager.instance.GetObject("SnakeBodypart");
        bodypart.transform.parent = transform.parent;
        bodypart.transform.localPosition = position;

        //Change color
        Color targetColor = useMainColor ? mainColor : seconddaryColor;
        useMainColor = !useMainColor;
        bodypart.GetComponent<SpriteRenderer>().color = targetColor;

        //Show
        bodypart.SetActive(true);

        return bodypart;
    }

    void SpawnBodyUnderHead(Vector2 position)
    {
        GameObject go = SpawnBody(position);
        snakesBody.Insert(0, go);
    }

    void SpawnBodyInEnd(Vector2 position)
    {
        GameObject go = SpawnBody(position);
        snakesBody.Add(go);
    }

    /// <summary>
    /// Resets the snake to its initial state by repositioning the head, cleaning up body parts,
    /// and reinitializing the snake's body. This method is called at the start of every new episode.
    /// </summary>
    public void ResetSnake()
    {
        // Reset head position to origin.
        transform.localPosition = Vector2.zero;
    
        // Reset movement direction to upward.
        movementDirection = Vector2.up;
    
        // Return all current body parts to the pool.
        // Ensure PoolManager.ReturnObject deactivates these objects (e.g. via bodyPart.SetActive(false)).
        foreach (var bodyPart in snakesBody)
        {
            PoolManager.instance.ReturnObject("SnakeBodypart", bodyPart);
        }
    
        // Clear the snake's body parts list.
        snakesBody.Clear();
    
        // Spawn the initial snake body parts behind the head.
        for (int i = 1; i <= startBodySize; i++)
        {
            // Calculate spawn position based on head position and movement direction.
            SpawnBodyInEnd((Vector2)transform.localPosition - (movementDirection * i));
        }
    
        // Re-subscribe the Move method to the GameStep event for the new episode.
        SnakeGameManager.instance.GameStep += Move;
    }


    public Vector2Int GetNextTile()
    {
        return new Vector2Int(
            Mathf.RoundToInt(transform.localPosition.x + movementDirection.x),
            Mathf.RoundToInt(transform.localPosition.y + movementDirection.y)
        );
    }

    public Vector2 GetDirection()
    {
        return movementDirection;
    }

    /// <summary>
    /// Handles snake death by resetting the snake immediately and ending the current episode.
    /// This method is called when the snake collides with a wall or its own body.
    /// </summary>
    public void Dead()
    {
        Debug.Log("Dead");
        SnakeGameManager.instance.GameStep -= Move;
        // Immediately clear any remaining body parts and reset the snake's state.
        ResetSnake();
        // Penalize the agent and end the episode to trigger a reset.
        GetComponent<SnakeAgent>().PenalizeForDeath();
    }


}