using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Adds number of points gained during a period of time, builds up over time
public class ExtraPoints : MonoBehaviour, IPickUp
{
    public Snake snake;
    public GameManager gameManager;
    public float Timer { get; set; }
    public float timer = 15;
    public bool Active { get; set; }

    public int extraScore = 1;

    // Start is called before the first frame update
    void Awake()
    {
        Timer = timer;
        Active = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.fruitPicked && Active)
        {
            gameManager.score += extraScore;
        }
    }

    public void Activate()
    {
        extraScore++;
        StartCoroutine(StartTimer());
    }

    public IEnumerator StartTimer()
    {
        Active = true;
        yield return new WaitForSeconds(Timer);
        Active = false;
        RemoveMaterial();
    }

    public void RemoveMaterial()
    {
        snake.RemovePickUpMaterial(Snake.Special.ExtraPoints);
    }
}
