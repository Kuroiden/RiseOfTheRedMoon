using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class NPCBehavior : MonoBehaviourPunCallbacks, IPunObservable
{
    private NPC_State npcState;
    private GameManager gameManager;

    [Header("AI Properties")]
    NavMeshAgent NPC_Nav;
    [SerializeField] private bool isAlly;
    [SerializeField] private GameObject AllyPlayer;
    private float DistanceFromPlayer;

    [Tooltip("Limit of how far AI could be before entering Follow state")]
    [SerializeField] private float MaxPlayerDistance;

    [Tooltip("Limit of how far any entity should be for AI to exit Patrol state")]
    [SerializeField] private float DetectionLimit;

    [Tooltip("Limit of any player to AI distance before seeking closest entity regardless of priority")]
    [SerializeField] private float PlayerAIRange;

    [Tooltip("Limit of how close any entity should be for AI to enter Attack state")]
    [SerializeField] private float AttackRange;

    [Header("Entity Tracking")]
    public List<GameObject> AllPlayers;
    public List<GameObject> AllNPCs;

    private GameObject closestPlayer;
    private GameObject closestAI;

    private float closestPlayer_Distance = 0f;
    private float closestAI_Distance = 0f;

    [Header("Photon PUN Variables")]
    private Vector3 net_Pos;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        Debug.Log("Game Manager found");

        NPC_Nav = GetComponent<NavMeshAgent>();

        // Freeze NPC rotation
        NPC_Nav.updateRotation = false;
        NPC_Nav.angularSpeed = 0;

        npcState = NPC_State.Passive;

        // Sync position with Photon
        net_Pos = transform.position;

        // Get all entities based on tag
        AllPlayers = GameObject.FindGameObjectsWithTag("Player").ToList();
        AllNPCs = GameObject.FindGameObjectsWithTag("AI").ToList();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateNPCState();

        if (isAlly)
        {
            if (AllPlayers.Contains(AllyPlayer)) AllPlayers.Remove(AllyPlayer);
            foreach (GameObject npc in AllNPCs)
            {
                if (npc.GetComponent<NPCBehavior>().AllyPlayer == AllyPlayer) AllNPCs.Remove(npc);
            }
        }
    }

    private void UpdateNPCState()
    {
        // Freeze NPC rotation
        this.transform.rotation = Quaternion.Euler(0, 0, 0);

        if (!gameManager.isDaytime && !isAlly) npcState = NPC_State.Patrol;

        switch (npcState) {
            case NPC_State.Follow:
                NPC_Follow();
                break;

            case NPC_State.Patrol:
                NPC_Patrol();
                break;

            case NPC_State.Chase:
                NPC_Chase();
                break;

            case NPC_State.Attack:
                NPC_Attack();
                break;
        }
    }

    private void MoveToTarget(Vector3 Target)
    {
        NPC_Nav.SetDestination(Target);

        // Change state to Attack when within attack range
        if (npcState == NPC_State.Chase)
        {
            if (Vector3.Distance(transform.position, Target) <= AttackRange)
            {
                npcState = NPC_State.Attack;
            }
        }
    }

    private void NPC_Follow()
    {
        Debug.Log($"Current NPC State: Follow");

        if (!gameManager.isDaytime) 
        {
            List<GameObject> EnemyPlayers = DetectEnemyPlayer();
            List<GameObject> EnemyAllies = DetectEnemyAlly();

            if (DistanceFromPlayer < MaxPlayerDistance)
            {
                if (EnemyPlayers.Count > 0 || EnemyAllies.Count > 0) DetectNearbyEntity(EnemyPlayers, EnemyAllies);
            }
        }
        
        MoveToTarget(AllyPlayer.transform.position);
    }

    private void NPC_Patrol()
    {
        Debug.Log($"Current NPC State: Patrol");

        DetectNearbyEntity(DetectEnemyPlayer(), DetectEnemyAlly());
    }

    private void NPC_Chase()
    {
        Debug.Log($"Current NPC State: Chase");

        Vector3 SetTarget = Vector3.zero;

        // Checks if ally is too far from player
        // If yes, return to player.
        // If no, pursue closest enemy.
        if (isAlly && DistanceFromPlayer > MaxPlayerDistance) npcState = NPC_State.Follow;
        else
        {
            // Prioritizes enemy AI ally tracking if within distance limit from enemy player or distance between both are equal
            if (Mathf.Abs(closestAI_Distance - closestPlayer_Distance) <= PlayerAIRange || Mathf.Abs(closestAI_Distance) == Mathf.Abs(closestPlayer_Distance)) SetTarget = closestAI.transform.position;
            else if (Mathf.Abs(closestAI_Distance) > Mathf.Abs(closestPlayer_Distance) || closestPlayer == null) SetTarget = closestAI.transform.position;
            else if (Mathf.Abs(closestAI_Distance) < Mathf.Abs(closestPlayer_Distance) || closestAI == null) SetTarget = closestPlayer.transform.position;
            else npcState = NPC_State.Patrol;

            MoveToTarget(SetTarget);
        }
       
        
    }

    private void NPC_Attack()
    {
        Debug.Log($"Current NPC State: Attack.");
        
        // Temporary to test enemy and ally tracking
        // Remove when adding attack conditions
        if (isAlly && DistanceFromPlayer > MaxPlayerDistance)
        {
            npcState = NPC_State.Follow;
        }
    }

    private List<GameObject> DetectEnemyPlayer()
    {
        List<GameObject> detectedPlayers = new List<GameObject>();

        foreach (GameObject player in AllPlayers)
        {
            if (Vector3.Distance(transform.position, player.transform.position) <= DetectionLimit) detectedPlayers.Add(player);
            else
            {
                if (detectedPlayers.Count > 0 && detectedPlayers.Contains(player)) detectedPlayers.Remove(player);
            }
        }

        return detectedPlayers;
    }

    private List<GameObject> DetectEnemyAlly()
    {
        List<GameObject> detectedNPCs = new List<GameObject>();

        foreach (GameObject npc in AllNPCs)
        {
            if (Vector3.Distance(transform.position, npc.transform.position) <= DetectionLimit) detectedNPCs.Add(npc);
            else
            {
                if (detectedNPCs.Count > 0 && detectedNPCs.Contains(npc)) detectedNPCs.Remove(npc);
            }
        }

        return detectedNPCs;
    }

    private void DetectNearbyEntity(List<GameObject> PlayersInRange, List<GameObject> NPCsInRange)
    {
        if (PlayersInRange.Count > 0 || NPCsInRange.Count > 0)
        {
            // Find the closest player entity
            for (int i = 0; i < PlayersInRange.Count; i++)
            {
                float get_DistanceToSelf = Vector3.Distance(transform.position, PlayersInRange[i].transform.position);

                if (closestPlayer_Distance == 0f || closestPlayer_Distance > get_DistanceToSelf)
                {
                    closestPlayer_Distance = get_DistanceToSelf;
                    closestPlayer = PlayersInRange[i];

                    Debug.Log($"{PlayersInRange[i]} detected");
                }
            }

            // Find the closest AI entity
            for (int i = 0; i < NPCsInRange.Count; i++)
            {
                float get_DistanceToSelf = Vector3.Distance(transform.position, NPCsInRange[i].transform.position);

                if (closestAI_Distance == 0f || closestAI_Distance > get_DistanceToSelf)
                {
                    closestAI_Distance = get_DistanceToSelf;
                    closestAI = NPCsInRange[i];

                    Debug.Log($"{NPCsInRange[i]} detected");
                }
            }

            if (!gameManager.isDaytime) npcState = NPC_State.Chase;

        }
        else
        {
            if (isAlly) npcState = NPC_State.Follow;
            else
            {
                if (!gameManager.isDaytime)  npcState = NPC_State.Patrol;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gameManager.isDaytime)
        {
            // Becomes player ally upon contact if it is not already an ally to any other player
            if (AllyPlayer == null & other.CompareTag("Player"))
            {
                npcState = NPC_State.Follow;
                isAlly = true;

                AllyPlayer = other.GameObject();

                Debug.Log($"NPC State Updated: {NPC_State.Follow}. NPC is ally to player ID {AllyPlayer.GetComponent<PlayerController>().playerID}");

                // Track distance from ally player
                DistanceFromPlayer = Vector3.Distance(transform.position, AllyPlayer.transform.position);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) stream.SendNext(transform.position);
        else net_Pos = (Vector3)stream.ReceiveNext();
    }
}
