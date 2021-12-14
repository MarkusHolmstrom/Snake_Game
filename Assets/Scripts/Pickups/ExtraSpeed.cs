using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Makes the snake go faster for a period of time when equiped
public class ExtraSpeed : MonoBehaviour, IPowerUp
{
    [SerializeField]
    private Snake _snake;
    [SerializeField]
    private GameManager _gameManager;
    public float Timer { get => _timer; set { _timer = value; } }
    [SerializeField]
    private float _timer = 15;
    public bool Active { get; set; }

    private float _startModifier;

    // Start is called before the first frame update
    void Awake()
    {
        Active = false;
        _startModifier = _gameManager.speedModifier;
    }

    public void Activate()
    {
        _gameManager.speedModifier = 1.5f;
        StartCoroutine(StartTimer());
    }

    public IEnumerator StartTimer()
    {
        Active = true;
        yield return new WaitForSeconds(Timer);
        _gameManager.speedModifier = _startModifier;
        Active = false;
        RemoveMaterial();
    }

    public void RemoveMaterial()
    {
        _snake.RemovePickUpMaterial(Snake.Special.ExtraSpeed);
    }
}
