using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Makes the snake go faster for a period of time when equiped
public class ExtraSpeed : MonoBehaviour, IPickUp
{
    public Snake snake;
    public GameManager gameManager;
    public float Timer { get; set; }
    public float timer = 15;
    public bool Active { get; set; }

    private float startModifier;

    // Start is called before the first frame update
    void Awake()
    {
        Timer = timer;
        Active = false;
        startModifier = gameManager.speedModifier;
    }

    public void Activate()
    {
        gameManager.speedModifier = 1.5f;
        StartCoroutine(StartTimer());
    }

    public IEnumerator StartTimer()
    {
        Active = true;
        yield return new WaitForSeconds(Timer);
        gameManager.speedModifier = startModifier;
        Active = false;
        RemoveMaterial();
    }

    public void RemoveMaterial()
    {
        snake.RemovePickUpMaterial(Snake.Special.ExtraSpeed);
    }
}
