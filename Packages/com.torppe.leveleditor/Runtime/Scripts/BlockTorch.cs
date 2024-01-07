using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlockTorch : Block, IDeactivatable, IPointerClickHandler
{
    public static Action<int> OnReductionRateChanged;

    [SerializeField]
    private Transform _endpoint;

    [SerializeField]
    private ConfigurationUi _configurationUi;

    private InputField _reductionRateInput;

    private InputField _increaseAmountInput;

    private LineRenderer _lineRenderer;

    public bool Deactivated { get; set; }

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        var existingTorches = FindObjectsOfType<BlockTorch>();
        var reductionRate = 10;

        foreach (var existingTorch in existingTorches)
        {
            if (existingTorch == this)
                continue;

            reductionRate = existingTorch._reductionRateInput.Value;
            break;
        }

        _reductionRateInput = _configurationUi.AddField("Reduction Rate", reductionRate, (value) => OnReductionRateChanged?.Invoke(value));
        _increaseAmountInput = _configurationUi.AddField("Increase Amount", 25);

        OnReductionRateChanged += (value) => _reductionRateInput.SetValueWithoutCallback(value);
    }

    private void OnDisable()
    {
        OnReductionRateChanged -= (value) => _reductionRateInput.SetValueWithoutCallback(value);
    }

    private void Update()
    {
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, _endpoint.position);
    }

    public override void Load(BlockData blockData)
    {
        var data = (BlockTorchData)blockData;

        base.Load(data);

        _reductionRateInput.Value = data.ReductionRate;
        _increaseAmountInput.Value = data.IncreaseAmount;
    }

    public override void Save()
    {
        base.Save();

        BlockTorchData data = new BlockTorchData();
        data.Copy(Data);
        data.Function = "torch";
        data.ReductionRate = _reductionRateInput.Value;
        data.IncreaseAmount = _increaseAmountInput.Value;

        Data = data;
    }

    public void ToggleConfigurationUi()
    {
        _configurationUi.gameObject.SetActive(!_configurationUi.gameObject.activeSelf);
    }

    public void Deactivate()
    {
        Deactivated = true;
        _lineRenderer.enabled = false;
        _endpoint.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleConfigurationUi();
    }
}

[Serializable]
public class BlockTorchData : BlockData
{
    public int ReductionRate;
    public int IncreaseAmount;
}


