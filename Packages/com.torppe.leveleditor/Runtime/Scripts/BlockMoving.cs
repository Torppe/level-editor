using System;
using TMPro;
using UnityEngine;

public class BlockMoving : Block, IDeactivatable
{
    [SerializeField]
    private Transform _endpoint;
    [SerializeField]
    private TMP_Text _endpointText;
    private int _speed = 3;
    private LineRenderer _lineRenderer;

    public bool Deactivated { get; set; }

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
        if (Deactivated)
            return;

        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, _endpoint.position);
    }

    public override void Load(BlockData blockData)
    {
        var data = (BlockMovingData)blockData;

        base.Load(data);
        _endpoint.position = transform.position + (Vector3)data.EndpointRelativePosition;
        SetSpeed(data.Speed);

        if (data.Deactivated)
            Deactivate();
    }

    public override void Save()
    {
        base.Save();

        BlockMovingData data = new BlockMovingData();
        data.Copy(Data);
        data.Function = "moving";
        data.EndpointRelativePosition = _endpoint.position - transform.position;
        data.Speed = _speed;
        data.Deactivated = Deactivated;

        Data = data;
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

    public void Deactivate()
    {
        Deactivated = true;
        _lineRenderer.enabled = false;
        _endpoint.gameObject.SetActive(false);
        _endpoint.GetComponent<DraggableUIElement>().OnClick -= ChangeSpeed;
    }
}

[Serializable]
public class BlockMovingData : BlockData
{
    public bool Deactivated;
    public int Speed;
    public Vector2 EndpointRelativePosition;
}
