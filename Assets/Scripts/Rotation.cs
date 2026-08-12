using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Rotation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public Transform target;//it will hold the position of the object;
    public float speed; // it will set the speed that it will orbit;
    // Update is called once per frame
    void Update()
    {// First Parameter: Target position
// Second Parameter: Axis of Rotation
//Third Parameter: Speed of Rotation
    transform.RotateAround(target.transform.position, target.transform.up, speed * Time.deltaTime);        
    }
}
