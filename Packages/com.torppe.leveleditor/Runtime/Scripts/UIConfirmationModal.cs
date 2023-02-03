using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIConfirmationModal : MonoBehaviour
{
    public static Action<string, Action> OnConfirm;

    [SerializeField]
    private GameObject _modal;
    [SerializeField]
    private TMP_Text _description;

    private Action _callback;

    void OnEnable()
    {
        OnConfirm += Confirm;
    }

    void OnDisable()
    {
        OnConfirm -= Confirm;
    }

    public void Confirm(bool confirmed)
    {
        if (confirmed)
        {
            _callback();
            _callback = null;
        }
        _modal.SetActive(false);
    }

    void Confirm(string description, Action callback)
    {
        _callback = callback;
        _modal.SetActive(true);
        _description.text = description;
    }
}
