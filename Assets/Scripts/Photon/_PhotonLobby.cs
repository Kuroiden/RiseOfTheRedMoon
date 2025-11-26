using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class _PhotonLobby : MonoBehaviourPunCallbacks
{
    [Header("Photon Components")]
    public static _PhotonLobby lobby;
    public string sceneToLoad;

    [Header("UI Objects")]
    public GameObject Menu_UI;
    public GameObject Lobby_UI;

    public Button b_CreateRoom;
    public Button b_JoinRoom;
    public GameObject b_Cancel;
    public Button b_ExitGame;
    public Button b_CopyCode;
    public Button b_LeaveRoom;
    public Button b_StartGame;

    public TMP_InputField f_GenerateCode;
    public TMP_InputField f_InputCode;

    public GameObject o_Offline;

    private void Awake()
    {
        lobby = this;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        o_Offline.SetActive(true); // Is visible if client is not connected to the server (default)
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Player has connected to the Photon Master server");
        PhotonNetwork.AutomaticallySyncScene = true;
        Menu_UI.SetActive(true);
        b_Cancel.SetActive(false);
        o_Offline.SetActive(false);
    }

    // Button Functions
    public void OnPlayerCreateRoom()
    {
        Debug.Log("Creating new room...");
        CreateRoom();

        b_Cancel.SetActive(true);
        Menu_UI.SetActive(false);
    }

    public void OnPlayerJoin()
    {
        Debug.Log("Joining room...");
        JoinWithCode();

        b_Cancel.SetActive(true);
        Menu_UI.SetActive(false);
    }

    public void OnPlayerCancel()
    {
        PhotonNetwork.LeaveRoom();
        Debug.Log("Operation canceled");
        b_Cancel.SetActive(false);
        Menu_UI.SetActive(true);
    }

    // Other Functions
    public void CreateRoom()
    {
        Debug.Log("Creating new room...");
        int RoomCode = Random.Range(0000, 10000);

        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 8
        };
        PhotonNetwork.CreateRoom(RoomCode.ToString(), roomOptions);
        Debug.Log($"Room {RoomCode} created!");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Tried to join a random game but failed. There must be a room with the same name");
        CreateRoom();
    }

    public void JoinWithCode()
    {
        string roomToJoin = f_InputCode.text.Trim();

        if (string.IsNullOrEmpty(roomToJoin))
        {
            Debug.Log("No room name entered → joining a random open room...");
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            Debug.Log($"Trying to join specific room: {roomToJoin}");
            PhotonNetwork.JoinRoom(roomToJoin);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room successfully");
        PhotonNetwork.LoadLevel(sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError("Tried to join a random game but failed. There must be no open room");
        CreateRoom();
    }
}
