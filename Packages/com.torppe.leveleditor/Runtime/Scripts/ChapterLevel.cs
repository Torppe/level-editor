using Newtonsoft.Json;
using UnityEngine;
using static LevelGenerator;

public class ChapterLevel : MonoBehaviour
{
    public string FileName;

    private LevelData _data;

    public void Load(LevelData data)
    {
        _data = data;
    }
}
