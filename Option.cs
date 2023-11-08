using System;
using System.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Toggle;

public class Option : MonoBehaviour
{
    // 리소스
    Sprite[] but_check = new Sprite[2];

    // 외부 오브젝트
    ResourceManager resourceManager;
    GameObject pause;
    Button but_back;

    // 해상도 관련
    TMP_Dropdown drop_resolution;
    Button but_checkFullScreen;

    // 소리 관련
    Scrollbar backVolumeBar;
    Scrollbar effectVolumeBar;
    Button but_muteBackVolume;
    Button but_muteEffectVolume;
    AudioSource effectAudioSource;
    AudioSource backAudioSource;

    private void Start()
    {
        // On/Off 토글 리소스 불러오기
        but_check[0] = Resources.Load<Sprite>("Sprites/UI/Icon_X");
        but_check[1] = Resources.Load<Sprite>("Sprites/UI/Icon_Check");

        resourceManager = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();
        pause = transform.parent.transform.Find("pause").gameObject;

        // 옵션 창 끄는 버튼
        but_back = transform.Find("but_back").GetComponent<Button>();
        but_back.onClick.AddListener(() =>
        {
            GameManager.PlayAudio(effectAudioSource, resourceManager.sounds["click"]);

            gameObject.SetActive(!gameObject.activeSelf);
            pause.SetActive(!pause.activeSelf);
        });

        backAudioSource = GameObject.Find("MusicManager").GetComponent<AudioSource>();
        effectAudioSource = GameObject.Find("EffectSoundManager").GetComponent<AudioSource>();

        AddResolutionEvent();
        AddChangeVolumeEvent();
        LoadData();

        gameObject.SetActive(false);
    }

    void LoadData()
    {
        // 해상도
        drop_resolution.captionText.text = resourceManager.gameEnv.resolution;
        ChangeResolution(0);
        ToggleEvent(but_checkFullScreen, ref resourceManager.gameEnv.isFullScreen, true);

        // 소리
        backVolumeBar.value = resourceManager.gameEnv.backSoundValue;
        effectVolumeBar.value = resourceManager.gameEnv.effectSoundValue;
        SetSound();
        ToggleEvent(but_muteBackVolume, ref resourceManager.gameEnv.isBackSound, true);
        ToggleEvent(but_muteEffectVolume, ref resourceManager.gameEnv.isEffectSound, true);
    }

    void AddResolutionEvent()
    {
        // 드롭다운 UI 가져오기
        drop_resolution = transform.Find("resolution").GetComponent<TMP_Dropdown>();
        drop_resolution.captionText.text = resourceManager.gameEnv.resolution;
        drop_resolution.onValueChanged.AddListener((index) => ChangeResolution(index));

        // 전체 화면 체크
        but_checkFullScreen = transform.Find("resolution").Find("check_fullScreen").GetComponent<Button>();
        but_checkFullScreen.onClick.AddListener(() => ToggleEvent(but_checkFullScreen, ref resourceManager.gameEnv.isFullScreen, false));
    }

    void ChangeResolution(int e)
    {
        resourceManager.gameEnv.resolution = drop_resolution.captionText.text;
        GameManager.ChangeScreen(resourceManager.gameEnv.resolution, resourceManager.gameEnv.isFullScreen);
    }

    void AddChangeVolumeEvent()
    {
        backVolumeBar = transform.Find("backVolume").GetComponent<Scrollbar>();
        effectVolumeBar = transform.Find("effectVolume").GetComponent<Scrollbar>();
        but_muteBackVolume = backVolumeBar.transform.Find("mute_backVolume").GetComponent<Button>();
        but_muteEffectVolume = effectVolumeBar.transform.Find("mute_effectVolume").GetComponent<Button>();

        backAudioSource = GameObject.Find("MusicManager").GetComponent<AudioSource>();
        effectAudioSource = GameObject.Find("EffectSoundManager").GetComponent<AudioSource>();

        backVolumeBar.onValueChanged.AddListener(value => ChangeVolume(ref resourceManager.gameEnv.backSoundValue, value));
        effectVolumeBar.onValueChanged.AddListener(value => ChangeVolume(ref resourceManager.gameEnv.effectSoundValue, value));
        but_muteBackVolume.onClick.AddListener(() => ToggleEvent(but_muteBackVolume, ref resourceManager.gameEnv.isBackSound, false));
        but_muteEffectVolume.onClick.AddListener(() => ToggleEvent(but_muteEffectVolume, ref resourceManager.gameEnv.isEffectSound, false));
    }

    void ChangeVolume(ref float tarSound, float value)
    {
        tarSound = value;

        SetSound();
    }

    void MuteVolume(AudioSource audioSource, bool isSound)
    {
        audioSource.mute = !isSound;
    }

    public void SetSound()
    {
        // 소리 설정
        backAudioSource.volume = resourceManager.gameEnv.backSoundValue;
        effectAudioSource.volume = resourceManager.gameEnv.effectSoundValue;
    }

    void ToggleEvent(Button tarBut, ref bool isTrue, bool isInitSetting)
    {
        if(!isInitSetting) GameManager.PlayAudio(effectAudioSource, resourceManager.sounds["click"]);

        // 게임 처음 시작했을 때 데이터를 로드하는 상황이면 예외적인 처리를 해줌
        if (isTrue)
            tarBut.GetComponent<Image>().sprite = isInitSetting ? but_check[1] : but_check[0];
        else
            tarBut.GetComponent<Image>().sprite = isInitSetting ? but_check[0] : but_check[1];

        // 전체화면 아이콘의 스프라이트 이름과 비교하여 체크(O = True, X = false)
        isTrue = tarBut.GetComponent<Image>().sprite.name.Equals(but_check[1].name);

        switch (tarBut.name)
        {
            case "check_fullScreen":
                ChangeResolution(0);
                break;
            case "mute_backVolume":
                MuteVolume(backAudioSource, resourceManager.gameEnv.isBackSound);
                break;
            case "mute_effectVolume":
                MuteVolume(effectAudioSource, resourceManager.gameEnv.isEffectSound);
                break;
        }
    }
}
