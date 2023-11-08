using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Resources;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class RoomListManager : MonoBehaviourPunCallbacks, INotInGame, IEssential
{
    // 내부 인스펙터

    // 외부 오브젝트
    public ResourceManager resourceManager;
    MainManager mainManager;
    public AudioSource effectAudioSource;

    // UI
    public GameObject canvas;
    public GameObject roomListPanel;
    public TextMeshProUGUI text_playerInfor;
    public Image image_playerIcon;
    public GameObject pause;
    public Animator changeScene;
    public GameObject panel_option;

    // 게임 진행 변수
    public string roomTitle;
    int nextSceneCnt;
    GameObject nowHRoomsPanel;
    TypedLobby lobby;
    List<GameObject> nowRooms = new List<GameObject>();
    List<RoomInfo> nowRoomList = new List<RoomInfo>();

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            effectAudioSource = GameObject.Find("EffectSoundManager").GetComponent<AudioSource>();
            LoadObject();

            // 플레이어 정보 동기화를 위해 딜레이를 줌
            StartCoroutine(GetPlayerInfor());

            // 포톤 서버에 연결
            PhotonNetwork.UseRpcMonoBehaviourCache = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void LoadObject()
    {
        resourceManager = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
        effectAudioSource = GameObject.Find("EffectSoundManager").GetComponent<AudioSource>();

        canvas = GameObject.Find("Canvas").gameObject;
        roomListPanel = canvas.transform.Find("roomList").Find("Viewport").Find("Content").gameObject;
        text_playerInfor = canvas.transform.Find("panel_player").Find("infor").GetComponent<TextMeshProUGUI>();
        image_playerIcon = canvas.transform.Find("panel_player").Find("icon").GetComponent<Image>();
        pause = canvas.transform.Find("pause").gameObject;
        changeScene = canvas.transform.Find("changeScene").GetComponent<Animator>();
        panel_option = canvas.transform.Find("panel_option").gameObject;
    }

    public override void OnConnectedToMaster()
    {
        lobby = new TypedLobby("a", LobbyType.Default);
        PhotonNetwork.JoinLobby(lobby);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 방 갱신해줌
        foreach(GameObject item in nowRooms)
        {
            Destroy(item);
        }
        nowHRoomsPanel = null;
        nowRooms.Clear();

        RoomInfo[] roomListArr = nowRoomList.ToArray();
        Array.Sort(roomListArr, (a, b) =>
        {
            return String.Compare(a.Name, b.Name);
        });

        foreach(RoomInfo item in roomList)
        {
            // 같은 방 이름은 제외
            int rs = Array.BinarySearch(roomListArr, item, new CompareRommName());
            Debug.Log("결과 : " + rs);

            if(rs > 0) nowRoomList.RemoveAt(rs - 1);
            nowRoomList.Add(item);
        }

        for (int i = 0; i < nowRoomList.Count; i++)
        {
            if (nowRoomList[i].MaxPlayers == 0)
            {
                nowRoomList.RemoveAt(i);
                continue;
            }

            // 수평에는 2개의 방이 들어감
            if (nowHRoomsPanel == null || nowHRoomsPanel.transform.childCount == 2)
            {
                nowHRoomsPanel = Instantiate(resourceManager.h_rooms);
                nowHRoomsPanel.transform.SetParent(roomListPanel.transform);
                nowHRoomsPanel.transform.localScale = new Vector3(1, 1, 1);
                nowRooms.Add(nowHRoomsPanel);
            }

            // 1600, 180
            RoomInfor vRoomPanel = Instantiate(resourceManager.v_room).GetComponent<RoomInfor>();
            vRoomPanel.transform.SetParent(nowHRoomsPanel.transform);
            vRoomPanel.transform.localScale = new Vector3(1, 1, 1);
            StartCoroutine(GetRoomInfor(vRoomPanel, nowRoomList[i]));
        }
    }

    private void Update()
    {
        CheckEndChangeSceneEvent(1);
    }

    IEnumerator GetPlayerInfor()
    {
        yield return new WaitForSeconds(0.5f);
        text_playerInfor.text = $"Name : {mainManager.nickName}\nCharacter : {resourceManager.character_animC[mainManager.chatacterIndex].name}";
        image_playerIcon.sprite = resourceManager.playerIcon[mainManager.chatacterIndex];
    }

    IEnumerator GetRoomInfor(RoomInfor vRoomPanel, RoomInfo roomInfo)
    {
        yield return new WaitForSeconds(0.5f);
        vRoomPanel.text_players.text = $"{roomInfo.PlayerCount} / {roomInfo.MaxPlayers}";
        vRoomPanel.text_roomTitle.text = roomInfo.Name;
        vRoomPanel.text_master.text = $"{roomInfo.CustomProperties["name"]}님의 방";
        vRoomPanel.icon_checkJoin.sprite = roomInfo.PlayerCount == roomInfo.MaxPlayers ? resourceManager.checkIcon[0] : resourceManager.checkIcon[1];
    }

    public void SearchRoom(TextMeshProUGUI text_roomTitle)
    {
        if (text_roomTitle.text.Length > 0) this.roomTitle = text_roomTitle.text;

        ChangeScene(2);
    }

    public void SetPanel(GameObject panel)
    {
        GameManager.PlayAudio(effectAudioSource, resourceManager.sounds["click"]);

        panel.SetActive(!panel.activeSelf);
        pause.SetActive(!pause.activeSelf);
    }

    public void ChangeScene(int sceneCnt)
    {
        // 이동할 스캔 인덱스 저장
        this.nextSceneCnt = sceneCnt;

        if (sceneCnt == 2)
            DontDestroyOnLoad(GameObject.Find("RoomListManager"));
        else if (sceneCnt == 0)
            Destroy(GameObject.Find("MainManager"));

        GameManager.PlayAudio(effectAudioSource, resourceManager.sounds["changeScene"]);

        changeScene.gameObject.SetActive(true);
        changeScene.SetTrigger("isChangeScene");
    }

    public void CheckEndChangeSceneEvent(int mySceneCnt)
    {
        if (SceneManager.GetActiveScene().buildIndex == mySceneCnt && GameManager.isEndAni(changeScene, "ChangeScene_1", 1.0f))
        {
            SceneManager.LoadScene(nextSceneCnt);
        }
    }

    public void SetSettingPanel()
    {
        GameManager.PlayAudio(effectAudioSource, resourceManager.sounds["click"]);

        panel_option.SetActive(!panel_option.activeSelf);
        pause.SetActive(!pause.activeSelf);
    }
}

public class CompareRommName : IComparer<RoomInfo>
{
    public int Compare(RoomInfo name1, RoomInfo name2)
    {
        return String.Compare(name1.Name, name2.Name);
    }
}