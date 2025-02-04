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

        //Make body
        for (int i = 1; i < startBodySize + 1; i++)
        {
            SpawnBodyInEnd((Vector2)transform.localPosition - (movementDirection * i)); 
        }
    }

    #region Movement
    public void Move()
    {
        Vector3 origonPoint = transform.localPosition;

        Vector2 moveToPoint = transform.localPosition + (Vector3)(movementDirection * Grid.gridSize);

        Vector2 boxSize = new Vector2(0.1f, 0.1f); // Small square area
        Collider2D hit = Physics2D.OverlapBox(moveToPoint, boxSize, 0f);

        if (hit != null)
        {
            GameObject go = hit.gameObject;
            if (go.CompareTag("edible"))
            {
                Grow();
                Destroy(go);
            }                
            else
                Dead();
        }


        //Move head
        transform.localPosition = moveToPoint;
                
        if(!snakeGrowing)
        {
            GameObject lastBodypart = snakesBody[snakesBody.Count - 1].gameObject;
            snakesBody.RemoveAt(snakesBody.Count - 1);
            lastBodypart.transform.localPosition = origonPoint;
            snakesBody.Insert(0, lastBodypart);
        }
        else
        {
            SpawnBodyUnderHead(origonPoint);
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


    public void Dead()
    {
        Debug.Log("Dead");
        SnakeGameManager.instance.GameStep -= Move;
    }
}
