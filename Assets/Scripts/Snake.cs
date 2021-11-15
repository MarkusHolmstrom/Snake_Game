using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool Head { get; set; }
    public int Coordinate { get; set; }
    public GameObject PickUpObject { get; set; }
    public enum Special { Speed, RemoveObstacle, Guide, ExtraPoints};
}

public class Snake : MonoBehaviour
{
    public PickUp pickUp;
    
    // https://www.c-sharpcorner.com/article/linked-list-implementation-in-c-sharp/ mer hjälp
    private LinkedList<Node> _snakeNodes = new LinkedList<Node>();

    Node head = new Node();

    // Start is called before the first frame update
    void Awake()
    {
        // LinkedListNode<Body> tail = _snake.FindLast(head);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Snake CreateSnake(int length, int areaResolution)
    {
        int firstlock = Random.Range(0, areaResolution - 1) + (areaResolution * length);

        for (int i = 0; i < length; i++)
        {
            Node body = new Node();
            if (i == 0)
            {
                _snakeNodes.AddFirst(head);
                head.Coordinate = firstlock - (areaResolution * i);
                head.Head = true;
            }
            else
            {
                _snakeNodes.AddLast(body);
                body.Coordinate = firstlock - (areaResolution * i);
            }
        }
        return this;
    }

    public int GetSnakeLength()
    {
        return _snakeNodes.Count;
    }

    public void SetCoordinates(List<int> coordinates)
    {
        if (coordinates.Count != _snakeNodes.Count)
        {
            Debug.LogError("Error: to many coordinates or the snejk is to short!");
            return;
        }
        else
        {
            int i = 0;
            foreach (Node node in _snakeNodes)
            {
                node.Coordinate = coordinates[i];
                i++;
            }
        }
    }

    public List<int> GetCoordinates()
    {
        List<int> temp = new List<int>();
        foreach (Node node in _snakeNodes)
        {
            temp.Add(node.Coordinate);
        }
        return temp;
    }
    public void AddToSnake(int coordinate)
    {
        Node node = new Node();
        node.Coordinate = coordinate;
        _snakeNodes.AddLast(node);
    }

    public int GetHeadCoordinate()
    {
        return head.Coordinate;
    }

    public List<int> MoveSnake(int newHeadCoordinate)
    {
        List<int> coordinates = new List<int>();
        // A temporary list to keep the old coordinates
        LinkedList<int> formerNodes = new LinkedList<int>();
        foreach (Node node in _snakeNodes)
        {
            formerNodes.AddLast(node.Coordinate);
            if (node.Head)
            {
                node.Coordinate = newHeadCoordinate;
            }
            else
            {
                node.Coordinate = formerNodes.Find(node.Coordinate).Previous.Value;
            }
            coordinates.Add(node.Coordinate);
        }
        return coordinates;
    }

    /// <summary>
    /// Gets a random pickup from list in the PickUp class, adds 
    /// and it to the first available spot in the snakes body, expt its head
    /// </summary>
    public void GetRandomPickUp()
    {
        foreach (Node node in _snakeNodes)
        {
            if (node.PickUpObject == null && !node.Head)
            {
                node.PickUpObject = pickUp.GetNewPickup();
            }
        }
    }
}
