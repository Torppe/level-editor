using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public abstract class Generator : MonoBehaviour
{
    public static Action<bool> OnPlay;
    [SerializeField]
    private List<GameObject> _disableOnPlay = new List<GameObject>();
    [SerializeField]
    private TMP_Text changeStateButtonText;

    [SerializeField]
    protected Transform _rootTransform;

    protected bool _editing = true;
    public string SaveFolder { get; private set; }
    protected string _saveSubFolder;

    protected JsonSerializerSettings _jsonSettings;

    public virtual void Start()
    {
        if (string.IsNullOrEmpty(_saveSubFolder))
            throw new ArgumentNullException("No save folder assigned!");

        SaveFolder = Application.dataPath + $"/Saves/{_saveSubFolder}/";
        _jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, TypeNameHandling = TypeNameHandling.Auto };
    }

    public virtual void Save(TMP_InputField inputField)
    {
        if (String.IsNullOrWhiteSpace(inputField.text))
        {
            Debug.LogError("ERROR: Can't save without a file name!");
            return;
        }

        var fileName = inputField.text.Trim() + ".json";

        var success = TrySave(fileName);
        if (!success)
            UIConfirmationModal.OnConfirm?.Invoke("File with that name already exists. Are you sure you want to overwrite it?", () => TrySave(fileName, true));
    }

    public virtual void Load(TMP_InputField inputField)
    {
        if (String.IsNullOrWhiteSpace(inputField.text))
        {
            Debug.LogError("ERROR: Can't load without a file name!");
            return;
        }

        string fileName = inputField.text.Trim() + ".json";
        string path = SaveFolder + fileName;

        if (!File.Exists(path))
        {
            Debug.LogError("ERROR: No file named " + fileName + " found!");
            return;
        }

        Load(File.ReadAllText(path));
    }

    public virtual void ChangeState()
    {
        _editing = !_editing;

        foreach (var o in _disableOnPlay)
            o.SetActive(_editing);

        changeStateButtonText.text = _editing ? "Play" : "Edit";

        OnPlay?.Invoke(_editing);
    }

    public virtual void Exit(bool confirm)
    {
        if (!confirm)
        {
            UIConfirmationModal.OnConfirm?.Invoke("Are you sure you want to exit?", () => Exit(true));
            return;
        }

        Application.Quit();
    }

    private bool TrySave(string fileName, bool overwrite = false)
    {
        if (!Directory.Exists(SaveFolder))
        {
            Directory.CreateDirectory(SaveFolder);
        }

        if (!overwrite && File.Exists(SaveFolder + fileName))
        {
            Debug.LogError("ERROR: There is already a save file with the same name!");
            return false;
        }

        Save(fileName);

        return true;
    }

    protected abstract void Save(string fileName);
    protected abstract void Load(string json);
}
