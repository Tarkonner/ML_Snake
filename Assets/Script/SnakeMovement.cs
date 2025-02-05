using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class SnakeMovement : MonoBehaviour
{
    //Movement
    private List<GameObject> snakesBody = new List<GameObject>();
    private Vector2 movementDirection = Vector2.up;
    private Vector2 moveDirectionLastTurn;
    [SerializeField] int startBodySize = 3;
    [SerializeField] Vector2 startDirection = Vector2.right;

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

        MakeSnake();
    }


    private void OnDisable()
    {
        //Move with game tick
        SnakeGameManager.instance.GameStep -= Move;
    }

    #region Movement
    public void Move()
    {
        Vector3 origonPoint = transform.localPosition;
        Vector2 moveToPoint = transform.localPosition + (Vector3)(movementDirection * Grid.gridSize);
        moveDirectionLastTurn = movementDirection;

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
            {
                Dead();
                return;
            }
        }


        //Move head
        transform.localPosition = moveToPoint;
                
        if(!snakeGrowing)
        {
            //Get part
            GameObject lastBodypart = snakesBody[snakesBody.Count - 1].gameObject;
            snakesBody.RemoveAt(snakesBody.Count - 1);
            
            //Place and change color
            lastBodypart.transform.localPosition = origonPoint;
            SetSnakesColor(lastBodypart);

            snakesBody.Insert(0, lastBodypart);
        }
        else
        {
            SpawnBodyUnderHead(origonPoint);
            snakeGrowing = false;
        }
    }

    public void MoveLeft()
    {
        if (moveDirectionLastTurn == Vector2.right)
            return;

        movementDirection = Vector2.left;

    }
    public void MoveRight()
    {
        if (moveDirectionLastTurn == Vector2.left)
            return;

        movementDirection = Vector2.right;
    }
    public void MoveUp()
    {
        if (moveDirectionLastTurn == Vector2.down)
            return;

        movementDirection = Vector2.up;
    }
    public void MoveDown()
    {
        if (moveDirectionLastTurn == Vector2.up)
            return;

        movementDirection = Vector2.down;
    }
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

        SetSnakesColor(bodypart);

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


    private void MakeSnake()
    {
        movementDirection = startDirection;

        //Make body
        for (int i = 1; i < startBodySize + 1; i++)
        {
            SpawnBodyInEnd((Vector2)transform.localPosition - (startDirection * i));
        }
    }

    public void Dead()
    {
        //Remove body
        foreach (GameObject item in snakesBody)
        {
            PoolManager.instance.ReturnObject("SnakeBodypart", item);
        }
        snakesBody.Clear();

        transform.localPosition = Vector2.zero;
        MakeSnake();
    }

    public void SetSnakesColor(GameObject bodypart)
    {
        Color targetColor = useMainColor ? mainColor : seconddaryColor;
        useMainColor = !useMainColor;
        bodypart.GetComponent<SpriteRenderer>().color = targetColor;
    }
}
