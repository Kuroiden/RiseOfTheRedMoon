using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class _PhotonPlayerSpawner : MonoBehaviourPunCallbacks
{
    [Header("Player Settings")]
    public GameObject playerPrefab; // must be in Resources folder
    private int playerId;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    //this is for the owner of the server (player who created the game)
    public void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            StartCoroutine(DelaySpawn());
        }
        else
        {
            Debug.LogWarning("Not connected to Photon or not in a room yet.");
        }
    }

    IEnumerator DelaySpawn()
    {
        yield return new WaitForSeconds(0.2f); // wait a frame or two
        SpawnPlayer();
        Debug.Log("Spawned player: " + playerId);
    }

    //this is accessed by players who joined the server
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room, spawning player...");
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is missing in inspector!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        // Prevent double-spawning if the player already exists
        if (PhotonNetwork.LocalPlayer.TagObject != null)
        {
            Debug.Log("Player already spawned, skipping.");
            return;
        }

        playerId = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log("playerID: " + playerId);

        Transform spawnLocation;

        if (playerId == 1)
        {
            spawnLocation = spawnPoints[0];
        }
        else if (playerId == 2)
        {
            spawnLocation = spawnPoints[1];
        }
        else
        {
            spawnLocation = spawnPoints[2];
        }

        // Instantiate over network
        GameObject newPlayer = PhotonNetwork.Instantiate(playerPrefab.name, spawnLocation.position, spawnLocation.rotation);

        // Get PhotonView & ColorSync script
        PhotonView pv = newPlayer.GetComponent<PhotonView>();

        // Choose color based on player ID
        Color chosenColor;
        if (playerId == 1) chosenColor = Color.red;
        else if (playerId == 2) chosenColor = Color.green;
        else chosenColor = Color.blue;

        // Use RPC to sync color across network (Buffered so late joiners see correct color too)
        pv.RPC("SetColor", RpcTarget.AllBuffered, chosenColor.r, chosenColor.g, chosenColor.b);

        // Store reference
        PhotonNetwork.LocalPlayer.TagObject = newPlayer;

        Debug.Log($"Spawned player {playerId} with color {chosenColor}");
    }
}
