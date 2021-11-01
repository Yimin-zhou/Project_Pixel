using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour
{
    public float movingSpeed = 15f;
    public float turnSmoothTime = 0.1f;
    public Transform characterCam;
    private float _turnSmoothVelocity;
    private CharacterController _characterController;
    
    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 movingDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;

        Move(movingDirection);
    }

    private void Move(Vector3 movingDirection)
    {
        if (movingDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(movingDirection.x, movingDirection.z) * Mathf.Rad2Deg + characterCam.eulerAngles.y;
            float finalAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity,turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, finalAngle, 0f);
            Vector3 finalDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward.normalized;
            _characterController.Move(finalDirection * movingSpeed * Time.deltaTime);
        }
    }

}
