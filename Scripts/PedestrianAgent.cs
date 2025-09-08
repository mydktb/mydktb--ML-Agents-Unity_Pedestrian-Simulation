using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using Random = UnityEngine.Random;
using TMPro;

public class PedestrianAgent : Agent
{
    Rigidbody m_Agent;
    PedestrianTrainingArea m_TrainingArea;

    [SerializeField] bool _useVectorObs = true;
    [SerializeField] bool _useBackTracking = false;

    [Header("Agent's Properties")]
    [SerializeField] public AgentType agentType;
    [Range(1f, 25f)] [SerializeField] float maxSpeed = 5f;
    [Range(100, 500)] [SerializeField] float maxRotation = 150f;
    [SerializeField] GameObject spawnArea;

    float score;

    [Header("Rewards")]
    [SerializeField] bool existentialReward = true;
    [SerializeField] float reachTargetReward = 1f;
    [SerializeField] float hitWrongTargetReward = -0.1f;
    [SerializeField] float hitWallReward = -1f;
    [SerializeField] float hitAgentReward = -0.1f;
    float backTrackingReward;


    [Header("Environment Text")]
    [SerializeField] bool useText;
    [SerializeField] Text tx_agentname; 
    [SerializeField] Text tx_Coordinates;
    [SerializeField] Text tx_AgentSpeed;
    [SerializeField] Text tx_CumulativeReward;
    [SerializeField] Text tx_TimeStep;
    [SerializeField] Text tx_EpisodesCompleted;
    [SerializeField] Text tx_hitObstacle;
    [SerializeField] Text tx_reachedTarget;

    float _hitObstacle;
    float _reachedTarget;

    [HideInInspector] public GridManager m_GridMananger;

    //[SerializeField] TMP_Text agentName;

    //CameraSensor CS;

    public enum AgentType
    {
        Test, // 0
        Blue, // 1
        Yellow, // 2
        Red, // 3
    }


    public override void Initialize()
    {
        m_Agent = GetComponent<Rigidbody>();
        m_TrainingArea = this.transform.parent.gameObject.GetComponent<PedestrianTrainingArea>();
        m_GridMananger = FindObjectOfType<GridManager>();
        _hitObstacle = 0;
        _reachedTarget = 0;
        // agentName.text = this.name.ToString();
        //CS = GetComponent<CameraSensor>();

    }

    public override void OnEpisodeBegin()
    {
        SpawnAgent(m_Agent.gameObject, spawnArea);
        if (((int)agentType) == 1)
        {
            foreach (var i in m_TrainingArea.m_Tiles) 
                i.ResetTiles("m_BellowVisitedTile");
        }
        if (((int)agentType) == 2)
        {
            foreach (var i in m_TrainingArea.m_Tiles) 
                i.ResetTiles("m_YellowVisitedTile");
        }
        if (((int)agentType) == 3)
        {
            foreach (var i in m_TrainingArea.m_Tiles) 
                i.ResetTiles("m_RellowVisitedTile");
        }


    }

    public override void CollectObservations(VectorSensor sensor)
    {

        if (_useVectorObs)
        {
            sensor.AddObservation(transform.InverseTransformVector(m_Agent.velocity)); // 3
            sensor.AddObservation(transform.forward); // 6
        }

    }

    void MoveAgent(ActionSegment<int> Discrete, ActionSegment<float> Continuous)
    {
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;

        int firstBranch = Discrete[0]; // Stop/GoForward
        int secondBranch = Discrete[1]; // Straight/Left/Right
        float speed = Remap(-1f, 1f, 1f, maxSpeed, Continuous[0]);


        switch (firstBranch)
        {
            case 0: dirToGo = Vector3.zero; break; // Stop
            case 1: dirToGo = transform.forward; break; // Go forward        
        }

        switch (secondBranch)
        {
            case 0: rotateDir = Vector3.zero; break; // No rotation
            case 1: rotateDir = transform.up; break; // Go right
            case 2: rotateDir = -transform.up; break; // Go left
        }

        m_Agent.transform.Rotate(rotateDir, Time.deltaTime * maxRotation);
        m_Agent.AddForce(dirToGo * speed, ForceMode.VelocityChange);
    }

    void MoveAgent(ActionSegment<int> Discrete)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var forwardAxis = Discrete[0];
        var rightAxis = Discrete[1];
        var rotateAxis = Discrete[2];
        var speed = Discrete[3];

        switch (forwardAxis)
        {

            case 1:
                dirToGo = transform.forward;
                break;
            case 2:
                dirToGo = -transform.forward * 0.5f;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right * 0.5f;
                break;
            case 2:
                dirToGo = transform.right * 0.5f;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }


        transform.Rotate(rotateDir, Time.deltaTime * 200f);
        m_Agent.AddForce(dirToGo * speed, ForceMode.VelocityChange);

    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        MoveAgent(actions.DiscreteActions, actions.ContinuousActions); // 2D 1C
        //var _visited_before = m_GridMananger.HasVisited(m_Agent.transform.position, ((int)agentType));
        //if (!_visited_before)
        //{
        //    m_GridMananger.Visit(m_Agent.transform.position, (int)agentType);
        //}

        if (existentialReward)
        {
            AddReward(-1f / MaxStep);
        }

        // User interface texts
        if (useText)
        {
            tx_agentname.text = "Agent Type: " + this.gameObject.name;
            tx_Coordinates.text = "Coordinates: " + m_Agent.transform.position.ToString();
            tx_AgentSpeed.text = "Speed: " + Math.Round(m_Agent.velocity.magnitude, 2).ToString() + " m/s";
            tx_CumulativeReward.text = "Culumilative Reward: " + GetCumulativeReward();
            tx_TimeStep.text = "Time Step: " + StepCount.ToString();
            tx_EpisodesCompleted.text = "Episodes Completed: " + CompletedEpisodes.ToString();
            tx_hitObstacle.text = "Obstacle Hit: " + _hitObstacle.ToString() + " times";
            tx_reachedTarget.text = "Target Reached: " + _reachedTarget.ToString() + " times";
        }
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        var continuousActionsOut = actionsOut.ContinuousActions;
        //RayCastInfo(m_rayPerceptionSensorComponent3D);

        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[1] = 2;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("m_obstacle"))
        {
            Debug.Log("WallHit");
            AddReward(hitWallReward);
            _hitObstacle++;
            EndEpisode();
        }

        if (collision.gameObject.CompareTag("m_agent") || collision.gameObject.CompareTag("m_orangeAgent"))
        {
            Debug.Log("AgentHit");
            AddReward(hitAgentReward);
        }

        switch (agentType)
        {
            case AgentType.Blue:

                if (_useBackTracking)
                {
                    if (collision.gameObject.CompareTag("m_BellowVisitedTile")) AddReward(backTrackingReward);
            
                }
                if (collision.gameObject.CompareTag("m_target"))
                {
                    Destroy(collision.gameObject);
                    Debug.Log("Blue Target Reached!");
                    AddReward(reachTargetReward);
                }
                else if (collision.gameObject.CompareTag("m_YellowTarget") || collision.gameObject.CompareTag("m_RedTarget") || collision.gameObject.CompareTag("m_GroupTarget"))
                {
                    Debug.Log("HitWorngTarget");
                    AddReward(hitWrongTargetReward);
                }
                break;

            case AgentType.Yellow:

                if (_useBackTracking)
                {
                    if (collision.gameObject.CompareTag("m_YellowVisitedTile")) AddReward(backTrackingReward);

                }
                if (collision.gameObject.CompareTag("m_YellowTarget"))
                {
                    Destroy(collision.gameObject);
                    Debug.Log("Yellow Target Reached!");
                    _reachedTarget++;
                    AddReward(reachTargetReward);
                }
                else if (collision.gameObject.CompareTag("m_target") || collision.gameObject.CompareTag("m_RedTarget") || collision.gameObject.CompareTag("m_GroupTarget"))
                {
                    Debug.Log("HitWorngTarget");
                    _hitObstacle++;
                    AddReward(hitWrongTargetReward);
                }
                break;

            case AgentType.Red:

                if (_useBackTracking)
                {
                    if (collision.gameObject.CompareTag("m_RellowVisitedTile")) AddReward(backTrackingReward);

                }
                if (collision.gameObject.CompareTag("m_RedTarget"))
                {
                    Destroy(collision.gameObject);
                    Debug.Log("Red Target Reached!");
                    AddReward(reachTargetReward);
                }
                else if (collision.gameObject.CompareTag("m_target") || collision.gameObject.CompareTag("m_YellowTarget") || collision.gameObject.CompareTag("m_GroupTarget"))
                {
                    Debug.Log("HitWorngTarget");
                    AddReward(hitWrongTargetReward);
                }

                break;
        }
    }

    private void OnTriggerStay(Collider other)

    {
        backTrackingReward = (-1f / MaxStep) * 2;

        if (_useBackTracking)
        {
            switch (agentType)
            {
                case AgentType.Blue:
                    if (other.gameObject.CompareTag("m_BellowVisitedTile"))
                    {
                        AddReward(backTrackingReward);
                        Debug.Log("Blue is Backtracking");
                    }
                    break;

                case AgentType.Yellow:
                    if (other.gameObject.CompareTag("m_YellowVisitedTile"))
                    {
                        AddReward(backTrackingReward);
                        Debug.Log("Yellow is Backtracking");
                    }
                    break;

                case AgentType.Red:
                    if (other.gameObject.CompareTag("m_RellowVisitedTile"))
                    {
                        AddReward(backTrackingReward);
                        Debug.Log("Red is Backtracking");
                    }
                    break;
            }
        }
        
    }

    public float Remap(float min, float max, float Min, float Max, float x)
    {
        var v = Min + (Max - Min) * ((x - min) / (max - min));
        return v;
    }

    public void SpawnAgent(GameObject objectToPlace, GameObject spawnArea)
    {
        var spawnTransform = spawnArea.transform;
        var xRange = spawnTransform.localScale.x / 2f;
        var zRange = spawnTransform.localScale.z / 2f;

        objectToPlace.transform.position = spawnArea.transform.position +
            new Vector3(Random.Range(-xRange, xRange), 0.92f, Random.Range(-zRange, zRange));
    }

    private void RayCastInfo(RayPerceptionSensorComponent3D rayComponent)
    {
        var rayOutputs = RayPerceptionSensor
                .Perceive(rayComponent.GetRayPerceptionInput())
                .RayOutputs;

        if (rayOutputs != null)
        {
            var lengthOfRayOutputs = RayPerceptionSensor
                    .Perceive(rayComponent.GetRayPerceptionInput())
                    .RayOutputs
                    .Length;

            for (int i = 0; i < lengthOfRayOutputs; i++)
            {
                GameObject goHit = rayOutputs[i].HitGameObject;
                if (goHit != null)
                {
                    // Found some of this code to Denormalized length
                    // calculation by looking trough the source code:
                    // RayPerceptionSensor.cs in Unity Github. (version 2.2.1)
                    var rayDirection = rayOutputs[i].EndPositionWorld - rayOutputs[i].StartPositionWorld;
                    var scaledRayLength = rayDirection.magnitude;
                    float rayHitDistance = rayOutputs[i].HitFraction * scaledRayLength;

                    // Print info:
                    string dispStr;
                    dispStr = "__RayPerceptionSensor - HitInfo__:\r\n";
                    dispStr = dispStr + "GameObject name: " + goHit.name + "\r\n";
                    dispStr = dispStr + "GameObject tag: " + goHit.tag + "\r\n";
                    dispStr = dispStr + "Hit distance of Ray: " + rayHitDistance + "\r\n";
                    Debug.Log(dispStr);
                }
            }
        }
    }


}
