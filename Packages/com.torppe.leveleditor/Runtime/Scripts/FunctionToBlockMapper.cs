using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FunctionToBlockMapper", menuName = "ScriptableObjects/FunctionToBlockMapper")]
public class FunctionToBlockMapper : ScriptableObject
{
    public List<Block> Blocks;
}
