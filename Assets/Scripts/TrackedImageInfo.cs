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

    Dictionary<string, GameObject> _spawnPrefabs = new Dictionary<string, GameObject>();
    ARTrackedImageManager _trackedImageManager;
    bool _allowDisplay;

    private void Awake()
    {
        _trackedImageManager = GetComponent<ARTrackedImageManager>();
        EventsManager.Instance.Subcribe(EventID.OnPlay, AllowToPlay);

        foreach(var prefab in _placeablePrefabs)
        {
            GameObject newPrefab = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            newPrefab.name = prefab.name;
            _spawnPrefabs.Add(prefab.name, newPrefab);
            Debug.Log("name: " + prefab.name);
        }
    }

    private void OnDestroy()
    {
        EventsManager.Instance.Unsubcribe(EventID.OnPlay, AllowToPlay);
    }

    private void OnEnable()
    {
        _trackedImageManager.trackedImagesChanged += ImageChanged;
    }

    private void OnDisable()
    {
        _trackedImageManager.trackedImagesChanged -= ImageChanged;
    }

    private void AllowToPlay(object obj)
    {
        _allowDisplay = true;
    }

    private void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        if (!_allowDisplay) return;

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
            _spawnPrefabs[trackImage.name].SetActive(false);
        }
    }

    private void UpdateImage(ARTrackedImage trackImage)
    {
        string name = trackImage.referenceImage.name;
        Vector3 position = trackImage.transform.position;

        GameObject prefab = _spawnPrefabs[name];
        prefab.transform.position = position;
        prefab.SetActive(true);
        Debug.Log("active true: " + prefab);

        foreach(GameObject go in _spawnPrefabs.Values)
        {
            if (go.name != name)
            {
                go.SetActive(false);
                Debug.Log("active false: " + prefab);
            }
        }
    }

    //old script
    /*[SerializeField]
    ARTrackedImageManager m_TrackedImageManager;

    void OnEnable() => m_TrackedImageManager.trackedImagesChanged += OnChanged;

    void OnDisable() => m_TrackedImageManager.trackedImagesChanged -= OnChanged;

    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            // Handle added event
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            // Handle updated event
        }

        foreach (var removedImage in eventArgs.removed)
        {
            // Handle removed event
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ListAllImages();
    }

    void ListAllImages()
    {
        Debug.Log(
            $"There are {m_TrackedImageManager.trackables.count} images being tracked.");

        foreach (var trackedImage in m_TrackedImageManager.trackables)
        {
            Debug.Log($"Image: {trackedImage.referenceImage.name} is at " +
                      $"{trackedImage.transform.position}");
        }
    }*/
}
