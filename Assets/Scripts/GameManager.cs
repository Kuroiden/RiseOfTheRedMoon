using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Session Variables")]
    public bool isDaytime;
    [SerializeField] private float daytimeDuration;
    private float daytimeTimer;

    [Header("UI Elements")]
    public TextMeshPro TimerUI;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        daytimeTimer = daytimeDuration;
        isDaytime = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDaytime)
        {
            daytimeTimer -= Time.deltaTime;

            if (daytimeTimer < 0)
            {
                daytimeTimer = 0;
                isDaytime = false;
            }

            // Update UI timer
            UpdateTimerUI();
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        StartDaylightTimer();
    }

    void StartDaylightTimer()
    {
        // Only master client decides when to start
        if (PhotonNetwork.IsMasterClient && !isDaytime)
        {
            photonView.RPC("RPC_StartDaylightTimer", RpcTarget.AllBuffered, PhotonNetwork.Time);
        }
    }

    [PunRPC]
    void RPC_StartDaylightTimer(double startTime)
    {
        daytimeTimer = daytimeDuration;
        isDaytime = true;
        Debug.Log("Photon PUN timer started!");
    }

    void UpdateTimerUI()
    {
        if (TimerUI != null)
        {
            int minutes = Mathf.FloorToInt(daytimeTimer / 60);
            int seconds = Mathf.FloorToInt(daytimeTimer % 60);
            TimerUI.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
