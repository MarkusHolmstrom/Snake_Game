using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles the most of the gameplay, based on this help from world wide web:
// https://sharpcoderblog.com/blog/snake-game-in-unity-3d
// It has been modified so it suits the assignements requirements

public class GameManager : MonoBehaviour
{
    //Game area resolution, the higher number means more blocks
    public int areaResolution = 22;

    public float snakeSpeed = 6f;
    public float speedModifier = 1.0f;

    public int startSnakeLength = 3;

    [Range(0, 40)]
    public int nrOfObstacles = 8;
    public bool obstacleGeneration = true;
    [Range(5, 20)]
    public int obstacleGenerationPace = 10;

    public Camera mainCamera;
    // Materials
    public Material groundMaterial;
    public Material snakeMaterial;
    public Material headMaterial;
    public Material fruitMaterial;
    public Material obstacleMaterial;

    public bool fruitPicked = false;

    public PickUp pickUp;
    [Range(1, 10)]
    public int pickUpRatio = 2;
    private bool _pickUpsActive = true;

    public int score = 0;
    // Gets activated from the Guide class
    public bool guideIsActive = false;

    // Grid system
    Renderer[] gameBlocks;
    List<int> snakeCoordinates = new List<int>();
    enum Direction { Up, Down, Left, Right };
    Direction snakeDirection = Direction.Right;
    float timeTmp = 0;
    // Index where the fruit is placed
    int fruitBlockIndex = -1;

    bool gameStarted = false;
    bool gameOver = false;
    // Camera scaling
    Bounds targetBounds;
    // Text styling
    GUIStyle mainStyle = new GUIStyle();

    private Stack<int> obstacleCoordinates = new Stack<int>();

    private Snake snake;

    private int pickUpIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        snake = GetComponent<Snake>();
        // Generate play area
        gameBlocks = new Renderer[areaResolution * areaResolution];
        for (int x = 0; x < areaResolution; x++)
        {
            for (int y = 0; y < areaResolution; y++)
            {
                GameObject quadPrimitive = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quadPrimitive.transform.position = new Vector3(x, 0, y);
                Destroy(quadPrimitive.GetComponent<Collider>());
                quadPrimitive.transform.localEulerAngles = new Vector3(90, 0, 0);
                quadPrimitive.transform.SetParent(transform);
                gameBlocks[(x * areaResolution) + y] = quadPrimitive.GetComponent<Renderer>();
                targetBounds.Encapsulate(gameBlocks[(x * areaResolution) + y].bounds);
            }
        }

        // Scale the MainCamera to fit the game blocks
        mainCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        mainCamera.orthographic = true;
        float screenRatio = Screen.width / Screen.height;
        float targetRatio = targetBounds.size.x / targetBounds.size.y;

        if (screenRatio >= targetRatio)
        {
            mainCamera.orthographicSize = targetBounds.size.y / 2;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            mainCamera.orthographicSize = targetBounds.size.y / 2 * differenceInSize;
        }
        mainCamera.transform.position = new Vector3(targetBounds.center.x, targetBounds.center.y + 1, targetBounds.center.z);

        // Create the Snake 
        InitializeSnake(startSnakeLength);

        SetMap();

        mainStyle.fontSize = 24;
        mainStyle.alignment = TextAnchor.MiddleCenter;
        mainStyle.normal.textColor = Color.white;
    }


    // Update is called once per frame
    void Update()
    {
        if (!gameStarted)
        {
            if (Input.anyKeyDown)
            {
                gameStarted = true;
            }
            return;
        }
        if (gameOver)
        {
            // Flicker the snake blocks
            if (timeTmp < 0.44f)
            {
                timeTmp += Time.deltaTime;
            }
            else
            {
                timeTmp = 0;
                for (int i = 0; i < snakeCoordinates.Count; i++)
                {
                    if (gameBlocks[snakeCoordinates[i]].sharedMaterial == groundMaterial)
                    {
                        gameBlocks[snakeCoordinates[i]].sharedMaterial = (i == 0 ? headMaterial : snakeMaterial);
                    }
                    else
                    {
                        gameBlocks[snakeCoordinates[i]].sharedMaterial = groundMaterial;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                InitializeSnake(startSnakeLength);
                gameOver = false;
                gameStarted = false;
            }
        }
        else
        {
            if (timeTmp < 1)
            {
                timeTmp += Time.deltaTime * snakeSpeed * speedModifier;
            }
            else
            {
                timeTmp = 0;
                if (snakeDirection == Direction.Right || snakeDirection == Direction.Left)
                {
                    // Detect if the Snake hit the sides
                    if (snakeDirection == Direction.Left && snake.GetHeadCoordinate() < areaResolution)
                    {
                        if (guideIsActive)
                        {
                            snakeDirection = GuideChangesDirection(snakeDirection);
                        }
                        else
                        {
                            gameOver = true;
                        }
                        return;
                    }
                    else if (snakeDirection == Direction.Right && snake.GetHeadCoordinate() >= (gameBlocks.Length - areaResolution))
                    {
                        if (guideIsActive)
                        {
                            snakeDirection = GuideChangesDirection(snakeDirection);
                        }
                        else
                        {
                            gameOver = true;
                        }
                        return;
                    }

                    int newCoordinate = snake.GetHeadCoordinate() + (snakeDirection == Direction.Left ? -areaResolution : areaResolution);
                    // Snake has ran into itself or hits obstacle: game over
                    if (snakeCoordinates.Contains(newCoordinate) || obstacleCoordinates.Contains(newCoordinate))
                    {
                        if (guideIsActive)
                        {
                            snakeDirection = GuideChangesDirection(snakeDirection);
                        }
                        else
                        {
                            gameOver = true;
                        }
                        return;
                    }
                    if (newCoordinate < gameBlocks.Length)
                    {
                        // Move snake to new position
                        snakeCoordinates = snake.MoveSnake(newCoordinate);
                        gameBlocks[snakeCoordinates[0]].transform.localEulerAngles = new Vector3(90, (snakeDirection == Direction.Left ? -90 : 90), 0);
                    }
                }
                else if (snakeDirection == Direction.Up || snakeDirection == Direction.Down)
                {
                    // Detect if snake hits the top or bottom
                    if (snakeDirection == Direction.Up && (snakeCoordinates[0] + 1) % areaResolution == 0)
                    {
                        if (guideIsActive)
                        {
                            snakeDirection = GuideChangesDirection(snakeDirection);
                        }
                        else
                        {
                            gameOver = true;
                        }
                        return;
                    }
                    else if (snakeDirection == Direction.Down && (snakeCoordinates[0] + 1) % areaResolution == 1)
                    {
                        if (guideIsActive)
                        {
                            snakeDirection = GuideChangesDirection(snakeDirection);
                        }
                        else
                        {
                            gameOver = true;
                        }
                        return;
                    }

                    int newCoordinate = snakeCoordinates[0] + (snakeDirection == Direction.Down ? -1 : 1);
                    // Snake has ran into itself or hits obstacle: game over
                    if (snakeCoordinates.Contains(newCoordinate) || obstacleCoordinates.Contains(newCoordinate))
                    {
                        if (guideIsActive)
                        {
                            snakeDirection = GuideChangesDirection(snakeDirection);
                        }
                        else
                        {
                            gameOver = true;
                        }
                        return;
                    }
                    if (newCoordinate < gameBlocks.Length)
                    {
                        // Move snake to new position
                        snakeCoordinates = snake.MoveSnake(newCoordinate);
                        gameBlocks[snakeCoordinates[0]].transform.localEulerAngles = new Vector3(90, (snakeDirection == Direction.Down ? 180 : 0), 0);
                    }
                }

                ApplyMaterials();
            }
            // Movement
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                int newCoordinate = snakeCoordinates[0] + areaResolution;
                if (!ContainsCoordinate(newCoordinate))
                {
                    snakeDirection = Direction.Right;
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                int newCoordinate = snakeCoordinates[0] - areaResolution;
                if (!ContainsCoordinate(newCoordinate))
                {
                    snakeDirection = Direction.Left;
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                int newCoordinate = snakeCoordinates[0] + 1;
                if (!ContainsCoordinate(newCoordinate))
                {
                    snakeDirection = Direction.Up;
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                int newCoordinate = snakeCoordinates[0] - 1;
                if (!ContainsCoordinate(newCoordinate))
                {
                    snakeDirection = Direction.Down;
                }
            }
        }

        if (fruitBlockIndex < 0)
        {
            fruitBlockIndex = GetNewIndex();
        }
        if (!_pickUpsActive && pickUpIndex < 0 && score % pickUpRatio == 0 && score != 0)
        {
            pickUpIndex = GetNewIndex();
        }
    }

    int GetNewIndex()
    {
        int indexTmp = Random.Range(0, gameBlocks.Length - 1);

        // Check if the block is not occupied with a snake or obstacle block
        for (int i = 0; i < snake.GetSnakeLength(); i++)
        {
            if (snake.GetCoordinates()[i] == indexTmp)
            {
                return -1;
            }
        }

        foreach (int item in obstacleCoordinates)
        {
            if (item == indexTmp)
            {
                return -1;
            }
        }

        return indexTmp;
    }

    void InitializeSnake(int length)
    {
        snakeCoordinates.Clear();
        snake.ClearSnake();
        snake = snake.CreateSnake(length, areaResolution);
        snakeCoordinates = snake.GetCoordinates();

        obstacleCoordinates = GenerateObstacles();

        gameBlocks[snakeCoordinates[0]].transform.localEulerAngles = new Vector3(90, 90, 0);
        fruitBlockIndex = -1;
        timeTmp = 1;
        snakeDirection = Direction.Right;
        score = 0;
        ApplyMaterials();
    }

    Stack<int> GenerateObstacles()
    {
        Stack<int> indexes = new Stack<int>();
        for (int i = 0; i < nrOfObstacles; i++)
        {
            indexes.Push(Random.Range(0, gameBlocks.Length));
        }
        return indexes;
    }

    void SetMap()
    {
        for (int i = 0; i < gameBlocks.Length; i++)
        {
            foreach (int coord in obstacleCoordinates)
            {
                if (i == coord)
                {
                    gameBlocks[i].sharedMaterial = obstacleMaterial;
                    gameBlocks[i].transform.localEulerAngles = new Vector3(90, 0, 0);
                    break;
                }
                else
                {
                    gameBlocks[i].sharedMaterial = groundMaterial;
                }
            }
        }
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

    void ApplyMaterials()
    {
        SetMap();
        // Apply Snake material
        for (int i = 0; i < gameBlocks.Length; i++)
        {
            fruitPicked = false;
            for (int a = 0; a < snake.GetSnakeLength(); a++)
            {
                if (snakeCoordinates[a] == i)
                {
                    gameBlocks[i].sharedMaterial = snake.GetMaterialFromLinkedList(snakeCoordinates[a]); 
                }
                if (snakeCoordinates[a] == fruitBlockIndex)
                {
                    // Pick a fruit
                    fruitPicked = true;
                }
                if (snakeCoordinates[a] == pickUpIndex)
                {
                    // Pick a pick up
                    snake.GetRandomPickUp();
                    pickUpIndex = -1;
                    _pickUpsActive = true;
                    score++;
                }
            }
            if (fruitPicked)
            {
                fruitBlockIndex = -1;
                // Add new block
                int snakeBlockRotationY = (int)gameBlocks[snakeCoordinates[snakeCoordinates.Count - 1]].transform.localEulerAngles.y;

                if (snakeBlockRotationY == 270)
                {
                    int coordinate = snakeCoordinates[snakeCoordinates.Count - 1] + areaResolution;
                    snakeCoordinates.Add(coordinate);
                    snake.AddToSnake(coordinate);
                }
                else if (snakeBlockRotationY == 90)
                {
                    int coordinate = snakeCoordinates[snakeCoordinates.Count - 1] - areaResolution;
                    snakeCoordinates.Add(coordinate);
                    snake.AddToSnake(coordinate);
                }
                else if (snakeBlockRotationY == 0)
                {
                    int coordinate = snakeCoordinates[snakeCoordinates.Count - 1] + 1;
                    snakeCoordinates.Add(coordinate);
                    snake.AddToSnake(coordinate);
                }
                else if (snakeBlockRotationY == 180)
                {
                    int coordinate = snakeCoordinates[snakeCoordinates.Count - 1] - 1;
                    snakeCoordinates.Add(coordinate);
                    snake.AddToSnake(coordinate);
                }
                score++;
            }

            if (score % pickUpRatio == 0 && score != 0 && _pickUpsActive)
            {
                _pickUpsActive = false;
            }

            if (i == fruitBlockIndex)
            {
                gameBlocks[i].sharedMaterial = fruitMaterial;
                gameBlocks[i].transform.localEulerAngles = new Vector3(90, 0, 0);
            }

            if (i == pickUpIndex)
            {
                gameBlocks[i].sharedMaterial = pickUp.pickUpMaterial;
                gameBlocks[i].transform.localEulerAngles = new Vector3(90, 0, 0);
            }

            if (obstacleGeneration && score % obstacleGenerationPace == 0 && score != 0)
            {
                AddNewObstacle();
                score++;
            }
        }
    }

    bool ContainsCoordinate(int coordinate)
    {
        for (int i = 0; i < snakeCoordinates.Count; i++)
        {
            if (snakeCoordinates[i] == coordinate)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Makes the game more difficult, adds obstacles at a pace
    /// </summary>
    private void AddNewObstacle()
    {
        bool emptySpotFound = false;
        while (!emptySpotFound)
        {
            int coordinate = Random.Range(0, gameBlocks.Length);
            if (!obstacleCoordinates.Contains(coordinate) && !snakeCoordinates.Contains(coordinate))
            {
                obstacleCoordinates.Push(coordinate);
                emptySpotFound = true;
            }
        }
    }
    /// <summary>
    /// Gets called with the pickup collected called "Remove Obstacle", removes the most recent obstacle
    /// </summary>
    public void RemoveObstacle()
    {
        if (obstacleCoordinates.Count == 0)
        {
            return;
        }
        obstacleCoordinates.Pop();
    }


    void OnGUI()
    {
        //Display Player score and other info 
        if (gameStarted)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, 5, 200, 20), score.ToString(), mainStyle);
        }
        else
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 10, 200, 20), "Press Any Key to Play\n(Use Arrows to Change Direction)", mainStyle);
        }
        if (gameOver)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 20, 200, 40), "Game Over\n(Press 'Space' to Restart)", mainStyle);
        }
    }
}
