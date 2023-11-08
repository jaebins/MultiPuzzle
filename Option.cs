using System;
using System.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Toggle;

public class Option : MonoBehaviour
{
    // ���ҽ�
    Sprite[] but_check = new Sprite[2];

    // �ܺ� ������Ʈ
    ResourceManager resourceManager;
    GameObject pause;
    Button but_back;

    // �ػ� ����
    TMP_Dropdown drop_resolution;
    Button but_checkFullScreen;

    // �Ҹ� ����
    Scrollbar backVolumeBar;
    Scrollbar effectVolumeBar;
    Button but_muteBackVolume;
    Button but_muteEffectVolume;
    AudioSource effectAudioSource;
    AudioSource backAudioSource;

    private void Start()
    {
        // On/Off ��� ���ҽ� �ҷ�����
        but_check[0] = Resources.Load<Sprite>("Sprites/UI/Icon_X");
        but_check[1] = Resources.Load<Sprite>("Sprites/UI/Icon_Check");

        resourceManager = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();
        pause = transform.parent.transform.Find("pause").gameObject;

        // �ɼ� â ���� ��ư
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
        // �ػ�
        drop_resolution.captionText.text = resourceManager.gameEnv.resolution;
        ChangeResolution(0);
        ToggleEvent(but_checkFullScreen, ref resourceManager.gameEnv.isFullScreen, true);

        // �Ҹ�
        backVolumeBar.value = resourceManager.gameEnv.backSoundValue;
        effectVolumeBar.value = resourceManager.gameEnv.effectSoundValue;
        SetSound();
        ToggleEvent(but_muteBackVolume, ref resourceManager.gameEnv.isBackSound, true);
        ToggleEvent(but_muteEffectVolume, ref resourceManager.gameEnv.isEffectSound, true);
    }

    void AddResolutionEvent()
    {
        // ��Ӵٿ� UI ��������
        drop_resolution = transform.Find("resolution").GetComponent<TMP_Dropdown>();
        drop_resolution.captionText.text = resourceManager.gameEnv.resolution;
        drop_resolution.onValueChanged.AddListener((index) => ChangeResolution(index));

        // ��ü ȭ�� üũ
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
        // �Ҹ� ����
        backAudioSource.volume = resourceManager.gameEnv.backSoundValue;
        effectAudioSource.volume = resourceManager.gameEnv.effectSoundValue;
    }

    void ToggleEvent(Button tarBut, ref bool isTrue, bool isInitSetting)
    {
        if(!isInitSetting) GameManager.PlayAudio(effectAudioSource, resourceManager.sounds["click"]);

        // ���� ó�� �������� �� �����͸� �ε��ϴ� ��Ȳ�̸� �������� ó���� ����
        if (isTrue)
            tarBut.GetComponent<Image>().sprite = isInitSetting ? but_check[1] : but_check[0];
        else
            tarBut.GetComponent<Image>().sprite = isInitSetting ? but_check[0] : but_check[1];

        // ��üȭ�� �������� ��������Ʈ �̸��� ���Ͽ� üũ(O = True, X = false)
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
