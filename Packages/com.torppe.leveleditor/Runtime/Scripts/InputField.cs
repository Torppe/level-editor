using TMPro;
using UnityEngine;

public class InputField : MonoBehaviour
{
    public string Label
    {
        get => _label;
        set
        {
            _label = value;
            LabelComponent.text = value.ToString();
        }
    }
    public int Value
    {
        get => _value;
        set
        {
            _value = value;
            InputFieldComponent.text = value.ToString();
        }
    }

    [SerializeField]
    private TMP_Text LabelComponent;
    [SerializeField]
    private TMP_InputField InputFieldComponent;

    private int _value;
    private string _label;

    void OnEnable()
    {
        InputFieldComponent.onValueChanged.AddListener((input) =>
        {
            if (int.TryParse(input, out int result))
            {
                Value = result;
            }
        });
    }

    void OnDisable()
    {
        InputFieldComponent.onValueChanged.RemoveAllListeners();
    }
}


