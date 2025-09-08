using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class FindTarget : Agent
{
    private Rigidbody m_Agent;
    public GameObject target;
    public GameObject poison;
    public GameObject startPoint;
    public GameObject endPoint;

    public GameObject spawnAreaA;
    public GameObject spawnAreaB;

    private Bounds spawnAreaBoundsA;
    private Bounds spawnAreaBoundsB;


    //private float agentSpeed = 1f;
    private float agentRotation = 200f;
    public bool useVectorObs;


    [Header("Environment Info")]
    [SerializeField] Text tx_Coordinates;
    [SerializeField] Text tx_CumulativeReward;
    [SerializeField] Text tx_DistanceFromTarget;
    [SerializeField] Text tx_TimeStep;
    [SerializeField] Text tx_EpisodesCompleted;

    public override void Initialize()
    {
        m_Agent = GetComponent<Rigidbody>();
        spawnAreaBoundsA = spawnAreaA.GetComponent<Collider>().bounds;
        spawnAreaBoundsB = spawnAreaB.GetComponent<Collider>().bounds;
        spawnAreaA.SetActive(false);
        spawnAreaB.SetActive(false);
 
    }

    public override void OnEpisodeBegin()
    {
        m_Agent.transform.position = GetRandomPosition(spawnAreaBoundsA, spawnAreaA);
        target.transform.position = GetRandomPosition(spawnAreaBoundsB, spawnAreaB);
        startPoint.transform.position = GetRandomPosition(spawnAreaBoundsA, spawnAreaA);
        endPoint.transform.position = GetRandomPosition(spawnAreaBoundsB, spawnAreaB);
        poison.transform.position = GetRandomPosition(spawnAreaBoundsA, spawnAreaA);
       
    }

    private Vector3 GetRandomPosition(Bounds bounds, GameObject area)
    {
        var randomPosX = UnityEngine.Random.Range(-bounds.extents.x, bounds.extents.x);
        var randomPosZ = UnityEngine.Random.Range(-bounds.extents.z, bounds.extents.z);
        var randomPos = area.transform.position + new Vector3(randomPosX, 0.0f, randomPosZ);
        return randomPos;
    }
    /*
    public void MoveAgent(ActionSegment<int> discreteActions)
    {
        int moveX = discreteActions
        int moveZ = discreteActions.ContinuousActions[1];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;

    }
    */


    public void MoveAgent(ActionSegment<int> DAct, ActionSegment<float> CAct)
    {     
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;

        int dAct_0 = DAct[0];
        int dAct_1 = DAct[1];
        float cAct_0 = Remap(-1f, 1f, 0f, 40f, CAct[0]);
        //float cAct_1 = Remap(-1f, 1f, 50f, 300f, CAct[1]);
    
        switch (dAct_0)
        {
            case 0: dirToGo = Vector3.zero; break; // Stop
            case 1: dirToGo = transform.forward; break; // Go forward        
            //case 2: dirToGo = -transform.forward; break; // Go backward
        }

        switch (dAct_1)
        {
            case 1: rotateDir = transform.up; break; // Go right
            case 2: rotateDir = -transform.up; break; // Go left
        }

        transform.Rotate(rotateDir, Time.deltaTime * agentRotation);
        m_Agent.AddForce(dirToGo * cAct_0, ForceMode.VelocityChange);
        //Debug.Log(cAct_0.ToString());
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float DistanceToTarget = Vector3.Distance(m_Agent.transform.position, target.transform.position);
        MoveAgent(actions.DiscreteActions, actions.ContinuousActions);
        AddReward(-1f / MaxStep);
        if (m_Agent.position.y < 3f)
        {
            SetReward(-1f);
            EndEpisode();
        }

        // User interface texts
        tx_Coordinates.text = "Coordinates: " + m_Agent.transform.position.ToString();
        tx_CumulativeReward.text = "Culumilative Reward: " + GetCumulativeReward();
        tx_DistanceFromTarget.text = "Distance From Target: " + DistanceToTarget.ToString();
        tx_TimeStep.text = "Time Step: " + StepCount.ToString();
        tx_EpisodesCompleted.text = "Episodes Completed: " + CompletedEpisodes.ToString();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        var continuousActionsOut = actionsOut.ContinuousActions;
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[1] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[1] = 2;
        }
        //else if (Input.GetKey(KeyCode.S))
        //{
        //    discreteActionsOut[0] = 2;
        //}
        else if (Input.GetKey(KeyCode.Space))
        {
            continuousActionsOut[0] = 100f;
        }

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVectorObs)
        {
            sensor.AddObservation(m_Agent.transform.localPosition);
            sensor.AddObservation(target.transform.localPosition);
            sensor.AddObservation(poison.transform.localPosition);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("m_obstacle"))
        {
            Debug.Log("hit a wall");
            AddReward(-0.1f);
            EndEpisode();
        }

    }

    public void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("m_target"))
        {
            Debug.Log("Target Reached!");
            SetReward(+5f);
            EndEpisode();
        }

        if (other.gameObject.CompareTag("m_poison"))
        {
            Debug.Log("hit posion");
            SetReward(-3f);
            EndEpisode();
        }

        if (other.gameObject.CompareTag("m_transformer"))
        {
            Debug.Log("teleported");
            m_Agent.transform.position = endPoint.transform.position;
            //SetReward(+1f);
        }

        //if (other.gameObject.CompareTag("m_agent"))
        //{
        //    Debug.Log("hit agnet");
        //    SetReward(-2f);
        //}

    }

    public static float Remap(float min, float max, float Min, float Max, float x)
    {
        return Min + (Max - Min) * ((x - min) / (max - min));
    }
}

