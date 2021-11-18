using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple removes an obstacle from the GameManager stack when this is equiped
public class ObstacleRemover : MonoBehaviour, IPickUp
{
    public GameManager gameManager;
    public float Timer { get; set; } // Not used here
    public bool Active { get; set; } // Not used here

    public void Activate()
    {
        gameManager.RemoveObstacle();
    }

    public IEnumerator StartTimer() // Not used here
    {
        yield return new WaitForEndOfFrame();
    }

    public void RemoveMaterial() // Not used here
    {

    }
}
