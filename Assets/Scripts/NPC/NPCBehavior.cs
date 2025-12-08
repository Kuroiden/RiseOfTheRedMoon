using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class NPCBehavior : MonoBehaviourPunCallbacks, IPunObservable, Damageable
{
    private NPC_State npcState;
    private GameManager gameManager;
    private NavMeshAgent nmAgent;
    [SerializeField] private GameObject npcObject;
    [SerializeField] private GameObject atkVFX;

    [Header("AI Properties")]
    public Texture[] WolfSprites; 
    private bool isFacingLeft;
    [SerializeField] private bool _IsAlly;
    public GameObject _AllyPlayer;
    [Tooltip("Distance/Radius of detection area for AI to pursue opponent")]
    public float _DetectionRange;
    [Tooltip("Distance/Radius of AI attack area")]
    public float _AttackRange;
    public GameObject _Target;
    [SerializeField] private float npcMaxHealth = 50f;
    public float npcCurrentHealth;
    [SerializeField] private bool _isAttacking;
    [SerializeField] private int atk;
    [SerializeField] private float atkCooldownDuration;
    private float atkCooldown;

    [Header("Enemy Tracking Variables")]
    public List<GameObject> _Enemies;
    public List<GameObject> _Opponents;
    public List<GameObject> Targets;

    [Header("Photon PUN Variables")]
    private Vector3 net_Pos;

    void Awake()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        nmAgent = GetComponent<NavMeshAgent>();
        npcState = NPC_State.Passive;
        isFacingLeft = false;
        atkVFX.SetActive(false);

        _IsAlly = false;

        npcCurrentHealth = npcMaxHealth;
    }

    void Start()
    {
        _Enemies = new List<GameObject>();
        _Opponents = new List<GameObject>();
        Targets = new List<GameObject>();
        atkCooldown = atkCooldownDuration;
    }

    void Update()
    {
        if (gameManager.isNighttime)
        {
            if (_Enemies.Count == 0) _Enemies = get_AllPlayers();
            if (_Opponents.Count == 0) _Opponents = get_AllAllies();

            EnemyDetection();
        }

        UpdateNPCState();
        UpdateSprite();
    }

    protected void UpdateNPCState()
    {
        switch (npcState)
        {
            case NPC_State.Passive:
                break;

            case NPC_State.Follow:
                _NPC_Follow();

                break;

            case NPC_State.Patrol:
                break;

            case NPC_State.Chase:
                _NPC_Chase();

                break;

            case NPC_State.Attack:
                _NPC_Attack();

                break;

            case NPC_State.Dead:
                Die();

                break;
        }
    }

    private void UpdateSprite()
    {
        if (nmAgent.desiredVelocity.x > 0) isFacingLeft = false;
        if (nmAgent.desiredVelocity.x < 0) isFacingLeft = true;

        if (isFacingLeft)
            npcObject.GetComponent<Renderer>().material.mainTexture = (npcState == NPC_State.Attack) ? WolfSprites[1] : WolfSprites[3];
        else
            npcObject.GetComponent<Renderer>().material.mainTexture = (npcState == NPC_State.Attack) ? WolfSprites[0] : WolfSprites[2];
    }

    private void MoveToTarget(GameObject Target)
    {
        if (Target != null)
        {
            nmAgent.SetDestination(Target.transform.position);
        }
    }

    private void _NPC_Follow()
    {
        MoveToTarget(_AllyPlayer);
    }

    private void _NPC_Chase()
    {
        MoveToTarget(_Target);

        if (Vector3.Distance(_Target.transform.position, transform.position) <= _AttackRange)
        {
            npcState = NPC_State.Attack;
        }
    }

    private void _NPC_Attack()
    {
        if (_Target != null)
        {
            float enemyhp;

            if (_Target.CompareTag("AI")) enemyhp = _Target.GetComponent<NPCBehavior>().npcCurrentHealth;
            else enemyhp = _Target.GetComponent<PlayerController>().playerCurrentHealth;

            Debug.Log($"{this.gameObject} is now attacking {_Target}");

            if (_isAttacking)
            {
                atkVFX.SetActive(true);

                atkCooldown -= Time.deltaTime;
                if (atkCooldown <= 0)
                {
                    atkVFX.SetActive(false);
                    _isAttacking = false;
                    atkCooldown = atkCooldownDuration;
                }
            }
            else 
            {
                if (enemyhp > 0)
                {
                    _isAttacking = true;

                    if (_Target.CompareTag("AI")) _Target.GetComponent<NPCBehavior>().TakeDamage(atk);
                    else _Target.GetComponent<PlayerController>().TakeDamage(atk);
                }
                else
                {
                    if (_Target.CompareTag("AI")) _Opponents.Remove(_Target);
                    else _Enemies.Remove(_Target);

                    Targets.Remove(_Target);

                    _Target = null;
                }
            }
        }
        else
        {
            if (_IsAlly) npcState = NPC_State.Follow;
            else npcState = NPC_State.Patrol;
        }
    }

    private void _NPC_Roam()
    {

    }

    public void TakeDamage(float damage)
    {
        // The Master Client (or the owner of the object) should usually handle health reduction
        // to prevent cheating and conflicts.
        /*if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }*/

        npcCurrentHealth -= damage;

        Debug.Log($"{gameObject.name} took {damage} damage. Remaining health: {npcCurrentHealth}");

        if (npcCurrentHealth <= 0)
        {
            npcState = NPC_State.Dead;
        }
    }
    private void Die()
    {
        if (npcCurrentHealth == 0)
        {
            Debug.Log(gameObject.name + " has died.");

            this.gameObject.SetActive(false);

            /*
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            */
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (gameManager.isDaytime)
        {
            if (_IsAlly == false && _AllyPlayer == null)
            {
                if (other.CompareTag("Player"))
                {
                    Debug.Log($"{other.gameObject} is now ally to {this}.");
                    _IsAlly = true;
                    _AllyPlayer = other.gameObject;

                    npcState = NPC_State.Follow;
                }
            }
        }
    }

    // AI Behaviors
    private List<GameObject> get_AllPlayers()
    {
        Debug.Log("Searching for opposing players...");
        List<GameObject> Players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        if (_IsAlly) Players.Remove(_AllyPlayer);

        if (Players.Count > 0) Debug.Log("Opposing players found.");
        else Debug.Log("ERROR: No opponents found. There might be only 1 player in the room.");

        return Players;
    }

    private List<GameObject> get_AllAllies()
    {
        Debug.Log("Searching for opposing players allies...");
        List<GameObject> Opponents = new List<GameObject>(GameObject.FindGameObjectsWithTag("AI"));
        Opponents.Remove(this.gameObject);
        if (_IsAlly)
        {
            Opponents.RemoveAll(o => o.gameObject.GetComponent<NPCBehavior>()._AllyPlayer == _AllyPlayer);
        }

        if (Opponents.Count > 0) Debug.Log("Opposing player allies found.");
        else Debug.Log("ERROR: No opponent allies found.");

        return Opponents;
    }

    private GameObject FindNearestEntity(List<GameObject> EntityList)
    {
        GameObject nearestEntity = null;
        float entityDist = 0f;

        if (EntityList.Count > 0)
        {
            for (int i = 0; i < EntityList.Count; i++)
            {
                float AllyToOppDist = Vector3.Distance(EntityList[i].transform.position, transform.position);

                if (entityDist == 0 && nearestEntity == null) nearestEntity = EntityList[i];
                else if (AllyToOppDist < entityDist) nearestEntity = EntityList[i];
            }
        }

        return nearestEntity;
    }

    public void EnemyDetection()
    {
        Debug.Log("Searching for nearby enemy...");

        if (_Target == null && npcState != NPC_State.Attack)
        {
            GameObject NearestPlayer = FindNearestEntity(_Enemies);

            GameObject Target = null;

            if (_Opponents.Count > 0)
            {
                foreach (GameObject x in _Opponents)
                {
                    if (x.GetComponent<NPCBehavior>()._AllyPlayer == NearestPlayer && Targets.Contains(x) == false) Targets.Add(x);
                }
            }

            if (Targets.Contains(NearestPlayer) == false) Targets.Add(NearestPlayer);

            if (Targets != null )
            {
                Target = FindNearestEntity(Targets);
            }
            else
            {
                if (_Enemies != null) Target = NearestPlayer;
                else
                {
                    if (_IsAlly) npcState = NPC_State.Follow;
                    else npcState = NPC_State.Patrol;
                }
            }

            if (Target != null)
            {
                if (Vector3.Distance(Target.transform.position, transform.position) <= _DetectionRange)
                {
                    Debug.Log("Enemy detected. Switching state to Chase");

                    npcState = NPC_State.Chase;
                    _Target = Target;
                }
                else if (_IsAlly) npcState = NPC_State.Follow;
                else npcState = NPC_State.Patrol;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) stream.SendNext(transform.position);
        else net_Pos = (Vector3)stream.ReceiveNext();
    }
}
