using UnityEngine;

public class ChestInteraction : MonoBehaviour
{
    private Animator _animator;
    private bool _isOpened = false;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject && !_isOpened)
                {
                    RotateChest();
                }
            }
        }
    }

    private void RotateChest()
    {
        _isOpened = true;
        _animator.SetTrigger("Rotation");
        //Debug.Log("Chest opened! Show reward.");
    }

    public void OpenChest()
    {
        _animator.SetTrigger("Open");
    }
}
