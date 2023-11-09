using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICarEngine : MonoBehaviour
{
    public Transform path;
    public float maxSteerAngle = 45f;
    public float turnSpeed = 5f;
    public float maxMotorTorque = 80f;
    public float maxBrakeTorque = 150f;
    public float currentSpeed;
    public float maxSpeed = 100f;
    public WheelCollider front_left_wheel_collider;
    public WheelCollider front_right_wheel_collider;
    public WheelCollider back_left_wheel_collider;
    public WheelCollider back_right_wheel_collider;
    public Vector3 centerOfMass;
    public bool isBraking = false;

    public List<Transform> nodes;
    public int currentNode = 0;

    [Header("Sensors")]
    public float sensorLength = 3f;
    //public float frontSensorPosition = 0.5f;
    public Vector3 frontSensorPosition = new Vector3(0f, 0.2f, 0.5f);
    public float frontSideSensorPosition = 0.2f;
    public float frontSensorAngle = 30f;
    public float frontSensorAngleX = 0.777f;
    private bool avoiding = false;

    [Header("Sensor Empty Objects")]
    public Transform frontSensorMiddle;
    public Transform frontSensorLeft;
    public Transform frontSensorRight;
    public Transform frontSensorMiddle2;
    public Transform frontSensorLeft2;
    public Transform frontSensorRight2;
    public Transform frontSensorLeftAngled;
    public Transform frontSensorRightAngled;
    //private float previousAvoidingMultiplier;
    private float targetSteerAngle = 0f;



    // Start is called before the first frame update
    void Start()
    {
        //GetComponent<Rigidbody>().centerOfMass = centerOfMass;
        GetComponent<Rigidbody>().ResetCenterOfMass();
        Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();

        for (int i = 0; i < pathTransforms.Length; i++)
        {
            if (pathTransforms[i] != path.transform)
            {
                nodes.Add(pathTransforms[i]);
            }
        }


    }

    private void FixedUpdate()
    {
        //Sensors();
        Sensors2();
        ApplySteer();
        Drive();
        CheckWayPointDistance();
        Braking();
        LerpToSteerAngle();
    }

    private void Sensors()
    {
        RaycastHit hit;
        Vector3 sensorStartPosition = transform.position;
        sensorStartPosition += transform.forward * frontSensorPosition.z;
        sensorStartPosition += transform.up * frontSensorPosition.y;
        Debug.Log(sensorStartPosition);
        float avoidMultiplier = 0f;
        avoiding = false;


        //front center sensor
        if (Physics.Raycast(sensorStartPosition, Quaternion.AngleAxis(frontSensorAngleX, Vector3.right) * transform.right, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Obstacle"))
            {

                avoiding = true;
            }


        }
        Debug.DrawLine(sensorStartPosition, hit.point);


        //front right sensor
        sensorStartPosition += transform.right * frontSideSensorPosition;
        if (Physics.Raycast(sensorStartPosition, Quaternion.AngleAxis(frontSensorAngleX, Vector3.right) * transform.right, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Obstacle"))
            {

                avoiding = true;
                avoidMultiplier -= 1f;
            }

        }
        Debug.DrawLine(sensorStartPosition, hit.point);

        //front right angle sensor
        if (Physics.Raycast(sensorStartPosition, Quaternion.AngleAxis(frontSensorAngle, Vector3.up) * transform.right, out hit, sensorLength))
        {

            if (!hit.collider.CompareTag("Obstacle"))
            {

                avoiding = true;
                avoidMultiplier -= 0.5f;
            }
        }
        Debug.DrawLine(sensorStartPosition, hit.point);

        //front left sensor
        sensorStartPosition -= transform.right * 2 * frontSideSensorPosition;
        if (Physics.Raycast(sensorStartPosition, Quaternion.AngleAxis(frontSensorAngleX, Vector3.right) * transform.right, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Obstacle"))
            {

                avoiding = true;
                avoidMultiplier += 1f;
            }
        }
        Debug.DrawLine(sensorStartPosition, hit.point);

        //front left angle sensor
        if (Physics.Raycast(sensorStartPosition, Quaternion.AngleAxis(-frontSensorAngle, Vector3.up) * transform.right, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Obstacle"))
            {

                avoiding = true;
                avoidMultiplier += 0.5f;
            }
        }
        Debug.DrawLine(sensorStartPosition, hit.point);
        if (avoiding)
        {
            front_left_wheel_collider.steerAngle = maxSteerAngle * avoidMultiplier;
            front_right_wheel_collider.steerAngle = maxSteerAngle * avoidMultiplier;
        }
    }

    private void Sensors2()
    {
        RaycastHit hit;
        float avoidingMultiplier = 0f;
        avoiding = false;

        ////front middle
        //if(Physics.Raycast(frontSensorMiddle.position,frontSensorMiddle2.position-frontSensorMiddle.position,out hit,sensorLength))
        //{
        //    if (!hit.collider.CompareTag("Obstacle"))
        //    {
        //        Debug.DrawLine(frontSensorMiddle.position, hit.point, Color.blue);
        //        avoiding = true;
        //    }

        //}


        //front left
        if (Physics.Raycast(frontSensorLeft.position, frontSensorLeft2.position - frontSensorLeft.position, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(frontSensorLeft.position, hit.point, Color.red);
                avoiding = true;
                avoidingMultiplier += 1f;
            }
        }
        //Debug.DrawRay(frontSensorLeft.position, (frontSensorLeft2.position - frontSensorLeft.position)*sensorLength, Color.red);

        else if (Physics.Raycast(frontSensorLeft.position, frontSensorLeftAngled.position - frontSensorLeft.position, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(frontSensorLeft.position, hit.point, Color.blue);
                avoiding = true;
                avoidingMultiplier += 0.5f;
            }
        }
        //Debug.DrawRay(frontSensorLeft.position, (frontSensorLeftAngled.position - frontSensorLeft.position)*sensorLength, Color.blue);



        //front right
        if (Physics.Raycast(frontSensorRight.position, frontSensorRight2.position - frontSensorRight.position, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(frontSensorRight.position, hit.point, Color.cyan);
                avoiding = true;
                avoidingMultiplier -= 1f;
            }
        }
        //Debug.DrawRay(frontSensorRight.position, (frontSensorRight2.position - frontSensorRight.position)*sensorLength, Color.cyan);

        else if (Physics.Raycast(frontSensorRight.position, frontSensorRightAngled.position - frontSensorRight.position, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(frontSensorRight.position, hit.point, Color.yellow);
                avoiding = true;
                avoidingMultiplier -= 0.5f;
            }
        }
        //Debug.DrawRay(frontSensorRight.position, (frontSensorRightAngled.position - frontSensorRight.position)*sensorLength, Color.yellow);


        //front center sensor
        if (avoidingMultiplier == 0)
        {
            if (Physics.Raycast(frontSensorMiddle.position, frontSensorMiddle2.position - frontSensorMiddle.position, out hit, sensorLength))
            {
                if (!hit.collider.CompareTag("Terrain"))
                {
                    Debug.DrawLine(frontSensorMiddle.position, hit.point, Color.green);
                    avoiding = true;
                    if (hit.normal.x < 0)
                    {
                        avoidingMultiplier = -1;
                    }
                    else
                    {
                        avoidingMultiplier = 1;
                    }
                }
            }
            Debug.DrawRay(frontSensorMiddle.position, (frontSensorMiddle2.position - frontSensorMiddle.position) * sensorLength, Color.green);
        }
        //Debug.Log("Before : "+avoidingMultiplier+" , "+avoiding.ToString());
        //if(avoidingMultiplier == 0 )
        //{
        //    if(previousAvoidingMultiplier!=0)
        //    {
        //        avoiding = true;
        //        if (MathF.Abs(previousAvoidingMultiplier) <= 0.11f) previousAvoidingMultiplier = 0;
        //        else avoidingMultiplier = (MathF.Abs( previousAvoidingMultiplier)-0.1f) * MathF.Sign(previousAvoidingMultiplier);
        //        previousAvoidingMultiplier = avoidingMultiplier;
        //    }

        //}
        //else
        //{
        //    previousAvoidingMultiplier = avoidingMultiplier;
        //}
        //Debug.Log("after : " + avoidingMultiplier + " , " + avoiding.ToString());

        if (avoiding)
        {
            targetSteerAngle = maxSteerAngle * avoidingMultiplier;

            //front_left_wheel_collider.steerAngle = maxSteerAngle * avoidingMultiplier;
            //front_right_wheel_collider.steerAngle = maxSteerAngle * avoidingMultiplier;
        }
        Debug.Log("avoiding multiplier : " + avoidingMultiplier);


    }

    private void CheckWayPointDistance()
    {
        Debug.Log("Distance to node : " + Vector3.Distance(transform.position, nodes[currentNode].position));
        if (Vector3.Distance(transform.position, nodes[currentNode].position) < 5f)
        {
            if (currentNode == nodes.Count - 1)
            {
                currentNode = 0;
            }
            else
            {
                currentNode++;
            }
        }
    }

    private void Drive()
    {
        currentSpeed = 2 * Mathf.PI * front_left_wheel_collider.radius * front_left_wheel_collider.rpm * 60 / 1000;
        if (currentSpeed < maxSpeed && !isBraking)
        {
            front_left_wheel_collider.motorTorque = maxMotorTorque;
            front_right_wheel_collider.motorTorque = maxMotorTorque;
        }
        else
        {
            front_left_wheel_collider.motorTorque = 0;
            front_right_wheel_collider.motorTorque = 0;
        }

    }

    private void ApplySteer()
    {
        if (avoiding) return;
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode].position);
        float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;
        targetSteerAngle = newSteer;

        //front_left_wheel_collider.steerAngle = newSteer;
        //front_right_wheel_collider.steerAngle = newSteer;
    }

    private void Braking()
    {
        if (isBraking)
        {
            back_right_wheel_collider.brakeTorque = maxBrakeTorque;
            back_left_wheel_collider.brakeTorque = maxBrakeTorque;
        }
        else
        {
            back_right_wheel_collider.brakeTorque = 0;
            back_left_wheel_collider.brakeTorque = 0;
        }
    }
    private void LerpToSteerAngle()
    {
        front_left_wheel_collider.steerAngle = Mathf.Lerp(front_left_wheel_collider.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
        front_right_wheel_collider.steerAngle = Mathf.Lerp(front_right_wheel_collider.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
    }

    private void OnDrawGizmos()
    {

    }
}
