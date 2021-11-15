using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCharacterController: MonoBehaviour {
    
    [Header("Raycast")]
    public float rayStartPointOffsetY = 0f;
    public float raycastLength = 3f;
    public float rayCastWidth = 2.5f;

    [Header("Slope")] 
    public float lowSlopeSpeedMultiplier = 1.5f;
    public float slopeLimit = 60f;
    public float sharpSlopeSpeedMultiplier = 1f;
    
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float turnSmoothTime = 0.1f;
    public Transform characterCam;
    
    [Header("Jump")]
    public float jumpGravityMultiplier = 6f;
    public float fallGravityMultiplier = 10f;
    public float jumpPower = 5f;
    public int maxJumps = 2;
    
    private Rigidbody _rb;
    private Animator _anim;
    private Vector3 _moveDirection = Vector3.zero;
    private float _slopeAngle;
    private float _slopeEffector;
    private float _vertical;
    private float _horizontal;
    private bool _jumpInput;
    private float _inputAmount;
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
        if (IsGrounded() && _slopeAngle <= 60f)
        {
            _remainingJumps = maxJumps;
        }
        if (_remainingJumps > 0)
        {
            _rb.velocity = new Vector3(_rb.velocity.x,jumpPower,_rb.velocity.z);
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

       AdjustJumpGravity();
       Move();
    }

    private void AdjustJumpGravity()
    {
        // if not grounded or on slope , increase down force
        if(!IsGrounded() && _rb.velocity.y > 0f)
        {
            _rb.velocity += jumpGravityMultiplier * Vector3.up * Physics.gravity.y * Time.fixedDeltaTime;
        }
        else if (!IsGrounded() && _rb.velocity.y < 0f)
        {
            _rb.velocity += fallGravityMultiplier * Vector3.up * Physics.gravity.y * Time.fixedDeltaTime;
        }
    }

    private void Move()
    {
        _rb.velocity = new Vector3((_moveDirection * GetMoveSpeed() * _inputAmount).x,_rb.velocity.y,(_moveDirection * GetMoveSpeed() * _inputAmount).z);
        // actual movement of the rigidbody + extra down force
        Vector3 hitNormal = Vector3.up;
        _slopeAngle = GetSlopeAngle(out hitNormal);
        if (_slopeAngle != 0f && _slopeAngle < slopeLimit && IsGrounded())
        {
            Vector3 direction = Vector3.ProjectOnPlane(_rb.velocity,hitNormal);
            _rb.velocity = new Vector3(direction.x,_rb.velocity.y,direction.z) * lowSlopeSpeedMultiplier;
            UnityEngine.Debug.DrawRay(_rb.position,_rb.velocity,Color.blue);
            Debug.Log("velo " + _rb.velocity + "slop yes! " + _slopeAngle);
        }
        else if (_slopeAngle >= slopeLimit)
        {
            _rb.velocity += new Vector3(_rb.velocity.x,sharpSlopeSpeedMultiplier * Physics.gravity.y * Time.fixedDeltaTime,_rb.velocity.z);
            Debug.Log("velo " + _rb.velocity + "_slopeAngle no" + _slopeAngle);
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
       return moveSpeed * (1f-_slopeEffector);
   }
   
   private bool IsGrounded()
   {
       if (FindGround())
       {
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

   private bool FloorRaycasts(float offsetx, float offsetz, float raycastLength)
   {
       RaycastHit hit;
       Vector3 raycastOriginPos = transform.TransformPoint(offsetx, rayStartPointOffsetY, offsetz);

       Color rayColor = Color.red;
       
       if (Physics.Raycast(raycastOriginPos, Vector3.down, out hit, raycastLength))
       {
           rayColor = Color.green;
           UnityEngine.Debug.DrawRay(raycastOriginPos, Vector3.down * raycastLength, rayColor);
           return true;
       }
       UnityEngine.Debug.DrawRay(raycastOriginPos, Vector3.down * raycastLength, rayColor);
       return false;
   }
   
   private Vector3 SlopeRaycasts(float offsetx, float offsetz, float raycastLength)
   {
       RaycastHit hit;
       Vector3 raycastOriginPos = transform.TransformPoint(offsetx, rayStartPointOffsetY, offsetz);
        
       UnityEngine.Debug.DrawRay(raycastOriginPos, Vector3.down * raycastLength, Color.black);
       if (Physics.Raycast(raycastOriginPos, Vector3.down, out hit, raycastLength))
       {
           _slopeEffector = Mathf.Clamp01(Vector3.Dot(-transform.forward, hit.normal));
           return hit.normal;
       }
       return Vector3.up;
   }

   private float GetSlopeAngle(out Vector3 hitNormal)
    {
        float angleFront = 0f;
        float angleBack = 0f;
        Vector3 normalFront = SlopeRaycasts(0, rayCastWidth, raycastLength);
        angleFront += Vector3.Angle(normalFront, Vector3.up);
        Vector3 normalBack = SlopeRaycasts(0, -rayCastWidth, raycastLength);
        angleBack += Vector3.Angle(normalBack, Vector3.up);

        if (angleFront > angleBack)
        {
            hitNormal = normalFront;
            return angleFront;
        }
        else
        {
            hitNormal = normalBack;
            return angleBack;
        }
    }
}