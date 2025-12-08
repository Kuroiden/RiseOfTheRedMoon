using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawning : MonoBehaviourPunCallbacks
{
    public GameObject npcObject;
    [SerializeField] private int spawnLimit = 0;
    
    void Start()
    {
        SpawnNPC();
    }

    public void SpawnNPC()
    {
        Vector3 spawnPos;
        Quaternion spawnRot = Quaternion.Euler(0, 0, 0);

        for (int x = 0; x < spawnLimit; x++)
        {
            spawnPos = new Vector3(Random.Range(-35f, 35.1f), 0.5f, Random.Range(-35f, 35.1f));

            Instantiate(npcObject, spawnPos, spawnRot);
        }
    }
}
