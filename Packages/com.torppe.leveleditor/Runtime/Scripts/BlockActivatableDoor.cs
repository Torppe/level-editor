using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockActivatableDoor : Block, IDeactivatable
{
    [SerializeField]
    private Transform _endpoint;
    private LineRenderer _lineRenderer;

    public bool Deactivated { get; set; }

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

        if (data.Deactivated)
            Deactivate();
    }

    public override void Save()
    {
        base.Save();

        BlockActivatableDoorData data = new BlockActivatableDoorData();
        data.Copy(Data);
        data.Function = "activatable_door";
        data.EndpointRelativePosition = _endpoint.position - transform.position;
        data.Deactivated = Deactivated;

        Data = data;
    }

    public void Deactivate()
    {
        Deactivated = true;
        _lineRenderer.enabled = false;
        _endpoint.gameObject.SetActive(false);
    }
}

[Serializable]
public class BlockActivatableDoorData : BlockData
{
    public bool Deactivated;
    public Vector2 EndpointRelativePosition;
}
