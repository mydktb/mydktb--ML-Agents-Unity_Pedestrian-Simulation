using System;
using System.Collections.Generic;
using Rhino.Geometry;
using UnityEngine;

public class GroupBounds : MonoBehaviour
{
    public Bounds bounds;
    Renderer[] renderers;
    [HideInInspector] public bool agentsInRangeAndReachingTarget = false;
    [HideInInspector] public bool agentsInRange = false;
    [HideInInspector] public float volume;
    [HideInInspector] public Vector3 centreOfBounds;
    [HideInInspector] public Point3d centre;
    [SerializeField] int maxVol = 100;
    GameObject[] groupTargets;
    
    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void FixedUpdate()
    {
        groupTargets = GameObject.FindGameObjectsWithTag("m_GroupTarget");
        bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; ++i)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        var centre = bounds.center;
  
       CheckAgentsRange(out volume);
       agentsInRangeAndReachingTarget = false;

        for (int i = 0; i < groupTargets.Length; i++)
        {
            if (bounds.Contains(groupTargets[i].transform.position) && CheckAgentsRange())
            {
                agentsInRangeAndReachingTarget = true;
                break;
            }
        }
    }

    private bool CheckAgentsRange(out float volume)
    {
        volume = 0;
        agentsInRange = false;
        var x = new Interval(-bounds.extents.x, bounds.extents.x);
        var y = new Interval(-bounds.extents.y, bounds.extents.y);
        var z = new Interval(-bounds.extents.z, bounds.extents.z);
        var plane = new Rhino.Geometry.Plane(Rhino.Geometry.Plane.WorldXY);
        Box box = new Box(plane, x, y, z);
        volume = (float)box.Volume;

        if (volume <= maxVol)
        {
            agentsInRange = true;
        }

        return agentsInRange;
    }

    private bool CheckAgentsRange()
    {
        volume = 0;
        var x = new Interval(-bounds.extents.x, bounds.extents.x);
        var y = new Interval(-bounds.extents.y, bounds.extents.y);
        var z = new Interval(-bounds.extents.z, bounds.extents.z);
        var plane = new Rhino.Geometry.Plane(Rhino.Geometry.Plane.WorldXY);
        Box box = new Box(plane, x, y, z);

        volume = (float)box.Volume;
        if (volume <= maxVol) agentsInRange = true;
        else agentsInRange = false;
        return agentsInRange;
    }

    private void OnDrawGizmos()
    {
        if (agentsInRange)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawWireCube(bounds.center, bounds.size);

    }

    
}
