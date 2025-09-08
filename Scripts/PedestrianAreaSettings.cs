using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class PedestrianAreaSettings : MonoBehaviour
{

    [HideInInspector] public PedestrianTrainingArea[] m_TrainingAreas;
    [HideInInspector] public GridManager[] m_GridManangers;
    [SerializeField] bool _useGrid = true;
    private void Start()
    {
        m_TrainingAreas = FindObjectsOfType<PedestrianTrainingArea>();
        m_GridManangers = FindObjectsOfType<GridManager>();
        Academy.Instance.OnEnvironmentReset += EnvironmentResetSettings;
    }
    private void EnvironmentResetSettings()
    {

        foreach (var i in m_TrainingAreas)
        {

            i.ResetTrainingEnv();
        }
        if (_useGrid)
        {
            foreach (var i in m_GridManangers)
            {
                i.GenerateGrid();
            }
        }
  
    }
}
