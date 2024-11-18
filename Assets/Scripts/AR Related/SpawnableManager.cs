using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class SpawnableManager : MonoBehaviour
{
    [SerializeField] ARRaycastManager _raycastManager;
    List<ARRaycastHit> _hits = new List<ARRaycastHit>();
    [SerializeField] GameObject _spawnablePrefab;

    [SerializeField] Camera _arCam;
    GameObject _spawnObject;

    // Start is called before the first frame update
    void Start()
    {
        _spawnObject = null;
        //if (_arCam != null)
            //Debug.Log("AR Cam not null");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 0)
            return;

        RaycastHit hit;
        Ray ray = _arCam.ScreenPointToRay(Input.GetTouch(0).position);

        if (_raycastManager.Raycast(Input.GetTouch(0).position, _hits))
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began && _spawnObject == null)
            {
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.CompareTag("GoldCoin"))
                    {
                        //bắn event track đc vật thể
                        _spawnObject = hit.collider.gameObject;
                        //SpawnPrefab(_hits[0].pose.position);
                    }
                    else
                    {
                        //SpawnPrefab(_hits[0].pose.position);
                    }
                }
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Moved && _spawnObject != null)
            {
                _spawnObject.transform.position = _hits[0].pose.position;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                _spawnObject = null;
            }
        }
    }

    private void SpawnPrefab(Vector3 position)
    {
        _spawnObject = Instantiate(_spawnablePrefab, position, Quaternion.identity);
    }
}
