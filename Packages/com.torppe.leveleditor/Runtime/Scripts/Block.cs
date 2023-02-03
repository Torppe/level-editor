using System;
using UnityEngine;

public class Block : MonoBehaviour
{
    public BlockData Data = new BlockData();

    public virtual void Save()
    {
        Save<BlockData>();
    }

    public virtual void Load(BlockData blockData)
    {
        Load<BlockData>(blockData);
    }

    protected void Save<T>() where T : BlockData, new()
    {
        var function = Data.Function;
        var groupId = Data.GroupId;

        Data = new T()
        {
            Position = Vector2Int.RoundToInt(transform.position),
            Rotation = transform.eulerAngles,
            Scale = transform.localScale,
            Function = function,
            GroupId = groupId
        };
    }

    protected void Load<T>(T data) where T : BlockData
    {
        Data = data;

        transform.position = (Vector3Int)data.Position;
        transform.eulerAngles = data.Rotation;
        transform.localScale = data.Scale;
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
    public object FunctionData;
}

