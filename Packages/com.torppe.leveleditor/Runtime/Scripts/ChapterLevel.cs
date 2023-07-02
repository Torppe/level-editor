using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static LevelGenerator;

public class ChapterLevel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public static Dictionary<string, Color> functionToColor = new Dictionary<string, Color>
    {
        {"death", Color.red},
        {"destroyOnTouch", Color.gray},
        {"moving", Color.black},
        {"oneWay", Color.yellow},
        {"player", Color.green},
        {"pullTo", Color.cyan},
        {"pullable", Color.magenta},
        {"swingable", Color.magenta},
        {"trampoline", Color.yellow},
    };
    public static Action<ChapterLevel, PointerEventData.InputButton> OnAnyClicked;
    public string FileName { get; private set; }
    public LevelData Data { get; private set; }


    [SerializeField]
    private TMP_Text _fileNameText;
    [SerializeField]
    private GameObject _highlight;

    private Vector3 _dragOffset;

    private bool _isDragging;

    private void Update()
    {
        if (_isDragging)
            Move();
    }

    private void Move()
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorldPosition.z = 0;

        var newPosition = mouseWorldPosition + _dragOffset;
        transform.position = new Vector3(Mathf.RoundToInt(newPosition.x), Mathf.RoundToInt(newPosition.y), 0);
    }

    public void Load(LevelData data, string filename)
    {
        FileName = filename;
        Data = data;

        RemoveEmptySpace(data);
        transform.localScale = new Vector3Int(data.Size.x, data.Size.y, 1);

        SetTexture(data);

        _fileNameText.text = FileName;
        _fileNameText.rectTransform.localScale = new Vector2(1 / ((float)data.Size.x / data.Size.y), 1);
    }

    private void SetTexture(LevelData data)
    {
        int height = data.Size.y;
        int width = data.Size.x;

        Color[] blank = new Color[width * height];
        for (int i = 0; i < blank.Length; i++)
        {
            blank[i] = Color.clear;
        }

        var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

        texture.filterMode = FilterMode.Point;
        texture.SetPixels(blank);

        foreach (var block in data.Blocks)
        {
            Color color = Color.white;

            if (!String.IsNullOrWhiteSpace(block.Function))
                color = functionToColor.TryGetValue(block.Function, out var value) ? value : Color.magenta;

            Vector2Int coords = block.Position;

            texture.SetPixel(coords.x, coords.y, color);
        }

        texture.Apply();

        var renderer = GetComponentInChildren<Renderer>();
        renderer.material.mainTexture = texture;
    }


    private void RemoveEmptySpace(LevelData data)
    {
        var minPos = Vector2.positiveInfinity;
        var maxPos = Vector2.negativeInfinity;

        foreach (var block in data.Blocks)
        {
            minPos = Vector2.Min(minPos, block.Position);
            maxPos = Vector2.Max(maxPos, block.Position);
        }
        var diameter = maxPos - minPos;
        var extendedDiameter = diameter + diameter.normalized;

        data.Size = new Vector2Int(Mathf.RoundToInt(extendedDiameter.x), Mathf.CeilToInt(extendedDiameter.y));

        foreach (var block in data.Blocks)
        {
            block.Position -= Vector2Int.CeilToInt(minPos);
        }

        // foreach (var functionData in grid.functionData)
        // {
        //     functionData.endPointPosition -= minPos;
        // }

        // foreach (var functionData in grid.groupFunctionData)
        // {
        //     functionData.endPointPosition -= minPos;
        // }
    }

    public void ToggleHighlight(bool toggled)
    {
        _highlight.SetActive(toggled);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _fileNameText.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _fileNameText.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isDragging = true;
        _dragOffset = transform.position - eventData.pointerCurrentRaycast.worldPosition;
        _dragOffset.z = 0;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isDragging = false;
        OnAnyClicked?.Invoke(this, eventData.button);
    }
}
