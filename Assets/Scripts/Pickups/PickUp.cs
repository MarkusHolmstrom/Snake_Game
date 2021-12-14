using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPowerUp
{
    float Timer { get; set; }
    bool Active { get; set; }
    void Activate();
    IEnumerator StartTimer();
    void RemoveMaterial();
}

// Manages the pickup powers available in the game, whenever the player reaches a point with a question mark
// a random power gets picked up and stores in the snakes body, in its linked list. Only one power is available
// per bodypart (expc the head, that cant have any). Its not a beautiful design, but it fits with the linked list.

public class PickUp : MonoBehaviour
{
    // Private variables, set in editor:
    [SerializeField]
    private GameManager _gameManager;
    [SerializeField]
    private ExtraPoints _extraPoints;
    [SerializeField]
    private ExtraSpeed _extraSpeed;
    [SerializeField]
    private Guide _guide;
    [SerializeField]
    private ObstacleRemover _obstacleRemover;

    // Public variables:
    public Material pickUpMaterial;

    public Material speedMaterial;
    public Material removeObstacleMaterial;
    public Material guideMaterial;
    public Material extraMaterial;

    public GameObject[] pickups = new GameObject[4];

    public GameObject GetNewPickup()
    {
        int randomIndex = Random.Range(0, pickups.Length);
        if (randomIndex == 0)
        {
            _extraPoints.Activate();
        }
        else if(randomIndex == 1)
        {
            _extraSpeed.Activate();
        }
        else if (randomIndex == 2)
        {
            _guide.Activate();
        }
        else if (randomIndex == 3)
        {
            _obstacleRemover.Activate();
        }
        return pickups[randomIndex];
    }
}
