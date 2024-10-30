using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocitySpin : MonoBehaviour
{
    public Rigidbody2D rb;
    public float rotationMultiplier = 8.5f;

    void Update()
    {
        // Calculate rotation 
        float rotationAngle = rb.velocity.magnitude - rotationMultiplier * 5;

        // Rotate around the Z-axis 
        transform.Rotate(0, 0, rotationAngle * Time.deltaTime);
    }
}
