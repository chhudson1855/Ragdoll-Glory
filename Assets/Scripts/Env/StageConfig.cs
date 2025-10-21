using System;
using System.Collections.Generic;
using UnityEngine;

public class StageConfig : MonoBehaviour
{
    public string StageName;
    public string StageType;
    public int DifficultyLevel;
    public int rarity;
    public bool CanPlace;
    public List<string> CanConnectWith = new List<string>();
    public List<string> CannotConnectWith = new List<string>();
}