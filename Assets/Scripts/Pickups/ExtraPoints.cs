using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Adds number of points gained during a period of time, builds up over time
public class ExtraPoints : MonoBehaviour, IPowerUp
{
    [SerializeField]
    private Snake _snake;
    [SerializeField]
    private GameManager _gameManager;
    public float Timer { get => _timer; set { _timer = value; } }
    [SerializeField]
    private float _timer = 15;
    public bool Active { get; set; }

    [SerializeField]
    private int _extraScore = 1;

    // Start is called before the first frame update
    void Awake()
    {
        Active = false;
    }

    public void Activate()
    {
        _extraScore++;
        _gameManager.extraScore = _extraScore;
        StartCoroutine(StartTimer());
    }

    public IEnumerator StartTimer()
    {
        Active = true;
        yield return new WaitForSeconds(Timer);
        _gameManager.extraScore = 1;
        Active = false;
        RemoveMaterial();
    }

    public void RemoveMaterial()
    {
        _snake.RemovePickUpMaterial(Snake.Special.ExtraPoints);
    }

    public void ResetExtraScore() 
    {
        _extraScore = 1;
    }
}
