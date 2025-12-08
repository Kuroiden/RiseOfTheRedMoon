using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using Photon.Pun.UtilityScripts;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Session Variables")]
    public static string RoomID;
    public bool isDaytime;
    public GameObject Dlight;
    public bool isNighttime;
    public GameObject Nlight    ;
    [SerializeField] private float daytimeDuration;

    [Header("UI Elements")]
    public TextMeshProUGUI RoomIDUI;
    public TextMeshProUGUI TimerUI;
    public TextMeshProUGUI MsgUI;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        Dlight.SetActive(true);
        isDaytime = true;
        isNighttime = false;
        RoomIDUI.text = RoomID.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDaytime)
        {
            daytimeDuration -= Time.deltaTime;

            if (daytimeDuration < 0)
            {
                MsgUI.text = "The red moon has risen.";

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
            int minutes = Mathf.FloorToInt(daytimeDuration / 60);
            int seconds = Mathf.FloorToInt(daytimeDuration % 60);
            TimerUI.text = $"{minutes:00}:{seconds:00}";

            if (daytimeDuration <= 0)
            {
                TimerUI.text = "";
            }
        }
    }
}
