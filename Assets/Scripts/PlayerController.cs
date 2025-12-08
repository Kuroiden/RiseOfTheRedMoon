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
    public GameManager gManager;

    [Header("Game Objects")]
    CharacterController Player;
    [SerializeField] private GameObject PlayerObj;
    [SerializeField] private GameObject WolfObj;
    [SerializeField] private GameObject BloodNado;
    [SerializeField] private GameObject Claws;
    [SerializeField] private GameObject hitBox;
    [SerializeField] private GameObject dashObj;
    [SerializeField] private ParticleSystem slashVFX;
    [SerializeField] private ParticleSystem dashVFX;
    [SerializeField] private float attackVerticalOffset = -0.5f;
    
    public Texture[] PlayerTexture; // This can be replaced with a Texture2DArray I think, I just don't know how to use it
    public Texture[] WolfTexture;
    public Sprite[] PlayerSprites;
    public Sprite[] WolfSprites;

    //[SerializeField] private Texture2DArray PlayerTextureArray;
    //[SerializeField] private Texture2DArray WolfTextureArray;

    private readonly int TextureIndexID = Shader.PropertyToID("_TextureIndex");
    private readonly int TextureArrayID = Shader.PropertyToID("_MainTexArray");

    private Renderer playerRenderer;

    [Header("Player Stats")]
    [SerializeField] private float playerMaxHealth = 100f;
    public float playerCurrentHealth;
    [SerializeField] private float playerMaxStamina = 100f;
    [SerializeField] private float playerCurrentStamina;
    [SerializeField] private float dmg = 35f;
    [SerializeField] private HitboxRegister hitboxController;

    [SerializeField] private float gravity;
    [SerializeField] private float walkSpd = 2.0f;
    [SerializeField] private float runSpd = 3.0f;
    [SerializeField] private float dashSpeed = 15.0f; 
    [SerializeField] private float dashDuration = 0.15f;

    [Header("Stamina Calcs")]
    [SerializeField] private float dashCost = 25f;
    [SerializeField] private float runCost = 5f;
    [SerializeField] private float staminaRegenRate = 3.5f;

    [Header("Player Position")]
    Vector3 updatePos;

    [Header("Player Actions")]
    public bool CanMove = true;
    public bool isAttacking = false;
    public bool isDashing = false;
    private Vector3 lastFacingDirection = Vector3.forward;

    [Header("Photon PUN Variables")]
    public int playerID;
    private Vector3 net_Pos;
    private Quaternion net_Rot;

    void Awake()
    {
        gManager = FindAnyObjectByType<GameManager>();
    }

    void Start()
    {
        playerCurrentHealth = playerMaxHealth;
        playerCurrentStamina = playerMaxStamina;
        pState = Player_State.Human;
        Player = GetComponent<CharacterController>();
        hitboxController = hitBox.GetComponent<HitboxRegister>(); 
        PlayerObj.SetActive(true);
        BloodNado.SetActive(false);
        hitBox.SetActive(false);
        net_Pos = transform.position;
        net_Rot = transform.rotation;


        //playerID = PhotonNetwork.LocalPlayer.ActorNumber;
    }

    void Update()
    {
        Vector3 moveForward = transform.TransformDirection(Vector3.forward);
        Vector3 moveRight = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float charDisplacementX = CanMove ? (isRunning ? runSpd : walkSpd) * Input.GetAxis("Vertical") : 0;
        float charDisplacementZ = CanMove ? (isRunning ? runSpd : walkSpd) * Input.GetAxis("Horizontal") : 0;

        Vector3 currentInputMovement = (moveForward * charDisplacementX) + (moveRight * charDisplacementZ);

        //updatePos = (moveForward * charDisplacementX) + (moveRight * charDisplacementZ);

        if (isRunning && currentInputMovement.magnitude > 0.1f)
        {
            if (playerCurrentStamina > 0)
            {
                playerCurrentStamina -= (runCost * Time.deltaTime);
                playerCurrentStamina = Mathf.Max(0, playerCurrentStamina);
            }

            if (playerCurrentStamina <= 0)
            {
                // Force running speed down to walk speed.
                charDisplacementX = CanMove ? walkSpd * Input.GetAxis("Vertical") : 0;
                charDisplacementZ = CanMove ? walkSpd * Input.GetAxis("Horizontal") : 0;
            }
        }
        else if (!isRunning && !isAttacking && !isDashing && playerCurrentStamina < playerMaxStamina)
        {
            float regenAmount = staminaRegenRate * Time.deltaTime;
            playerCurrentStamina = Mathf.Min(playerMaxStamina, playerCurrentStamina + regenAmount);
        }

        updatePos = (moveForward * charDisplacementX) + (moveRight * charDisplacementZ);

        // Keeps player grounded
        if (!Player.isGrounded) updatePos.y -= gravity * 100f * Time.deltaTime;

        if (CanMove && !isDashing)
        {
                Player.Move(updatePos * Time.deltaTime);
        }

        
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            // Update texture (already present)
            PlayerObj.GetComponent<Renderer>().material.mainTexture = (pState == Player_State.Human) ? PlayerTexture[0] : WolfTexture[0];
            
            lastFacingDirection = transform.forward;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            PlayerObj.GetComponent<Renderer>().material.mainTexture = (pState == Player_State.Human) ? PlayerTexture[1] : WolfTexture[1];
            lastFacingDirection = -transform.right;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            PlayerObj.GetComponent<Renderer>().material.mainTexture = (pState == Player_State.Human) ? PlayerTexture[2] : WolfTexture[2];
            lastFacingDirection = -transform.forward; 
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            PlayerObj.GetComponent<Renderer>().material.mainTexture = (pState == Player_State.Human) ? PlayerTexture[3] : WolfTexture[3];
            lastFacingDirection = transform.right;
        }
        

        if (gManager != null)
        {
            if (gManager.isNighttime)
            {
                pState = Player_State.Werewolf;
                BloodNado.SetActive(true);
                Claws.SetActive(true);
            }
            else
            {
                pState = Player_State.Human;
            }
        }

        if(Input.GetMouseButtonDown(0) && CanMove)
        {
            Attack();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) && CanMove)
        {
            Dash();
        }
    }

    void Attack()
    {
        if(pState == Player_State.Werewolf)
        {
            if (!isAttacking && !isDashing)
            {
               StartCoroutine(AttackSequence());
            }
        }

    }

    void Dash()
    {
        if (pState == Player_State.Werewolf)
        {
            if (playerCurrentStamina >= dashCost)
            {
                if (!isDashing && !isAttacking)
                {
                    playerCurrentStamina -= (int)dashCost;
                    StartCoroutine(DashSequence());
                }
            }
            else
            {
                Debug.Log("Not enough stamina to Dash!");
            }
        }
    }

    private IEnumerator AttackSequence()
    {
        isAttacking = true;
        
        float attackDuration = 0.2f;
        float attackDistance = 0.3f;

        Quaternion rotationOffset = Quaternion.Euler(-90f, 0f, 0f);

        Vector3 attackPosition = transform.position + lastFacingDirection * attackDistance;
        attackPosition.y += attackVerticalOffset;

        hitBox.transform.position = attackPosition;

        Quaternion finalRotation;

        if (lastFacingDirection != Vector3.zero)
        {
            Quaternion directionRotation = Quaternion.LookRotation(lastFacingDirection, Vector3.up);
            finalRotation = directionRotation * rotationOffset;
        }
        else
        {
            finalRotation = transform.rotation * rotationOffset;
        }

        hitBox.transform.rotation = finalRotation;

        if (hitboxController != null)
        {
            hitboxController.Initialize(dmg);
        }

        hitBox.SetActive(true);

        if (slashVFX != null)
        {
          
            slashVFX.transform.position = hitBox.transform.position;
            slashVFX.transform.rotation = hitBox.transform.rotation;
            slashVFX.Play();
        }

        yield return new WaitForSeconds(attackDuration);


        hitBox.SetActive(false);
        isAttacking = false;

       
    }

    private IEnumerator DashSequence()
    {
        isDashing = true;
        CanMove = false; 
        dashObj.SetActive(true);
        isAttacking = false;

        Vector3 vfxSpawnPosition = transform.position;

        Vector3 oppositeDirection = -lastFacingDirection;

        Quaternion vfxRotation = Quaternion.LookRotation(oppositeDirection, Vector3.up);

        if (dashVFX != null)
        {
            dashVFX.transform.position = vfxSpawnPosition;
            dashVFX.transform.rotation = vfxRotation;
            dashVFX.Play();
        }

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            Vector3 dashMovement = lastFacingDirection * dashSpeed * Time.deltaTime;
            Player.Move(dashMovement);

            yield return null; // Wait for the next frame
        }

        isDashing = false;
        CanMove = true;
        dashObj.SetActive(false);
    }

    void SetPlayerFrame(Texture2DArray sheetArray, int frameIndex)
    {
        if (playerRenderer == null) return;

        playerRenderer.material.SetTexture(TextureArrayID, sheetArray);
        playerRenderer.material.SetFloat(TextureIndexID, (float)frameIndex);
    }

    [PunRPC]
    public void TakeDamage(float dmg)
    {
        if (pState == Player_State.Dead) return;

        playerCurrentHealth -= dmg;

        if (playerCurrentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        pState = Player_State.Dead;
        CanMove = false;

        this.gameObject.SetActive(false);

        //PlayerObj.SetActive(false);
        //BloodNado.SetActive(false);
        //Claws.SetActive(false);

        //GetComponent<CharacterController>().enabled = false;
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
