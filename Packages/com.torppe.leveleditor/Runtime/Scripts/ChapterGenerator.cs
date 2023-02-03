using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class ChapterGenerator : Generator
{
    private GameObject _selectedLevel;
    private bool _autoScroller = false;
    private List<GameObject> _deathWalls = new List<GameObject>();
    private List<GameObject> _levels = new List<GameObject>();


    private void Awake()
    {
        _saveSubFolder = "Chapters";
    }

    protected override void Load(string json)
    {
        var data = JsonConvert.DeserializeObject<ChapterData>(json, _jsonSettings);

        _autoScroller = data.Autoscroller;

        _deathWalls.ForEach(l => Destroy(l.gameObject));
        _deathWalls.Clear();

        _levels.ForEach(l => Destroy(l.gameObject));
        _levels.Clear();

        _selectedLevel = null;
    }

    protected override void Save(string fileName)
    {
        var data = new ChapterData
        {
            Autoscroller = _autoScroller,
            Deathwalls = _deathWalls.Select(w => new DeathWallData()).ToList(),
            Levels = _levels.Select(w => new ChapterLevelData()).ToList(),
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented, _jsonSettings);
        File.WriteAllText($"{SaveFolder}{fileName}", json);
    }

    [Serializable]
    public class ChapterData
    {
        public bool Autoscroller = false;
        public List<ChapterLevelData> Levels = new List<ChapterLevelData>();
        public List<DeathWallData> Deathwalls = new List<DeathWallData>();
    }

    [Serializable]
    public class ChapterLevelData
    {
        public Vector2 Position;
        public string FileName;
    }

    [Serializable]
    public class DeathWallData
    {
        public Vector2 Position;
        public Vector2 Endpoint;
        public Vector2 Scaleposition;
    }
}
