using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class NavigationAgent : Agent
{
    [SerializeField] bool useVectorObs;
    [SerializeField] bool useText;
    Rigidbody m_Agent;

    [Header("Environment Propeties")]
    [SerializeField] EnvMode envMode;
    [SerializeField] GameObject[] obstaclesSite;
    [SerializeField] GameObject[] maze;
    
    [SerializeField] GameObject target;
    [SerializeField] GameObject poison;
    [SerializeField] GameObject[] spawnAreas;

    [Range(0, 10f)]
    [SerializeField] int maxSpeed;
    [Range(1, 250)]
    [SerializeField] int maxRot;

    [Header("Rewards")]
    [SerializeField] float reachTarget = 1f;
    [SerializeField] float hitWall = -0.1f;
    [SerializeField] float hitPoison = -0.1f;
    [SerializeField] bool useInverseDistance;

    [Header("Environment Text")]
    [SerializeField] Text tx_Coordinates;
    [SerializeField] Text tx_AgentSpeed;
    [SerializeField] Text tx_DistanceFromTarget;
    [SerializeField] Text tx_CumulativeReward;
    [SerializeField] Text tx_TimeStep;
    [SerializeField] Text tx_EpisodesCompleted;

    enum EnvMode
    {
        None,
        Obstacles,
        Maze,
    }
    
    public override void Initialize()
    {
        m_Agent = GetComponent<Rigidbody>();
    }
    public void SetupEnvironment()
    {
        for (int i = 0; i < obstaclesSite.Length; i++)
        {           
            obstaclesSite[i].SetActive(false);
            maze[i].SetActive(false);
        }

        switch (envMode)
        {
            case EnvMode.None:
                break;
            case EnvMode.Obstacles:
                obstaclesSite[UnityEngine.Random.Range(0, obstaclesSite.Length)].SetActive(true);
                break;
            case EnvMode.Maze:
                maze[UnityEngine.Random.Range(0, maze.Length)].SetActive(true);
                break;
        }
    }

    public override void OnEpisodeBegin()
    {
        SetupEnvironment();
        PlaceObject(m_Agent.gameObject, spawnAreas[0]);
        PlaceObject(target, spawnAreas[1]);
        PlaceObject(poison, spawnAreas[1]);
        m_Agent.transform.LookAt(target.transform);
    }

     void MoveAgent(ActionSegment<float> actions)
    {
        float moveX = actions[0];
        float moveZ = actions[1];

        m_Agent.transform.position += new Vector3(moveX, 0, moveZ) * Time.deltaTime * 50f;
    }

     void MoveAgent(ActionSegment<int> actions, ActionSegment<float> actions2)
    {
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;

        int discreteAction_0 = actions[0]; // Stop/GoForward
        int discreteAction_1 = actions[1]; // Straight/Left/Right
        //int discreteAction_2 = actions[2]; // Speed
        //int discreteAction_3 = actions[3];
        float agentSpeed = Remap(-1f, 1f, 0, maxSpeed, actions2[0]);
        float rotSpeed = Remap(-1f, 1f, 0, maxRot, actions2[1]);
      

        switch (discreteAction_0)
        {
            case 0: dirToGo = Vector3.zero; break; // Stop
            case 1: dirToGo = transform.forward; break; // Go forward        
        }

        switch (discreteAction_1)
        {
            case 0: rotateDir = Vector3.zero; break; // No rotation
            case 1: rotateDir = transform.up; break; // Go right
            case 2: rotateDir = -transform.up; break; // Go left
        }

        m_Agent.transform.Rotate(rotateDir, Time.deltaTime * rotSpeed);
        m_Agent.AddForce(dirToGo * agentSpeed, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float DistanceToTarget = Vector3.Distance(m_Agent.transform.position, target.transform.position);
        MoveAgent(actions.DiscreteActions, actions.ContinuousActions);
        AddReward(-1f / MaxStep);
        if (useInverseDistance)
        {
            AddReward(1f / DistanceToTarget);
        }
        if (m_Agent.transform.position.y < 0)
        {
            AddReward(-1f);
            EndEpisode();
        }

        // User interface texts
        if (useText)
        {
            tx_Coordinates.text = "Coordinates: " + m_Agent.transform.position.ToString();
            tx_AgentSpeed.text = "Speed: " + Math.Round(m_Agent.velocity.magnitude, 2).ToString() + " m/s";
            tx_DistanceFromTarget.text = "Distance To Target: " + Math.Round(DistanceToTarget, 2).ToString() + " m";
            tx_CumulativeReward.text = "Culumilative Reward: " + GetCumulativeReward();                 
            tx_TimeStep.text = "Time Step: " + StepCount.ToString();
            tx_EpisodesCompleted.text = "Episodes Completed: " + CompletedEpisodes.ToString();       
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        var continuousActionsOut = actionsOut.ContinuousActions;

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
        if (Input.GetKey(KeyCode.Space))
        {
            continuousActionsOut[0] = 100f;
        }

        //continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");
        //continuousActionsOut[1] = Input.GetAxisRaw("Vertical");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObs)
        {
            sensor.AddObservation(m_Agent.transform.localPosition);
            sensor.AddObservation(target.transform.localPosition);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("m_obstacle"))
        {
            Debug.Log("hit a wall");
            AddReward(hitWall);
            //EndEpisode();
        }
        else if (collision.gameObject.CompareTag("m_agent"))
        {
            Debug.Log("hit an agent");
            AddReward(hitPoison);
        }
    }

    public void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("m_target"))
        {
            Debug.Log("Target Reached!");
            SetReward(reachTarget);
            EndEpisode();
        }

        if (other.gameObject.CompareTag("m_poison"))
        {
            Debug.Log(hitPoison);
            SetReward(-0.1f);
            EndEpisode();
        }
    }
    public void PlaceObject(GameObject objectToPlace, GameObject spawnArea)
    {
        var spawnTransform = spawnArea.transform;
        var xRange = spawnTransform.localScale.x / 2f;
        var zRange = spawnTransform.localScale.z / 2f;

        objectToPlace.transform.position = spawnArea.transform.position + 
            new Vector3(UnityEngine.Random.Range(-xRange, xRange), 1.5f, UnityEngine.Random.Range(-zRange, zRange));
    }

    public static float Remap(float min, float max, float Min, float Max, float x)
    {
        return Min + (Max - Min) * ((x - min) / (max - min));
    }

    public static int Remap(int min, int max, int Min, int Max, int x)
    {
        return Min + (Max - Min) * ((x - min) / (max - min));
    }
}
