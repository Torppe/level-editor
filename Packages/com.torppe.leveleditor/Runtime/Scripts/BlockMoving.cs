using System;
using UnityEngine;

public class BlockMoving : Block
{
    public Vector2 EndpointPosition => _endpoint.position;

    [SerializeField]
    private Transform _endpoint;
    private int _speed;

    public override void Load(BlockData blockData)
    {
        var data = (BlockMovingData)blockData;
        base.Load<BlockMovingData>(data);

        _endpoint.position = data.EndpointPosition;
        _speed = data.Speed;
    }

    public override void Save()
    {
        base.Save<BlockMovingData>();

        var data = (BlockMovingData)Data;
        data.Function = "moving";
        data.EndpointPosition = EndpointPosition;
        data.Speed = _speed;
    }
}

[Serializable]
public class BlockMovingData : BlockData
{
    public int Speed;
    public Vector2 EndpointPosition;
}
