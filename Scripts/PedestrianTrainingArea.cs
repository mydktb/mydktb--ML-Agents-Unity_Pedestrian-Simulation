using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Random = UnityEngine.Random;
using Unity.MLAgentsExamples;


public class PedestrianTrainingArea : Area
{
    [SerializeField] private int Episode_Length = 2500;
    int Episode_Timer = 0;

    [System.Serializable]
    public class GroupAgentsInfo
    {
        public PedestrianAgentGroup Agent;
        [HideInInspector] public Rigidbody Rb;
    }

    [SerializeField] PedestrianAgent[] m_Agent;
    [SerializeField] List<GroupAgentsInfo> m_GroupAgents = new List<GroupAgentsInfo>();
    [SerializeField] bool _useGroupAgents = true;
    [SerializeField] bool _existentialGroupReward = true;
    [SerializeField] bool _respawnTargets = false;
    [SerializeField] TrainingMode trainingMode;
    [SerializeField] GameObject walls;
    [SerializeField] GameObject[] A_SimpleObstacles;
    [SerializeField] GameObject[] A_MazeA;
    [SerializeField] GameObject[] A_MazeB;
    [SerializeField] GameObject[] A_IndoorA;
    [SerializeField] GameObject[] A_IndoorB;
    [SerializeField] GameObject[] A_Rooms;
    [SerializeField] GameObject[] A_Urban;
    public GameObject[] spawnAreas;
    [SerializeField] int blueTargetNum;
    [SerializeField] int yellowTargetNum;
    [SerializeField] int redTargetNum;
    [SerializeField] int grouTargetNum;

    [SerializeField] GameObject blueTarget;
    [SerializeField] GameObject yelloTarget;
    [SerializeField] GameObject redTarget;
    [SerializeField] GameObject groupTarget;

    SimpleMultiAgentGroup m_MultiAgentGroup;
    GroupBounds m_GroupBounds;

    [HideInInspector] public List<GameObject> blueList = new List<GameObject>();
    [HideInInspector] public List<GameObject> yellowList = new List<GameObject>();
    [HideInInspector] public List<GameObject> redList = new List<GameObject>();
    [HideInInspector] public List<GameObject> orangeList = new List<GameObject>();
    [HideInInspector] public List<Tile> m_Tiles = new List<Tile>();

    [HideInInspector] public GridManager m_GridMananger;

    enum TrainingMode
    {
        Empty,
        SimpleObstacles,
        MazeA,
        MazeB,
        IndoorA,
        IndoorB,
        Room0,
        Room1,
        Room2,
        Room3,
        Room4,
        Room5,
        Room6,
        Room7,
        Room8,
        Room9

    }
    private void Awake()
    {
        StartCoroutine(LateStart());
        m_MultiAgentGroup = new SimpleMultiAgentGroup();
        if (_useGroupAgents)
        {
            m_GroupBounds = GameObject.Find("0-OGroupAgents").GetComponent<GroupBounds>();
        }
        if (m_GroupAgents != null)
        {
            foreach (var ag in m_GroupAgents)
            {
                ag.Rb = ag.Agent.GetComponent<Rigidbody>();

                if (ag.Agent.group == PedestrianAgentGroup.Group.orange)
                {
                    m_MultiAgentGroup.RegisterAgent(ag.Agent);
                }
            }
        }
        m_GridMananger = FindObjectOfType<GridManager>();
        ResetTrainingEnv();
    }
    IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        GetTilesLists();
    }
    private void FixedUpdate()
    {
        Episode_Timer++;
        GetTargetLists();       
        if (_existentialGroupReward) m_MultiAgentGroup.AddGroupReward(-1f / Episode_Length);
        if (m_Agent != null) IndivdualAgentLogic();
        if (m_GroupAgents != null) GroupAgentsLogic();
        if (_respawnTargets)
        {
            if (blueList.Count == 0) SpawnTargets(blueTarget, spawnAreas[0], blueTargetNum);
            if (yellowList.Count == 0) SpawnTargets(yelloTarget, spawnAreas[1], yellowTargetNum);
            if (redList.Count == 0) SpawnTargets(redTarget, spawnAreas[2], redTargetNum);
            if (orangeList.Count == 0) SpawnTargets(groupTarget, spawnAreas[3], grouTargetNum);
        }
        CheckTargetY();
    }
    public void ResetTrainingEnv()
    {   
        Episode_Timer = 0;

        ClearObjects(blueList);
        ClearObjects(redList);
        ClearObjects(yellowList);
        ClearObjects(orangeList);

        SetupEnvironment();

        SpawnTargets(blueTarget, spawnAreas[0], blueTargetNum);     
        SpawnTargets(yelloTarget, spawnAreas[1], yellowTargetNum);     
        SpawnTargets(redTarget, spawnAreas[2], redTargetNum);     
        SpawnTargets(groupTarget, spawnAreas[3], grouTargetNum);
    }
    private void IndivdualAgentLogic()
    {
        if (m_Agent != null)
        {
            if (Episode_Timer >= Episode_Length && Episode_Length > 0)
            {
                for (int i = 0; i < m_Agent.Length; i++)
                {

                    m_Agent[i].EndEpisode();
                }

                ResetTrainingEnv();
            }

            for (int i = 0; i < m_Agent.Length; i++)
            {
                if (m_Agent[i].transform.position.y < -1f)
                {
                    // Agent fell off platform
                    m_Agent[i].SetReward(-1f);
                    m_Agent[i].EndEpisode();
                    ResetTrainingEnv();
                }

                switch (m_Agent[i].agentType)
                {
                    case PedestrianAgent.AgentType.Blue:
                        if (blueList.Count == 0)
                        {
                            m_Agent[i].EndEpisode();
                            SpawnTargets(blueTarget, spawnAreas[0], blueTargetNum);
                        }
                        break;
                    case PedestrianAgent.AgentType.Yellow:
                        if (yellowList.Count == 0)
                        {
                            m_Agent[i].EndEpisode();
                            SpawnTargets(yelloTarget, spawnAreas[1], yellowTargetNum);
                        }
                        break;
                    case PedestrianAgent.AgentType.Red:
                        if (redList.Count == 0)
                        {
                            m_Agent[i].EndEpisode();
                            SpawnTargets(redTarget, spawnAreas[2], redTargetNum);
                        }
                        break;
                }
            }
        }
    }
    private void GroupAgentsLogic()
    {
        if (_useGroupAgents)
        {
            //for (int i = 0; i < m_GroupAgents.Count; i++)
            //{
            //    //if (m_GroupAgents[i].Agent._inRange)
            //    //{
            //    //    Debug.Log("It's true working fine");
            //    //    m_MultiAgentGroup.AddGroupReward(+1 / Episode_Length);
            //    //}
            //    if (m_GroupAgents[i].Agent._inRangeAndTarget)
            //    {
            //        Debug.Log("It's true working fine2222");
            //        m_MultiAgentGroup.AddGroupReward(+1f);
            //    }
            //}
            if (m_GroupAgents != null)
            {
                if (Episode_Timer >= Episode_Length && Episode_Length > 0)
                {
                    m_MultiAgentGroup.GroupEpisodeInterrupted();
                    ResetTrainingEnv();
                }

                if (m_GroupBounds.agentsInRange)
                {
                    m_MultiAgentGroup.AddGroupReward(+1 / Episode_Length);
                    Debug.Log("Group are in range");
                }

                if (m_GroupBounds.agentsInRangeAndReachingTarget)
                {
                    m_MultiAgentGroup.AddGroupReward(+1f);
                    m_MultiAgentGroup.EndGroupEpisode();
                    Debug.Log("Group reached target");
                    ResetTrainingEnv();
                }
            }
        }       
    }
    private void SetupEnvironment()
    {
        walls.SetActive(false);
        for (int i = 0; i < A_SimpleObstacles.Length; i++) A_SimpleObstacles[i].SetActive(false);
        for (int i = 0; i < A_MazeA.Length; i++) A_MazeA[i].SetActive(false);
        for (int i = 0; i < A_MazeB.Length; i++) A_MazeB[i].SetActive(false);
        for (int i = 0; i < A_IndoorA.Length; i++) A_IndoorA[i].SetActive(false);
        for (int i = 0; i < A_IndoorB.Length; i++) A_IndoorB[i].SetActive(false);
        for (int i = 0; i < A_Rooms.Length; i++) A_Rooms[i].SetActive(false);

        switch (trainingMode)
        {
            case TrainingMode.Empty:
                walls.SetActive(true);
                break;
            case TrainingMode.SimpleObstacles:
                A_SimpleObstacles[Random.Range(0, A_SimpleObstacles.Length)].SetActive(true);
                break;
            case TrainingMode.MazeA:
                A_MazeA[Random.Range(0, A_MazeA.Length)].SetActive(true);
                break;
            case TrainingMode.MazeB:
                A_MazeB[Random.Range(0, A_MazeB.Length)].SetActive(true);
                break;
            case TrainingMode.IndoorA:
                A_IndoorA[Random.Range(0, A_IndoorA.Length)].SetActive(true);
                break;
            case TrainingMode.IndoorB:
                A_IndoorB[Random.Range(0, A_IndoorB.Length)].SetActive(true);
                break;
            case TrainingMode.Room0:
                A_Rooms[0].SetActive(true);
                break;
            case TrainingMode.Room1:
                A_Rooms[1].SetActive(true);
                break;
            case TrainingMode.Room2:
                A_Rooms[2].SetActive(true);
                break;
            case TrainingMode.Room3:
                A_Rooms[3].SetActive(true);
                break;
            case TrainingMode.Room4:
                A_Rooms[4].SetActive(true);
                break;
            case TrainingMode.Room5:
                A_Rooms[5].SetActive(true);
                break;
            case TrainingMode.Room6:
                A_Rooms[6].SetActive(true);
                break;
            case TrainingMode.Room7:
                A_Rooms[7].SetActive(true);
                break;
            case TrainingMode.Room8:
                A_Rooms[8].SetActive(true);
                break;
            case TrainingMode.Room9:
                A_Rooms[9].SetActive(true);
                break;
        }
    }
    private void SpawnTargets(GameObject objectToSpawn, GameObject spawnArea, int targetsNum)
    {
        var xRange = spawnArea.transform.localScale.x / 2f;
        var zRange = spawnArea.transform.localScale.z / 2f;

        for (int i = 0; i < targetsNum; i++)
        {
            var newObjcect = Instantiate(objectToSpawn, spawnArea.transform.parent);
                newObjcect.transform.localPosition = spawnArea.transform.localPosition + new Vector3(Random.Range(-xRange, xRange), 4f, Random.Range(-zRange, zRange));   
        }
    }
    private void ClearObjects(List<GameObject> objects)
    {
        foreach (var f in objects)
        {
            Destroy(f);
        }
    }
    private void GetTilesLists()
    {
        m_Tiles = new List<Tile>();
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            m_Tiles.Add(tile);
        }
    }
    private void GetTargetLists()
    {
        blueList = new List<GameObject>();
        yellowList = new List<GameObject>();
        redList = new List<GameObject>();
        orangeList = new List<GameObject>();

        foreach (Transform child in this.transform)
        {
            if (child.tag == "m_YellowTarget")
            {
                yellowList.Add(child.gameObject);
            }
            else if (child.tag == "m_RedTarget")
            {
                redList.Add(child.gameObject);
            }
            else if (child.tag == "m_target")
            {
                blueList.Add(child.gameObject);
            }
            else if (child.tag == "m_GroupTarget")
            {
                orangeList.Add(child.gameObject);
            }
        }
    }
    private void CheckTargetY()
    {
        for (int i = 0; i < blueList.Count; i++)
        {
            if (blueList[i].transform.position.y < -1f)
                Destroy(blueList[i]);
        }

        for (int i = 0; i < yellowList.Count; i++)
        {
            if (yellowList[i].transform.position.y < -1f)
                Destroy(yellowList[i]);
        }

        for (int i = 0; i < redList.Count; i++)
        {
            if (redList[i].transform.position.y < -1f)
                Destroy(redList[i]);
        }

        for (int i = 0; i < orangeList.Count; i++)
        {
            if (orangeList[i].transform.position.y < -1f)
                Destroy(orangeList[i]);
        }
    }
    public override void ResetArea()
    {
    }
}

