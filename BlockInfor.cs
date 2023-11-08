using Photon.Pun;
using System;
using UnityEngine;

public class BlockInfor : MonoBehaviourPunCallbacks, IPunObservable
{
    // �ܺ� ��ũ��Ʈ
    public GameManager gameManager;

    // ���� �ν�����
    PhotonView pv;
    public SpriteRenderer spriteRenderer;
    public Animator anime;

    // ���� ����
    int startCnt = 0;
    bool isInitSetting;

    // ���� �����͵�
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
        // �ٸ� �÷��̾ �� �����͸� �޾Ҵٸ�
        if (gameManager != null && gameManager.isStart)
        {
            if (startCnt == 0) startCnt = 1;

            ChagneSprite();
            SetAniState();
        }
    }

    void ChagneSprite()
    {
        // �ִϸ��̼� ���������� animator���� contorller�� �ݵ�� �������, ���� ��������Ʈ ������ ����
        bool isSameSprite = gameManager.resourceManager.block_sprites[spriteIndex].name.Equals(spriteRenderer.sprite.name);
        if (!isSameSprite && !isBomb)
            anime.runtimeAnimatorController = null;

        // ���� �ִϸ��̼��� ������ ��������Ʈ, ũ�� ����
        spriteRenderer.sprite = !isSameSprite ? gameManager.resourceManager.block_sprites[spriteIndex] : spriteRenderer.sprite;
        transform.localScale = Setting.BLOCK_SIZE;

        anime.runtimeAnimatorController = anime.runtimeAnimatorController == null ? gameManager.resourceManager.block_animC : anime.runtimeAnimatorController;
    }

    void SetAniState()
    {
        // ���� �ִϸ��̼��� �����ٸ�
        if (GameManager.isEndAni(anime, "block_bomb", 1.0f))
        {
            isBomb = false;
            isShow = true;

            // �ٸ� �� Ÿ�� �ҷ�����
            if (pv.IsMine) 
                spriteIndex = UnityEngine.Random.Range(0, Setting.BLOCK_SOURCE_LENGTH);
        }
        // �� ���ȯ �ִϸ��̼��� �����ٸ�
        if (GameManager.isEndAni(anime, "block_show", 1.0f))
            isShow = false;

        // �� ó�� ��ȯ �ִϸ��̼��� �����ٸ�
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
        // �ڽ��� �� �����͵��� �ٸ� �÷��̾�� ����
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

            //Debug.Log("����:" + objName);
        }
        // ���۹��� ������ ����
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
            

            //Debug.Log("����:" + objName);
        }
    }
}
