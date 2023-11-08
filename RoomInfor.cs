using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomInfor : MonoBehaviour
{
    RoomListManager roomListManager;

    // 내부 인스펙터
    EventTrigger ev;

    // 외부 오브젝트
    public TextMeshProUGUI text_players;
    public TextMeshProUGUI text_roomTitle;
    public TextMeshProUGUI text_master;
    public Image icon_checkJoin;

    private void Start()
    {
        roomListManager = GameObject.Find("RoomListManager").GetComponent<RoomListManager>();

        ev = GetComponent<EventTrigger>();
        text_players = transform.Find("text_players").GetComponent<TextMeshProUGUI>();
        text_roomTitle = transform.Find("text_roomTitle").GetComponent<TextMeshProUGUI>();
        text_master = transform.Find("text_master").GetComponent<TextMeshProUGUI>();
        icon_checkJoin = transform.Find("icon_checkJoin").GetComponent<Image>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((e) =>
        {
            GameManager.PlayAudio(roomListManager.effectAudioSource, roomListManager.resourceManager.sounds["click"]);

            roomListManager.SearchRoom(text_roomTitle);
        });
        ev.triggers.Add(entry);
    }
}
