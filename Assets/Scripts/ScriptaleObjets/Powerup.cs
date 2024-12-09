using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Powerup", order = 2)]
public class Powerup : ScriptableObject
{
    public string PowerupName;
    public Sprite PowerupIcon;
    public int Price;
    public string Description;
}
