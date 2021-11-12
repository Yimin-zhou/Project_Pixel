using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class AdditionalIK : MonoBehaviour
{
    public String tageName;
    public float lockSpeed = 1f;
    public float resetPosY = 1.5f;
    public float defaultLookLength = 10f;

    public Transform handLeftOrigin;
    public Transform handRightOrigin;
    public float shoulderRaycastLength = 3f;
    public LayerMask playerLayer;
    
    private LookAtIK _lookAtIK;
    private FullBodyBipedIK _fullIK;
    private Vector3 _targetPosition;
    private bool _useLookAt;

    // Start is called before the first frame update
    void Start()
    {
        _lookAtIK = GetComponent<LookAtIK>();
        _fullIK = GetComponent<FullBodyBipedIK>();
    }

    private void Update()
    {
        setHeadIK();
        setHandIK();
    }

    private void setHeadIK()
    {
        if (!_useLookAt)
        {
            Vector3 defaultTarget = transform.position + transform.forward.normalized * defaultLookLength;
            _lookAtIK.solver.IKPosition = Vector3.Lerp(_lookAtIK.solver.IKPosition,new Vector3(defaultTarget.x,defaultTarget.y + resetPosY,defaultTarget.z),Time.deltaTime * lockSpeed);
        }
        else
        {
            _lookAtIK.solver.IKPosition = Vector3.Lerp(_lookAtIK.solver.IKPosition,_targetPosition,Time.deltaTime * lockSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tageName))
        {
            _targetPosition = other.transform.position;
            _useLookAt = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tageName))
        {
            _useLookAt = false;
        }
    }

    private void setHandIK()
    {
        RaycastHit leftHit;
        if (Physics.Raycast(handLeftOrigin.position, transform.forward, out leftHit, shoulderRaycastLength))
        {
            UnityEngine.Debug.DrawRay(handLeftOrigin.position, transform.forward * shoulderRaycastLength, Color.green);

            if (leftHit.transform.gameObject.layer != playerLayer)
            {
                Quaternion rot = Quaternion.LookRotation(transform.forward);    
                float distanceArmHit = Vector3.Distance(_fullIK.references.leftHand.position, leftHit.point);
                float lerpWeight = Mathf.Clamp01(distanceArmHit / shoulderRaycastLength);
    
                //_fullIK.solver.leftHandEffector.rotation = Quaternion.FromToRotation(Vector3.up, leftHit.normal) * rot;
                //_fullIK.solver.leftHandEffector.rotationWeight = Mathf.Clamp01(1 - lerpWeight);
                    
                _fullIK.solver.leftHandEffector.position = leftHit.point;
                _fullIK.solver.leftHandEffector.positionWeight = Mathf.Clamp01(1 - lerpWeight);
            }
            
        }
        else
        {
            _fullIK.solver.leftHandEffector.rotationWeight = 0f;
            _fullIK.solver.leftHandEffector.positionWeight = 0f;
            UnityEngine.Debug.DrawRay(handLeftOrigin.position, transform.forward * shoulderRaycastLength, Color.red);
        }
        
        RaycastHit rightHit;
        if (Physics.Raycast(handRightOrigin.position, transform.forward, out rightHit, shoulderRaycastLength))
        {
            UnityEngine.Debug.DrawRay(handRightOrigin.position, transform.forward * shoulderRaycastLength, Color.green);

            if (rightHit.transform.gameObject.layer != playerLayer)
            {
                Quaternion rot = Quaternion.LookRotation(transform.forward);    
                float distanceArmHit = Vector3.Distance(_fullIK.references.rightHand.position, rightHit.point);
                float lerpWeight = Mathf.Clamp01(distanceArmHit / shoulderRaycastLength);

                //_fullIK.solver.rightHandEffector.rotation = Quaternion.FromToRotation(Vector3.up, rightHit.normal) * rot;
                //_fullIK.solver.rightHandEffector.rotationWeight = Mathf.Clamp01(1 - lerpWeight);
                
                _fullIK.solver.rightHandEffector.position = rightHit.point;
                _fullIK.solver.rightHandEffector.positionWeight = Mathf.Clamp01(1 - lerpWeight);
            }
        }
        else
        {
            _fullIK.solver.rightHandEffector.rotationWeight = 0f;
            _fullIK.solver.rightHandEffector.positionWeight = 0f;

            UnityEngine.Debug.DrawRay(handRightOrigin.position, transform.forward * shoulderRaycastLength, Color.red);
        }
    }
}
