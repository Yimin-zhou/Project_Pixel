using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacterController: MonoBehaviour {
    
    public float rayStartPointOffsetY = 0f;
    public float raycastLength = 3f;
    public float rayCastWidth = 2.5f;
    public float slopeLimit = 60f;
    public float slopeSpeedMultiplier = 1f;
    public float moveSpeed = 6f;
    public float jumpGravityMultiplier = 6f;
    public float fallGravityMultiplier = 10f;
    public float jumpPower = 5f;
    public int maxJumps = 1;
    public float turnSmoothTime = 0.1f;
    public Transform characterCam;

    private Rigidbody _rb;
    private Animator _anim;
    private Vector3 _moveDirection = Vector3.zero;
    private float _slopeAngle;
    private float _slopeEffector;
    private float _vertical;
    private float _horizontal;
    private bool _jumpInput;
    private float _inputAmount;
    private Vector3 _gravity = Vector3.zero;
    private int _remainingJumps;
    private float _turnSmoothVelocity;

    void Start () 
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _remainingJumps = maxJumps;
    }
 
    private void Update()
    {
        Debug.Log(_gravity);
        _horizontal = Input.GetAxis("Horizontal");
        _vertical = Input.GetAxis("Vertical");
        _jumpInput = Input.GetButton("Jump");
        
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
        if (_remainingJumps > 0)
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
        if(!IsGrounded() && _rb.velocity.y > 0)
        {
            _gravity += jumpGravityMultiplier * Vector3.up * Physics.gravity.y * Time.fixedDeltaTime;
        }
        else if (!IsGrounded() && _rb.velocity.y < 0)
        {
            _gravity += fallGravityMultiplier * Vector3.up * Physics.gravity.y * Time.fixedDeltaTime;
        }
        
        // actual movement of the rigidbody + extra down force
        _rb.velocity = (_moveDirection * GetMoveSpeed() * _inputAmount) + _gravity;
        _slopeAngle = GetSlopeAngle();
        if (_slopeAngle < slopeLimit && IsGrounded())
        {
            _rb.velocity = Quaternion.AngleAxis( _slopeAngle, Vector3.forward) * _rb.velocity;
            UnityEngine.Debug.DrawRay(_rb.position,_rb.velocity,Color.blue);
            _gravity.y = 0;
        }
        else
        {
            _gravity += slopeSpeedMultiplier * Vector3.up * Physics.gravity.y * Time.fixedDeltaTime;
        }
    }
   
   private Vector3 GetMoveDirection(Vector3 moveDirection)
   {
       // base movement on camera
       float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + characterCam.eulerAngles.y;
       float finalAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, turnSmoothTime);
       _rb.MoveRotation(Quaternion.Euler(0f, finalAngle, 0f)); 
       Vector3 finalDirection = (Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward).normalized;

       return finalDirection;
   }

   private float GetMoveSpeed()
   {
       //move slower
       return moveSpeed * (1-_slopeEffector);
   }

   private bool FloorRaycasts(float offsetx, float offsetz, float raycastLength)
   {
       RaycastHit hit;
       Vector3 raycastOriginPos = transform.TransformPoint(offsetx, rayStartPointOffsetY, offsetz);
        
       UnityEngine.Debug.DrawRay(raycastOriginPos, Vector3.down * raycastLength, Color.magenta);
       if (Physics.Raycast(raycastOriginPos, Vector3.down, out hit, raycastLength))
       {
           return true;
       }
       return false;
   }
   private float SlopeRaycasts(float offsetx, float offsetz, float raycastLength)
   {
       RaycastHit hit;
       Vector3 raycastOriginPos = transform.TransformPoint(offsetx, rayStartPointOffsetY, offsetz);
        
       UnityEngine.Debug.DrawRay(raycastOriginPos, Vector3.down * raycastLength, Color.black);
       if (Physics.Raycast(raycastOriginPos, Vector3.down, out hit, raycastLength))
       {
           _slopeEffector = Mathf.Clamp01(Vector3.Dot(-transform.forward, hit.normal));
           float angle = Vector3.Angle(hit.normal, Vector3.up);
           return angle;
       }
       return 0f;
   }
   
    
    private bool IsGrounded()
    {
        if (FindGround())
        {
            _remainingJumps = maxJumps;
            return true;
        }
        return false;
    }

    private bool FindGround()
    {
        if (FloorRaycasts(0, -rayCastWidth, raycastLength) || FloorRaycasts(0, rayCastWidth, raycastLength))
        {
            return true;
        }
        return false;
        
    }
    
    private float GetSlopeAngle()
    {
        float angle = 0f;
        angle += SlopeRaycasts(0, rayCastWidth, raycastLength);
        if (angle > 0f) return angle;
        angle += SlopeRaycasts(0, -rayCastWidth, raycastLength);
        if (angle > 0f) return angle;
        return angle;
    }
}