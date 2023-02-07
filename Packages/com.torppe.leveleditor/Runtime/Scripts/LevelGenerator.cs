using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelGenerator : Generator
{
    public FunctionToBlockMapper BlockDatabase => _functionToBlockMapper;
    public Block SelectedBlock;

    [SerializeField]
    private Camera _mainCamera;
    [SerializeField]
    private FunctionToBlockMapper _functionToBlockMapper;
    [SerializeField]
    private GameObject _brush;
    [SerializeField]
    private GameObject _gridPrefab;
    [SerializeField]
    private bool _edgeScrollEnabled = false;

    private (Vector2Int appliedValue, Vector2Int editedValue) _gridSize = new() { appliedValue = new Vector2Int(200, 200), editedValue = new Vector2Int(200, 200) };

    private bool _alternateActionHeld = false;
    private bool _clickHeld = false;
    private bool _rightClickHeld = false;


    private Transform _uiElement = null;

    private Dictionary<Vector2Int, Block> _blocks = new Dictionary<Vector2Int, Block>();
    private List<Renderer> _gridObjects = new List<Renderer>();

    void OnEnable()
    {
        DraggableUIElement.OnClickUI += (uiElement) => _uiElement = uiElement;
    }
    void OnDisable()
    {
        DraggableUIElement.OnClickUI -= (uiElement) => _uiElement = uiElement;
    }

    void Awake()
    {
        _saveSubFolder = "Levels";
        SelectedBlock = _functionToBlockMapper.Blocks.First();
        LoadGrid(_gridSize.appliedValue.x, _gridSize.appliedValue.y);
    }

    void Update()
    {
        if (!_editing)
            return;

        MoveCamera();
        MoveBrush();
        MoveUIElement();
        SetBlocks();
    }

    public void OnClick(InputValue value)
    {
        _clickHeld = value.isPressed;
    }

    public void OnRightClick(InputValue value)
    {
        _rightClickHeld = value.isPressed;

    }

    public void OnAlternateAction(InputValue value)
    {
        _alternateActionHeld = value.isPressed;
    }

    public void OnScrollWheel(InputValue value)
    {
        var scrollValue = value.Get<Vector2>().y;
        if (scrollValue > 0)
        {
            if (_alternateActionHeld)
                RotateBrush(1);
            else
                ChangeBrushSize(1);
        }
        else if (scrollValue < 0)
        {
            if (_alternateActionHeld)
                RotateBrush(-1);
            else
                ChangeBrushSize(-1);
        }
    }

    public override void ChangeState()
    {
        base.ChangeState();

        LevelData data = CreateLevelData();
        var positionToLevel = new Dictionary<Vector3, LevelData>() { { Vector2.zero, data } };
        OnToggleState?.Invoke(_editing, positionToLevel);
    }

    public void ChangeBrushSize(int changeAmount)
    {
        var newSize = (Vector2)_brush.transform.localScale + Vector2.one * changeAmount * 2;

        if (newSize.x <= 0)
            newSize = new Vector2(1, 1);
        else if (newSize.x > 9)
            newSize = new Vector2(9, 9);

        _brush.transform.localScale = newSize;
    }

    public void RotateBrush(int direction)
    {
        _brush.transform.Rotate(Vector3.forward * 90 * direction);
    }

    public void SetGridWidth(TMP_InputField input)
    {
        _gridSize.editedValue.x = int.Parse(input.text);
    }

    public void SetGridHeight(TMP_InputField input)
    {
        _gridSize.editedValue.y = int.Parse(input.text);
    }

    public void ApplyGridSize()
    {
        _gridSize.appliedValue = _gridSize.editedValue;

        LoadGrid(_gridSize.appliedValue.x, _gridSize.appliedValue.y);
        RemoveBlocks();
    }

    private void MoveCamera()
    {
        if (_edgeScrollEnabled)
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();

            var width = Screen.width;
            var height = Screen.height;
            var edgeSize = 10;
            var moveAmount = 50 * Time.deltaTime;

            if (mousePosition.x < edgeSize)
            {
                _mainCamera.transform.position += Vector3.left * moveAmount;
            }
            if (mousePosition.x > width - edgeSize)
            {
                _mainCamera.transform.position += Vector3.right * moveAmount;
            }
            if (mousePosition.y > height - edgeSize)
            {
                _mainCamera.transform.position += Vector3.up * moveAmount;
            }
            if (mousePosition.y < edgeSize)
            {
                _mainCamera.transform.position += Vector3.down * moveAmount;
            }
        }
    }

    private void MoveBrush()
    {
        _brush.transform.position = (Vector3Int)GetClampedMousePosition();
    }

    private void MoveUIElement()
    {
        if (_uiElement)
            _uiElement.transform.position = (Vector3Int)GetClampedMousePosition();
    }

    private void SetBlocks()
    {
        if ((!_clickHeld && !_rightClickHeld) || _uiElement || Mouse.current.position.ReadValue().x < 170)
            return;

        int offset = (int)_brush.transform.localScale.x / 2;

        for (int x = -offset; x <= offset; x++)
        {
            for (int y = -offset; y <= offset; y++)
            {
                if (_clickHeld)
                    TryAddBlock(GetClampedMousePosition() + new Vector2Int(x, y));
                else if (_rightClickHeld)
                    TryRemoveBlock(GetClampedMousePosition() + new Vector2Int(x, y));
            }
        }
    }

    private Vector2Int GetClampedMousePosition()
    {
        var mouseWorldPosition = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPosition.z = 0;

        return Vector2Int.RoundToInt(mouseWorldPosition);
    }

    private bool TryAddBlock(Vector2Int position)
    {
        if (IsOutOfBounds(position))
            return false;

        if (_blocks.ContainsKey(position))
            return false;

        Block block = Instantiate(SelectedBlock, (Vector3Int)position, _brush.transform.rotation, _rootTransform);

        //TODO
        //Prevent many players

        _blocks.Add(position, block);

        return true;
    }

    private bool TryRemoveBlock(Vector2Int position)
    {
        if (IsOutOfBounds(position))
            return false;

        if (_blocks.TryGetValue(position, out var block))
        {
            Destroy(block.gameObject);
            _blocks.Remove(position);

            return true;
        }

        return false;
    }

    private bool IsOutOfBounds(Vector2Int position)
    {
        if (position.x < 0 || position.y < 0 || position.y > _gridSize.appliedValue.y - 1 || position.x > _gridSize.appliedValue.x - 1)
            return true;

        return false;
    }

    protected override void Save(string fileName)
    {
        LevelData data = CreateLevelData();
        string json = JsonConvert.SerializeObject(data, _jsonSettings);
        File.WriteAllText($"{SaveFolder}{fileName}", json);
    }

    private LevelData CreateLevelData()
    {
        var values = _blocks.Values;

        foreach (var block in values)
            block.Save();

        LevelData data = new LevelData
        {
            size = _gridSize.appliedValue,
            blocks = values.Select(b => b.Data).ToList(),
        };

        return data;
    }

    protected override void Load(string json)
    {
        LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json, _jsonSettings);

        LoadGrid(levelData.size.x, levelData.size.y);
        LoadBlocks(levelData);
    }

    private void LoadGrid(int width, int height)
    {
        _gridSize.appliedValue = _gridSize.editedValue = new Vector2Int(width, height);

        _gridObjects.ForEach(r => Destroy(r.gameObject));
        _gridObjects.Clear();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var position = new Vector2(x, y);
                var go = Instantiate(_gridPrefab, new Vector2(x, y), Quaternion.identity, _rootTransform);
                _gridObjects.Add(go.GetComponent<Renderer>());
            }
        }
    }

    private void LoadBlocks(LevelData levelData)
    {
        RemoveBlocks();

        var prefabs = _functionToBlockMapper.Blocks;

        foreach (var blockData in levelData.blocks)
        {
            Block blockPrefab = string.IsNullOrWhiteSpace(blockData.Function) ? prefabs.First() : prefabs.Find(b => b.Data.Function.Equals(blockData.Function));

            Block instantiatedObject = Instantiate(blockPrefab);
            instantiatedObject.transform.SetParent(_rootTransform);
            instantiatedObject.Load(blockData);

            _blocks.Add(blockData.Position, instantiatedObject);
        }
    }

    private void RemoveBlocks()
    {
        foreach (var block in _blocks.Values)
            Destroy(block.gameObject);

        _blocks.Clear();
    }


    [Serializable]
    public class LevelData
    {
        public Vector2Int size;

        public List<BlockData> blocks;
    }
}