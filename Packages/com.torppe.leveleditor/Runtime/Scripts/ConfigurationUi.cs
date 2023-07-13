using UnityEngine;

public class ConfigurationUi : MonoBehaviour
{
    [SerializeField]
    private InputField _inputFieldPrefab;
    [SerializeField]
    private Transform _parent;

    public InputField AddField(string label, int defaultValue = 0)
    {
        var inputField = Instantiate(_inputFieldPrefab, _parent);
        inputField.Label = label;
        inputField.Value = defaultValue;

        return inputField;
    }
}


