using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Security.Cryptography.X509Certificates;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Session Variables")]
    public bool isDaytime;
    public GameObject Dlight;
    public bool isNighttime;
    public GameObject Nlight    ;
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
        Dlight.SetActive(true);
        daytimeTimer = daytimeDuration;
        isDaytime = true;
        isNighttime = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDaytime)
        {
            daytimeDuration -= Time.deltaTime;

            if (daytimeDuration < 0)
            {
                daytimeDuration = 0;
                isDaytime = false;
                isNighttime = true;
                Dlight.SetActive(false);
                Nlight.SetActive(true);


                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC("RPC_ChangeDayNightState", RpcTarget.AllBuffered, isNighttime);
                }

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

    [PunRPC]
    void RPC_ChangeDayNightState(bool isNight)
    {
        isNighttime = isNight;
        isDaytime = !isNight;

        Dlight.SetActive(isDaytime);
        Nlight.SetActive(isNighttime);

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
