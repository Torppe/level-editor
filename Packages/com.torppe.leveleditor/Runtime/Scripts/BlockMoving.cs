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

    public bool deactivated;

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
        if (deactivated)
            return;

        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, EndpointPosition);
    }

    public override void Load(BlockData blockData)
    {
        var data = (BlockMovingData)blockData;

        base.Load(data);

        if (data.Deactivated)
        {
            ToggleEndpoint(false);
        }
        else
        {
            _endpoint.position = transform.position + (Vector3)data.EndpointRelativePosition;
            SetSpeed(data.Speed);
        }
    }

    public override void Save()
    {
        base.Save();

        BlockMovingData data = new BlockMovingData();
        data.Copy(Data);
        data.Function = "moving";
        data.Deactivated = deactivated;

        if (!deactivated)
        {
            data.EndpointRelativePosition = EndpointPosition - (Vector2)transform.position;
            data.Speed = _speed;
        }

        Data = data;
    }

    public void ToggleEndpoint(bool toggled)
    {
        deactivated = !toggled;
        _lineRenderer.enabled = toggled;
        _endpoint.gameObject.SetActive(toggled);
        if (!toggled)
            _endpoint.GetComponent<DraggableUIElement>().OnClick -= ChangeSpeed;
    }

    private void SetSpeed(int speed)
    {
        _speed = speed;
        _endpointText.text = speed.ToString();
    }

    private void ChangeSpeed()
    {
        var newSpeed = _speed += 3;

        newSpeed += 3;
        if (newSpeed > 15)
            newSpeed = 3;

        SetSpeed(newSpeed);
    }
}

[Serializable]
public class BlockMovingData : BlockData
{
    public bool Deactivated;
    public int Speed;
    public Vector2 EndpointRelativePosition;
}
