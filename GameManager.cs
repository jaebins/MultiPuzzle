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
    // 내부 인스펙터
    PhotonView pv;

    // 외부 오브젝트
    public Camera myCamera;
    public Manager manager;
    MainManager mainManager;
    AudioSource effectAudioSource;
    public ResourceManager resourceManager;
    public GameManager enemyGameManager;
    
    // 인게임 오브젝트
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

    // 게임 환경
    public BlockInfor[][] blocks;
    public Vector2[][] blocksPos;
    public int myID = 0;
    int enemyID = 0;
    public string nickName = string.Empty;
    public int characterIndex = -1;

    // 게임 진행 관련 변수
    List<Vector2> selectedIndex = new List<Vector2>();
    int score;
    float maxHealth = 25;
    public float nowHealth;
    float flowStartCount = 0.2f;
    bool initSetting;
    public bool isStart;
    bool isLose;

    // 랭크별 닉네임 휘장
    // 로비에 방 목록 만들기
    // 빌드할 때는 전체화면

    void Start()
    {
        pv = GetComponent<PhotonView>();

        // 포톤 아이디 할당
        myID = int.Parse(pv.ViewID.ToString()[0].ToString());
        enemyID = myID == 1 ? 2 : 1;

        // 이름, 체력 설정
        name += "_" + (int)myID;
        nowHealth = maxHealth;

        blocks = new BlockInfor[(int)Setting.BLOCK_LENGTH.y][];
        blocksPos = new Vector2[(int)Setting.BLOCK_LENGTH.y][];

        LoadObject_before();
        StartCoroutine(LoadObject_after()); // 오브젝트 생성 딜레이

        //joinText.text = objectManager.blocks[0].GetComponent<SpriteRenderer>().sprite.name;
    }

    void LoadObject_before()
    {
        // Object 로드
        mainManager = GameObject.Find("MainManager").GetComponent<MainManager>();
        resourceManager = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();
        effectAudioSource = GameObject.Find("EffectSoundManager").GetComponent<AudioSource>();
        myCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        background = GameObject.Find("background").GetComponent<SpriteRenderer>();
        background.sprite = resourceManager.background[UnityEngine.Random.Range(0, resourceManager.background.Length)]; // 랜덤 배경

        // UI 로드
        canvas = GameObject.Find("Canvas");
        nickNameText = canvas.transform.Find($"player_{myID}").Find($"panel_playerName{myID}").Find($"text_playerName{myID}").GetComponent<TextMeshProUGUI>();
        scoreText = canvas.transform.Find($"player_{myID}").Find($"panel_playerScore{myID}").Find($"text_playerScore{myID}").GetComponent<TextMeshProUGUI>();
        img_player = canvas.transform.Find($"player_{myID}").Find($"character_player{myID}").GetComponent<Image>();
        
        if (pv.IsMine){ // 해당 오브젝트가 내꺼라면 지금 적용하고 캐릭터 인덱스 전송
            // 나중에 수정
            characterIndex = myID;
            nickName = mainManager.nickName;
            characterIndex = mainManager.chatacterIndex;

            // 내 패널 표시 아이콘
            GameObject isMeIcon = canvas.transform.Find($"player_{myID}").Find($"img_isMePlayer{myID}").gameObject;
            isMeIcon.SetActive(true);

            // 게임 정지 배경
            pause = canvas.transform.Find($"pause").gameObject;
            pause.SetActive(false);

            // 카운트 다운
            text_startCnt = canvas.transform.Find($"text_startCnt").GetComponent<Animator>();
            text_startCnt.gameObject.SetActive(false);

            // 게임 결과
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

        if (pv.IsMine) // 로컬일때는 블럭을 생성해 block 배열에 넣음
            InsertBlock();
        else // 원격일때는 원래 생성되있던 블럭들을 block 배열에 넣음, 원격 플레이어 생성 동기화를 위해 0.5초 딜레이
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
                // 랜덤의 블록 스프라이트 적용
                BlockInfor block = PhotonNetwork.Instantiate("block", Vector2.zero, Quaternion.identity).GetComponent<BlockInfor>();

                //블록 인스턴스에서 변수를 지정 후 다른 클라이언트한테 뿌리기
                block.objName = $"{PhotonNetwork.LocalPlayer.ActorNumber}:block{(Setting.BLOCK_LENGTH.y * i) + j}";
                block.gameManagerName = name;
                block.myParent = $"player(Clone)_{PhotonNetwork.LocalPlayer.ActorNumber}";
                block.myIndex = new Vector2(j, i);
                block.spriteIndex = UnityEngine.Random.Range(0, Setting.BLOCK_SOURCE_LENGTH);

                // 로컬 오브젝트 설정
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
            // 플레이어가 들어왔다면
            if (text_startCnt.GetInteger("startCount") == 0 && PhotonNetwork.PlayerList.Length == 2)
            {
                text_startCnt.gameObject.SetActive(true);
                text_startCnt.SetTrigger("startCount");
            }

            // 시작 카운트 중
            if (isEndAni(text_startCnt, "StartCount", flowStartCount))
            {

                text_startCnt.GetComponent<TextMeshProUGUI>().text = (5 - (int)((flowStartCount * 10) / 2)).ToString();
                flowStartCount += 0.2f;

                if (flowStartCount >= 1.2f)
                {
                    PlayAudio(effectAudioSource, resourceManager.sounds["gameStart"]);
                    text_startCnt.gameObject.SetActive(false);
                    isStart = true; // 게임 시작
                }
                else PlayAudio(effectAudioSource, resourceManager.sounds["countDown"]);
            }
        }

        // 누가 나갔다면 방을 터트림
        if (isStart && PhotonNetwork.PlayerList.Length < 2)
            manager.ReturnLobby();
    }
    
    void SetAniState()
    {
        // 결과창 띄우고 난 뒤 소리 재생
        if(pv.IsMine && isEndAni(panel_result, "GameResult", 1.0f))
        {
            panel_result.SetBool("isResult", false);
            if(isLose) PlayAudio(effectAudioSource, resourceManager.sounds["lose"]);
            else PlayAudio(effectAudioSource, resourceManager.sounds["win"]);
        }
    }

    void InGame()
    {
        // 게임 시작을 했다면
        if (isStart && !manager.isEndGame)
        {
            if (pv.IsMine && Input.GetMouseButtonDown(0))
            {
                // 매번 레이캐스트 쏘는걸 방지
                Vector3 mousePos = myCamera.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mousePos, transform.forward, 1f);
                SelectBlock(hit);
            }

            scoreText.text = score.ToString();
        }

        // 캐릭터 애니메이션 송신
        if (img_player.GetComponent<Animator>().runtimeAnimatorController == null && characterIndex != -1)
            img_player.GetComponent<Animator>().runtimeAnimatorController = resourceManager.character_animC[characterIndex];
        
        // 닉네임 송신
        if (nickNameText.text.Length == 0 && !nickName.IsNullOrEmpty())
            nickNameText.text = nickName;
    }

    void SelectBlock(RaycastHit2D hit)
    {
        // 만약 내 블럭이 아닌 것을 손댄다면 패스
        if(hit && hit.transform.tag.Equals("Block"))
        {
            if (!myID.ToString().Equals(hit.transform.name[0].ToString()))
                return;
        }

        PlayAudio(effectAudioSource, resourceManager.sounds["clickBlock"]);

        // 선택
        if (hit && hit.transform.tag.Equals("Block") &&
            selectedIndex.Count == 1 && hit.transform.name.Equals(GetBlocksToIndex(selectedIndex[0]).name))
        {
            ResetSelectBlock(GetBlocksToIndex(selectedIndex[0]));
        }
        else if (hit && hit.transform.tag.Equals("Block") && 
            selectedIndex.Count == 0)
        {
            // 클릭한 블록 가져오기
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

        // 인자로 넘어온 hit와 현재 선택된 블럭이 같지 않다면 실행
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

        // 만약 위치가 다 바꼈다면
        //Debug.Log((int)pos1.x + " : " + (int)goalPos1.x + " - " + (int)pos1.y + " : " + (int)goalPos1.y);
        if ((int)pos1.x <= (int)goalPos1.x + 1 && (int)pos1.x >= (int)goalPos1.x - 1 && 
            (int)pos1.y <= (int)goalPos1.y + 1 && (int)pos1.y >= (int)goalPos1.y - 1)
        {
            // 위치 값 고정을 위해
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

            // 가로 세로 둘다 성공이라면 추가 점수, 데미지
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

        // 데미지 입은 ID 오브젝트들 가져옴
        enemyGameManager = enemyGameManager == null ? GameObject.Find($"GameManager(Clone)_{enemyID}").GetComponent<GameManager>() : enemyGameManager;
        enemyBlockPanel = enemyBlockPanel == null ? GameObject.Find($"player(Clone)_{enemyID}").transform.Find("player").GetComponent<PlayerPanel>() : enemyBlockPanel;

        // 적 체력은 감소
        enemyGameManager.ChangeHealthTrigger(decHealth);
        enemyBlockPanel.isHit = true;

        // 내 체력은 증가
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

        // 깎이는 이벤트
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
        // 나 자신의 게임오브젝트에게만 트리거 발동
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
