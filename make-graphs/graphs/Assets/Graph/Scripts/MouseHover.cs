using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Action MouseOverOnceFunc = null;
    public Action MouseOutOnceFunc = null;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (MouseOverOnceFunc != null) MouseOverOnceFunc();

    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (MouseOutOnceFunc != null) MouseOutOnceFunc();
    }
}
