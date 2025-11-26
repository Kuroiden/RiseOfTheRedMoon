using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    [Header("Game Objects")]
    CharacterController Player;
    [SerializeField] private GameObject PlayerObj;
    public Texture[] PlayerTexture; // This can be replaced with a Texture2DArray I think, I just don't know how to use it

    [Header("Player Position")]
    Vector3 updatePos;

    [SerializeField] private float gravity;
    [SerializeField] private float walkSpd = 2.0f;
    [SerializeField] private float runSpd = 3.0f;

    public bool CanMove = true;

    void Start()
    {
        Player = GetComponent<CharacterController>();
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
            // Update player position
            Player.Move(updatePos * Time.deltaTime);            
        }

        // Updates player sprite based on movement
        // Replace this if using Texture2DArray
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) PlayerObj.GetComponent<Renderer>().material.mainTexture = PlayerTexture[0];
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) PlayerObj.GetComponent<Renderer>().material.mainTexture = PlayerTexture[1];
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) PlayerObj.GetComponent<Renderer>().material.mainTexture = PlayerTexture[2];
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) PlayerObj.GetComponent<Renderer>().material.mainTexture = PlayerTexture[3];
    }
}
