using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterRoom : MonoBehaviour
{
    public void EnterNewRoom()
    {
        SceneManager.LoadScene("GameLevel");
    }
}
