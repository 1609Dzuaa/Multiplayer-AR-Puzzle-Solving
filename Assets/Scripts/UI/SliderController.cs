using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [SerializeField] bool isSFXSource;
    Slider slider;
    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    public void OnValueChanged()
    {
        float value = slider.value;
        SoundsManager.Instance.ChangeSourceVolume(value, isSFXSource);
    }
}
