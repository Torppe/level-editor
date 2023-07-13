using System;
using TMPro;
using UnityEngine;

public class BlockFlame : Block
{
    public Vector2 EndpointPosition => _endpoint.position;
    private int _timeOn = 2;
    private int _timeOff = 2;

    [SerializeField]
    private TMP_InputField _timeOnInput;
    [SerializeField]
    private TMP_InputField _timeOffInput;

    [SerializeField]
    private Transform _endpoint;
    [SerializeField]
    private GameObject _configurationUi;
    private LineRenderer _lineRenderer;

    private void OnEnable()
    {
        _endpoint.GetComponent<DraggableUIElement>().OnClick += ToggleConfigurationUi;
        _timeOffInput.onValueChanged.AddListener((value) =>
        {
            if (int.TryParse(value, out int result))
            {
                _timeOff = result;
            }
        });

        _timeOnInput.onValueChanged.AddListener((value) =>
        {
            if (int.TryParse(value, out int result))
            {
                _timeOn = result;
            }
        });
    }

    private void OnDisable()
    {
        _endpoint.GetComponent<DraggableUIElement>().OnClick -= ToggleConfigurationUi;
        _timeOffInput.onValueChanged.RemoveAllListeners();
        _timeOnInput.onValueChanged.RemoveAllListeners();
    }

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        _timeOffInput.text = _timeOff.ToString();
        _timeOnInput.text = _timeOn.ToString();

        _timeOffInput.onValueChanged.AddListener((value) =>
        {
            if (int.TryParse(value, out int result))
            {
                _timeOff = result;
            }
        });

        _timeOnInput.onValueChanged.AddListener((value) =>
        {
            if (int.TryParse(value, out int result))
            {
                _timeOn = result;
            }
        });
    }

    private void Update()
    {
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, EndpointPosition);
    }

    public override void Load(BlockData blockData)
    {
        var data = (BlockFlameData)blockData;

        base.Load(data);
        _endpoint.position = transform.position + (Vector3)data.EndpointRelativePosition;

        _timeOn = data.TimeOn;
        _timeOff = data.TimeOff;

        _timeOffInput.text = _timeOff.ToString();
        _timeOnInput.text = _timeOn.ToString();
    }

    public override void Save()
    {
        base.Save();

        BlockFlameData data = new BlockFlameData();
        data.Copy(Data);
        data.Function = "timed_death_obstacle";
        data.TimeOn = _timeOn;
        data.TimeOff = _timeOff;
        data.EndpointRelativePosition = EndpointPosition - (Vector2)transform.position;

        Data = data;
    }

    public void ToggleConfigurationUi()
    {
        _configurationUi.SetActive(!_configurationUi.activeSelf);
    }
}

[Serializable]
public class BlockFlameData : BlockData
{
    public int TimeOn;
    public int TimeOff;
    public Vector2 EndpointRelativePosition;
}
