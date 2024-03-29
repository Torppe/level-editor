using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUIElement : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public static Action<Transform> OnClickUI;
    public Action OnClick;
    private Vector3 dragOffset;

    private bool preventClick;

    public virtual void OnDrag(PointerEventData eventData)
    {
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (preventClick)
            preventClick = false;
        else
            OnClick?.Invoke();
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        preventClick = true;
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
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnClickUI?.Invoke(null);
    }
}
