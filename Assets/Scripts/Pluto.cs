using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Pluto : MonoBehaviour 
{
    public Transform target; // It will hold the position of the object (e.g., the Sun)
    public float speed;      // It will set the speed that it will orbit
    
    [Header("Pluto Settings")]
    [Tooltip("Pluto's actual orbital inclination is roughly 17 degrees.")]
    public float orbitalInclination = 17f; 

    private Vector3 customOrbitAxis;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    {
        // 1. Start with the default upright axis
        Vector3 defaultAxis = target.transform.up;

        // 2. Create a rotation matrix tilting forward by your inclination degrees
        Quaternion tiltRotation = Quaternion.Euler(orbitalInclination, 0, 0);

        // 3. Multiply the tilt by the default axis to get a permanent diagonal axis
        customOrbitAxis = tiltRotation * defaultAxis;
    }

    // Update is called once per frame
    void Update() 
    {
        // First Parameter: Target position 
        // Second Parameter: Custom diagonal axis of rotation 
        // Third Parameter: Speed of Rotation 
        transform.RotateAround(target.transform.position, customOrbitAxis, speed * Time.deltaTime);
    }
}