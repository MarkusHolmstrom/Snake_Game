using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple removes an obstacle from the GameManager stack when this is equiped
public class ObstacleRemover : MonoBehaviour
{
    public GameManager gameManager;

    public void Activate()
    {
        gameManager.RemoveObstacle();
    }
}
