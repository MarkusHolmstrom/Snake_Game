using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SnakeList;

// Nodes for the linked list in Snake, one node per part of the body

public class Node
{
    public bool Head { get; set; }
    public int Coordinate { get; set; }
    public GameObject PickUpObject { get; set; }
    public Material SnakeMaterial { get; set; }
}

// Handles the most regarding the snake
public class Snake : MonoBehaviour
{
    public PickUp pickUp;
    public enum Special { ExtraSpeed, RemoveObstacle, Guide, ExtraPoints };

    private LinkedSnakeList<Node> _snakeNodes = new LinkedSnakeList<Node>();

    private GameManager gameManager;

    Node head = new Node();

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GetComponent<GameManager>();
    }


    public Snake CreateNewSnake(int length, int areaResolution)
    {
        _snakeNodes = new LinkedSnakeList<Node>();
        int firstlock = Random.Range(0, areaResolution - 1) + (areaResolution * length);

        for (int i = 0; i < length; i++)
        {
            Node node = new Node();
            if (i == 0)
            {
                _snakeNodes.AddFirst(head);
                head.Coordinate = firstlock - (areaResolution * i);
                head.Head = true;
            }
            else
            {
                _snakeNodes.AddLast(node);
                node.Coordinate = firstlock - (areaResolution * i);
            }
            node.SnakeMaterial = gameManager.snakeMaterial;
            node.PickUpObject = null;
        }
        return this;
    }

    public int GetSnakeLength()
    {
        return _snakeNodes.Count();
    }

    public void SetCoordinates(List<int> coordinates)
    {
        if (coordinates.Count != _snakeNodes.Count())
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
        Node node = new Node
        {
            Coordinate = coordinate,
            SnakeMaterial = gameManager.snakeMaterial,
            PickUpObject = null
        };
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
            // Save the former position, I made it reversed because sometimes AddLast() made Find().Previous
            // count the wrong way and the snake wouldnt grow correct way every time
            formerNodes.AddFirst(node.Coordinate); 
            if (node.Head)
            {
                node.Coordinate = newHeadCoordinate;
            }
            else
            {
                node.Coordinate = formerNodes.Find(node.Coordinate).Next.Value;
            }
            coordinates.Add(node.Coordinate); 
        }
        return coordinates;
    }

    /// <summary>
    /// Gets a random pickup from list in the PickUp class, adds 
    /// and it to the first available spot in the snakes body, expt its head
    /// if the snake is full of pick ups, then nothing new gets added
    /// </summary>
    public void GetRandomPickUp()
    {
        foreach (Node node in _snakeNodes)
        {
            if (node.PickUpObject == null && !node.Head)
            {
                node.PickUpObject = pickUp.GetNewPickup();
                node.SnakeMaterial = GetPickUpMaterial(node.PickUpObject);
                return;
            }
        }
    }

    public Material GetMaterialFromLinkedList(int coordinate)
    {
        foreach (Node node in _snakeNodes)
        {
            if (node.Head && node.Coordinate == coordinate)
            {
                return gameManager.headMaterial;
            }
            else if (node.Coordinate == coordinate && !node.Head)
            {
                return node.SnakeMaterial;
            }
        }
        Debug.LogError("Error: this should not be reached, not enough nodes in the snake");
        return gameManager.snakeMaterial;
    }

    public bool RemovePickUpMaterial(Special special)
    {
        if (special == Special.ExtraPoints)
        {
            return FindAndChangeMaterialNode(pickUp.pickups[0]);
        }
        else if (special == Special.ExtraSpeed)
        {
            return FindAndChangeMaterialNode(pickUp.pickups[1]);
        }
        else if (special == Special.Guide)
        {
            return FindAndChangeMaterialNode(pickUp.pickups[2]);
        }
        else if (special == Special.RemoveObstacle)
        {
            return FindAndChangeMaterialNode(pickUp.pickups[3]);
        }
        else
        {
            return false;
        }
    }

    private bool FindAndChangeMaterialNode(GameObject pickup)
    {
        foreach (Node node in _snakeNodes)
        {
            if (node.PickUpObject == pickup)
            {
                node.SnakeMaterial = gameManager.snakeMaterial;
            }
        }
        return false;
    }

    private Material GetPickUpMaterial(GameObject pickup)
    {
        // Check if the components are there, bu they are not needed, hence the "discard"
        if (pickup.TryGetComponent(out ObstacleRemover _))
        {
            return pickUp.removeObstacleMaterial;
        }
        else if (pickup.TryGetComponent(out ExtraPoints _))
        {
            return pickUp.extraMaterial;
        }
        else if (pickup.TryGetComponent(out ExtraSpeed _))
        {
            return pickUp.speedMaterial;
        }
        else if (pickup.TryGetComponent(out Guide _))
        {
            return pickUp.guideMaterial;
        }
        else
        {
            Debug.LogError("Error: this should not be reached!");
            return gameManager.snakeMaterial;
        }
    }
}
