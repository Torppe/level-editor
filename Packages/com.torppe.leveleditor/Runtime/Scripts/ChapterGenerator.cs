using System;
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
    private Camera _mainCamera;

    private void OnEnable()
    {
        ChapterLevel.OnAnyClicked += SelectLevel;
    }
    private void OnDisable()
    {
        ChapterLevel.OnAnyClicked -= SelectLevel;
    }

    private void Awake()
    {
        _mainCamera = Camera.main;
        _saveSubFolder = "Chapters";
        _levelFolder = Application.dataPath + $"/Saves/Levels/";
    }

    private void Update()
    {
        MoveCamera();
    }

    private void SelectLevel(ChapterLevel level, InputButton inputButton)
    {
        if (inputButton == InputButton.Left)
        {
            if (_selectedLevel)
            {
                _selectedLevel.ToggleHighlight(false);
                _selectedLevel.Data.SelectedLevel = false;
            }

            _selectedLevel = level;
            _selectedLevel.ToggleHighlight(true);
            _selectedLevel.Data.SelectedLevel = true;
        }
        else if (inputButton == InputButton.Right)
        {
            if (level == _selectedLevel)
                _selectedLevel = null;

            _levels.Remove(level);
            Destroy(level.gameObject);
        }
    }

    public void LoadLevel(TMP_InputField input)
    {
        var fileName = input.text.Trim();
        if (_levels.Find(l => fileName.Equals(l.FileName)))
            return;
        // LoadLevel(fileName, Vector2.zero);
        FindFreeSpaceAndLoadLevel(fileName);
    }

    public void LoadLevel(string fileName, Vector2 position)
    {
        if (String.IsNullOrWhiteSpace(fileName))
            return;

        string path = $"{_levelFolder}{fileName}.json";

        if (!File.Exists(path))
        {
            Debug.LogError("ERROR: No file named " + path + " found!");
            return;
        }

        var levelData = JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(path), _jsonSettings);

        var level = Instantiate(_levelPrefab, position, Quaternion.identity);
        level.Load(levelData, fileName);

        _levels.Add(level);
        _disableOnPlay.Add(level.gameObject);
    }

    public void FindFreeSpaceAndLoadLevel(string fileName)
    {
        if (String.IsNullOrWhiteSpace(fileName))
            return;

        string path = $"{_levelFolder}{fileName}.json";

        if (!File.Exists(path))
        {
            Debug.LogError("ERROR: No file named " + path + " found!");
            return;
        }

        var levelData = JsonConvert.DeserializeObject<LevelData>(File.ReadAllText(path), _jsonSettings);

        var instantiatedLevel = Instantiate(_levelPrefab, Vector2.zero, Quaternion.identity);
        instantiatedLevel.Load(levelData, fileName);

        instantiatedLevel.transform.position = FindFreeSpace(instantiatedLevel);

        _levels.Add(instantiatedLevel);
        _disableOnPlay.Add(instantiatedLevel.gameObject);
    }

    private Vector2 FindFreeSpace(ChapterLevel level)
    {
        if (_levels.Count == 0)
        {
            return Vector2.zero;
        }

        var combinedBoundingBoxCenter = _levels.Aggregate(Vector3.zero, (acc, x) => acc + x.transform.position) / _levels.Count;
        var combinedBoundingBox = new Bounds(combinedBoundingBoxCenter, Vector3.zero);
        var levelBoundingBoxes = new List<Bounds>();

        foreach (var existingLevels in _levels)
        {
            Vector3 size = (Vector3Int)existingLevels.Data.Size;
            size.z = 1;
            var bounds = new Bounds(existingLevels.transform.position, size);

            combinedBoundingBox.Encapsulate(bounds);
            levelBoundingBoxes.Add(bounds);
        }

        var freeSpaceFound = false;
        var position = Vector2.zero;
        Vector2Int levelSize = level.Data.Size;
        Vector2Int halfSize = levelSize / 2;

        for (float x = combinedBoundingBox.min.x; x < combinedBoundingBox.max.x; x++)
        {
            for (float y = combinedBoundingBox.min.y; y < combinedBoundingBox.max.y; y++)
            {
                var point = new Vector2(x, y) + halfSize;
                var newLevelBounds = new Bounds(point, (Vector3Int)levelSize);

                if (levelBoundingBoxes.Any(b => b.Intersects(newLevelBounds)))
                    continue;

                position = point;
                freeSpaceFound = true;
                break;
            }

            if (freeSpaceFound)
                break;
        }

        if (!freeSpaceFound)
        {
            position = (Vector2)combinedBoundingBox.max + halfSize;
        }

        return position;
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

        foreach (ChapterLevelData level in data.Levels)
        {
            LoadLevel(level.FileName, level.Position);
        }
    }

    public void AddDeathWall()
    {
        AddDeathWall(null);
    }

    private void AddDeathWall(DeathWallData deathWallData)
    {
        var pos = _mainCamera.transform.position;
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
            Levels = _levels.Select(l => new ChapterLevelData { FileName = l.FileName, Position = l.transform.position }).ToList(),
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

    public void SwitchEditor(bool confirm)
    {
        if (!confirm)
        {
            UIConfirmationModal.OnConfirm?.Invoke("Are you sure you want to switch the editor? Your unsaved progress will be lost.", () => SwitchEditor(true));
            return;
        }

        OnEditorSwitch?.Invoke("LevelGenerator");
    }

    public void OnCenterCamera()
    {
        var camPos = _mainCamera.transform.position;
        _mainCamera.transform.position = new Vector3(0, 0, camPos.z);
    }

    private void MoveCamera()
    {
        var moveAmount = 500 * Time.deltaTime;
        _mainCamera.transform.position += Utils.GetMouseEdgeScrollDirection() * moveAmount;
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
