using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using static LevelGenerator;
using static UnityEngine.EventSystems.PointerEventData;

public class ChapterGenerator : Generator
{
    [SerializeField]
    private ChapterLevel _levelPrefab;
    [SerializeField]
    private DeathWall _deathWallPrefab;

    private ChapterLevel _selectedLevel;
    private bool _autoScroller = false;
    private List<DeathWall> _deathWalls = new List<DeathWall>();
    private List<ChapterLevel> _levels = new List<ChapterLevel>();
    private string _levelFolder;

    private void OnEnable()
    {
        ChapterLevel.OnAnyClicked += SelectLevel;
    }
    private void OnDisable()
    {
        ChapterLevel.OnAnyClicked -= SelectLevel;
    }

    private void SelectLevel(ChapterLevel level, InputButton inputButton)
    {
        if (inputButton == InputButton.Left)
        {
            _selectedLevel?.ToggleHighlight(false);
            _selectedLevel = level;
            _selectedLevel.ToggleHighlight(true);
        }
        else if (inputButton == InputButton.Right)
        {
            if (level == _selectedLevel)
                _selectedLevel = null;

            _levels.Remove(level);
            Destroy(level.gameObject);
        }
    }

    private void Awake()
    {
        _saveSubFolder = "Chapters";
        _levelFolder = Application.dataPath + $"/Saves/Levels/";
    }

    public void LoadLevel(TMP_InputField input)
    {
        string fileName = input.text.Trim();

        if (String.IsNullOrWhiteSpace(fileName) || _levels.Find(l => fileName.Equals(l.FileName)))
        {
            return;
        }

        string path = $"{_levelFolder}{fileName}.json";

        if (!File.Exists(path))
        {
            Debug.LogError("ERROR: No file named " + path + " found!");
            return;
        }

        var levelData = JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(path), _jsonSettings);

        var level = Instantiate(_levelPrefab);
        level.Load(levelData, fileName);

        _levels.Add(level);
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

        foreach (var deathWall in data.DeathWalls)
        {
            AddDeathWall(deathWall);
        }

        foreach (var level in data.Levels)
        {
            var path = $"{SaveFolder}{level.FileName}.json";
            var levelData = JsonConvert.DeserializeObject<LevelData>(path, _jsonSettings);

            var chapterLevel = Instantiate(_levelPrefab, level.Position, Quaternion.identity);
            chapterLevel.Load(levelData, level.FileName);

            _levels.Add(chapterLevel);
        }
    }

    public void AddDeathWall()
    {
        AddDeathWall(null);
    }

    private void AddDeathWall(DeathWallData deathWallData)
    {
        // var pos = editCam.transform.position;
        var pos = Camera.main.transform.position;
        pos.z = 0;

        var deathWall = Instantiate(_deathWallPrefab, pos, Quaternion.identity, _rootTransform);
        deathWall.Load(deathWallData);

        _deathWalls.Add(deathWall);
    }

    protected override void Save(string fileName)
    {
        var data = new ChapterData
        {
            Autoscroller = _autoScroller,
            DeathWalls = _deathWalls.Select(w => new DeathWallData()).ToList(),
            Levels = _levels.Select(w => new ChapterLevelData()).ToList(),
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented, _jsonSettings);
        File.WriteAllText($"{SaveFolder}{fileName}", json);
    }

    public override void ChangeState()
    {
        base.ChangeState();

        Dictionary<Vector3, LevelData> positionToLevel = _levels.ToDictionary(l => l.transform.position, l => l.Data);
        OnToggleState?.Invoke(_editing, positionToLevel);
    }

    [Serializable]
    public class ChapterData
    {
        public bool Autoscroller = false;
        public List<ChapterLevelData> Levels = new List<ChapterLevelData>();
        public List<DeathWallData> DeathWalls = new List<DeathWallData>();
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
        public Vector2 ScalePosition;
    }
}
