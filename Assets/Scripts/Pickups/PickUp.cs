using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{

    public GameManager gameManager;

    public Material pickUpMaterial;

    public Material speedMaterial;
    public Material removeObstacleMaterial;
    public Material guideMaterial;
    public Material extraMaterial;


    //public GameObject speed;
    //public GameObject armour;
    //public GameObject guide;

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
        return pickups[randomIndex];
    }
}
