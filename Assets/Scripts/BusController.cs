using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class BusController : MonoBehaviour
{
    private float horizontalInput;
    private float verticalInput;
    private bool isBreaking;
    [SerializeField] private float currentBreakForce;
    [SerializeField] private Rigidbody busRB;
    

    private float currenSteeringAngle;

    [SerializeField] private float motorForce;
    [SerializeField] private float breakForce;
    [SerializeField] private float maxSteeringAngle;

    [SerializeField] private WheelCollider front_left_wheel_collider;
    [SerializeField] private WheelCollider front_right_wheel_collider;
    [SerializeField] private WheelCollider rear_left_wheel_collider;
    [SerializeField] private WheelCollider rear_right_wheel_collider;

    [SerializeField] private Transform front_left_wheel_transform;
    [SerializeField] private Transform front_right_wheel_transform;
    [SerializeField] private Transform rear_left_wheel_transform;
    [SerializeField] private Transform rear_right_wheel_transform;

    [SerializeField] private Transform checkPoint;
    [SerializeField] private Text distanceText;
    public float currentSpeed;
    [SerializeField] private float maxSpeed;

    [Header("Speedometer")]
    private const float MAX_SPEED_ANGLE = -20;
    private const float ZERO_SPEED_ANGLE = 210;
    [SerializeField] private Transform needleTransform;

    [Header("Reverse button")]
    private bool reversePressed = false;
    [SerializeField] private Button reverseButton;

    [Header("BusStop")]
    private BusStop busStop;
    

    [Header("NitroBoost")]
    [SerializeField] float boostMotorTorque;
    [SerializeField] float baseMotorTorque;
    [SerializeField] ParticleSystem speedEffect;
    bool nitrousBeingHeld = false;
    [SerializeField] float nitrousAmount = 100f;
    private float nitrousTimer = 0f;
    [SerializeField] Slider nitroSlider;

    [Header("Steering Wheel")]
    [SerializeField] SteeringWheel steeringWheel;

    [Header("BusGateOpen Button")]
    [SerializeField] Animator busAnim;
    bool busGatesOpenPressed = false;
    bool busGatesAreClosed = true;

    [Header("Camera Switch")]
    [SerializeField] CinemachineVirtualCamera vcam1;
    [SerializeField] CinemachineVirtualCamera vcam2;
    [SerializeField] CinemachineVirtualCamera vcam3;
    [SerializeField] CinemachineVirtualCamera vcam4;
    


    private void Awake()
    {
        currentSpeed = 0;
        maxSpeed = 100f;

        busStop = FindObjectOfType<BusStop>();
    }
    private void Start()
    {
        busRB.centerOfMass = new Vector3(0, -0.9f, 0);
    }
    private void Update()
    {
        //DistanceChecker();
        //SpeedOMeter();
        CamerasSwitch();

        nitrousAmount = Mathf.Clamp(nitrousAmount, 0f, 100f);
        nitroSlider.value = nitrousAmount; 
        if(nitrousBeingHeld)
        {
            nitrousAmount -= 0.35f;
        }
        if(nitrousAmount<=0)
        {
            motorForce = baseMotorTorque;
            speedEffect.gameObject.SetActive(false);
            maxSpeed = 150f;

        }

        if(busGatesOpenPressed && busGatesAreClosed)
        {
            busAnim.Play("Bus2_Gates_01");
            busGatesAreClosed = false;
            
            vcam1.Priority = 10;
            vcam2.Priority = 10;
            vcam3.Priority = 10;
            vcam4.Priority = 20;
        }
        else if(!busGatesOpenPressed && !busGatesAreClosed)
        {
            busAnim.Play("Bus2_Gates_Closing");
            busGatesAreClosed=true;
        }
    }
    private void FixedUpdate()
    {
        GetInput();
        NitrousBoost();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Nitro")
        {
            if(nitrousAmount < 100f)
            {
                nitrousAmount += 25f;
                Destroy(other.gameObject);
            }
            
        }
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        //horizontalInput = steeringWheel.GetClampedValue();
        verticalInput = Input.GetAxis("Vertical");
        //verticalInput = nitrousBeingHeld ? 1 : steeringWheel.GetVertical();
        isBreaking = Input.GetKey(KeyCode.Space);
        Debug.Log("IsBreaking : " + isBreaking);
    }

    private void HandleMotor()
    {
        currentSpeed = busRB.velocity.magnitude * 3.6f;
        if(currentSpeed<maxSpeed)
        {
            front_left_wheel_collider.motorTorque = verticalInput * motorForce;
            front_right_wheel_collider.motorTorque = verticalInput * motorForce;
        }
        else
        {
            front_left_wheel_collider.motorTorque = 0;
            front_right_wheel_collider.motorTorque = 0;
        }
        
        currentBreakForce = isBreaking ? breakForce : 0f;
        ApplyBrakes();
        //currentSpeed = 2 * Mathf.PI * front_left_wheel_collider.radius * front_left_wheel_collider.rpm * 60 / 1000;
        Debug.Log("Current bus speed :  " + currentSpeed);
    }

    private void ApplyBrakes()
    {
        front_left_wheel_collider.brakeTorque = currentBreakForce;
        front_right_wheel_collider.brakeTorque = currentBreakForce;
        rear_left_wheel_collider.brakeTorque = currentBreakForce;
        rear_right_wheel_collider.brakeTorque = currentBreakForce;
    }

    private void HandleSteering()
    {
        currenSteeringAngle = maxSteeringAngle * horizontalInput;
        front_left_wheel_collider.steerAngle = currenSteeringAngle;
        front_right_wheel_collider.steerAngle = currenSteeringAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(front_left_wheel_collider, front_right_wheel_transform);
        UpdateSingleWheel(front_right_wheel_collider, front_right_wheel_transform);
        UpdateSingleWheel(rear_left_wheel_collider, rear_left_wheel_transform);
        UpdateSingleWheel(rear_right_wheel_collider, rear_right_wheel_transform);
    }

    private void UpdateSingleWheel(WheelCollider wheel_collider, Transform wheel_transform)
    {
        Vector3 pos;
        Quaternion rot;
        wheel_collider.GetWorldPose(out pos, out rot);
        wheel_transform.position = pos;
        wheel_transform.rotation = rot;
    }

    private void DistanceChecker()
    {
        float distance = Vector3.Distance(checkPoint.position, transform.position);

        distanceText.text = "Distance : " + distance.ToString("F2") + " meters";
    }

    private void SpeedOMeter()
    {

        needleTransform.eulerAngles = new Vector3(0, 0, GetSpeedRotation());
    }

    private float GetSpeedRotation()
    {
        float totalAngleSize = ZERO_SPEED_ANGLE - MAX_SPEED_ANGLE;
        float speedNormalized = currentSpeed / maxSpeed;
        return ZERO_SPEED_ANGLE - speedNormalized * totalAngleSize;
    }

    public void OnReverseButtonPressed()
    {
        reversePressed = !reversePressed;
        ColorBlock cb = reverseButton.colors;

        if (reversePressed)
        {
            cb.selectedColor = Color.green;
            reverseButton.colors = cb;
        }
        if (!reversePressed)
        {
            cb.selectedColor = Color.white;
            reverseButton.colors = cb;
        }
        motorForce = motorForce * -1f;
    }

    public void NitrousBoost()
    {
        
        if(Input.GetKey(KeyCode.LeftShift))
        {
            if(nitrousAmount>0 && nitrousAmount<=100)
            {
                speedEffect.gameObject.SetActive(true);
                motorForce = boostMotorTorque;
                maxSpeed = 300f;
                nitrousAmount -= 0.35f;

            }

            
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            motorForce = baseMotorTorque;
            speedEffect.gameObject.SetActive(false);
            maxSpeed = 150f;
            
        }
        
    }

    public void OnNitroPressed()
    {
        if(nitrousAmount>0 && nitrousAmount<=100)
        {
            nitrousBeingHeld = true;
            speedEffect.gameObject.SetActive(true);
            motorForce = boostMotorTorque;
            maxSpeed = 300f;

        }
        
    }
    public void OnNitroReleased()
    {
        nitrousBeingHeld = false;
        motorForce = baseMotorTorque;
        speedEffect.gameObject.SetActive(false);
        maxSpeed = 150f;
    }

    private void CamerasSwitch()
    {
        if (currenSteeringAngle == 0 && !BusStop.atBusStop && !busGatesOpenPressed)
        {

            vcam1.Priority = 20;
            vcam2.Priority = 10;
            vcam3.Priority = 10;
            vcam4.Priority = 10;

            
        }
        else if (currenSteeringAngle > 0 && !BusStop.atBusStop && !busGatesOpenPressed)
        {

            //vcam1.Priority = 10;
            //vcam2.Priority = 20;
            //vcam3.Priority = 10;
            //vcam4.Priority = 10;
        }
        else if (currenSteeringAngle < 0 && !BusStop.atBusStop && !busGatesOpenPressed)
        {
            //vcam1.Priority = 10;
            //vcam2.Priority = 10;
            //vcam3.Priority = 20;
            //vcam4.Priority = 10;
        }
        if (BusStop.atBusStop && !busGatesOpenPressed)
        {
           
            vcam1.Priority = 10;
            vcam2.Priority = 10;
            vcam3.Priority = 10;
            vcam4.Priority = 20;

        }
    }

    public void BusGatesOpenButtonPressed()
    {
        busGatesOpenPressed = !busGatesOpenPressed;
    }

    
}
