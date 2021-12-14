using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple guide to help the player when facing possible defeat during a specific time period

public class Guide : MonoBehaviour, IPowerUp
{
    [SerializeField]
    private Snake _snake;
    [SerializeField]
    private GameManager _gameManager;
    public float Timer { get => _timer; set { _timer = value; } }
    [SerializeField]
    private float _timer = 15;
    public bool Active { get; set; }

    // Start is called before the first frame update
    void Awake()
    {
        Active = false;
    }

    public void Activate()
    {
        _gameManager.guideIsActive = true;
        StartCoroutine(StartTimer());
    }

    public IEnumerator StartTimer()
    {
        Active = true;
        yield return new WaitForSeconds(Timer);
        _gameManager.guideIsActive = false;
        Active = false;
        RemoveMaterial();
    }

    public void RemoveMaterial()
    {
        _snake.RemovePickUpMaterial(Snake.Special.Guide);
    }
}
