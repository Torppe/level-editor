using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockActivatableDoor : Block
{
    [SerializeField]
    private Transform _endpoint;
    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, _endpoint.position);
    }

    public override void Load(BlockData blockData)
    {
        var data = (BlockActivatableDoorData)blockData;

        base.Load(data);
        _endpoint.position = transform.position + (Vector3)data.EndpointRelativePosition;
    }

    public override void Save()
    {
        base.Save();

        BlockActivatableDoorData data = new BlockActivatableDoorData();
        data.Copy(Data);
        data.Function = "activatable_door";
        data.EndpointRelativePosition = _endpoint.position - transform.position;

        Data = data;
    }
}

[Serializable]
public class BlockActivatableDoorData : BlockData
{
    public Vector2 EndpointRelativePosition;
}
