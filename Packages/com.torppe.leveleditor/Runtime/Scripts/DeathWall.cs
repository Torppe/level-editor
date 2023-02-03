using UnityEngine;
using UnityEngine.EventSystems;
using static ChapterGenerator;

public class DeathWall : DraggableUIElement
{
    public Vector2 EndpointPosition => endpoint.position;
    public Vector2 ScalePosition => scale.position;

    [SerializeField]
    private LineRenderer wallLine;
    [SerializeField]
    private LineRenderer endpointLine;
    [SerializeField]
    private Transform scale;
    [SerializeField]
    private Transform endpoint;

    [SerializeField]
    private Color endpointHandleColor;
    [SerializeField]
    private Color scaleHandleColor;

    void Awake()
    {
        scale.GetComponent<MeshRenderer>().material.color = scaleHandleColor;
        endpoint.GetComponent<MeshRenderer>().material.color = endpointHandleColor;
    }

    void Update()
    {
        var dir = transform.position - scale.position;

        wallLine.SetPosition(0, scale.position);
        wallLine.SetPosition(1, scale.position + dir * 2);

        endpointLine.SetPosition(0, transform.position);
        endpointLine.SetPosition(1, endpoint.position);
    }

    public void Load(DeathWallData data)
    {
        if (data == null)
            return;
        transform.position = data.Position;
        endpoint.position = data.Endpoint;
        scale.position = data.ScalePosition;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            Destroy(gameObject);
    }
}
