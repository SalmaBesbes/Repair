﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchMonitorExample : MonoBehaviour, IInputListener
{
    TouchInputEventManager tim;
    [Sirenix.OdinInspector.ShowPropertyResolver]
    TouchInputEventManager TouchInputEventManager
    {
        get
        {
            if (tim == null) tim = GetComponent<TouchInputEventManager>();
            return tim;
        }
    }

    bool draggable;
    Vector2 Offset;
    Vector2 inputScreenToWordPosition;

    private void Start()
    {
        TouchInputEventManager.RegisterForPointerEvent(this, TouchInputEventManager.ePointerEvent.OnPointerClick);
        TouchInputEventManager.RegisterForPointerEvent(this, TouchInputEventManager.ePointerEvent.OnPointerDown);
        TouchInputEventManager.RegisterForPointerEvent(this, TouchInputEventManager.ePointerEvent.OnPointerSwipeUpdate);
        TouchInputEventManager.RegisterForPointerEvent(this, TouchInputEventManager.ePointerEvent.OnPointerUp);
    }
    public void OnPointerClick(PointerInfo pointerInfo)
    {
        Debug.Log("Click ! from " + pointerInfo.inputSource + " at " + pointerInfo.screenPosition);
    }
    #region callbacks
    public void OnPointerDoubleClick(PointerInfo pointerInfo)
    {

    }

    public void OnPointerHoldEnd(PointerInfo pointerInfo, float duration)
    {

    }

    public void OnPointerHoldStart(PointerInfo pointerInfo)
    {

    }

    public void OnPointerHoldUpdate(PointerInfo pointerInfo, float duration)
    {

    }

    public void OnPointerMove(PointerInfo pointerInfo)
    {

    }

    public void OnPointerSwipeEnd(PointerInfo pointerInfo, Swipe currentSwipe)
    {

    }

    public void OnPointerSwipeStart(PointerInfo pointerInfo, Swipe currentSwipe)
    {
    }

    #endregion

    GameObject DraggleObject;

    public void OnPointerDown(PointerInfo pointerInfo)
    {
        inputScreenToWordPosition = (Vector2)TouchInputEventManager.Cam.ScreenToWorldPoint(pointerInfo.screenPosition);
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(TouchInputEventManager.Cam.ScreenPointToRay(pointerInfo.screenPosition));

        if (rayHit.collider != null && rayHit.collider.gameObject.layer == 8)
        {
            draggable = true;
            Offset = inputScreenToWordPosition - (Vector2)rayHit.collider.transform.position;
            DraggleObject = rayHit.collider.gameObject;
            rayHit.collider.gameObject.GetComponent<TargetJoint2D>().enabled= false;
        }
    }

    bool checkPointerScreen(PointerInfo pointerInfo)
    {
        if (pointerInfo.screenPosition.x <= 0 || /*pointerInfo.screenPosition.y <= 0 ||*/ pointerInfo.screenPosition.x >= Screen.width - 1 /*|| pointerInfo.screenPosition.y >= Screen.height - 1*/)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    Vector2 OldPointPostion;
    Vector2 CurrentPointPosition;

    public void OnPointerSwipeUpdate(PointerInfo pointerInfo, Swipe currentSwipe, bool newWaypoint)
    {
        if (draggable)
        {
            if (currentSwipe.Waypoints.Count > 2)
            {

                OldPointPostion = currentSwipe.Waypoints[currentSwipe.Waypoints.Count - 2].ScreenPosition;
                CurrentPointPosition = currentSwipe.Waypoints[currentSwipe.Waypoints.Count - 1].ScreenPosition;

            }

            inputScreenToWordPosition = (Vector2)TouchInputEventManager.Cam.ScreenToWorldPoint(pointerInfo.screenPosition);

            if (checkPointerScreen(pointerInfo))
            {
                DraggleObject.transform.position = inputScreenToWordPosition + Offset;
            }
            else
            {

                Vector2 newinputScreenToWordPosition =
                        (Vector2)TouchInputEventManager.Cam.ScreenToWorldPoint
                        (new Vector2(Mathf.Abs(inputScreenToWordPosition.x - Screen.width - 1),
                        Mathf.Abs(inputScreenToWordPosition.x - Screen.height - 1)));

                if (pointerInfo.screenPosition.x <= 0)
                {
                    newinputScreenToWordPosition =
                        (Vector2)TouchInputEventManager.Cam.ScreenToWorldPoint
                        (new Vector2(Mathf.Abs(inputScreenToWordPosition.x - 1),
                        Mathf.Abs(inputScreenToWordPosition.x - Screen.height - 1)));

                    DraggleObject.transform.position = new Vector2(newinputScreenToWordPosition.x, inputScreenToWordPosition.y);
                }
                else if (pointerInfo.screenPosition.x <= 0 || pointerInfo.screenPosition.y >= Screen.height - 1)
                {
                    DraggleObject.transform.position = new Vector2(inputScreenToWordPosition.x, newinputScreenToWordPosition.y);
                }
                else if (pointerInfo.screenPosition.x >= Screen.width - 1)
                {
                    DraggleObject.transform.position = new Vector2(newinputScreenToWordPosition.x, inputScreenToWordPosition.y);
                }
            }

        }
    }

    public void OnPointerUp(PointerInfo pointerInfo)
    {
        if (draggable)
        {
            draggable = false;
            DraggleObject.GetComponent<TargetJoint2D>().enabled = true;
        }
    }

}
