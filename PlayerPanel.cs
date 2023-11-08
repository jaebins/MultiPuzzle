using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;

public class PlayerPanel : MonoBehaviourPunCallbacks, IPunObservable
{
    // 내부 인스펙터
    SpriteRenderer spriteRenderer;
    public Animator animator;
    PhotonView pv;

    // 외부 오브젝트
    GameManager gameManager;

    // 게임 진행 변수
    int myID;
    public bool isHit;
    public bool isDead;
    bool isShowResult;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        pv = transform.parent.GetComponent<PhotonView>();

        myID = int.Parse(pv.ViewID.ToString()[0].ToString());
        transform.parent.name += $"_{myID}";
        gameManager = GameObject.Find($"GameManager(Clone)_{myID}").GetComponent<GameManager>();

        if(myID == 1) transform.parent.position = Setting.PANEL_POS_1;
        else if (myID == 2) transform.parent.position = Setting.PANEL_POS_2;

        spriteRenderer.sprite = gameManager.resourceManager.playerPanel[myID - 1][0];
        animator.runtimeAnimatorController = gameManager.resourceManager.playerPanel_animC[myID - 1][0];
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = gameManager.resourceManager.playerPanel[myID - 1][1];
        transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController = gameManager.resourceManager.playerPanel_animC[myID - 1][1];
    }

    private void Update()
    {
        if (GameManager.isEndAni(animator, "player_hit", 1.0f))
            isHit = false;

        if(GameManager.isEndAni(animator, "player_dead", 1.0f) && !isShowResult)
        {
            isShowResult = true;
            gameManager.manager.ShowResult();
        }

        animator.SetBool(nameof(isHit), isHit);
        animator.SetBool(nameof(isDead), isDead);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isDead);
        }
        else
        {
            isDead = (bool)stream.ReceiveNext();
        }
    }
}
