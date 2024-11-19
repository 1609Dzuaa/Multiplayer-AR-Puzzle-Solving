using System.Runtime.CompilerServices;
using UnityEngine;
using static GameEnums;

public class ChestInteraction : MonoBehaviour
{
    [SerializeField] Transform _parent;
    [SerializeField] ParticleSystem _psConfetti;
    [SerializeField] ParticleSystem _psSparkle;
    [SerializeField] Transform _confettiPosition;

    private Animator _anim;
    Question _questInfo;
    private bool _isOpened = false;
    const int STATE_ROTATION = 1;
    const int STATE_OPEN = 2;
    const int STATE_CLOSED = 3;

    private void Awake()
    {
        EventsManager.Instance.Subcribe(EventID.OnReceiveQuestInfo, ReceiveQuestInfo);
    }

    void Start()
    {
        _anim = GetComponent<Animator>();
        _psSparkle.Play();
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubcribe(EventID.OnReceiveQuestInfo, ReceiveQuestInfo);
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
        //hiện tại đang có bug nếu quest 1 đằng nhưng track ra cái chưa có quest thì nó hiện no hint left
        //trong khi vẫn còn 1 hint
        //audio của 1 trong 3 thằng đang bị disable đầu game
        QuestManager.Instance.RemoveQuest(_questInfo);
        EventsManager.Instance.Notify(EventID.OnTrackedImageSuccess, _questInfo);
        UIManager.Instance.TogglePopup(EPopupID.PopupReward, true);
        //QuestManager.Instance.RemoveQuest(_questInfo);
    }
}
