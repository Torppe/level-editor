using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIBlockButton : MonoBehaviour
{
    public void Setup(string name, params Action[] actions)
    {
        var text = GetComponentInChildren<TMP_Text>();

        if (text)
        {
            text.text = name;
        }

        foreach (var action in actions)
        {
            GetComponent<Button>().onClick.AddListener(new UnityAction(action));
        }
    }
}