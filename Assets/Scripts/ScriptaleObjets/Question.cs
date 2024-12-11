using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Quest", order = 1)]
public class Question : ScriptableObject
{
    [Header("Thuộc tính này phải match với tên của image được track")]
    public string ImageName;

    [Header("Hint Related")]
    public string Hint;
    public string NextHint;
    public int Score;
    [Range(1, 10)] public int Round; //quest này thuộc round bnhieu
}
