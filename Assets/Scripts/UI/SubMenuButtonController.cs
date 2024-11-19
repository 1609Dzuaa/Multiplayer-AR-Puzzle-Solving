using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnums;

public class SubMenuButtonController : MonoBehaviour
{
    public void OnClick()
    {
        SoundsManager.Instance.PlaySfx(ESoundName.Button1SFX);
    }
}
