using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    public Room currentRoom;
    public ZolaRLAgent enemyInroom;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void BossAct2Killed()
    {
        PlayerPrefs.SetInt("ACT1Completed", 0);
        PlayerPrefs.SetInt("gameDone", 1);
        PlayerPrefs.SetInt("masterOfMatter", 1);
        PlayerPrefs.SetInt("theAvenger", 1);
        PlayerPrefs.SetInt("anomaly", 1);
        SceneManager.LoadScene("Training");
    }
}
