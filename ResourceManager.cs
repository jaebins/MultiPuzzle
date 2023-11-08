using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    // 리소스
    public Sprite[] background;
    public Sprite[] playerIcon;
    public Sprite[][] playerPanel;
    public Sprite[] block_sprites;
    public Sprite[] resultText;
    public Sprite[] checkIcon;

    public RuntimeAnimatorController[] character_animC;
    public RuntimeAnimatorController[][] playerPanel_animC;
    public RuntimeAnimatorController[][] playerEffect_animC;
    public RuntimeAnimatorController block_animC;

    public Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();
    public Dictionary<string, TextAsset> jsons = new Dictionary<string, TextAsset>();

    public GameObject panel_optionPrefeb;
    public GameObject h_rooms;
    public GameObject v_room;

    // 데이터 파일 관리
    public GameEnv gameEnv;

    void Awake()
    {
        LoadSprites();
        LoadAnimatorController();
        LoadPrefeb();
        LoadResources();
        LoadJson();
    }

    void LoadSprites()
    {
        background = Resources.LoadAll<Sprite>("Sprites/Backgrounds");
        playerIcon = Resources.LoadAll<Sprite>("Sprites/PlayerIcons");
        playerPanel = new Sprite[Setting.MAXPLAYERS][];
        for (int i = 0; i < Setting.MAXPLAYERS; i++)
        {
            Sprite[] temp = Resources.LoadAll<Sprite>($"Sprites/Players/Player{i + 1}");
            playerPanel[i] = temp;
        }
        block_sprites = Resources.LoadAll<Sprite>("Sprites/Blocks");
        resultText = Resources.LoadAll<Sprite>("Sprites/UI/GameResult/Text");
    }

    void LoadAnimatorController()
    {
        character_animC = Resources.LoadAll<AnimatorOverrideController>("Animations/PlayerCharacters");
        playerPanel_animC = new RuntimeAnimatorController[Setting.MAXPLAYERS][];
        for (int i = 0; i < Setting.MAXPLAYERS; i++)
        {
            RuntimeAnimatorController[] temp = Resources.LoadAll<RuntimeAnimatorController>($"Animations/PlayerPanels/PlayerPanel{i + 1}");
            playerPanel_animC[i] = temp;
        }
        playerEffect_animC = new RuntimeAnimatorController[Setting.MAXPLAYERS][];
        for (int i = 0; i < Setting.MAXPLAYERS; i++)
        {
            RuntimeAnimatorController[] temp = Resources.LoadAll<RuntimeAnimatorController>($"Animations/AttackEffects/AttackEffect{i + 1}");
            playerEffect_animC[i] = temp;
        }
        block_animC = Resources.Load<RuntimeAnimatorController>("Animations/Block/block");
    }

    void LoadPrefeb()
    {
        panel_optionPrefeb = Resources.Load<GameObject>("panel_option");
        h_rooms = Resources.Load<GameObject>("h_rooms");
        v_room = Resources.Load<GameObject>("v_room");
        checkIcon = Resources.LoadAll<Sprite>("Sprites/RoomIcons");
    }

    void LoadResources()
    {
        AudioClip[] audioClips = Resources.LoadAll<AudioClip>("Sounds");
        foreach(AudioClip item in audioClips)
        {
            sounds.Add(item.name, item);
        }

        TextAsset[] textAssets = Resources.LoadAll<TextAsset>("Json");
        foreach(TextAsset item in textAssets)
        {
            jsons.Add(item.name, item);
        }
    }

    public void LoadJson()
    {
        // 데이터 파일이 존재하지 않는다면 리소스 파일에서 초기 환경설정 불러옴
        Debug.Log(Setting.SAVE_GAMEENV_PATH);
        if (!File.Exists(Setting.SAVE_GAMEENV_PATH))
            gameEnv = JsonUtility.FromJson<GameEnv>(jsons["GameEnv"].text);
        else
            gameEnv = JsonUtility.FromJson<GameEnv>(File.ReadAllText(Setting.SAVE_GAMEENV_PATH));

        // 해상도 로딩
        GameManager.ChangeScreen(gameEnv.resolution, gameEnv.isFullScreen);
    }

    private void OnApplicationQuit()
    {
        // 앱 종료 떄 데이터 저장
        string gemeEnv_data = JsonUtility.ToJson(gameEnv);
        if (!File.Exists(Setting.SAVE_GAMEENV_PATH))
        {
            using (FileStream fs = File.Create(Setting.SAVE_GAMEENV_PATH))
                fs.Close();
        }
        File.WriteAllText(Setting.SAVE_GAMEENV_PATH, gemeEnv_data);
        //Debug.Log($"{gemeEnv_data}\n{Setting.SAVE_GAMEENV_PATH}");
    }
}

public class GameEnv
{
    public string resolution;
    public bool isFullScreen;
    public float backSoundValue;
    public float effectSoundValue;
    public bool isBackSound;
    public bool isEffectSound;
}
