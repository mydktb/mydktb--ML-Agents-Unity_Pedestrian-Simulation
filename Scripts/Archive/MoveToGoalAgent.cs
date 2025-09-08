using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Material winMat;
    [SerializeField] private Material loseMat;
    [SerializeField] private MeshRenderer floorMeshRenderer;
    [SerializeField] public float moveSpeed;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-20f,20f), 0, Random.Range(-20f, 20f));
        targetTransform.localPosition = new Vector3(Random.Range(-20f, 20f), 0, Random.Range(-20f, 20f));
    }
    // the agent can observe its own position and the position of the target.
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        //Debug.Log(actions.DiscreteActions[0]);
        //Debug.Log(actions.ContinuousActions[0]);

        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        // if reached goal
        if (other.TryGetComponent<Goal>(out Goal goal))
        {
            SetReward(+1.5f);
            floorMeshRenderer.material = winMat;
            EndEpisode();
        }

        //if fall off platform
        if (other.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-1f);
            floorMeshRenderer.material = loseMat;
            EndEpisode();
        }


    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> conActions = actionsOut.ContinuousActions;
        conActions[0] = Input.GetAxisRaw("Horizontal");
        conActions[1] = Input.GetAxisRaw("Vertical");
    }
}
