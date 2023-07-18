using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockLava : Block
{
    [SerializeField]
    private Transform _endpoint;
    [SerializeField]
    private Transform _platform;
    [SerializeField]
    private ConfigurationUi _configurationUi;

    private InputField _ascendSpeedInput;
    private InputField _descendSpeedInput;
    private InputField _activeTimeInput;
    private InputField _deactiveTimeInput;
    private InputField _width;
    private InputField _height;

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

        _ascendSpeedInput = _configurationUi.AddField("Ascend Speed", 15);
        _descendSpeedInput = _configurationUi.AddField("Descend Speed", 20);
        _activeTimeInput = _configurationUi.AddField("Active Time", 1);
        _deactiveTimeInput = _configurationUi.AddField("Deactive Time", 3);
        _width = _configurationUi.AddField("Width", 1, (value) => ChangePlatformSize(new Vector2(value, _platform.localScale.y)));
        _height = _configurationUi.AddField("Height", 1, (value) => ChangePlatformSize(new Vector2(_platform.localScale.x, value)));
    }

    private void ChangePlatformSize(Vector2 size)
    {
        _platform.localScale = new Vector3(size.x, size.y, _platform.localScale.z);

        var newPosition = _platform.localPosition;
        newPosition.y = (size.y / 2) - 0.5f;

        _platform.localPosition = newPosition;
    }

    private void Update()
    {
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, _endpoint.position);
    }

    public override void Load(BlockData blockData)
    {
        var data = (BlockLavaData)blockData;

        base.Load(data);
        _endpoint.position = transform.position + (Vector3)data.EndpointRelativePosition;
        _ascendSpeedInput.Value = data.AscendSpeed;
        _descendSpeedInput.Value = data.DescendSpeed;
        _activeTimeInput.Value = data.ActiveTime;
        _deactiveTimeInput.Value = data.DeactiveTime;
        ChangePlatformSize(data.Size);
    }

    public override void Save()
    {
        base.Save();

        BlockLavaData data = new BlockLavaData();
        data.Copy(Data);
        data.Function = "lava_platform";
        data.AscendSpeed = _ascendSpeedInput.Value;
        data.DescendSpeed = _descendSpeedInput.Value;
        data.ActiveTime = _activeTimeInput.Value;
        data.DeactiveTime = _deactiveTimeInput.Value;
        data.EndpointRelativePosition = _endpoint.position - transform.position;
        data.Size = _platform.localScale;

        Data = data;
    }

    public void ToggleConfigurationUi()
    {
        _configurationUi.gameObject.SetActive(!_configurationUi.gameObject.activeSelf);
    }
}

[Serializable]
public class BlockLavaData : BlockData
{
    public int AscendSpeed;
    public int DescendSpeed;
    public int ActiveTime;
    public int DeactiveTime;
    public Vector2 EndpointRelativePosition;
    public Vector2 Size;
}


