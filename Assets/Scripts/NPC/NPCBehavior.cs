using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.AI;
using Unity.VisualScripting;

public class NPCBehavior : MonoBehaviourPunCallbacks
{
    private NPC_State npcState;

    [Header("AI Properties")]
    NavMeshAgent NPC_Nav;
    [SerializeField] private bool isAlly;
    [SerializeField] private GameObject AllyPlayer;

    // Start is called before the first frame update
    void Start()
    {
        NPC_Nav = GetComponent<NavMeshAgent>();

        npcState = NPC_State.Passive;
    }

    // Update is called once per frame
    void Update()
    {
        //if (!GameManager.isDaytime)
        //{

        //}

        UpdateNPCState();
    }

    private void UpdateNPCState()
    {
        // Find Closest player //

        // Update NPC state
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

        // Freeze NPC Rotation
        this.transform.rotation = Quaternion.EulerRotation(0, 0, 0);

        if (npcState == NPC_State.Patrol)
        {

        }
    }

    private void NPC_Follow()
    {
        Vector3 currentTarget = AllyPlayer.transform.position;
        MoveToTarget(currentTarget);
    }

    private void NPC_Patrol()
    {
        // Create list of all players and their positions


        // Find nearest player


        //MoveToTarget();
    }

    private void NPC_Chase()
    {
        //MoveToTarget();
    }

    private void NPC_Attack()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            npcState = NPC_State.Follow;
            isAlly = true;

            AllyPlayer = other.GameObject();

            Debug.Log($"NPC State Updated: {NPC_State.Follow}. NPC is ally to player ID {AllyPlayer.GetComponent<PlayerController>().playerID}");
        }
    }
}
