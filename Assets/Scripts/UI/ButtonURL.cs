using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonURL : MonoBehaviour
{
    [SerializeField] string _URL;

    public void OnClick()
    {
        Application.OpenURL(_URL);
    }
}
