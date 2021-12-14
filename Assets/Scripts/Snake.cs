using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SnakeList;

// Nodes for the linked list in Snake, one node per part of the body

public class Node
{
    public bool Head { get; set; }
    public int IntCoordinate { get; set; }
    public Coordinate Coordinate { get; set; }
    public GameObject PickUpObject { get; set; }
    public Material SnakeMaterial { get; set; }
}

// Handles the most regarding the snake
public class Snake : MonoBehaviour
{
    [SerializeField]
    private PickUp _pickUp;
    public enum Special { ExtraSpeed, RemoveObstacle, Guide, ExtraPoints };

    private LinkedSnakeList<Node> _snakeNodes = new LinkedSnakeList<Node>();

    private GameManager _gameManager;

    private Node _head = new Node();

    private GameGrid _gameGrid;

    // Start is called before the first frame update
    void Awake()
    {
        _gameManager = GetComponent<GameManager>();
        _gameGrid = GetComponent<GameGrid>();
    }


    public Snake CreateNewSnake(int length, int areaResolution)
    {
        _snakeNodes = new LinkedSnakeList<Node>();
        int offset = length + 1;
        Coordinate headCoord = new Coordinate(Random.Range(offset, areaResolution - offset), 
            Random.Range(offset, areaResolution - offset));

        for (int i = 0; i < length; i++)
        {
            Node node = new Node();
            if (i == 0)
            {
                _snakeNodes.AddFirst(_head);
                _head.Coordinate = headCoord;
                _head.Head = true;
                node.SnakeMaterial = _gameManager.headMaterial;
            }
            else
            {
                _snakeNodes.AddLast(node);
                node.Coordinate = new Coordinate(headCoord.x - i, headCoord.y);
                node.SnakeMaterial = _gameManager.snakeMaterial;
            }
            node.PickUpObject = null;
            _gameGrid.MakeCoordinateOccupied(node.Coordinate);
        }
        return this;
    }

    public int GetSnakeLength()
    {
        return _snakeNodes.Count();
    }


    public List<Coordinate> GetCoordinates()
    {
        List<Coordinate> temp = new List<Coordinate>();
        foreach (Node node in _snakeNodes)
        {
            temp.Add(node.Coordinate);
        }
        return temp;
    }

    public void AddToSnake(Coordinate coordinate)
    {
        Node node = new Node
        {
            Coordinate = coordinate,
            SnakeMaterial = _gameManager.snakeMaterial,
            PickUpObject = null
        };
        _snakeNodes.AddLast(node);
    }

    public Coordinate GetHeadCoordinate()
    {
        return _head.Coordinate;
    }

    public List<Coordinate> MoveSnake(Coordinate newHeadCoordinate)
    {
        List<Coordinate> coordinates = new List<Coordinate>();
        // A temporary list to keep the old coordinates
        LinkedList<Coordinate> formerNodeCoordinates = new LinkedList<Coordinate>();
        foreach (Node node in _snakeNodes)
        {
            // Save the former position, I made it reversed because sometimes AddLast() made Find().Previous
            // count the wrong way and the snake wouldnt grow the correct way every time
            formerNodeCoordinates.AddFirst(node.Coordinate); 
            if (node.Head)
            {
                node.Coordinate = newHeadCoordinate;
            }
            else
            {
                _gameGrid.MakeCoordinateFree(node.Coordinate); // Safe play to empty the grid that the snake leaves 
                node.Coordinate = formerNodeCoordinates.Find(node.Coordinate).Next.Value;
            }
            coordinates.Add(node.Coordinate);
            _gameGrid.MakeCoordinateOccupied(node.Coordinate);
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
                node.PickUpObject = _pickUp.GetNewPickup();
                node.SnakeMaterial = GetPickUpMaterial(node.PickUpObject);
                return;
            }
        }
    }

    public Material GetMaterialFromLinkedList(int index)
    {
        int i = -1;
        foreach (Node node in _snakeNodes)
        {
            i++;
            if (node.Head && 0 == index)
            {
                return _gameManager.headMaterial;
            }
            else if (!node.Head && i == index)
            {
                return node.SnakeMaterial;
            }
        }
        Debug.LogError("Error: this should not be reached, not enough nodes in the snake");
        return _gameManager.snakeMaterial;
    }

    public bool RemovePickUpMaterial(Special special)
    {
        if (special == Special.ExtraPoints)
        {
            return FindAndChangeMaterialNode(_pickUp.pickups[0]);
        }
        else if (special == Special.ExtraSpeed)
        {
            return FindAndChangeMaterialNode(_pickUp.pickups[1]);
        }
        else if (special == Special.Guide)
        {
            return FindAndChangeMaterialNode(_pickUp.pickups[2]);
        }
        else if (special == Special.RemoveObstacle)
        {
            return FindAndChangeMaterialNode(_pickUp.pickups[3]);
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
                node.SnakeMaterial = _gameManager.snakeMaterial;
                return true;
            }
        }
        return false;
    }

    private Material GetPickUpMaterial(GameObject pickup)
    {
        // Check if the components are there, but they are not needed, hence the "discard"
        if (pickup.TryGetComponent(out ObstacleRemover _))
        {
            return _pickUp.removeObstacleMaterial;
        }
        else if (pickup.TryGetComponent(out ExtraPoints _))
        {
            return _pickUp.extraMaterial;
        }
        else if (pickup.TryGetComponent(out ExtraSpeed _))
        {
            return _pickUp.speedMaterial;
        }
        else if (pickup.TryGetComponent(out Guide _))
        {
            return _pickUp.guideMaterial;
        }
        else
        {
            Debug.LogWarning("Warning: this should not be reached! We are out of materials for pick ups!");
            return _gameManager.snakeMaterial;
        }
    }
}
