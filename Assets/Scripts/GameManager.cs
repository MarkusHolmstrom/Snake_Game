using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// https://sharpcoderblog.com/blog/snake-game-in-unity-3d
public class GameManager : MonoBehaviour
{
    //Game area resolution, the higher number means more blocks
    public int areaResolution = 22;
    //Snake movement speed
    public float snakeSpeed = 6f;

    public int startSnakeLength = 3;

    [Range(0, 40)]
    public int nrOfObstacles = 8;
    //Main Camera
    public Camera mainCamera;
    //Materials
    public Material groundMaterial;
    public Material snakeMaterial;
    public Material headMaterial;
    public Material fruitMaterial;
    public Material obstacleMaterial;

    //Grid system
    Renderer[] gameBlocks;
    //Snake coordenates
    List<int> snakeCoordinates = new List<int>();
    enum Direction { Up, Down, Left, Right };
    Direction snakeDirection = Direction.Right;
    float timeTmp = 0;
    //Block where the fruit is placed
    int fruitBlockIndex = -1;
    //Total accumulated points
    int totalPoints = 0;
    //Game status
    bool gameStarted = false;
    bool gameOver = false;
    //Camera scaling
    Bounds targetBounds;
    //Text styling
    GUIStyle mainStyle = new GUIStyle();

    private List<int> obstacleIndex = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        //Generate play area
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

        //Scale the MainCamera to fit the game blocks
        mainCamera.transform.eulerAngles = new Vector3(90, 0, 0);
        mainCamera.orthographic = true;
        float screenRatio = (float)Screen.width / (float)Screen.height;
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

        //Generate the Snake with 3 blocks
        InitializeSnake(startSnakeLength);
        //ApplyMaterials();
        obstacleIndex = GenerateObstacles();
        SetMap();

        mainStyle.fontSize = 24;
        mainStyle.alignment = TextAnchor.MiddleCenter;
        mainStyle.normal.textColor = Color.white;
    }

    void InitializeSnake(int length)
    {
        snakeCoordinates.Clear();
        int firstlock = Random.Range(0, areaResolution - 1) + (areaResolution * length);

        for (int i = 0; i < length; i++)
        {
            snakeCoordinates.Add(firstlock - (areaResolution * i));
        }
        //snakeCoordinates.Add(firstlock);
        //snakeCoordinates.Add(firstlock - areaResolution);
        //snakeCoordinates.Add(firstlock - (areaResolution * 2));

        gameBlocks[snakeCoordinates[0]].transform.localEulerAngles = new Vector3(90, 90, 0);
        fruitBlockIndex = -1;
        timeTmp = 1;
        snakeDirection = Direction.Right;
        totalPoints = 0;
        ApplyMaterials();
        
    }

    List<int> GenerateObstacles() // spara som ccords oxå
    {
        List<int> indexes = new List<int>();
        for (int i = 0; i < nrOfObstacles; i++)
        {
            indexes.Add(Random.Range(0, gameBlocks.Length));
        }
        return indexes;
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
            //Flicker the snake blocks
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
                // ApplyMaterials();
                gameOver = false;
                gameStarted = false;
            }
        }
        else
        {
            if (timeTmp < 1)
            {
                timeTmp += Time.deltaTime * snakeSpeed;
            }
            else
            {
                timeTmp = 0;
                if (snakeDirection == Direction.Right || snakeDirection == Direction.Left)
                {
                    //Detect if the Snake hit the sides
                    if (snakeDirection == Direction.Left && snakeCoordinates[0] < areaResolution)
                    {
                        gameOver = true;
                        return;
                    }
                    else if (snakeDirection == Direction.Right && snakeCoordinates[0] >= (gameBlocks.Length - areaResolution))
                    {
                        gameOver = true;
                        return;
                    }

                    int newCoordinate = snakeCoordinates[0] + (snakeDirection == Direction.Left ? -areaResolution : areaResolution);
                    //Snake has ran into itself, game over
                    if (snakeCoordinates.Contains(newCoordinate))
                    {
                        gameOver = true;
                        return;
                    }
                    if (newCoordinate < gameBlocks.Length)
                    {
                        for (int i = snakeCoordinates.Count - 1; i > 0; i--)
                        {
                            snakeCoordinates[i] = snakeCoordinates[i - 1];
                        }
                        snakeCoordinates[0] = newCoordinate;
                        gameBlocks[snakeCoordinates[0]].transform.localEulerAngles = new Vector3(90, (snakeDirection == Direction.Left ? -90 : 90), 0);
                    }
                }
                else if (snakeDirection == Direction.Up || snakeDirection == Direction.Down)
                {
                    //Detect if snake hits the top or bottom
                    if (snakeDirection == Direction.Up && (snakeCoordinates[0] + 1) % areaResolution == 0)
                    {
                        gameOver = true;
                        return;
                    }
                    else if (snakeDirection == Direction.Down && (snakeCoordinates[0] + 1) % areaResolution == 1)
                    {
                        gameOver = true;
                        return;
                    }

                    int newCoordinate = snakeCoordinates[0] + (snakeDirection == Direction.Down ? -1 : 1);
                    //Snake has ran into itself, game over
                    if (snakeCoordinates.Contains(newCoordinate))
                    {
                        gameOver = true;
                        return;
                    }
                    if (newCoordinate < gameBlocks.Length)
                    {
                        for (int i = snakeCoordinates.Count - 1; i > 0; i--)
                        {
                            snakeCoordinates[i] = snakeCoordinates[i - 1];
                        }
                        snakeCoordinates[0] = newCoordinate;
                        gameBlocks[snakeCoordinates[0]].transform.localEulerAngles = new Vector3(90, (snakeDirection == Direction.Down ? 180 : 0), 0);
                    }
                }

                ApplyMaterials();
            }

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
            //Place a fruit block
            int indexTmp = Random.Range(0, gameBlocks.Length - 1);

            //Check if the block is not occupied with a snake block
            for (int i = 0; i < snakeCoordinates.Count; i++)
            {
                if (snakeCoordinates[i] == indexTmp)
                {
                    indexTmp = -1;
                    break;
                }
            }

            fruitBlockIndex = indexTmp;
        }
    }

    void SetMap()
    {
        for (int i = 0; i < gameBlocks.Length; i++)
        {
            foreach (int item in obstacleIndex)
            {
                if (i == item)
                {
                    gameBlocks[i].sharedMaterial = obstacleMaterial;
                    break;
                }
                else
                {
                    gameBlocks[i].sharedMaterial = groundMaterial;
                }
            }
            
        }
    }

    void ApplyMaterials()
    {
        SetMap();
        //Apply Snake material
        for (int i = 0; i < gameBlocks.Length; i++)
        {
            //gameBlocks[i].sharedMaterial = groundMaterial;
            bool fruitPicked = false;
            for (int a = 0; a < snakeCoordinates.Count; a++)
            {
                if (snakeCoordinates[a] == i)
                {
                    gameBlocks[i].sharedMaterial = (a == 0 ? headMaterial : snakeMaterial);
                }
                if (snakeCoordinates[a] == fruitBlockIndex)
                {
                    //Pick a fruit
                    fruitPicked = true;
                }
            }
            if (fruitPicked)
            {
                fruitBlockIndex = -1;
                //Add new block
                int snakeBlockRotationY = (int)gameBlocks[snakeCoordinates[snakeCoordinates.Count - 1]].transform.localEulerAngles.y;
                
                if (snakeBlockRotationY == 270)
                {
                    snakeCoordinates.Add(snakeCoordinates[snakeCoordinates.Count - 1] + areaResolution);
                }
                else if (snakeBlockRotationY == 90)
                {
                    snakeCoordinates.Add(snakeCoordinates[snakeCoordinates.Count - 1] - areaResolution);
                }
                else if (snakeBlockRotationY == 0)
                {
                    snakeCoordinates.Add(snakeCoordinates[snakeCoordinates.Count - 1] + 1);
                }
                else if (snakeBlockRotationY == 180)
                {
                    snakeCoordinates.Add(snakeCoordinates[snakeCoordinates.Count - 1] - 1);
                }
                totalPoints++;
            }
            if (i == fruitBlockIndex)
            {
                gameBlocks[i].sharedMaterial = fruitMaterial;
                gameBlocks[i].transform.localEulerAngles = new Vector3(90, 0, 0);
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

    void OnGUI()
    {
        //Display Player score and other info 
        if (gameStarted)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, 5, 200, 20), totalPoints.ToString(), mainStyle);
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
