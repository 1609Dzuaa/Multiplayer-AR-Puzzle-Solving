using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownController : MonoBehaviour
{
    [SerializeField] TMP_Dropdown _dropdown;

    public void GetDropdownValue()
    {
        int index = _dropdown.value;
        string selectedOption = _dropdown.options[index].text;
        Debug.Log("Val: " + selectedOption);
    }
}
