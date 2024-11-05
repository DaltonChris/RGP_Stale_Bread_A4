using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocitySpin : MonoBehaviour
{
    Rigidbody2D rb;
    public float Min_rotationMultiplier = 3.5f;
    public float Max_rotationMultiplier = 8.5f;
    float rotationMultiplier;
    public bool isNegativeRotate;
    public bool isRandomRotate = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (isRandomRotate == true)//coin flip a rotate direction
        {
            int randomRotate = Random.Range(0, 1);
            if (randomRotate == 0) isNegativeRotate = true;
            else if (randomRotate == 1) isNegativeRotate = false;
        }
        // get random rotation mutilpler within the min / max    
        rotationMultiplier = Random.Range(Min_rotationMultiplier, Max_rotationMultiplier);
    }
    void Update()
    {
        float rotationAngle = rb.velocity.magnitude;
        if (isNegativeRotate)
        {
            // Calculate neg rotation 
            rotationAngle = rb.velocity.magnitude - rotationMultiplier * 5;
        }
        else
        {
            // Calculate positive rotation 
            rotationAngle = rb.velocity.magnitude + rotationMultiplier * 5;
        }

        // Rotate around the Z-axis 
        transform.Rotate(0, 0, rotationAngle * Time.deltaTime);
    }
}
