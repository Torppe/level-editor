using System;
using UnityEngine;

public class ConfigurationUi : MonoBehaviour
{
    public static Action<bool> OnEdit;
    [SerializeField]
    private InputField _inputFieldPrefab;
    [SerializeField]
    private Transform _parent;

    private int _inputFieldAmount;

    void Start()
    {
        var rectTransform = GetComponent<RectTransform>();

        var size = rectTransform.sizeDelta;
        size.y = _inputFieldAmount * 1.46f;

        rectTransform.sizeDelta = size;
    }

    void OnEnable()
    {
        OnEdit?.Invoke(true);
    }

    void OnDisable()
    {
        OnEdit?.Invoke(false);
    }

    public InputField AddField(string label, int defaultValue = 0, Action<int> onValueChanged = null)
    {
        var inputField = Instantiate(_inputFieldPrefab, _parent);
        inputField.Label = label;
        inputField.Value = defaultValue;
        if (onValueChanged != null)
            inputField.callback = onValueChanged;

        _inputFieldAmount++;

        return inputField;
    }
}


