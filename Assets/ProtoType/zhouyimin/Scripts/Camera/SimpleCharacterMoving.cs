using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacterMoving : MonoBehaviour
{
    public Vector3  MovingDirection = Vector3.up;
    public float MovingSpeed = 100f;

    void Update()
    {
     transform.position += MovingDirection * -MovingSpeed * Time.deltaTime;
    }
}
