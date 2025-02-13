using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.XR.Interaction.Toolkit;

public class SteerRotation : MonoBehaviour
{
    private float initialLeftHandRotation;
    private bool leftHandInitialRotationCaptured = false;
    private float initialRightHandRotation;
    private bool rightHandInitialRotationCaptured = false;
    private Quaternion initialWheelRotation;
    private Quaternion initialVehicleRotation;
    private InputData _inputData;
    public GameObject rightHand;
    public GameObject Needle;
    public GameObject wheel;
    private bool rightHandOnWheel = false;
    public float maxMoveSpeed = 220f;
    public float moveSpeed = 70;
    public GameObject leftHand;
    private bool leftHandOnWheel = false;
    public GameObject Vehicle;
    private Rigidbody VehicleRigidBody;
    public float turnDampening = 60;
    public InputActionReference rightHandGripAction;
    public InputActionReference leftHandGripAction;
    float currentLeftHandRotation;
    Vector3 forwardMovement;
    public Movement movement;
    public Vector2 joyStickValueR = new Vector2(0, 0);
    public float speedAngle = 0;
    private float currentAngle = -180f;
    public float smoothSpeed = 2f;


    void Start()
    {
        _inputData = GetComponent<InputData>();
        VehicleRigidBody = Vehicle.GetComponent<Rigidbody>();
        initialVehicleRotation = Vehicle.transform.localRotation;
        initialWheelRotation = wheel.transform.localRotation;
        //forwardMovement = moveSpeed * Time.deltaTime * Vehicle.transform.forward;

    }

    void Update()
    {
        currentLeftHandRotation = leftHand.transform.localEulerAngles.z;
        CheckLeftHandOnWheel();
        CheckRightHandOnWheel();
        // VehicleRigidBody.velocity = forwardMovement;
        if (movement.isEdgeDetected)
        {
            moveSpeed = 0;
        }
        ReleaseHandsFromWheel();


    }

    private void CheckLeftHandOnWheel()
    {

        // Vector2 joyStickValueL = new Vector2(0, 0);
        if (leftHandOnWheel && leftHandGripAction.action.ReadValue<float>() > 0.1f)
        {
            if (!leftHandInitialRotationCaptured)
            {
                initialLeftHandRotation = leftHand.transform.localRotation.z;
                leftHandInitialRotationCaptured = true;
            }
            else
            {
                float rotationDifference = (currentLeftHandRotation - initialLeftHandRotation) - 360;
                //debug.log("currentLeftHandRotation" + currentLeftHandRotation + " initialLeftHandRotation  " + initialLeftHandRotation + " rotationDifference  " + rotationDifference);
                //debug.log("rotationdifference " + Math.Abs(rotationDifference % 360));
                if ((Math.Abs(rotationDifference % 360) > 10) && (Math.Abs(rotationDifference % 360) < 350))
                {
                    if (rotationDifference < 0)
                    {
                        ConvertHandRotationToSteeringWheelRotation(-rotationDifference + 360);
                        TurnVehicle(-rotationDifference + 360);
                    }
                    else
                    {
                        ConvertHandRotationToSteeringWheelRotation(rotationDifference);
                        TurnVehicle(rotationDifference);
                    }
                }
                // if (_inputData._leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out joyStickValueL))
                // {
                //     UpdateSpeedometer(joyStickValueL.y);
                // }

            }
            // Vector3 forwardMovement = joyStickValueL.y * Vehicle.transform.forward;
            // VehicleRigidBody.velocity = forwardMovement * moveSpeed;
        }
        else
        {
            leftHandInitialRotationCaptured = false;
        }
    }

    private void CheckRightHandOnWheel()
    {
        if (_inputData._rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out joyStickValueR))
        {
            
            if (joyStickValueR.y == 0)
            {
                moveSpeed = 0;
            }
            else if (joyStickValueR.y < 0)
            {
                //VehicleRigidBody.velocity = Vector3.zero;
                joyStickValueR = -joyStickValueR;
                moveSpeed = 20;
                movement.isAccelerating = false;
                movement.isDecelerating = true;
                speedAngle -= 10 + joyStickValueR.y;
            }
            else
            {
                moveSpeed = 80;
                VehicleRigidBody.velocity = forwardMovement;
                movement.isAccelerating = true;
                movement.isDecelerating = false;
                speedAngle += 10 + joyStickValueR.y;
            }
            forwardMovement = 10 * joyStickValueR.y * moveSpeed * Time.deltaTime * Vehicle.transform.forward;
            UpdateSpeedometer(joyStickValueR.y);
            //VehicleRigidBody.velocity = forwardMovement * 2;
            // 
            // VehicleRigidBody.velocity = new Vector3(0,0,forwardMovement.z * moveSpeed);
            // //debug.log("Velcity "+VehicleRigidBody.velocity);
        }
    }

    private void ConvertHandRotationToSteeringWheelRotation(float rotationDelta)
    {
        Vector3 currentRotation = wheel.transform.localEulerAngles;
        float targetRotationZ = currentRotation.z + rotationDelta;

        if (wheel.transform.localEulerAngles.z >= 270)
        {
            targetRotationZ = 270 + rotationDelta;
            wheel.transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, 270);
        }
        else if (wheel.transform.localEulerAngles.z <= 90)
        {
            targetRotationZ = 90 + rotationDelta;
            wheel.transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, 90);
        }
        Quaternion targetRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, targetRotationZ);
        wheel.transform.localRotation = Quaternion.RotateTowards(wheel.transform.localRotation, targetRotation, turnDampening * 0.7f * Time.deltaTime);

        //debug.log("Rotateeeeee: " + rotationDelta);
    }
    private void ReleaseHandsFromWheel()
    {
        if (rightHandOnWheel && rightHandGripAction.action.ReadValue<float>() <= 0.1f)
        {
            rightHandOnWheel = false;
        }

        if (leftHandOnWheel && leftHandGripAction.action.ReadValue<float>() <= 0.1f)
        {
            leftHandOnWheel = false;
            leftHandInitialRotationCaptured = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PlayerHandL"))
        {
            if (!leftHandOnWheel && leftHandGripAction.action.ReadValue<float>() > 0.1f)
            {
                leftHandOnWheel = true;
                initialLeftHandRotation = leftHand.transform.localEulerAngles.z;
                leftHandInitialRotationCaptured = true;
            }
        }
        if (other.CompareTag("PlayerHandR"))
        {
            if (!rightHandOnWheel && rightHandGripAction.action.ReadValue<float>() > 0.1f)
            {
                rightHandOnWheel = true;
                initialRightHandRotation = rightHand.transform.localEulerAngles.z;
                rightHandInitialRotationCaptured = true;
            }
        }
    }

    void UpdateSpeedometer(float moveSpeed)
    {
        float targetAngle;
        if (moveSpeed >= 0 && moveSpeed <= 0.5f)
        {
            targetAngle = 360f * moveSpeed - 180f;
        }
        else if (moveSpeed > 0.5f && moveSpeed <= 1)
        {
            targetAngle = 360f * moveSpeed + 160f;
        }
        else if (moveSpeed < -1)
        {
            targetAngle = 0;
        }
        else
        {
            targetAngle = Needle.transform.localRotation.y;
        }
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, smoothSpeed * Time.deltaTime);
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, currentAngle, 0f));
        Needle.transform.localRotation = targetRotation;
    }

    private void TurnVehicle(float rotationDelta)
    {
        var turn = Vehicle.transform.localEulerAngles.y;
        if (Vehicle.transform.localEulerAngles.y > 90 && Vehicle.transform.localEulerAngles.y < 180)
        {
            turn = 90;
        }
        else if (Vehicle.transform.localEulerAngles.y < 270 && Vehicle.transform.localEulerAngles.y > 180)
        {
            turn = -90;
        }
        //debug.log("Vehicle Rotate: " + rotationDelta);
        VehicleRigidBody.MoveRotation(Quaternion.RotateTowards(Vehicle.transform.rotation, Quaternion.Euler(0, Math.Abs(turn + rotationDelta), 0), Time.deltaTime * turnDampening));
    }
}
