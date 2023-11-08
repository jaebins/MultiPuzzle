using Photon.Pun;
using System;
using UnityEngine;

public class BlockInfor : MonoBehaviourPunCallbacks, IPunObservable
{
    // 외부 스크립트
    public GameManager gameManager;

    // 내부 인스펙터
    PhotonView pv;
    public SpriteRenderer spriteRenderer;
    public Animator anime;

    // 게임 진행
    int startCnt = 0;
    bool isInitSetting;

    // 전송 데이터들
    public string objName;
    public string gameManagerName;
    public string myParent;
    public Vector2 myIndex;
    public int spriteIndex = -1;
    public bool isSelect;
    public bool isBomb;
    public bool isShow;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anime = GetComponent<Animator>();
    }

    private void Update()
    {
        // 다른 플레이어가 블럭 데이터를 받았다면
        if (gameManager != null && gameManager.isStart)
        {
            if (startCnt == 0) startCnt = 1;

            ChagneSprite();
            SetAniState();
        }
    }

    void ChagneSprite()
    {
        // 애니메이션 오류때문에 animator에서 contorller를 반드시 빼줘야함, 폭발 스프라이트 변경은 예외
        bool isSameSprite = gameManager.resourceManager.block_sprites[spriteIndex].name.Equals(spriteRenderer.sprite.name);
        if (!isSameSprite && !isBomb)
            anime.runtimeAnimatorController = null;

        // 시작 애니메이션이 끝나면 스프라이트, 크기 적용
        spriteRenderer.sprite = !isSameSprite ? gameManager.resourceManager.block_sprites[spriteIndex] : spriteRenderer.sprite;
        transform.localScale = Setting.BLOCK_SIZE;

        anime.runtimeAnimatorController = anime.runtimeAnimatorController == null ? gameManager.resourceManager.block_animC : anime.runtimeAnimatorController;
    }

    void SetAniState()
    {
        // 폭발 애니메이션이 끝난다면
        if (GameManager.isEndAni(anime, "block_bomb", 1.0f))
        {
            isBomb = false;
            isShow = true;

            // 다른 블럭 타입 불러오기
            if (pv.IsMine) 
                spriteIndex = UnityEngine.Random.Range(0, Setting.BLOCK_SOURCE_LENGTH);
        }
        // 블럭 재소환 애니메이션이 끝난다면
        if (GameManager.isEndAni(anime, "block_show", 1.0f))
            isShow = false;

        // 블럭 처음 소환 애니메이션이 끝난다면
        if (GameManager.isEndAni(anime, "block_start", 1.0f))
        {
            startCnt = 2;
        }

        anime.SetInteger(nameof(startCnt), startCnt);
        anime.SetBool(nameof(isSelect), isSelect);
        anime.SetBool(nameof(isBomb), isBomb);
        anime.SetBool(nameof(isShow), isShow);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 자신의 블럭 데이터들을 다른 플레이어에게 전송
        if (stream.IsWriting)
        {
            if (!isInitSetting)
            {
                stream.SendNext(objName);
                stream.SendNext(gameManagerName);
                stream.SendNext(myParent);

                isInitSetting = true;
            }

            stream.SendNext(myIndex);
            stream.SendNext(spriteIndex);
            stream.SendNext(isSelect);
            stream.SendNext(isBomb);
            stream.SendNext(isShow);

            //Debug.Log("전송:" + objName);
        }
        // 전송받은 데이터 수신
        else
        {
            try
            {
                if (name.Equals("block(Clone)")) name = (string)stream.ReceiveNext();
                if (gameManager == null) gameManager = GameObject.Find((string)stream.ReceiveNext()).GetComponent<GameManager>();
                if (transform.parent == null) transform.SetParent(GameObject.Find((string)stream.ReceiveNext()).transform.Find("player"));

                myIndex = (Vector2)stream.ReceiveNext();
                spriteIndex = (int)stream.ReceiveNext();
                isSelect = (bool)stream.ReceiveNext();
                isBomb = (bool)stream.ReceiveNext();
                isShow = (bool)stream.ReceiveNext();
            } catch(Exception e)
            {
                Debug.LogError(e);
                Application.Quit();
            }
            

            //Debug.Log("받음:" + objName);
        }
    }
}
