using Unity.Netcode;
using UnityEngine;
using static GameEnums;

public class ChestInteraction : NetworkBehaviour
{
    [SerializeField] Transform _parent;
    [SerializeField] ParticleSystem _psConfetti;
    [SerializeField] ParticleSystem _psSparkle;
    [SerializeField] Transform _confettiPosition;
    //NetworkList<ulong> _listFastestPlayers; //store id của 3 thằng nhanh nhất


    private Animator _anim;
    Question _questInfo;
    private bool _isOpened = false;
    const int STATE_ROTATION = 1;
    const int STATE_OPEN = 2;
    const int STATE_CLOSED = 3;

    private void Awake()
    {
        EventsManager.Subscribe(EventID.OnReceiveQuestInfo, ReceiveQuestInfo);
        //_listFastestPlayers = new NetworkList<ulong>();
    }

    void Start()
    {
        _anim = GetComponent<Animator>();
        _psSparkle.Play();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        EventsManager.Unsubscribe(EventID.OnReceiveQuestInfo, ReceiveQuestInfo);
    }
    private void ReceiveQuestInfo(object obj)
    {
        _questInfo = (Question)obj;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == _parent.gameObject && !_isOpened)
                {
                    RotateChest();
                }
            }
        }
#else

    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == _parent.gameObject && !_isOpened)
            {
                RotateChest();
            }
        }
    }
#endif
    }

    private void RotateChest()
    {
        _isOpened = true;
        _anim.SetInteger("state", STATE_ROTATION);
        //Debug.Log("Chest opened! Show reward.");
    }

    public void OpenChest()
    {
        _anim.SetInteger("state", STATE_OPEN);
    }

    public void SpawnConfetti()
    {
        Instantiate(_psConfetti, _confettiPosition.position, Quaternion.identity).Play();
    }

    public void PopupReward()
    {
        //gửi thông tin của quest ở đây
        //track thành công thì bắn thông tin đi;
        //QuestManager.Instance.RemoveQuest();
        if (PowerupManager.Instance.Stake)
            PowerupManager.Instance.HintSolved = true;
        EventsManager.Notify(EventID.OnTrackedImageSuccess, _questInfo);
        UIManager.Instance.TogglePopup(EPopupID.PopupReward, true);
        if (RoundManager.Instance.IsHost)
        {
            //Debug.Log("host incre count");
            RoundManager.Instance.IncreaseCountServerRpc();
        }
        else if (RoundManager.Instance.IsOwner)
        {
            //Debug.Log("client owner incre count");
            RoundManager.Instance.IncreaseCountServerRpc();
        }
        //QuestManager.Instance.RemoveQuest(_questInfo);
    }
}
