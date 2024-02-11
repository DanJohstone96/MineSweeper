using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragableWindow : MonoBehaviour , IDragHandler
{
    [SerializeField]
    private RectTransform DragRect;

    [SerializeField]
    private Canvas Canvas;

    /// <summary>
    /// Implementation of OnDrag From I Drag Handler interface, allows the score window to be moved.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        if (!transform.GetChild(0).gameObject.activeInHierarchy) 
        {
            return;
        }

        DragRect.anchoredPosition += eventData.delta/Canvas.scaleFactor;
    }
}
