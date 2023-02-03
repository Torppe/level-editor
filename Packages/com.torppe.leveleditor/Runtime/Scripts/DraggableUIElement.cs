using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUIElement : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public static Action<Transform> OnClickUI;
    private Vector3 dragOffset;

    public virtual void OnDrag(PointerEventData eventData)
    {
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnClickUI?.Invoke(transform);
        Debug.Log("clicked");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnClickUI?.Invoke(null);
        Debug.Log("clicked2");
    }
}
