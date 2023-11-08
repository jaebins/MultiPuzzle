using Photon.Pun;
using UnityEngine;

public class PlayerEffect : MonoBehaviour, IPunObservable
{
    // 내부 인스펙터
    PhotonView pv;
    Animator animator;
    AudioSource audioSource;

    // 외부 오브젝트
    GameManager gameManager;

    int myID;
    public bool isAttack;

    void Start()
    {
        pv = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        myID = int.Parse(pv.ViewID.ToString()[0].ToString());
        transform.parent.name = transform.parent.name != $"player_attackEffect(Clone)_{myID}" ? $"player_attackEffect(Clone)_{myID}" : transform.parent.name;
        gameManager = GameObject.Find($"GameManager(Clone)_{myID}").GetComponent<GameManager>();
        
        int effectIndex = int.Parse(name[^1].ToString()[0].ToString());
        animator.runtimeAnimatorController = gameManager.resourceManager.playerEffect_animC[myID - 1][effectIndex];
    }

    private void Update()
    {
        if(GameManager.isEndAni(animator, "attackEffect", 1.0f)){
            audioSource.clip = null;
            isAttack = false;
        }
        // 하나의 이펙트만 소리 재생
        else if(name.Equals("effect1") && audioSource.clip == null && GameManager.isEndAni(animator, "attackEffect", 0.35f))
        {
            GameManager.PlayAudio(audioSource, gameManager.resourceManager.sounds["attackEffect"]);
        }

        animator.SetBool(nameof(isAttack), isAttack);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting && gameObject.activeSelf)
        {
            stream.SendNext(isAttack);
        }
        else
        {
            isAttack = (bool)stream.ReceiveNext();
        }
    }
}
