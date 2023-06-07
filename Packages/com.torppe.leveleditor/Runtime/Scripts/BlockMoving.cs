using System;
using TMPro;
using UnityEngine;

public class BlockMoving : Block
{
    public Vector2 EndpointPosition => _endpoint.position;

    [SerializeField]
    private Transform _endpoint;
    [SerializeField]
    private TMP_Text _endpointText;
    private int _speed = 3;
    private LineRenderer _lineRenderer;

    private void OnEnable()
    {
        _endpoint.GetComponent<DraggableUIElement>().OnClick += ChangeSpeed;
    }

    private void OnDisable()
    {
        _endpoint.GetComponent<DraggableUIElement>().OnClick -= ChangeSpeed;
    }

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, EndpointPosition);
    }

    public override void Load(BlockData blockData)
    {
        var data = (BlockMovingData)blockData;

        base.Load(data);

        _endpoint.position = transform.position + (Vector3)data.EndpointRelativePosition;
        _speed = data.Speed;
    }

    public override void Save()
    {
        base.Save();

        BlockMovingData data = new BlockMovingData();
        data.Copy(Data);
        data.Function = "moving";
        data.EndpointRelativePosition = EndpointPosition - (Vector2)transform.position;
        data.Speed = _speed;

        Data = data;
    }

    private void ChangeSpeed()
    {
        _speed += 3;
        if (_speed > 15)
            _speed = 3;

        _endpointText.text = _speed.ToString();
    }
}

[Serializable]
public class BlockMovingData : BlockData
{
    public int Speed;
    public Vector2 EndpointRelativePosition;
}
