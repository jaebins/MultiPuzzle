using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable, IEssential
{
    // ���� �ν�����
    PhotonView pv;

    // �ܺ� ������Ʈ
    public Camera myCamera;
    public Manager manager;
    MainManager mainManager;
    AudioSource effectAudioSource;
    public ResourceManager resourceManager;
    public GameManager enemyGameManager;
    
    // �ΰ��� ������Ʈ
    public SpriteRenderer background;
    public PlayerPanel myBlockPanel;
    public PlayerPanel enemyBlockPanel;
    public PlayerEffect[] myEffect;

    // UI
    [HideInInspector] public GameObject canvas;
    TextMeshProUGUI nickNameText;
    TextMeshProUGUI scoreText;
    Image img_player;
    Scrollbar healthbar;
    public GameObject pause;
    public Animator text_startCnt;
    public Animator panel_result;

    // ���� ȯ��
    public BlockInfor[][] blocks;
    public Vector2[][] blocksPos;
    public int myID = 0;
    int enemyID = 0;
    public string nickName = string.Empty;
    public int characterIndex = -1;

    // ���� ���� ���� ����
    List<Vector2> selectedIndex = new List<Vector2>();
    int score;
    float maxHealth = 25;
    public float nowHealth;
    float flowStartCount = 0.2f;
    bool initSetting;
    public bool isStart;
    bool isLose;

    // ��ũ�� �г��� ����
    // �κ� �� ��� �����
    // ������ ���� ��üȭ��

    void Start()
    {
        pv = GetComponent<PhotonView>();

        // ���� ���̵� �Ҵ�
        myID = int.Parse(pv.ViewID.ToString()[0].ToString());
        enemyID = myID == 1 ? 2 : 1;

        // �̸�, ü�� ����
        name += "_" + (int)myID;
        nowHealth = maxHealth;

        blocks = new BlockInfor[(int)Setting.BLOCK_LENGTH.y][];
        blocksPos = new Vector2[(int)Setting.BLOCK_LENGTH.y][];

        LoadObject_before();
        StartCoroutine(LoadObject_after()); // ������Ʈ ���� ������

        //joinText.text = objectManager.blocks[0].GetComponent<SpriteRenderer>().sprite.name;
    }

    void LoadObject_before()
    {
        // Object �ε�
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
        resourceManager = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();
        effectAudioSource = GameObject.Find("EffectSoundManager").GetComponent<AudioSource>();
        myCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        background = GameObject.Find("background").GetComponent<SpriteRenderer>();
        background.sprite = resourceManager.background[UnityEngine.Random.Range(0, resourceManager.background.Length)]; // ���� ���

        // UI �ε�
        canvas = GameObject.Find("Canvas");
        nickNameText = canvas.transform.Find($"player_{myID}").Find($"panel_playerName{myID}").Find($"text_playerName{myID}").GetComponent<TextMeshProUGUI>();
        scoreText = canvas.transform.Find($"player_{myID}").Find($"panel_playerScore{myID}").Find($"text_playerScore{myID}").GetComponent<TextMeshProUGUI>();
        img_player = canvas.transform.Find($"player_{myID}").Find($"character_player{myID}").GetComponent<Image>();
        
        if (pv.IsMine){ // �ش� ������Ʈ�� ������� ���� �����ϰ� ĳ���� �ε��� ����
            // ���߿� ����
            characterIndex = myID;
            nickName = mainManager.nickName;
            characterIndex = mainManager.chatacterIndex;

            // �� �г� ǥ�� ������
            GameObject isMeIcon = canvas.transform.Find($"player_{myID}").Find($"img_isMePlayer{myID}").gameObject;
            isMeIcon.SetActive(true);

            // ���� ���� ���
            pause = canvas.transform.Find($"pause").gameObject;
            pause.SetActive(false);

            // ī��Ʈ �ٿ�
            text_startCnt = canvas.transform.Find($"text_startCnt").GetComponent<Animator>();
            text_startCnt.gameObject.SetActive(false);

            // ���� ���
            panel_result = canvas.transform.Find($"panel_result").GetComponent<Animator>();
            panel_result.gameObject.SetActive(false);
        }
        healthbar = canvas.transform.Find($"player_{myID}").Find($"healthbar_player{myID}").GetComponent<Scrollbar>();
    }

    IEnumerator LoadObject_after()
    {
        yield return new WaitForSeconds(0.5f);
        manager = GameObject.Find("Manager(Clone)").GetComponent<Manager>();
        myBlockPanel = GameObject.Find($"player(Clone)_{myID}").transform.Find("player").GetComponent<PlayerPanel>();
        myEffect = new PlayerEffect[GameObject.Find($"player_attackEffect(Clone)_{myID}").transform.childCount];
        for (int i = 0; i < myEffect.Length; i++)
            myEffect[i] = GameObject.Find($"player_attackEffect(Clone)_{myID}").transform.GetChild(i).GetComponent<PlayerEffect>();

        if (pv.IsMine) // �����϶��� ���� ������ block �迭�� ����
            InsertBlock();
        else // �����϶��� ���� �������ִ� ������ block �迭�� ����, ���� �÷��̾� ���� ����ȭ�� ���� 0.5�� ������
            StartCoroutine(LoadEnemyBlock());
    }

    IEnumerator LoadEnemyBlock()
    {
        yield return new WaitForSeconds(0.5f);

        Vector2 pos = Setting.BLOCK_STARTPOS;

        for (int i = 0; i < Setting.BLOCK_LENGTH.y; i++)
        {
            BlockInfor[] blocks_h = new BlockInfor[(int)Setting.BLOCK_LENGTH.y];
            Vector2[] blocksPos_h = new Vector2[(int)Setting.BLOCK_LENGTH.x];

            for (int j = 0; j < Setting.BLOCK_LENGTH.x; j++)
            {
                blocks_h[j] = GameObject.Find($"player(Clone)_{enemyID}").transform.Find("player").Find($"{enemyID}:block{(Setting.BLOCK_LENGTH.y * i) + j}").GetComponent<BlockInfor>();

                blocksPos_h[j] = pos;
                pos.x += Setting.BLOCK_MARGIN.x;
            }

            blocks[i] = blocks_h;
            blocksPos[i] = blocksPos_h;

            pos.x = Setting.BLOCK_STARTPOS.x;
            pos.y -= Setting.BLOCK_MARGIN.y;
        }
    }

    public void InsertBlock()
    {
        Vector2 pos = Setting.BLOCK_STARTPOS;

        for (int i = 0; i < Setting.BLOCK_LENGTH.y; i++)
        {
            BlockInfor[] blocks_h = new BlockInfor[(int)Setting.BLOCK_LENGTH.x];
            Vector2[] blocksPos_h = new Vector2[(int)Setting.BLOCK_LENGTH.x];

            for (int j = 0; j < Setting.BLOCK_LENGTH.x; j++)
            {
                // ������ ��� ��������Ʈ ����
                BlockInfor block = PhotonNetwork.Instantiate("block", Vector2.zero, Quaternion.identity).GetComponent<BlockInfor>();

                //��� �ν��Ͻ����� ������ ���� �� �ٸ� Ŭ���̾�Ʈ���� �Ѹ���
                block.objName = $"{PhotonNetwork.LocalPlayer.ActorNumber}:block{(Setting.BLOCK_LENGTH.y * i) + j}";
                block.gameManagerName = name;
                block.myParent = $"player(Clone)_{PhotonNetwork.LocalPlayer.ActorNumber}";
                block.myIndex = new Vector2(j, i);
                block.spriteIndex = UnityEngine.Random.Range(0, Setting.BLOCK_SOURCE_LENGTH);

                // ���� ������Ʈ ����
                block.name = block.objName;
                block.gameManager = GameObject.Find(block.gameManagerName).GetComponent<GameManager>();
                block.transform.SetParent(GameObject.Find(block.myParent).transform.Find("player").transform);

                //block.name = $"{PhotonNetwork.LocalPlayer.ActorNumber}:block{(Setting.BLOCK_LENGTH.y * i) + j}";
                //block.gameManager = GetComponent<GameManager>();
                //block.transform.SetParent(myBlockPanel.transform);
                //block.myIndex = new Vector2(j, i);
                //block.spriteIndex = UnityEngine.Random.Range(0, 2);

                block.transform.localScale = Setting.BLOCK_SIZE;
                block.transform.localPosition = pos;

                blocks_h[j] = block;
                blocksPos_h[j] = pos;

                pos.x += Setting.BLOCK_MARGIN.x;
            }

            blocks[i] = blocks_h;
            blocksPos[i] = blocksPos_h;

            pos.x = Setting.BLOCK_STARTPOS.x;
            pos.y -= Setting.BLOCK_MARGIN.y;
        }
    }

    void Update()
    {
        CheckUserCount();
        SetAniState();
        InGame();
    }

    void CheckUserCount()
    {
        if(pv.IsMine && !isStart)
        {
            // �÷��̾ ���Դٸ�
            if (text_startCnt.GetInteger("startCount") == 0 && PhotonNetwork.PlayerList.Length == 2)
            {
                text_startCnt.gameObject.SetActive(true);
                text_startCnt.SetTrigger("startCount");
            }

            // ���� ī��Ʈ ��
            if (isEndAni(text_startCnt, "StartCount", flowStartCount))
            {

                text_startCnt.GetComponent<TextMeshProUGUI>().text = (5 - (int)((flowStartCount * 10) / 2)).ToString();
                flowStartCount += 0.2f;

                if (flowStartCount >= 1.2f)
                {
                    PlayAudio(effectAudioSource, resourceManager.sounds["gameStart"]);
                    text_startCnt.gameObject.SetActive(false);
                    isStart = true; // ���� ����
                }
                else PlayAudio(effectAudioSource, resourceManager.sounds["countDown"]);
            }
        }

        // ���� �����ٸ� ���� ��Ʈ��
        if (isStart && PhotonNetwork.PlayerList.Length < 2)
            manager.ReturnLobby();
    }
    
    void SetAniState()
    {
        // ���â ���� �� �� �Ҹ� ���
        if(pv.IsMine && isEndAni(panel_result, "GameResult", 1.0f))
        {
            panel_result.SetBool("isResult", false);
            if(isLose) PlayAudio(effectAudioSource, resourceManager.sounds["lose"]);
            else PlayAudio(effectAudioSource, resourceManager.sounds["win"]);
        }
    }

    void InGame()
    {
        // ���� ������ �ߴٸ�
        if (isStart && !manager.isEndGame)
        {
            if (pv.IsMine && Input.GetMouseButtonDown(0))
            {
                // �Ź� ����ĳ��Ʈ ��°� ����
                Vector3 mousePos = myCamera.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, transform.forward, 1f);
                SelectBlock(hit);
            }

            scoreText.text = score.ToString();
        }

        // ĳ���� �ִϸ��̼� �۽�
        if (img_player.GetComponent<Animator>().runtimeAnimatorController == null && characterIndex != -1)
            img_player.GetComponent<Animator>().runtimeAnimatorController = resourceManager.character_animC[characterIndex];
        
        // �г��� �۽�
        if (nickNameText.text.Length == 0 && !nickName.IsNullOrEmpty())
            nickNameText.text = nickName;
    }

    void SelectBlock(RaycastHit2D hit)
    {
        // ���� �� ���� �ƴ� ���� �մ�ٸ� �н�
        if(hit && hit.transform.tag.Equals("Block"))
        {
            if (!myID.ToString().Equals(hit.transform.name[0].ToString()))
                return;
        }

        PlayAudio(effectAudioSource, resourceManager.sounds["clickBlock"]);

        // ����
        if (hit && hit.transform.tag.Equals("Block") &&
            selectedIndex.Count == 1 && hit.transform.name.Equals(GetBlocksToIndex(selectedIndex[0]).name))
        {
            ResetSelectBlock(GetBlocksToIndex(selectedIndex[0]));
        }
        else if (hit && hit.transform.tag.Equals("Block") && 
            selectedIndex.Count == 0)
        {
            // Ŭ���� ��� ��������
            selectedIndex.Add(hit.transform.GetComponent<BlockInfor>().myIndex);
            hit.transform.GetComponent<BlockInfor>().isSelect = true;
        }
        else if (hit && hit.transform.tag.Equals("Block") && 
            selectedIndex.Count == 1)
        {
            selectedIndex.Add(hit.transform.GetComponent<BlockInfor>().myIndex);
            GameObject selectedB = GetBlocksToIndex(selectedIndex[0]).gameObject;
            GameObject nowSelectB = GetBlocksToIndex(selectedIndex[1]).gameObject;
            Vector2 selectedPos = selectedB.transform.position;
            Vector2 nowSelectPos = nowSelectB.transform.position;
            hit.transform.GetComponent<BlockInfor>().isSelect = true;

            PlayAudio(effectAudioSource, resourceManager.sounds["moveBlock"]);
            StartCoroutine(MoveBlocksMotion(selectedB, nowSelectB, selectedPos, nowSelectPos, nowSelectPos, selectedPos, 0.07f));
        }
    }

    void ResetSelectBlock(BlockInfor hit)
    {
        hit.isSelect = false;
        hit.anime.SetBool("isSelect", false);

        // ���ڷ� �Ѿ�� hit�� ���� ���õ� ���� ���� �ʴٸ� ����
        if (!hit.name.Equals(GetBlocksToIndex(selectedIndex[0])))
        {
            GetBlocksToIndex(selectedIndex[0]).isSelect = false;
            GetBlocksToIndex(selectedIndex[0]).anime.SetBool("isSelect", false);
        }
        selectedIndex.Clear();
    }

    IEnumerator MoveBlocksMotion(GameObject block1, GameObject block2, Vector2 pos1, Vector2 pos2, Vector2 goalPos1, Vector2 goalPos2, float delay)
    {
        yield return new WaitForSeconds(delay);

        block1.transform.position = Vector2.Lerp(pos1, goalPos1, 0.1f);
        block2.transform.position = Vector2.Lerp(pos2, goalPos2, 0.1f);

        // ���� ��ġ�� �� �ٲ��ٸ�
        //Debug.Log((int)pos1.x + " : " + (int)goalPos1.x + " - " + (int)pos1.y + " : " + (int)goalPos1.y);
        if ((int)pos1.x <= (int)goalPos1.x + 1 && (int)pos1.x >= (int)goalPos1.x - 1 && 
            (int)pos1.y <= (int)goalPos1.y + 1 && (int)pos1.y >= (int)goalPos1.y - 1)
        {
            // ��ġ �� ������ ����
            Vector2 temp = GetBlocksToIndex(selectedIndex[1]).myIndex;
            blocks[(int)selectedIndex[1].y][(int)selectedIndex[1].x].myIndex =
                blocks[(int)selectedIndex[0].y][(int)selectedIndex[0].x].myIndex;
            blocks[(int)selectedIndex[0].y][(int)selectedIndex[0].x].myIndex = temp;

            BlockInfor temp2 = GetBlocksToIndex(selectedIndex[1]);
            blocks[(int)selectedIndex[1].y][(int)selectedIndex[1].x] = blocks[(int)selectedIndex[0].y][(int)selectedIndex[0].x];
            blocks[(int)selectedIndex[0].y][(int)selectedIndex[0].x] = temp2;

            CheckBomb(selectedIndex[0]);
            CheckBomb(selectedIndex[1]);

            ResetSelectBlock(GetBlocksToIndex(selectedIndex[1]));
        }
        else StartCoroutine(MoveBlocksMotion(block1, block2, block1.transform.position, block2.transform.position, goalPos1, goalPos2, 0.01f));
    }
    
    void CheckBomb(Vector2 index)
    {
        // sprite.name -> spriteIndex 
        int mid1_x = (int)index.x, mid2_x = (int)index.x;
        int start_x = 0, end_x = (int)Setting.BLOCK_LENGTH.x - 1;
        bool sucX = false;

        while (true)
        {
            if (mid1_x > start_x) mid1_x--;
            if(mid2_x < end_x) mid2_x++;

            if (!blocks[(int)index.y][(int)index.x].spriteRenderer.sprite.name.Equals(blocks[(int)index.y][(int)mid1_x].spriteRenderer.sprite.name))
                break;
            if (!blocks[(int)index.y][(int)index.x].spriteRenderer.sprite.name.Equals(blocks[(int)index.y][(int)mid2_x].spriteRenderer.sprite.name))
                break;


            if (mid1_x == start_x && mid2_x == end_x)
            {
                sucX = true;
                break;
            }
        }

        int mid1_y = (int)index.y, mid2_y = (int)index.y;
        int start_y = 0, end_y = (int)Setting.BLOCK_LENGTH.y - 1;
        bool sucY = false;

        while (true)
        {
            if (mid1_y > start_y) mid1_y--;
            if (mid2_y < end_y) mid2_y++;

            if (!blocks[(int)index.y][(int)index.x].spriteRenderer.sprite.name.Equals(blocks[(int)mid1_y][(int)index.x].spriteRenderer.sprite.name))
                break;
            if (!blocks[(int)index.y][(int)index.x].spriteRenderer.sprite.name.Equals(blocks[(int)mid2_y][(int)index.x].spriteRenderer.sprite.name))
                break;

            if (mid1_y == start_y && mid2_y == end_y)
            {
                sucY = true;
                break;
            }
        }

        int score = 350;
        int decHealth = -7;
        int incHealth = 5;

        if (sucX)
        {
            for (int i = 0; i < Setting.BLOCK_LENGTH.x; i++)
            {
                blocks[(int)index.y][i].isBomb = true;
                myEffect[0].gameObject.transform.position = new Vector2(UnityEngine.Random.Range(200, 700), UnityEngine.Random.Range(100, -300));
            }

            Hit(score, decHealth, incHealth);
        }
        if (sucY)
        {
            for (int i = 0; i < Setting.BLOCK_LENGTH.y; i++)
            {
                blocks[i][(int)index.x].isBomb = true;
                myEffect[0].gameObject.transform.position = new Vector2(UnityEngine.Random.Range(200, 700), UnityEngine.Random.Range(100, -300));
            }

            // ���� ���� �Ѵ� �����̶�� �߰� ����, ������
            if (sucX)
            {
                score += 800;
                decHealth -= 5;
                incHealth += 5;
            }

            Hit(score, decHealth, incHealth);
        }
    }

    public void Hit(int score, int decHealth, int incHealth)
    {
        PlayAudio(effectAudioSource, resourceManager.sounds["boomBlock"]);

        foreach (PlayerEffect item in myEffect)
        {
            item.isAttack = true;
        }

        this.score += score;

        // ������ ���� ID ������Ʈ�� ������
        enemyGameManager = enemyGameManager == null ? GameObject.Find($"GameManager(Clone)_{enemyID}").GetComponent<GameManager>() : enemyGameManager;
        enemyBlockPanel = enemyBlockPanel == null ? GameObject.Find($"player(Clone)_{enemyID}").transform.Find("player").GetComponent<PlayerPanel>() : enemyBlockPanel;

        // �� ü���� ����
        enemyGameManager.ChangeHealthTrigger(decHealth);
        enemyBlockPanel.isHit = true;

        // �� ü���� ����
        if(nowHealth + incHealth <= maxHealth)
            ChangeHealthTrigger(incHealth);
    }

    public void ChangeHealthTrigger(int healthValue)
    {
        pv.RPC("ChangeHealth", RpcTarget.All, healthValue);
    }

    [PunRPC]
    public void ChangeHealth(int healthValue)
    {
        nowHealth += healthValue;
        float incHealthBarValue = healthValue / maxHealth < 0 ? (healthValue / maxHealth) * -1 : healthValue / maxHealth;

        // ���̴� �̺�Ʈ
        if(healthValue < 0)
            StartCoroutine(HealthEvent(incHealthBarValue, -0.01f));
        else 
            StartCoroutine(HealthEvent(incHealthBarValue, 0.01f));

        if (nowHealth <= 0)
        {
            manager.EndGameTrigger();
            isLose = true;
            myBlockPanel.isDead = true;
        }
    }

    IEnumerator HealthEvent(float incHealthBarValue, float weight)
    {
        yield return new WaitForSeconds(0.15f);

        incHealthBarValue -= 0.01f;

        if (incHealthBarValue > 0)
        {
            healthbar.size += weight;
            StartCoroutine(HealthEvent(incHealthBarValue, weight));
        }
    }

    public void EndGame()
    {
        // �� �ڽ��� ���ӿ�����Ʈ���Ը� Ʈ���� �ߵ�
        if (pv.IsMine)
        {
            pause.SetActive(true);

            if (isLose)
            {
                panel_result.transform.Find("img_result").GetComponent<Image>().sprite = resourceManager.resultText[0];
            }
            else
            {
                panel_result.transform.Find("img_result").GetComponent<Image>().sprite = resourceManager.resultText[1];
            }

            panel_result.gameObject.SetActive(true);
            panel_result.SetBool("isResult", true);
        }
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            if (!initSetting)
            {
                stream.SendNext(nickName);
                stream.SendNext(characterIndex);
                initSetting = true;
            }
            stream.SendNext(isStart);
            stream.SendNext(score);
        }
        else
        {
            nickName = nickName.IsNullOrEmpty() ? (string)stream.ReceiveNext() : nickName;
            characterIndex = characterIndex == -1 ? (int)stream.ReceiveNext() : characterIndex;
            isStart = (bool)stream.ReceiveNext();
            score = (int)stream.ReceiveNext();
        }
    }

    BlockInfor GetBlocksToIndex(Vector2 index)
    {
        return blocks[(int)index.y][(int)index.x];
    }

    public static bool isEndAni(Animator animator, string name, float time)
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(name) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= time;
    }

    public static void PlayAudio(AudioSource effectAudioSource, AudioClip clip)
    {
        effectAudioSource.clip = clip;
        effectAudioSource.Play();
    }

    public static void ChangeScreen(string text, bool isFullScreen)
    {
        string[] cutResolution = text.Split('x');
        int[] convert = Array.ConvertAll<string, int>(cutResolution, x => int.Parse(x));
        Screen.SetResolution(convert[0], convert[1], isFullScreen);
    }

    public void OnApplicationQuit()
    {
        manager.ReturnLobby();
    }
}
