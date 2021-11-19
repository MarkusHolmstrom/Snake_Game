using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple guide to help the player when facing possible defeat during a specific time period

public class Guide : MonoBehaviour, IPickUp
{
    public Snake snake;
    public GameManager gameManager;
    public float Timer { get; set; }
    public float timer = 15;
    public bool Active { get; set; }

    // Start is called before the first frame update
    void Awake()
    {
        Timer = timer;
        Active = false;
    }

    public void Activate()
    {
        gameManager.guideIsActive = true;
        StartCoroutine(StartTimer());
    }


    public IEnumerator StartTimer()
    {
        Active = true;
        yield return new WaitForSeconds(Timer);
        gameManager.guideIsActive = false;
        Active = false;
        RemoveMaterial();
    }

    public void RemoveMaterial()
    {
        snake.RemovePickUpMaterial(Snake.Special.Guide);
    }
}
