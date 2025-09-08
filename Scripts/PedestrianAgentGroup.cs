using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class PedestrianAgentGroup : Agent
{
    Rigidbody m_Agent;
    PedestrianTrainingArea m_PedestrianTrainingArea;
    GroupBounds m_GroupBounds;

    public enum Group
    {
        orange = 0,
    }

    [HideInInspector] public Group group;
    [SerializeField] bool _useVectorObs = true;
    [SerializeField] GameObject spawnArea;
    [Range(1f, 25f)][SerializeField] float maxSpeed = 2f;

    [Header("Individual Rewards")]
    [SerializeField] bool useIndividualRewards = false;
    [SerializeField] bool _useExistentialReward = false;
    [HideInInspector] float groupedReward;
    [SerializeField] float reachTargetReward = +1f;
    [SerializeField] float hitWrongTargetReward = -0.1f;
    [SerializeField] float hitWallReward = -1f;
    [SerializeField] float hitAgentReward = -0.1f;
    List<GameObject> _GroupAgents;

    [Header("Environment Text")]
    [SerializeField] bool useText;
    [SerializeField] Text tx_agentname;
    [SerializeField] Text tx_Coordinates;
    [SerializeField] Text tx_AgentSpeed;
    [SerializeField] Text tx_CumulativeReward;
    [SerializeField] Text tx_TimeStep;
    [SerializeField] Text tx_EpisodesCompleted;

    Vector3 boundsCentre;
    Vector3 vecToCentre;
    float vecMag;
    public bool _inRange = false;
    public bool _inRangeAndTarget = false;



    public override void Initialize()
    {
        m_Agent = GetComponent<Rigidbody>();
        m_GroupBounds = GameObject.Find("0-OGroupAgents").GetComponent<GroupBounds>();
        GetGroupList();
        for (int i = 0; i < _GroupAgents.Count; i++) 
            _GroupAgents[i].GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        SpawnAgent(m_Agent.gameObject, spawnArea);

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

        m_Agent.transform.Rotate(rotateDir, Time.deltaTime * 200f);
        m_Agent.AddForce(dirToGo * speed, ForceMode.VelocityChange);
    }

    void MoveAgent(ActionSegment<int> Discrete)
    {
        Vector3 dirToGo = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;

        int firstBranch = Discrete[0]; // Stop/GoForward
        int secondBranch = Discrete[1]; // Straight/Left/Right

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


        m_Agent.transform.Rotate(rotateDir, Time.deltaTime * 200f);
        m_Agent.AddForce(dirToGo * 2f, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        _inRangeAndTarget = false;
        //MoveAgent(actions.DiscreteActions);
        MoveAgent(actions.DiscreteActions, actions.ContinuousActions);
        GroupingReward();
        if (_useExistentialReward)
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
        }


        Debug.Log(_inRangeAndTarget);
    }

    void GroupingReward()
    {
        var boundsCentre = m_GroupBounds.bounds.center;
        var vecToCentre = boundsCentre - transform.position;
        var vecMag = vecToCentre.magnitude;;

        if (vecMag < 2.5f)
        {
            _inRange = true;
            AddReward(1f / MaxStep);
            Debug.DrawLine(transform.position, boundsCentre, Color.green);
            Debug.Log("In Rage: " + _inRange);
        }
        else
        {        
            _inRange = false;
            Debug.DrawLine(transform.position, boundsCentre, Color.red);
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        if (_useVectorObs)
        {
            //sensor.AddObservation(m_GroupBounds.agentsInRange);
            //sensor.AddObservation(m_GroupBounds.agentsInRangeAndReachingTarget);
            sensor.AddObservation(vecToCentre);
            sensor.AddObservation(transform.forward);
            sensor.AddObservation(_inRange);
            sensor.AddObservation(_inRangeAndTarget);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

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

    public void OnCollisionEnter(Collision collision)
    {
        if (useIndividualRewards)
        {
            if (collision.gameObject.CompareTag("m_obstacle"))
            {
                Debug.Log("WallHit");
                AddReward(hitWallReward);
                EndEpisode();
            }

            if (collision.gameObject.CompareTag("m_agent") || collision.gameObject.CompareTag("m_orangeAgent"))
            {
                Debug.Log("AgentHit");
                AddReward(hitAgentReward);
            }

            if (collision.gameObject.CompareTag("m_YellowTarget") || collision.gameObject.CompareTag("m_RedTarget") || collision.gameObject.CompareTag("m_target"))
            {
                Debug.Log("HitWorngTarget");
                AddReward(hitWrongTargetReward);
            }

            if (collision.gameObject.CompareTag("m_GroupTarget") && _inRange)
            {
                _inRangeAndTarget = true;
                Destroy(collision.gameObject);
                Debug.Log("_inRangeAndTarget: " + _inRangeAndTarget);
                AddReward(reachTargetReward);
            }
            //else if (collision.gameObject.CompareTag("m_GroupTarget"))
            //{
            //    Destroy(collision.gameObject);
            //}
 
        }
    }

    private void SpawnAgent(GameObject objectToPlace, GameObject spawnArea)
    {
        var spawnTransform = spawnArea.transform;
        var xRange = spawnTransform.localScale.x / 2f;
        var zRange = spawnTransform.localScale.z / 2f;

        objectToPlace.transform.position = spawnArea.transform.position +
            new Vector3(Random.Range(-xRange, xRange), 1.5f, Random.Range(-zRange, zRange));
    }

    private void GetGroupList()
    {
        _GroupAgents = new List<GameObject>();

        foreach (Transform child in m_GroupBounds.gameObject.transform)
        {
            if (child.tag == "m_orangeAgent")
            {
                _GroupAgents.Add(child.gameObject);
            }
        }
    }

    public float Remap(float min, float max, float Min, float Max, float x)
    {
        var v = Min + (Max - Min) * ((x - min) / (max - min));
        return v;
    }
}
