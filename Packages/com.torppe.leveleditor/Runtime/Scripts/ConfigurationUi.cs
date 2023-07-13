using System;
using UnityEngine;

public class ConfigurationUi : MonoBehaviour
{
    public static Action<bool> OnEdit;
    [SerializeField]
    private InputField _inputFieldPrefab;
    [SerializeField]
    private Transform _parent;

    void OnEnable()
    {
        OnEdit?.Invoke(true);
    }

    void OnDisable()
    {
        OnEdit?.Invoke(false);
    }

    public InputField AddField(string label, int defaultValue = 0)
    {
        var inputField = Instantiate(_inputFieldPrefab, _parent);
        inputField.Label = label;
        inputField.Value = defaultValue;

        return inputField;
    }
}


