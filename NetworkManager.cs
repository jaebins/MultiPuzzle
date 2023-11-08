using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    // 외부 오브젝트
    public MainManager mainManager;
    public RoomListManager roomListManager;

    TypedLobby lobby;

    private void Start()
    {
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
        roomListManager = GameObject.Find("RoomListManager").GetComponent<RoomListManager>();

        //Screen.SetResolution(1920, 1080, false);
        lobby = new TypedLobby("a", LobbyType.Default);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = Setting.MAXPLAYERS;
        roomOptions.CustomRoomProperties = new Hashtable
        {
            { "name", mainManager.nickName }
        };

        roomOptions.CustomRoomPropertiesForLobby = new string[1] { "name" };
        PhotonNetwork.JoinOrCreateRoom(roomListManager.roomTitle, roomOptions, lobby);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("방 생성");
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate("Manager", Vector2.zero, Quaternion.identity);
        PhotonNetwork.Instantiate("GameManager", Vector2.zero, Quaternion.identity);
        PhotonNetwork.Instantiate("player", Vector2.zero, Quaternion.identity);
        PhotonNetwork.Instantiate("player_attackEffect", Vector2.zero, Quaternion.identity);

        //Debug.Log($"{connectionManager.nickName}-방 입장");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log($"{returnCode} : {message}");
        Application.Quit();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"{returnCode} : {message}");
        Application.Quit();
    }
}
