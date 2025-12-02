using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    private Rigidbody2D rb;
    private Vector2 velocity;
    private Player_State pState;
    private GameManager gManager;

    [Header("Game Objects")]
    CharacterController Player;
    [SerializeField] private GameObject PlayerObj;
    [SerializeField] private GameObject WolfObj;
    public Texture[] PlayerTexture; // This can be replaced with a Texture2DArray I think, I just don't know how to use it
    public Texture[] WolfTexture; 

    [Header("Player Position")]
    Vector3 updatePos;

    [SerializeField] private float gravity;
    [SerializeField] private float walkSpd = 2.0f;
    [SerializeField] private float runSpd = 3.0f;

    public bool CanMove = true;

    [Header("Photon PUN Variables")]
    public int playerID;
    private Vector3 net_Pos;
    private Quaternion net_Rot;

    void Start()
    {
        pState = Player_State.Human;
        Player = GetComponent<CharacterController>();
        gManager = FindAnyObjectByType<GameManager>(); 
        PlayerObj.SetActive(true);
        net_Pos = transform.position;
        net_Rot = transform.rotation;

        //playerID = PhotonNetwork.LocalPlayer.ActorNumber;
    }

    void Update()
    {
        Vector3 moveForward = transform.TransformDirection(Vector3.forward);
        Vector3 moveRight = transform.TransformDirection(Vector3.right);

        // Move character
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float charDisplacementX = CanMove ? (isRunning ? runSpd : walkSpd) * Input.GetAxis("Vertical") : 0;
        float charDisplacementZ = CanMove ? (isRunning ? runSpd : walkSpd) * Input.GetAxis("Horizontal") : 0;

        updatePos = (moveForward * charDisplacementX) + (moveRight * charDisplacementZ);

        // Keeps player grounded
        if (!Player.isGrounded) updatePos.y -= gravity * 100f * Time.deltaTime;

        if (CanMove)
        {
            //if (photonView.IsMine)
            //{
                // Update player position
                Player.Move(updatePos * Time.deltaTime);

                if (Input.GetMouseButton(0)) Attack();
           // }
            //else
            //{
            //    transform.position = Vector3.Lerp(transform.position, net_Pos, Time.deltaTime * 10f);
            //    transform.rotation = Quaternion.Lerp(transform.rotation, net_Rot, Time.deltaTime);
            //}
        }

        if (gManager != null)
        {
            if (gManager.isNighttime)
            {
                pState = Player_State.Werewolf;
            }
            else
            {
                pState = Player_State.Human;
            }
        }
       
        //if (photonView.IsMine && gManager != null)

        //if (photonView.IsMine)
        //{
        // Updates player sprite based on movement
        // Replace this if using Texture2DArray
        if (pState == Player_State.Human)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) PlayerObj.GetComponent<Renderer>().material.mainTexture = PlayerTexture[0];
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) PlayerObj.GetComponent<Renderer>().material.mainTexture = PlayerTexture[1];
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) PlayerObj.GetComponent<Renderer>().material.mainTexture = PlayerTexture[2];
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) PlayerObj.GetComponent<Renderer>().material.mainTexture = PlayerTexture[3];
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) PlayerObj.GetComponent<Renderer>().material.mainTexture = WolfTexture[0];
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) PlayerObj.GetComponent<Renderer>().material.mainTexture = WolfTexture[1];
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) PlayerObj.GetComponent<Renderer>().material.mainTexture = WolfTexture[2];
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) PlayerObj.GetComponent<Renderer>().material.mainTexture = WolfTexture[3];
        }
        //}
    }

    void Attack()
    {
        
    }

    public void changeState(This @this)
    {
        if (gManager.isNighttime) 
        { 
            pState = Player_State.Werewolf; 
            PlayerObj.SetActive(false);
            WolfObj.SetActive(true);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            net_Pos = (Vector3)stream.ReceiveNext();
            net_Rot = (Quaternion)stream.ReceiveNext();
        }
    }
}
