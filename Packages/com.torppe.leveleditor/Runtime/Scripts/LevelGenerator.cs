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
    private Material _groupedMaterial;
    [SerializeField]
    private bool _edgeScrollEnabled = false;
    private (Vector2Int appliedValue, Vector2Int editedValue) _gridSize = new() { appliedValue = new Vector2Int(64, 36), editedValue = new Vector2Int(64, 36) };
    private bool _groupEditing = false;
    private bool _alternateActionHeld = false;
    private bool _clickHeld = false;
    private bool _rightClickHeld = false;
    private bool _preventActions = false;
    private Transform _uiElement = null;
    private Dictionary<Vector2Int, Block> _blocks = new Dictionary<Vector2Int, Block>();
    private HashSet<string> _singularBlocks = new HashSet<string>();
    private HashSet<Block> _blocksToGroup = new HashSet<Block>();
    private Transform _gridObjectsParent;
    private Dictionary<Vector2, Renderer> _gridObjects = new Dictionary<Vector2, Renderer>();
    private Vector2 wasdMovement;

    void OnEnable()
    {
        DraggableUIElement.OnClickUI += (uiElement) => _uiElement = uiElement;
        ConfigurationUi.OnEdit += (preventActions) => _preventActions = preventActions;
    }
    void OnDisable()
    {
        DraggableUIElement.OnClickUI -= (uiElement) => _uiElement = uiElement;
        ConfigurationUi.OnEdit -= (preventActions) => _preventActions = preventActions;
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

        if (_preventActions)
            return;

        MoveBrush();
        MoveUIElement();
        SetBlocks();
    }

    public void OnNavigate(InputValue value)
    {
        wasdMovement = value.Get<Vector2>();
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

    public void OnCenterCamera()
    {
        var camPos = _mainCamera.transform.position;
        _mainCamera.transform.position = new Vector3(0, 0, camPos.z);
    }

    public override void ChangeState()
    {
        base.ChangeState();

        LevelData data = CreateLevelData();
        data.SelectedLevel = true;

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

    public void SwitchEditor(bool confirm)
    {
        if (!confirm)
        {
            UIConfirmationModal.OnConfirm?.Invoke("Are you sure you want to switch the editor? Your unsaved progress will be lost.", () => SwitchEditor(true));
            return;
        }

        OnEditorSwitch?.Invoke("ChapterGenerator");
    }

    public void SwitchEditMode(bool groupEditing)
    {
        _groupEditing = groupEditing;

        foreach (var block in _blocksToGroup)
            block.Highlight(false);

        _blocksToGroup.Clear();
    }

    public void ApplyGroup()
    {
        if (_blocksToGroup.Count == 0)
            return;

        ApplyGroup(_blocksToGroup, Guid.NewGuid().ToString());

        _blocksToGroup.Clear();
    }

    public void ApplyGroup(IEnumerable<Block> blocks, string groupId)
    {
        if (blocks.Count() == 0)
            return;

        var activatedBlock = blocks.FirstOrDefault(b => b.TryGetComponent<IDeactivatable>(out var d) && !d.Deactivated);
        if (activatedBlock == null)
            activatedBlock = blocks.First();

        foreach (var block in blocks)
        {
            block.Data.GroupId = groupId.ToString();
            block.ChangeMaterial(_groupedMaterial);

            if (block == activatedBlock)
                continue;

            if (block.TryGetComponent<IDeactivatable>(out var deactivatable))
                deactivatable.Deactivate();
        }
    }

    private void MoveCamera()
    {
        var moveAmount = 50 * Time.deltaTime;

        if (wasdMovement != Vector2.zero)
        {
            _mainCamera.transform.position += (Vector3)wasdMovement * moveAmount;
            return;
        }

        if (_edgeScrollEnabled)
        {
            _mainCamera.transform.position += Utils.GetMouseEdgeScrollDirection() * moveAmount;
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
                var clampedPosition = GetClampedMousePosition() + new Vector2Int(x, y);

                if (_clickHeld)
                {
                    if (!_groupEditing)
                        AddBlock(clampedPosition);
                    else
                        AddToGroup(clampedPosition);
                }
                else if (_rightClickHeld)
                {
                    if (!_groupEditing)
                        RemoveBlock(clampedPosition);
                    else
                        RemoveFromGroup(clampedPosition);
                }
            }
        }
    }

    private Vector2Int GetClampedMousePosition()
    {
        var mouseWorldPosition = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPosition.z = 0;

        return Vector2Int.RoundToInt(mouseWorldPosition);
    }

    private void AddBlock(Vector2Int position)
    {
        if (IsOutOfBounds(position))
            return;

        if (_blocks.ContainsKey(position))
            return;

        if (_singularBlocks.Contains(SelectedBlock.Data.Function))
            return;

        var rotation = SelectedBlock.IsRotateable ? _brush.transform.rotation : Quaternion.identity;
        Block block = Instantiate(SelectedBlock, (Vector3Int)position, rotation, _rootTransform);

        _blocks.Add(position, block);

        if (SelectedBlock.IsSingular)
            _singularBlocks.Add(SelectedBlock.Data.Function);
    }

    private void RemoveBlock(Vector2Int position)
    {
        if (IsOutOfBounds(position))
            return;

        if (_blocks.TryGetValue(position, out var block))
        {
            var groupId = block.Data.GroupId;
            if (!string.IsNullOrWhiteSpace(groupId))
            {
                RemoveGroup(groupId);
            }
            else
            {
                if (_singularBlocks.Contains(block.Data.Function))
                    _singularBlocks.Remove(block.Data.Function);

                _blocks.Remove(position);
                Destroy(block.gameObject);
            }
        }
    }

    private void RemoveGroup(string groupId)
    {
        var blocksToRemove = _blocks.Values.Where(b => b.Data.GroupId == groupId).ToArray();

        foreach (var block in blocksToRemove)
        {
            var positionInt = Vector2Int.RoundToInt(block.transform.position);
            _blocks.Remove(positionInt);
            Destroy(block.gameObject);
        }
    }

    private void AddToGroup(Vector2Int position)
    {
        if (IsOutOfBounds(position))
            return;
        if (!_blocks.TryGetValue(position, out var block))
            return;
        if (!block.IsGroupable)
            return;
        if (!string.IsNullOrEmpty(block.Data.GroupId))
            return;
        if (_blocksToGroup.Contains(block))
            return;
        if (_blocksToGroup.Count > 0 && _blocksToGroup.First().Data.Function != block.Data.Function)
            return;

        _blocksToGroup.Add(block);
        block.Highlight(true);
    }

    private void RemoveFromGroup(Vector2Int position)
    {
        if (IsOutOfBounds(position))
            return;
        if (!_blocks.TryGetValue(position, out var block))
            return;
        if (!_blocksToGroup.Contains(block))
            return;

        _blocksToGroup.Remove(block);
        block.Highlight(false);
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
            Size = _gridSize.appliedValue,
            Blocks = values.Select(b => b.Data).ToList(),
        };

        return data;
    }

    protected override void Load(string json)
    {
        LevelData levelData = JsonConvert.DeserializeObject<LevelData>(json, _jsonSettings);

        LoadGrid(levelData.Size.x, levelData.Size.y);
        LoadBlocks(levelData);
    }

    private void LoadGrid(int width, int height)
    {
        _gridSize.appliedValue = _gridSize.editedValue = new Vector2Int(width, height);

        if (!_gridObjectsParent)
        {
            _gridObjectsParent = new GameObject("grid").transform;
            _gridObjectsParent.SetParent(_rootTransform);
        }

        foreach (var o in _gridObjects)
        {
            o.Value.gameObject.SetActive(false);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var position = new Vector2(x, y);
                if (_gridObjects.TryGetValue(position, out var renderer))
                {
                    renderer.gameObject.SetActive(true);
                }
                else
                {
                    var go = Instantiate(_gridPrefab, new Vector2(x, y), Quaternion.identity, _gridObjectsParent);
                    _gridObjects.Add(position, go.GetComponent<Renderer>());
                }
            }
        }

        OnCenterCamera();
    }

    private void LoadBlocks(LevelData levelData)
    {
        RemoveBlocks();

        var prefabs = _functionToBlockMapper.Blocks.ToDictionary(b => b.Data.Function);
        var groups = new Dictionary<string, List<Block>>();

        foreach (var blockData in levelData.Blocks)
        {
            Block blockPrefab = prefabs.TryGetValue(blockData.Function, out var value) ? value : prefabs.First().Value;

            Block instantiatedObject = Instantiate(blockPrefab, _rootTransform);
            instantiatedObject.Load(blockData);

            _blocks.Add(blockData.Position, instantiatedObject);

            if (!string.IsNullOrWhiteSpace(blockData.GroupId))
            {
                string guid = blockData.GroupId;

                if (groups.TryGetValue(guid, out var functionAndBlocks))
                {
                    functionAndBlocks.Add(instantiatedObject);
                }
                else
                {
                    groups.Add(guid, new List<Block> { instantiatedObject });
                }
            }
        }

        foreach (var group in groups)
        {
            ApplyGroup(group.Value, group.Key);
        }
    }

    private void RemoveBlocks()
    {
        foreach (var block in _blocks.Values)
            Destroy(block.gameObject);

        _blocks.Clear();
        _singularBlocks.Clear();
    }

    [Serializable]
    public class LevelData
    {
        public bool SelectedLevel;
        public Vector2Int Size;
        public List<BlockData> Blocks;
    }
}