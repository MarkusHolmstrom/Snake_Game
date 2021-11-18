using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPickUp
{
    float Timer { get; set; }
    bool Active { get; set; }
    void Activate();
    IEnumerator StartTimer();
    void RemoveMaterial();
}

// Manages the pickup powers available in the game, whenever the player reaches a point with a question mark
// a random power gets picked up and stores in the snakes body, in its linked list. Only one power is available
// per bodypart (expc the head). 

public class PickUp : MonoBehaviour
{
    public GameManager gameManager;
    public ExtraPoints extraPoints;
    public ExtraSpeed extraSpeed;
    public Guide guide;
    public ObstacleRemover obstacleRemover;

    public Material pickUpMaterial;

    public Material speedMaterial;
    public Material removeObstacleMaterial;
    public Material guideMaterial;
    public Material extraMaterial;

    public GameObject[] pickups = new GameObject[4];


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GetNewPickup()
    {
        int randomIndex = Random.Range(0, pickups.Length);
        if (randomIndex == 0)
        {
            extraPoints.Activate();
        }
        else if(randomIndex == 1)
        {
            extraSpeed.Activate();
        }
        else if (randomIndex == 2)
        {
            guide.Activate();
        }
        else if (randomIndex == 3)
        {
            obstacleRemover.Activate();
        }
        return pickups[randomIndex];
    }
}
