using System.Collections.Generic;
using UnityEngine;

public struct Coordinate
{
    public int x;
    public int y;

    public Coordinate(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
[RequireComponent(typeof(GameManager))]
public class GameGrid : MonoBehaviour
{
    
    public GameObject[,] Grid { get; private set; }
    private List<Coordinate> _freeCoordinates = new List<Coordinate>();

    private GameManager _gameManager;

    // Start is called before the first frame update
    void Awake()
    {
        _gameManager = GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Bounds SetGrid(int gridSize)
    {
        Bounds bounds = new Bounds();
        Grid = new GameObject[gridSize, gridSize];
        // Creates a grid with free positions for spawning fruit and pick ups
        // Avoids the edges of the map so no spawning will occur there
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if ((x != 0 || x != gridSize - 1) || (y != 0 || y != gridSize - 1))
                {
                    _freeCoordinates.Add(new Coordinate(x, y));
                }
                GameObject quadPrimitive = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quadPrimitive.transform.position = new Vector3(x, 0, y);
                Destroy(quadPrimitive.GetComponent<Collider>());
                quadPrimitive.transform.localEulerAngles = new Vector3(90, 0, 0);
                quadPrimitive.transform.SetParent(transform);
                Grid[x, y] = quadPrimitive;
                Renderer temp = Grid[x, y].GetComponent<Renderer>();
                temp.material = _gameManager.groundMaterial;
                bounds.Encapsulate(temp.bounds);
            }
        }
        return bounds;
    }

    public void MakeCoordinateOccupied(Coordinate c)
    {
        if (_freeCoordinates.Contains(c))
        {
            _freeCoordinates.Remove(c);
        }
    }

    public void MakeCoordinateFree(Coordinate c)
    {
        if (!_freeCoordinates.Contains(c))
        {
            _freeCoordinates.Add(c);
        }
    }

    /// <summary>
    /// Returns a coordinate that is free and generated at random
    /// </summary>
    /// <returns></returns>
    public Coordinate GetFreeCoordinate()
    {
        return _freeCoordinates[Random.Range(0, _freeCoordinates.Count - 1)];
    }
}
