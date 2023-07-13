using System;
using UnityEngine;

public class BlockFlame : Block
{
    private InputField _timeOnInput;
    private InputField _timeOffInput;

    [SerializeField]
    private Transform _endpoint;
    [SerializeField]
    private ConfigurationUi _configurationUi;
    private LineRenderer _lineRenderer;

    private void OnEnable()
    {
        _endpoint.GetComponent<DraggableUIElement>().OnClick += ToggleConfigurationUi;
    }

    private void OnDisable()
    {
        _endpoint.GetComponent<DraggableUIElement>().OnClick -= ToggleConfigurationUi;
    }

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _timeOnInput = _configurationUi.AddField("Time On", 2);
        _timeOffInput = _configurationUi.AddField("Time Off", 2);
    }

    private void Update()
    {
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, _endpoint.position);
    }

    public override void Load(BlockData blockData)
    {
        var data = (BlockFlameData)blockData;

        base.Load(data);
        _endpoint.position = transform.position + (Vector3)data.EndpointRelativePosition;

        _timeOnInput.Value = data.TimeOn;
        _timeOffInput.Value = data.TimeOff;
    }

    public override void Save()
    {
        base.Save();

        BlockFlameData data = new BlockFlameData();
        data.Copy(Data);
        data.Function = "timed_death_obstacle";
        data.TimeOn = _timeOnInput.Value;
        data.TimeOff = _timeOffInput.Value;
        data.EndpointRelativePosition = _endpoint.position - transform.position;

        Data = data;
    }

    public void ToggleConfigurationUi()
    {
        _configurationUi.gameObject.SetActive(!_configurationUi.gameObject.activeSelf);
    }
}

[Serializable]
public class BlockFlameData : BlockData
{
    public int TimeOn;
    public int TimeOff;
    public Vector2 EndpointRelativePosition;
}
