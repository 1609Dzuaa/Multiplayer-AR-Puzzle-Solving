using UnityEngine;

public class ChestInteraction : MonoBehaviour
{
    [SerializeField] Transform _parent;
    [SerializeField] ParticleSystem _psConfetti;
    [SerializeField] Transform _confettiPosition;

    private Animator _anim;
    private bool _isOpened = false;
    const int STATE_ROTATION = 1;
    const int STATE_OPEN = 2;
    const int STATE_CLOSED = 3;

    void Start()
    {
        _anim = GetComponent<Animator>();
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
}
