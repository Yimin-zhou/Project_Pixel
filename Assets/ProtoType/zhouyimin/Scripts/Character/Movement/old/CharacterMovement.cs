using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement: MonoBehaviour {
 
    public float floorOffsetY;
    public float slopeLimit = 60f;
    public float slopeSpeedMultiplier = 1f;
    public float startPointOffsetY = 0f;
    public Vector4 raycastWidthOffset = Vector4.zero;
    public float raycastWidth = 0.25f;
    public float raycastLength = 3f;
    public float moveSpeed = 6f;
    public float jumpPower = 5f;
    public int maxJumps = 1;
    public float turnSmoothTime = 0.1f;
    public Transform characterCam;

    private Rigidbody _rb;
    private Animator _anim;
    private Vector3 _moveDirection = Vector3.zero;
    private float _vertical;
    private float _horizontal;
    private bool _jumpInput;
    private float _inputAmount;
    private Vector3 _floorMovement;
    private Vector3 _gravity;
    private int _remainingJumps = 1;
    private float _slopeEffector;
    private Vector3 _floorNormal;
    private float _turnSmoothVelocity;
    private Vector3 _combinedRaycast;
    
    void Start () 
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
    }
 
    private void Update()
    {
        _horizontal = Input.GetAxis("Horizontal");
        _vertical = Input.GetAxis("Vertical");
        _jumpInput = Input.GetButtonDown("Jump");
        
        _inputAmount = GetRawSpeed(_horizontal, _vertical);
        
        //jump
        if (_jumpInput)
        {
            DoJump();
        }
        
        PlayAnimation(_inputAmount);
    }

    private float GetRawSpeed(float horizontal, float vertical)
    {
        // calculate raw speed make sure the input doesnt go negative or above 1;
        return Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
    }

    private void PlayAnimation(float inputAmount)
    {
        // TODO use blend tree for animation
        if (inputAmount<0.01)
        {
            _anim.SetBool("isRunning",false);
        }
        else
        {
            _anim.SetBool("isRunning",true);
        }
    }
    
    private void DoJump()
    {
        if (_remainingJumps-1 > 0)
        {
            _gravity.y = jumpPower;
            _remainingJumps--;
        }
    }

    private void FixedUpdate () 
    {
       _moveDirection = new Vector3(_horizontal,0,_vertical).normalized;
       if (_moveDirection.magnitude > 0.1f)
       {
           _moveDirection = GetMoveDirection(_moveDirection);
       }
       // if not grounded or on slope , increase down force
        if(!IsGrounded() || _slopeEffector >= 0.1f)
        {
            _gravity += Vector3.up * Physics.gravity.y * Time.fixedDeltaTime;
        }
        
        // actual movement of the rigidbody + extra down force
        _rb.velocity = (_moveDirection * GetMoveSpeed() * _inputAmount) + 10 * _gravity;
 
        // find the Y position via raycasts
        _floorMovement =  new Vector3(_rb.position.x, FindFloor().y+floorOffsetY, _rb.position.z);
 
        Debug.Log("floor movement: " +_floorMovement + "rb position: " + _rb.position + "Is gounded: " + IsGrounded() + "rb velocity y: " + _rb.velocity.y);
        
        // only stick to floor when grounded
        if(IsGrounded() && _floorMovement != _rb.position  && _rb.velocity.y <= 0)
        {
            // move the rigidbody to the floor
            _rb.MovePosition(_floorMovement);
            // reset gravity
            _gravity.y = 0;
        }
    }
   
   private Vector3 GetMoveDirection(Vector3 moveDirection)
   {
       // base movement on camera
       float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + characterCam.eulerAngles.y;
       float finalAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
       _rb.MoveRotation(Quaternion.Euler(0f, finalAngle, 0f)); 
       Vector3 finalDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward.normalized;
       return finalDirection;
   }

   private float GetMoveSpeed()
   {
       //move slower on slope
       return Mathf.Clamp(moveSpeed + (_slopeEffector * slopeSpeedMultiplier), 0, moveSpeed + 1);
   }

   private Vector3 FindFloor()
    {
        // check floor on 5 raycasts   , get the average when not Vector3.zero  
        int floorAverage = 1;
 
        _combinedRaycast = FloorRaycasts(0, 0, raycastLength);
        floorAverage += (GetFloorAverage(raycastWidth+raycastWidthOffset.x, 0) 
                         + GetFloorAverage(-raycastWidth+raycastWidthOffset.y, 0) 
                         + GetFloorAverage(0, raycastWidth+raycastWidthOffset.z) 
                         + GetFloorAverage(0, -raycastWidth*raycastWidthOffset.w));
        
        return _combinedRaycast / floorAverage;
    }
 
    // only add to average floor position if its not Vector3.zero
    private int GetFloorAverage(float offsetx, float offsetz)
    {
        if (FloorRaycasts(offsetx, offsetz, raycastLength) != Vector3.zero)
        {
            _combinedRaycast += FloorRaycasts(offsetx, offsetz, raycastLength);
            return 1;
        }
        return 0;
    }
    
    private Vector3 FloorRaycasts(float offsetx, float offsetz, float raycastLength)
    {
        RaycastHit hit;
        Vector3 raycastOriginPos = transform.TransformPoint(offsetx, startPointOffsetY, offsetz);
        
        UnityEngine.Debug.DrawRay(raycastOriginPos, Vector3.down * raycastLength, Color.magenta);
        if (Physics.Raycast(raycastOriginPos, Vector3.down, out hit, raycastLength))
        {
            _floorNormal = hit.normal;
            //limit the moving angle
            if (Vector3.Angle(_floorNormal,Vector3.up) < slopeLimit)
            {
                return hit.point;
            }
            return Vector3.zero;
        }
        return Vector3.zero;
    }
    
    private bool IsGrounded()
    {
        if (FloorRaycasts(0,0, raycastLength) != Vector3.zero)
        {
            //lower speed on slope
            _slopeEffector = Vector3.Dot(transform.forward, _floorNormal);
            _remainingJumps = maxJumps;
            return true;
        }
        return false;
    }
}