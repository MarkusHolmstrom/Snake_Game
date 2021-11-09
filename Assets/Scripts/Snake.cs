using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Body
{
    public Material material { get; set; }
    // pick ups; speed, armour, bomb?, guide (sväng auto nära fara)
}

public class Snake : MonoBehaviour
{
    // https://www.c-sharpcorner.com/article/linked-list-implementation-in-c-sharp/ mer hjälp
    private LinkedList<Body> _snake = new LinkedList<Body>();

    Body head = new Body();

    // Start is called before the first frame update
    void Start()
    {
        // LinkedListNode<Body> tail = _snake.FindLast(head);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Snake CreateSnake(List<int> snakeCoordinates, int length, int areaResolution)
    {
        snakeCoordinates.Clear();
        int firstlock = Random.Range(0, areaResolution - 1) + (areaResolution * length);

        for (int i = 0; i < length; i++)
        {
            if (i == 0)
            {
                _snake.AddFirst(head);
            }
            else
            {
                Body body = new Body();
                _snake.AddLast(body);
            }
            
            snakeCoordinates.Add(firstlock - (areaResolution * i));
        }
        return this;
    }

    public void SetMaterial(Body body, Material material)
    {
        body.material = material;
    }

    public void AddToSnake()
    {

    }
}
