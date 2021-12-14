using System.Collections.Generic;
using UnityEngine;

// Handles the most of the gameplay, based on this help from the world wide web:
// https://sharpcoderblog.com/blog/snake-game-in-unity-3d
// It has been modified so it suits the assignements requirements

public class GameManager : MonoBehaviour
{
    // Game area resolution, the higher number means more blocks
    // Recomended to play with 900x900 resolution
    [Header("Play with x = y resolution for playability")]
    [Header("Size of the grid: Grid Size * Grid Size")]
    [SerializeField]
    private int _gridSize = 22;

    [SerializeField]
    private float _snakeSpeed = 6f;
    public float speedModifier = 1.0f;

    [SerializeField]
    private int _startSnakeLength = 3;

    [Range(0, 40)]
    [SerializeField]
    private int _nrOfObstacles = 8;
    [SerializeField]
    private bool _obstacleGeneration = true;
    [Range(5, 20)]
    [SerializeField]
    private int _obstacleGenerationPace = 10;

    [SerializeField]
    private Camera _mainCamera;
    // Materials
    public Material groundMaterial;
    public Material snakeMaterial;
    public Material headMaterial;
    public Material fruitMaterial;
    public Material obstacleMaterial;

    public bool fruitPicked = false;

    public PickUp pickUp;
    [Range(1, 10)]
    private int _pickUpRatio = 2;
    private bool _pickUpsActive = true;

    [Header("The added points when picking up fruit or power-up")]
    public int extraScore = 1;
    [SerializeField]
    private ExtraPoints _extraPoints;
    [SerializeField]
    private int score = 0;
    // Gets activated from the Guide class
    public bool guideIsActive = false;

    // Grid system
    private GameGrid _gameGrid;

    private List<Coordinate> _snakeCoordinates = new List<Coordinate>();
    private enum Direction { Up, Down, Left, Right };
    private Direction _snakeDirection = Direction.Right;
    private float _timeTmp = 0;
    // Coordinate where the fruit is placed
    private Coordinate _fruitBlockCoordinate = new Coordinate(-1, -1);

    private bool _gameStarted = false;
    private bool _gameOver = false;
    // Camera scaling
    private Bounds _targetBounds = new Bounds();
    // Text styling
    private GUIStyle _mainStyle = new GUIStyle();

    private Stack<Coordinate> _obstacleCoordinates = new Stack<Coordinate>();

    private Snake _snake;

    private Coordinate _pickUpCoordinate = new Coordinate(-1, -1);

    // Start is called before the first frame update
    void Start()
    {
        _snake = GetComponent<Snake>();
        _gameGrid = GetComponent<GameGrid>();

        // Generate play area
        _targetBounds = _gameGrid.SetGrid(_gridSize);

        // Scale the MainCamera to fit the game blocks
        _mainCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        _mainCamera.orthographic = true;
        float screenRatio = Screen.width / Screen.height;
        float targetRatio = _targetBounds.size.x / _targetBounds.size.y;

        if (screenRatio >= targetRatio)
        {
            _mainCamera.orthographicSize = _targetBounds.size.y / 2;
        }
        else
        {
            // Ugly fix to camera when its resultion is set to "Free Aspect",
            // also works with other resultions when screen is wider than its height
            _mainCamera.orthographicSize = 11;
        }
        _mainCamera.transform.position = new Vector3(_targetBounds.center.x, _targetBounds.center.y + 1, _targetBounds.center.z);

        // Create the Snake 
        InitializeSnake(_startSnakeLength);

        _mainStyle.fontSize = 24;
        _mainStyle.alignment = TextAnchor.MiddleCenter;
        _mainStyle.normal.textColor = Color.white;
    }


    // Update is called once per frame
    void Update()
    {
        if (!_gameStarted)
        {
            if (Input.anyKeyDown)
            {
                _gameStarted = true;
            }
            return;
        }
        if (_gameOver)
        {
            // Flicker the snake blocks
            if (_timeTmp < 0.44f)
            {
                _timeTmp += Time.deltaTime;
            }
            else
            {
                _timeTmp = 0;
                for (int i = 0; i < _snakeCoordinates.Count; i++)
                {
                    if (_gameGrid.Grid[_snakeCoordinates[i].x, _snakeCoordinates[i].y].GetComponent<Renderer>().material == groundMaterial)
                    {
                        _gameGrid.Grid[_snakeCoordinates[i].x, _snakeCoordinates[i].y].GetComponent<Renderer>().material = (i == 0 ? headMaterial : snakeMaterial);
                    }
                    else
                    {
                        _gameGrid.Grid[_snakeCoordinates[i].x, _snakeCoordinates[i].y].GetComponent<Renderer>().material = groundMaterial;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                InitializeSnake(_startSnakeLength);
                _gameOver = false;
                _gameStarted = false;
            }
        }
        else
        {
            if (_timeTmp < 1)
            {
                _timeTmp += Time.deltaTime * _snakeSpeed * speedModifier;
            }
            else
            {
                _timeTmp = 0;
                // Grid based movement 
                Coordinate newCoordinate = GetNewCoordinate(_snake.GetHeadCoordinate(), _snakeDirection);
                if (_snakeDirection == Direction.Right || _snakeDirection == Direction.Left)
                {
                    // Detect if the Snake hit the sides
                    if (_snakeDirection == Direction.Left && _snakeCoordinates[0].x <= 0)
                    {
                        if (guideIsActive)
                        {
                            _snakeDirection = GuideChangesDirection(_snakeDirection);
                        }
                        else
                        {
                            _gameOver = true;
                        }
                        return;
                    }
                    else if (_snakeDirection == Direction.Right && _snakeCoordinates[0].x >= _gridSize - 1)
                    {
                        if (guideIsActive)
                        {
                            _snakeDirection = GuideChangesDirection(_snakeDirection);
                        }
                        else
                        {
                            _gameOver = true;
                        }
                        return;
                    }

                    // Snake has ran into itself or hits obstacle: game over
                    if (_snakeCoordinates.Contains(newCoordinate) || _obstacleCoordinates.Contains(newCoordinate))
                    {
                        if (guideIsActive)
                        {
                            _snakeDirection = GuideChangesDirection(_snakeDirection);
                        }
                        else
                        {
                            _gameOver = true;
                        }
                        return;
                    }
                    if (newCoordinate.x < _gridSize && newCoordinate.y < _gridSize &&
                        newCoordinate.x >= 0 && newCoordinate.y >= 0)
                    {
                        // Move snake to new position
                        _snakeCoordinates = _snake.MoveSnake(newCoordinate);
                        _gameGrid.Grid[_snakeCoordinates[0].x, _snakeCoordinates[0].y] 
                            .transform.localEulerAngles = new Vector3(90, (_snakeDirection == Direction.Left ? -90 : 90), 0);
                    }
                }
                else if (_snakeDirection == Direction.Up || _snakeDirection == Direction.Down)
                {
                    // Detect if snake hits the top or bottom
                    if (_snakeDirection == Direction.Up && _snakeCoordinates[0].y >= _gridSize - 1)
                    {
                        if (guideIsActive)
                        {
                            _snakeDirection = GuideChangesDirection(_snakeDirection);
                        }
                        else
                        {
                            _gameOver = true;
                        }
                        return;
                    }
                    else if (_snakeDirection == Direction.Down && (_snakeCoordinates[0].y + 1) % _gridSize == 1)
                    {
                        if (guideIsActive)
                        {
                            _snakeDirection = GuideChangesDirection(_snakeDirection);
                        }
                        else
                        {
                            _gameOver = true;
                        }
                        return;
                    }

                    // Snake has ran into itself or hits obstacle: game over
                    if (_snakeCoordinates.Contains(newCoordinate) || _obstacleCoordinates.Contains(newCoordinate))
                    {
                        if (guideIsActive)
                        {
                            _snakeDirection = GuideChangesDirection(_snakeDirection);
                        }
                        else
                        {
                            _gameOver = true;
                        }
                        return;
                    }
                    else if (newCoordinate.x < _gridSize && newCoordinate.y < _gridSize && 
                        newCoordinate.x >= 0 && newCoordinate.y >= 0)
                    {
                        // Move snake to new position
                        _snakeCoordinates = _snake.MoveSnake(newCoordinate);
                        _gameGrid.Grid[_snakeCoordinates[0].x, _snakeCoordinates[0].y] 
                            .transform.localEulerAngles = new Vector3(90, (_snakeDirection == Direction.Down ? 180 : 0), 0);
                    }
                }

                ApplyMaterials();
            }
            
        }
        // Direction
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Coordinate newCoordinate = new Coordinate(_snakeCoordinates[0].x + 1, _snakeCoordinates[0].y);
            if (!_snakeCoordinates.Contains(newCoordinate))
            {
                _snakeDirection = Direction.Right;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Coordinate newCoordinate = new Coordinate(_snakeCoordinates[0].x - 1, _snakeCoordinates[0].y);
            if (!_snakeCoordinates.Contains(newCoordinate))
            {
                _snakeDirection = Direction.Left;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Coordinate newCoordinate = new Coordinate(_snakeCoordinates[0].x, _snakeCoordinates[0].y + 1);
            if (!_snakeCoordinates.Contains(newCoordinate))
            {
                _snakeDirection = Direction.Up;
            }
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Coordinate newCoordinate = new Coordinate(_snakeCoordinates[0].x, _snakeCoordinates[0].y - 1);
            if (!_snakeCoordinates.Contains(newCoordinate))
            {
                _snakeDirection = Direction.Down;
            }
        }

        if (_fruitBlockCoordinate.x < 0)
        {
            _fruitBlockCoordinate = _gameGrid.GetFreeCoordinate();
        }
        if (!_pickUpsActive && _pickUpCoordinate.x < 0 && score % _pickUpRatio == 0 && score != 0)
        {
            _pickUpCoordinate = _gameGrid.GetFreeCoordinate();
        }
    }
    /// <summary>
    /// Grid based movement, based on direction
    /// </summary>
    /// <param name="headCoord"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private Coordinate GetNewCoordinate(Coordinate headCoord, Direction direction)
    {
        if (direction == Direction.Down)
        {
             headCoord.y--;
        }
        else if (direction == Direction.Up)
        {
            headCoord.y++;
        }
        else if (direction == Direction.Left)
        {
            headCoord.x--;
        }
        else // if (direction == Direction.Right)
        {
            headCoord.x++;
        }
        return headCoord;
    }


    private void InitializeSnake(int length)
    {
        _snakeCoordinates.Clear();
        _snake = _snake.CreateNewSnake(length, _gridSize);
        _snakeCoordinates = _snake.GetCoordinates();

        _obstacleCoordinates = GenerateObstacles();
        
        _gameGrid.Grid[_snakeCoordinates[0].x, _snakeCoordinates[0].y].transform.localEulerAngles = new Vector3(90, 90, 0);
        _fruitBlockCoordinate.x = -1;
        _pickUpCoordinate.x = -1;
        _timeTmp = 1;
        _snakeDirection = Direction.Right;
        score = 0;
        speedModifier = 1;
        _extraPoints.ResetExtraScore();
        ApplyMaterials();
    }

    private Stack<Coordinate> GenerateObstacles()
    {
        Stack<Coordinate> indexes = new Stack<Coordinate>();
        for (int i = 0; i < _nrOfObstacles; i++)
        {
            Coordinate obstacleCoord = new Coordinate(Random.Range(0, _gridSize - 1),
                Random.Range(0, _gridSize - 1));
            indexes.Push(obstacleCoord);
            _gameGrid.MakeCoordinateOccupied(obstacleCoord);
        }
        return indexes;
    }

    /// <summary>
    /// Gets activated when the pickup power "Guide" is active and the player is about to lose
    /// </summary>
    private Direction GuideChangesDirection(Direction curDirection)
    {
        if (curDirection == Direction.Up)
        {
            return Direction.Left;
        }
        else if (curDirection == Direction.Down)
        {
            return Direction.Right;
        }
        else if (curDirection == Direction.Left)
        {
            return Direction.Down;
        }
        else // If right:
        {
            return Direction.Up;
        }
    }

    private void ApplyMaterials()
    {
        // Apply Snake material
        List<Coordinate> coordinates = _snake.GetCoordinates();
        for (int a = 0; a < _snake.GetSnakeLength(); a++)
        {
            _gameGrid.Grid[coordinates[a].x, coordinates[a].y].GetComponent<Renderer>().material = _snake.GetMaterialFromLinkedList(a);
        }
        for (int x = 0; x < _gridSize; x++)
        {
            for (int y = 0; y < _gridSize; y++)
            {
                fruitPicked = false;
                Coordinate c = new Coordinate(x, y);
                if (_snake.GetHeadCoordinate().Equals(_fruitBlockCoordinate))
                {
                    // Pick a fruit
                    fruitPicked = true;
                }
                if (_snake.GetHeadCoordinate().Equals(_pickUpCoordinate))
                {
                    // Pick a pick up
                    _snake.GetRandomPickUp();
                    _pickUpCoordinate.x = -1;
                    _pickUpsActive = true;
                    score += extraScore;
                }

                if (fruitPicked)
                {
                    _fruitBlockCoordinate.x = -1;
                    // Add new block
                    Coordinate coord = _snake.GetCoordinates()[_snakeCoordinates.Count - 1];
                    int snakeBlockRotationY = (int)_gameGrid.Grid[coord.x, coord.y].transform.localEulerAngles.y;

                    if (snakeBlockRotationY == 270)
                    {
                        Coordinate coordinate = new Coordinate(coord.x + 1, coord.y);
                        _snakeCoordinates.Add(coordinate);
                        _snake.AddToSnake(coordinate);

                    }
                    else if (snakeBlockRotationY == 90)
                    {
                        Coordinate coordinate = new Coordinate(coord.x - 1, coord.y);
                        _snakeCoordinates.Add(coordinate);
                        _snake.AddToSnake(coordinate);
                    }
                    else if (snakeBlockRotationY == 0)
                    {
                        Coordinate coordinate = new Coordinate(coord.x, coord.y - 1);
                        _snakeCoordinates.Add(coordinate);
                        _snake.AddToSnake(coordinate);
                    }
                    else if (snakeBlockRotationY == 180)
                    {
                        Coordinate coordinate = new Coordinate(coord.x, coord.y + 1);
                        _snakeCoordinates.Add(coordinate);
                        _snake.AddToSnake(coordinate);
                    }
                    score += extraScore;
                }

                if (score % _pickUpRatio == 0 && score != 0 && _pickUpsActive)
                {
                    _pickUpsActive = false;
                }

                if (_obstacleCoordinates.Contains(c))
                {
                    _gameGrid.Grid[x, y].GetComponent<Renderer>().material = obstacleMaterial;
                    _gameGrid.Grid[x, y].transform.localEulerAngles = new Vector3(90, 0, 0);
                }

                if (c.Equals(_fruitBlockCoordinate) && !fruitPicked)
                {
                    _gameGrid.Grid[x,y].GetComponent<Renderer>().material = fruitMaterial;
                    _gameGrid.Grid[x, y].transform.localEulerAngles = new Vector3(90, 0, 0);
                }
                else if (c.Equals(_pickUpCoordinate))
                {
                    _gameGrid.Grid[x, y].GetComponent<Renderer>().material = pickUp.pickUpMaterial;
                    _gameGrid.Grid[x, y].transform.localEulerAngles = new Vector3(90, 0, 0);
                }
                else if (!_snakeCoordinates.Contains(c) && !_obstacleCoordinates.Contains(c))
                {
                    _gameGrid.Grid[x, y].GetComponent<Renderer>().material = groundMaterial;
                    _gameGrid.Grid[x, y].transform.localEulerAngles = new Vector3(90, 0, 0);
                }

                if (_obstacleGeneration && score % _obstacleGenerationPace == 0 && score != 0)
                {
                    AddNewObstacle();
                    score += extraScore;
                }
            }
        }
    }

    /// <summary>
    /// Makes the game more difficult, adds obstacles at a pace
    /// </summary>
    private void AddNewObstacle()
    {
        bool emptySpotFound = false;
        while (!emptySpotFound)
        {
            Coordinate coordinate = _gameGrid.GetFreeCoordinate(); 
            if (!_snakeCoordinates.Contains(coordinate))
            {
                _obstacleCoordinates.Push(coordinate);
                _gameGrid.MakeCoordinateOccupied(_obstacleCoordinates.Peek());
                emptySpotFound = true;
            }
        }
    }
    /// <summary>
    /// Gets called with the pickup collected called "Remove Obstacle", removes the most recent obstacle
    /// </summary>
    public void RemoveObstacle()
    {
        if (_obstacleCoordinates.Count == 0)
        {
            return;
        }
        _gameGrid.MakeCoordinateFree(_obstacleCoordinates.Peek());
        _obstacleCoordinates.Pop();
    }


    void OnGUI()
    {
        //Display Player score and other info 
        if (_gameStarted)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, 5, 200, 20), score.ToString(), _mainStyle);
        }
        else
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 10, 200, 20), "Press Any Key to Play\n(Use Arrows to Change Direction)", _mainStyle);
        }
        if (_gameOver)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 20, 200, 40), "Game Over\n(Press 'Space' to Restart)", _mainStyle);
        }
    }
}
