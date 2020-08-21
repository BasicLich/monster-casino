using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool mouse_over = false;
    public GameObject tooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouse_over = true;
        //Debug.Log("Mouse enter");
        tooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouse_over = false;
        //Debug.Log("Mouse exit");
        tooltip.SetActive(false);
    }

}
