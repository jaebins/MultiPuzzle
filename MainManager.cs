using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class MainManager : MonoBehaviour, INotInGame
{
    // 내부 인스펙터

    // 외부 오브젝트
    ResourceManager resourceManager;
    AudioSource effectAudioSource;

    // UI
    GameObject canvas;
    public TextMeshProUGUI input_name;
    public Animator img_character;
    public TextMeshProUGUI text_characterName;
    public Animator changeScene;
    public GameObject pause;
    public GameObject panel_option;

    // 데이터
    public string nickName;
    public int chatacterIndex;

    // 게임 진행 변수
    int plusPage;
    int nextSceneCnt;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            LoadObject();

            img_character.runtimeAnimatorController = resourceManager.character_animC[chatacterIndex];
            text_characterName.text = resourceManager.character_animC[chatacterIndex].name;
        }
    }

    void LoadObject() 
    {
        // 해상도 설정
        resourceManager = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();

        canvas = GameObject.Find("Canvas").gameObject;
        input_name = canvas.transform.Find("playerInfor").Find("input_name").Find("Text Area").Find("Text").GetComponent<TextMeshProUGUI>();
        img_character = canvas.transform.Find("playerInfor").Find("img_character").GetComponent<Animator>();
        text_characterName = canvas.transform.Find("playerInfor").Find("panel_characterName").Find("img_characterName").GetComponent<TextMeshProUGUI>();
        changeScene = canvas.transform.Find("changeScene").GetComponent<Animator>();
        pause = canvas.transform.Find("pause").gameObject;
        panel_option = canvas.transform.Find("panel_option").gameObject;

        effectAudioSource = GameObject.Find("EffectSoundManager").GetComponent<AudioSource>();
    }

    private void Update()
    {
        CheckEndChangeSceneEvent(0);

        // 캐릭터 페이지 변경
        if (SceneManager.GetActiveScene().buildIndex == 0 && GameManager.isEndAni(img_character, "changePage", 1.0f))
        {
            chatacterIndex += plusPage;

            img_character.runtimeAnimatorController = resourceManager.character_animC[chatacterIndex];
            text_characterName.text = resourceManager.character_animC[chatacterIndex].name;

            plusPage = 0;
            img_character.SetBool("isChangePage", false);
        }
    }

    public void ChangeCharacterPage(int plusPage)
    {
        if (chatacterIndex + plusPage < 0 || chatacterIndex + plusPage == resourceManager.character_animC.Length) return;

        GameManager.PlayAudio(effectAudioSource, resourceManager.sounds["click"]);

        this.plusPage = plusPage;
        img_character.SetBool("isChangePage", true);
    }

    public void GoRoomLists()
    {
        if (input_name.text.Length > 5 || input_name.text.Length == 1)
        {
            // 나중에 금지 패널 추가
            Debug.Log("닉네임이 너무김");
            return;
        }

        ChangeScene(1);
    }

    public void ChangeScene(int sceneCnt)
    {
        nickName = input_name.text;

        this.nextSceneCnt = sceneCnt;

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(resourceManager);

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
