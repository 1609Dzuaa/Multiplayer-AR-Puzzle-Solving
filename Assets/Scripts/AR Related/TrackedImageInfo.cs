using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using static GameEnums;

[RequireComponent(typeof(ARTrackedImageManager))]
public class TrackedImageInfo : MonoBehaviour
{
    [SerializeField] GameObject[] _placeablePrefabs;

    Dictionary<string, GameObject> _dictSpawnPrefabs = new Dictionary<string, GameObject>();
    ARTrackedImageManager _trackedImageManager;

    private void Awake()
    {
        _trackedImageManager = GetComponent<ARTrackedImageManager>();
        EventsManager.Instance.Subcribe(EventID.OnTrackedImageSuccess, RemovePrefab);

        //mảng các obj sẽ spawn ra
        //trong script chest sẽ có SO của quest, dictionary bên này đổi khoá thành 
        //for _arrChest[i].SO = QuestM.ListQuest[i];
        //_dict.Add(_arrChest[i].SO.ID, _arrChest[i])
        //

        foreach(var prefab in _placeablePrefabs)
        {
            GameObject newPrefab = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            newPrefab.name = prefab.name;
            _dictSpawnPrefabs.Add(prefab.name, newPrefab);
            newPrefab.SetActive(false);
            Debug.Log("name: " + prefab.name);
        }
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubcribe(EventID.OnTrackedImageSuccess, RemovePrefab);
    }

    private void RemovePrefab(object obj)
    {
        Question questInfo = obj as Question;
        Destroy(_dictSpawnPrefabs[questInfo.ImageName]);
        _dictSpawnPrefabs.Remove(questInfo.ImageName);
        Debug.Log("remove: " +  questInfo.ImageName);
    }

    private void OnEnable()
    {
        _trackedImageManager.trackedImagesChanged += ImageChanged;
    }

    private void OnDisable()
    {
        _trackedImageManager.trackedImagesChanged -= ImageChanged;
    }

    private void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackImage in eventArgs.added)
        {
            UpdateImage(trackImage);
        }

        foreach (ARTrackedImage trackImage in eventArgs.updated)
        {
            UpdateImage(trackImage);
        }

        foreach (ARTrackedImage trackImage in eventArgs.removed)
        {
            _dictSpawnPrefabs[trackImage.name].SetActive(false);
            Debug.Log("active false, image name: " + trackImage.name);
        }
    }

    private void UpdateImage(ARTrackedImage trackImage)
    {
        string name = trackImage.referenceImage.name;
        Vector3 position = trackImage.transform.position;

        if (_dictSpawnPrefabs.ContainsKey(name))
        {
            GameObject prefab = _dictSpawnPrefabs[name];
            prefab.transform.position = position;
            prefab.SetActive(true);
            Question quest = QuestManager.Instance.ListQuest.Find(x => x.ImageName == name);

            if (!quest) Debug.Log("Question of image: " + name + " get null");
            else EventsManager.Instance.Notify(EventID.OnReceiveQuestInfo, quest);

            Debug.Log("active true: " + prefab);

            foreach (GameObject go in _dictSpawnPrefabs.Values)
            {
                if (go.name != name && go.activeSelf)
                {
                    go.SetActive(false);
                    Debug.Log("active false, goName, name: " + go.name + ", " + name);
                }
            }
        }
    }
}
