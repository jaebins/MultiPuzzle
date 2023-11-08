using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviourPunCallbacks
{
    PhotonView pv;

    public bool isEndGame;

    private void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    public void ShowResult()
    {
        for (int i = 0; i < 2; i++)
        {
            GameManager gameManager = GameObject.Find($"GameManager(Clone)_{i + 1}").GetComponent<GameManager>();
            gameManager.EndGame();
        }
    }

    public void EndGameTrigger()
    {
        pv.RPC("EndGame", RpcTarget.All);
    }
    
    [PunRPC]
    void EndGame()
    {
        isEndGame = true;
    }

    public void ReturnLobby()
    {
        // 내가 처음으로 나갔다면 방을 닫음
        if (PhotonNetwork.PlayerList.Length == 2)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }

        PhotonNetwork.LeaveRoom();
        DontDestroyOnLoad(GameObject.Find("MainManager"));
        Destroy(GameObject.Find("RoomListManager").gameObject);
        SceneManager.LoadScene(1);
    }
}
