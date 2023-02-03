using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIContentLoader : MonoBehaviour
{
    [SerializeField]
    private LevelGenerator _levelGenerator;

    [SerializeField]
    private UIBlockButton _buttonPrefab;

    [SerializeField]
    private Transform _contentParent;

    void Start()
    {
        foreach (var block in _levelGenerator.BlockDatabase.Blocks)
        {
            var button = Instantiate(_buttonPrefab);
            button.transform.SetParent(_contentParent);
            button.Setup
            (
                block.Data.Function,
                () => _levelGenerator.SelectedBlock = block
            );
        }
    }
}
