using System;
using UnityEngine;

public class Block : MonoBehaviour
{
    public BlockData Data = new BlockData();
    public bool IsGroupable;
    [SerializeField]
    private GameObject HighlightGo;


    public virtual void Save()
    {
        var function = Data.Function;
        var groupId = Data.GroupId;

        Data = new BlockData()
        {
            Position = Vector2Int.RoundToInt(transform.position),
            Rotation = transform.eulerAngles,
            Scale = transform.localScale,
            Function = function,
            GroupId = groupId
        };
    }

    public virtual void Load(BlockData data)
    {
        Data = data;

        transform.position = (Vector3Int)data.Position;
        transform.eulerAngles = data.Rotation;
        transform.localScale = data.Scale;
    }

    public void Highlight(bool highlighted)
    {
        HighlightGo.SetActive(highlighted);
    }

    public void ApplyGroup(Material material)
    {
        HighlightGo.GetComponent<Renderer>().material = material;
    }
}

[Serializable]
public class BlockData
{
    [HideInInspector]
    public Vector2Int Position;
    [HideInInspector]
    public Vector2 Rotation;
    [HideInInspector]
    public Vector3 Scale;
    [HideInInspector]
    public string GroupId;
    public string Function;

    public void Copy(BlockData data)
    {
        Position = data.Position;
        Rotation = data.Rotation;
        Scale = data.Scale;
        GroupId = data.GroupId;
        Function = data.Function;
    }
}

