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

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!transform.GetChild(0).gameObject.activeInHierarchy) 
        {
            return;
        }

        DragRect.anchoredPosition += eventData.delta/Canvas.scaleFactor;
    }
}
