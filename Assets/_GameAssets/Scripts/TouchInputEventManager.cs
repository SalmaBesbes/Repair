
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum eInputSource { mouse, touch }
[Serializable]
public class InputSource
{
    public eInputSource source;
    public int id;

    public InputSource(eInputSource source, int id)
    {
        this.source = source;
        this.id = id;
    }
}
[Serializable]
public class PointerInfo
{
    public InputSource inputSource;

    public Vector2 screenPosition;

    public PointerInfo(InputSource inputSource, Vector2 screenPosition)
    {
        this.inputSource = inputSource;

        this.screenPosition = screenPosition;
    }
}
[Serializable]
public class Swipe
{
    [SerializeField]
    private List<SwipeWaypoint> swipeWayPoints = new List<SwipeWaypoint>();
    public List<SwipeWaypoint> Waypoints { get { return swipeWayPoints; } }

    public void AddWP(Vector2 Pos)
    {
        if (swipeWayPoints.Count > 0)
        {
            swipeWayPoints[swipeWayPoints.Count - 1].Next = new SwipeWaypoint(Pos);
            swipeWayPoints.Add(swipeWayPoints[swipeWayPoints.Count - 1].Next);
        }
        else
        {
            swipeWayPoints.Add(new SwipeWaypoint(Pos));
            startTime = swipeWayPoints[swipeWayPoints.Count - 1].TimeStamp;
        }

    }
    private float startTime;

    public float StartTime { get { return startTime; } }

    public void Clear()
    {
        swipeWayPoints.Clear();
    }
}
[Serializable]
public class SwipeWaypoint
{
    public SwipeWaypoint(Vector2 Pos, SwipeWaypoint next = null)
    {
        screenPosition = Pos;
        timeStamp = Time.realtimeSinceStartup;
        Next = next;
    }
    [SerializeField]
    private Vector2 screenPosition;
    [SerializeField]
    private float timeStamp;
    public SwipeWaypoint Next;
    public Vector2 ScreenPosition { get { return screenPosition; } }
    public float TimeStamp { get { return timeStamp; } }
}



public class TouchInputEventManager : MonoBehaviour
{

    public enum eStatus { free, down, holding, swiping }


    [Header("Status")]


    [Space]
    [Header("Setup")]
    public Camera Cam;

    [SerializeField]
    public LineRenderer LR;


    public eInputSource inputSource = eInputSource.mouse;
    [SerializeField]
    public InputStatusMonitor inputStatus;
    [SerializeField]
    public InputSource source;
    
    [Space]
    public float TapDistanceThreshold = 3;
    public float HoldDelay = 0.3f;
    public float DoubleTapwindow = 0.3f;
    public bool tapFrame = false;
    public bool doubleTapFrame = false;


    [Range(0.001f, 1)]
    public float MinSwipeScreenPercent = 0.05f;
    [Range(0.001f, 1)]
    public float MinimumSwipeVelocity = 0.05f;
    [Range(0.1f, 80)]
    public float MinSwipeAngleDelta = 1f;

    [Range(0.001f, 1)]
    public float MinSwipeDelta = 0.05f;
    [Range(0.001f, 1)]
    public float MaxSwipeDelta = 0.1f;
    [Range(0, 0.8f)]
    public float SwipeReleaseDelay = 0.3f;
    float swipetimer;

    public AnimationCurve swipeThresholdFunct = AnimationCurve.EaseInOut(0,0,2,2);
    float ScreenPercent(float distance) { return (distance / Mathf.Sqrt(Camera.current.pixelWidth * Camera.current.pixelWidth + Camera.current.pixelHeight * Camera.current.pixelHeight)); }
    private void OnDrawGizmos()
    {

        //Plane p = new Plane(-Camera.current.transform.forward, Camera.current.transform.position + Camera.current.transform.forward);
        //Ray r0 = Camera.current.ScreenPointToRay(Vector2.zero);

        //Ray r1 = Camera.current.ScreenPointToRay(Vector2.one * MinimumSwipeScreenDistancePercentage * Mathf.Sqrt(Camera.current.pixelWidth * Camera.current.pixelWidth + Camera.current.pixelHeight * Camera.current.pixelHeight)) ;

        //float f1=0, f2 = 0;

        //p.Raycast(r0, out f1);
        //p.Raycast(r1, out f2);

        //Vector3 p1 =  r0.origin + r0.direction * f1;

        //Vector3 p2 = r1.origin + r1.direction * f2;


        //Gizmos.color = Color.green;
        //Gizmos.DrawLine(p1, p2);
    }

    private void Awake()
    {
        InitRegisters();
    }
    private void Start()
    {
        BootRegistrationCoroutine();
        if (LR == null)LR = Cam.GetComponent<LineRenderer>();
        if (LR != null)
        {
            LR.startWidth = 0.001f;
            LR.endWidth = 0.001f;
            LR.startColor = Color.green;
            LR.endColor = Color.yellow;
        }

    }

    void UpdateLine(InputStatusMonitor inputStatusMonitor)
    {
        Plane p = new Plane(-Cam.transform.forward, Cam.transform.forward + Cam.transform.position);


        List<Vector3> poss = new List<Vector3>();
        Vector3 pos = Vector3.zero;
        float f;
        Ray r;
        foreach (SwipeWaypoint e in inputStatusMonitor.currentSwipe.Waypoints)
        {
            pos = Vector3.zero;
            r = Cam.ScreenPointToRay(e.ScreenPosition);
            if (!p.Raycast(r, out f)) continue;
            pos = r.origin + r.direction * f;
            poss.Add(pos);

        }
        pos = Vector3.zero;
        r = Cam.ScreenPointToRay(inputStatusMonitor.currentScreenPosition);
        if (p.Raycast(r, out f))
        {
            pos = r.origin + r.direction * f;
            poss.Add(pos);
        }
         if (LR != null)
        {
            LR.positionCount = poss.Count;
            LR.SetPositions(poss.ToArray());
        }

    }
    private void OnEnable()
    {
        BootRegistrationCoroutine();
    }

    // Update is called once per frame
    public bool isMonitoring = false;
    Coroutine MonitorLoopCoroutine;
    IEnumerator MonitorLoop()
    {
        isMonitoring = true;
        while (HasListeners())
        {
            if (inputStatus.inputSource != source)
            {
                inputStatus = new InputStatusMonitor(source);
            }

            DetermineStatus(inputStatus);

            yield return new WaitForEndOfFrame();
        }

  
        isMonitoring = false;
    }


    bool HasListeners() {
        return ((PointerDownCalls.Count > 0)
                    || (PointerUpCalls.Count > 0)
                    || (PointerMoveCalls.Count > 0)
                    || (PointerClickCalls.Count > 0)
                    || (PointerDoubleClickCalls.Count > 0)
                    || (PointerHoldStartCalls.Count > 0)
                    || (PointerHoldUpdateCalls.Count > 0)
                    || (PointerHoldEndCalls.Count > 0)
                    || (PointerSwipeStartCalls.Count > 0)
                    || (PointerSwipeUpdateCalls.Count > 0)
                    || (PointerSwipeEndCalls.Count > 0));
    }

    void BootRegistrationCoroutine()
    {
        if (HasListeners() && !isMonitoring)
        {
            MonitorLoopCoroutine = StartCoroutine(MonitorLoop());
        }
    }


    Vector2 GetCurrentPos(InputSource inputSource)
    {
        Vector2 pos = Vector2.zero;

        switch (inputSource.source)
        {
            case eInputSource.mouse: pos = Input.mousePosition; break;
            case eInputSource.touch: pos = Input.GetTouch(inputSource.id).position; break;
        }

        return pos;
        // return GetPos(pos);
    }





    bool IsControlFree(InputSource inputSource)
    {
        switch (inputSource.source)
        {
            case eInputSource.mouse: return (!Input.GetMouseButton(inputSource.id));
            case eInputSource.touch: return (Input.touchCount < 1 || Input.GetTouch(inputSource.id).phase == TouchPhase.Canceled || Input.GetTouch(inputSource.id).phase == TouchPhase.Ended);
        }
        return false;
    }

    void CallEndStatus(InputStatusMonitor inputStatusMonitor, eStatus NewStatus)
    {
        eStatus OldStatus = inputStatusMonitor.Status;
        inputStatusMonitor.Status = NewStatus;
        switch (OldStatus)
        {
            case eStatus.free:
                {
                    PointerDown(inputStatusMonitor);
                    break;
                }
            case eStatus.holding:
                {
                    PointerHoldEnd(inputStatusMonitor);
                    break;
                }
            case eStatus.swiping:
                {
                    PointerSwipeEnd(inputStatusMonitor);
                    break;
                }

        }

        return;
    }
    void DetermineStatus(InputStatusMonitor inputStatusMonitor)
    {

        switch (inputStatusMonitor.Status)
        {
            case eStatus.free:
                {
                    if (IsControlFree(inputStatusMonitor.inputSource))
                    {
                        if (inputStatusMonitor.Status != eStatus.free)
                        {

                            CallEndStatus(inputStatusMonitor, eStatus.free);
                            PointerUp(inputStatusMonitor);
                            return;
                        }
                        else
                        {
                            // OnPointerUpUpdate();
                        }
                        return;

                    }
                    else
                    {
                        goto case eStatus.down;
                    }
                }
            case eStatus.down:
                {
                    if (inputStatusMonitor.Status != eStatus.down)
                    {
                        if (inputStatusMonitor.Status == eStatus.free)
                        {
                            inputStatusMonitor.contactStart = Time.timeSinceLevelLoad;
                            inputStatusMonitor.contactStartScreenPosition = GetCurrentPos(inputStatusMonitor.inputSource);
                            inputStatusMonitor.currentScreenPosition = inputStatusMonitor.contactStartScreenPosition;
                            inputStatusMonitor.oldScreenPosition = inputStatusMonitor.currentScreenPosition;

                        }
                        if (Time.timeSinceLevelLoad - inputStatusMonitor.lastTap <= DoubleTapwindow && (inputStatusMonitor.currentScreenPosition - inputStatusMonitor.lastTapScreenPosition).magnitude <= TapDistanceThreshold)
                        {
                            //goto case eStatus.dbltap;
                            PointerDoubleClick(inputStatusMonitor);
                        }
                        else
                        {
                            CallEndStatus(inputStatusMonitor, eStatus.down);
                        }

                    }
                    else
                    {
                        if (IsControlFree(inputStatusMonitor.inputSource))
                        {

                            inputStatusMonitor.lastTap = Time.timeSinceLevelLoad;
                            inputStatusMonitor.oldScreenPosition = inputStatusMonitor.currentScreenPosition;
                            inputStatusMonitor.currentScreenPosition = GetCurrentPos(inputStatusMonitor.inputSource);
                            inputStatusMonitor.lastTapScreenPosition = inputStatusMonitor.currentScreenPosition;
                            PointerClick(inputStatusMonitor);
                            goto case eStatus.free;

                        }
                        else
                        {
                            inputStatusMonitor.oldScreenPosition = inputStatusMonitor.currentScreenPosition;
                            inputStatusMonitor.currentScreenPosition = GetCurrentPos(inputStatusMonitor.inputSource);



                            if (SwipeThreshold(inputStatusMonitor))
                            {
                                goto case eStatus.swiping;
                            }
                            if (Time.timeSinceLevelLoad - inputStatusMonitor.contactStart >= HoldDelay)
                            {
                                goto case eStatus.holding;
                            }
                        }

                    }
                    //            OnPointerDownUpdate();
                    inputStatusMonitor.Status = eStatus.down;
                    return;
                }


            case eStatus.holding:
                {
                    if (IsControlFree(inputStatusMonitor.inputSource))
                    {

                        inputStatusMonitor.lastTap = Time.timeSinceLevelLoad;
                        inputStatusMonitor.oldScreenPosition = inputStatusMonitor.currentScreenPosition;
                        inputStatusMonitor.currentScreenPosition = GetCurrentPos(inputStatusMonitor.inputSource);
                        inputStatusMonitor.lastTapScreenPosition = inputStatusMonitor.currentScreenPosition;

                        //OnPointerHoldEnd();
                        goto case eStatus.free;

                    }
                    else
                    {
                        if (inputStatusMonitor.Status != eStatus.holding)
                        {
                            CallEndStatus(inputStatusMonitor, eStatus.holding);
                            PointerHoldStart(inputStatusMonitor);
                            return;
                        }
                        inputStatusMonitor.oldScreenPosition = inputStatusMonitor.currentScreenPosition;
                        inputStatusMonitor.currentScreenPosition = GetCurrentPos(inputStatusMonitor.inputSource);
                        if (SwipeThreshold(inputStatusMonitor))
                        {
                            //OnPointerHoldEnd(); 
                            goto case eStatus.swiping;
                        }
                    }
                    PointerHoldUpdate(inputStatusMonitor);
                    return;
                }
            case eStatus.swiping:
                {
                    if (inputStatusMonitor.Status != eStatus.swiping)
                    {
                        //    if (Status == eStatus.down || Status == eStatus.holding)
                        //    {
                        inputStatusMonitor.swipeStartScreenPosition = inputStatusMonitor.currentScreenPosition;


                        swipetimer = SwipeReleaseDelay;
                        //    }
                        CallEndStatus(inputStatusMonitor, eStatus.swiping);
                        inputStatusMonitor.currentSwipe.Clear();
                        inputStatusMonitor.swipeDirection = Vector2.zero;
                        inputStatusMonitor.currentSwipe.AddWP(inputStatusMonitor.swipeStartScreenPosition);
                        PointerSwipeStart(inputStatusMonitor);
                    }
                    else
                    {
                        if (IsControlFree(inputStatusMonitor.inputSource))
                        {
                            inputStatusMonitor.lastTap = Time.timeSinceLevelLoad;
                            inputStatusMonitor.oldScreenPosition = inputStatusMonitor.currentScreenPosition;
                            inputStatusMonitor.currentScreenPosition = GetCurrentPos(inputStatusMonitor.inputSource);
                            inputStatusMonitor.lastTapScreenPosition = inputStatusMonitor.currentScreenPosition;


                            //OnPointerSwipeEnd();
                            goto case eStatus.free;
                        }
                        else
                        {
                            inputStatusMonitor.oldScreenPosition = inputStatusMonitor.currentScreenPosition;
                            inputStatusMonitor.currentScreenPosition = GetCurrentPos(inputStatusMonitor.inputSource);

                            if ((inputStatusMonitor.currentScreenPosition - inputStatusMonitor.oldScreenPosition).magnitude / Time.deltaTime <= MinimumSwipeVelocity)
                            {
                                swipetimer -= Time.deltaTime;
                                if (swipetimer <= 0)
                                {
                                    //OnPointerSwipeEnd();

                                    goto case eStatus.holding;
                                }
                            }
                            else
                            {
                                swipetimer = SwipeReleaseDelay;
                            }

                        }

                    }
                    inputStatusMonitor.Status = eStatus.swiping;
                    PointerSwipeUpdate(inputStatusMonitor);
                    return;
                }
        }
    }

    private bool SwipeThreshold(InputStatusMonitor inputStatusMonitor)
    {
        float speedTh = ScreenPercent((inputStatusMonitor.currentScreenPosition - inputStatusMonitor.oldScreenPosition).magnitude) / Time.deltaTime / MinimumSwipeVelocity;
        float distTH = (ScreenPercent((inputStatusMonitor.currentScreenPosition - inputStatusMonitor.contactStartScreenPosition).magnitude) / MinSwipeScreenPercent);

        float th = (swipeThresholdFunct.Evaluate(speedTh)* swipeThresholdFunct.Evaluate(distTH))/2;
        if (th >= 1)
        {

           // Debug.Log("speedTh " + speedTh + " distTH " + distTH + " th " + th);
            return true;
        }

        return false;
    }

    [Serializable]
    public class InputStatusMonitor
    {
        private Touch OldT, T;
        public Vector2 contactStartScreenPosition;
        
        public Vector2 swipeStartScreenPosition;
        public Vector2 currentScreenPosition;
        public Vector2 oldScreenPosition;
        public Vector2 swipeDirection = Vector2.zero;
        public Vector2 lastTapScreenPosition;
        public float contactStart;
        public float highestDelta = 0;
        public float lastTap;
        public float holdStart;
        // float d, Od;
        public bool tap, doubleTap;

        public eStatus Status = eStatus.free;
        [SerializeField]
        public Swipe currentSwipe;
        public InputSource inputSource;
        public List<Vector2> swipeBuffer = new List<Vector2>();
        public InputStatusMonitor(InputSource inputSource)
        {
            this.inputSource = inputSource;
            currentSwipe = new Swipe();
             OldT = new Touch();
            T = new Touch();
        }
    }


    void PointerDown(InputStatusMonitor inputStatusMonitor)
    {
        //Debug.Log("Down");
        foreach (IInputListener c in PointerDownCalls)
        {
            c.OnPointerDown(new PointerInfo(inputStatusMonitor.inputSource, inputStatusMonitor.currentScreenPosition));
        }

    }
    void PointerUp(InputStatusMonitor inputStatusMonitor)
    {
        //Debug.Log("Up");
        foreach (IInputListener c in PointerUpCalls)
        {
            c.OnPointerUp(new PointerInfo(inputStatusMonitor.inputSource, inputStatusMonitor.currentScreenPosition));
        }
    }
    void PointerMove(InputStatusMonitor inputStatusMonitor)
    {
        //       //Debug.Log("");
        foreach (IInputListener c in PointerMoveCalls)
        {
            c.OnPointerMove(new PointerInfo(inputStatusMonitor.inputSource, inputStatusMonitor.currentScreenPosition));
        }
    }
    void PointerSwipeStart(InputStatusMonitor inputStatusMonitor)
    {


        foreach (IInputListener c in PointerSwipeStartCalls)
        {
            c.OnPointerSwipeStart(new PointerInfo(inputStatusMonitor.inputSource, inputStatusMonitor.currentScreenPosition), inputStatusMonitor.currentSwipe);
        }
        //Debug.Log("SwipeStart");
    }

    
    void PointerSwipeUpdate(InputStatusMonitor inputStatusMonitor)
    {
        bool WPAdded = false;
        if (inputStatusMonitor == null)
        {
            //Debug.LogError("inputStatusMonitor null"); return;
        }
        if (inputStatusMonitor.swipeDirection == Vector2.zero)
        {
            inputStatusMonitor.swipeDirection = (inputStatusMonitor.currentScreenPosition - inputStatusMonitor.oldScreenPosition).normalized;
        }
        else
        {
            // Vector2 oldp = ((inputStatusMonitor.currentSwipe.Waypoints.Count > 0) ? inputStatusMonitor.currentSwipe.Waypoints[inputStatusMonitor.currentSwipe.Waypoints.Count - 1].ScreenPosition : inputStatusMonitor.oldScreenPosition);

            if (inputStatusMonitor.currentSwipe.Waypoints.Count == 0) inputStatusMonitor.currentSwipe.Waypoints.Add(new SwipeWaypoint((inputStatusMonitor.contactStartScreenPosition)));
            Vector2 oldp = inputStatusMonitor.currentSwipe.Waypoints[inputStatusMonitor.currentSwipe.Waypoints.Count - 1].ScreenPosition;

            float delta = (inputStatusMonitor.currentScreenPosition - inputStatusMonitor.currentSwipe.Waypoints[inputStatusMonitor.currentSwipe.Waypoints.Count - 1].ScreenPosition).magnitude;

            bool trackback = false;
            if (inputStatusMonitor.highestDelta< delta)
            {
                inputStatusMonitor.highestDelta = delta;
            }else 
            {
                trackback =   ScreenPercent(inputStatusMonitor.highestDelta-delta) >= MinSwipeScreenPercent;
            }
            if (Vector2.Angle((inputStatusMonitor.currentScreenPosition - oldp).normalized, inputStatusMonitor.swipeDirection) >= MinSwipeAngleDelta && ScreenPercent((inputStatusMonitor.currentScreenPosition - oldp).magnitude)>=MinSwipeDelta || ScreenPercent((inputStatusMonitor.currentScreenPosition - oldp).magnitude) >= MaxSwipeDelta || trackback)
            {
                Vector2 newWP = inputStatusMonitor.currentScreenPosition;
                if (inputStatusMonitor.swipeBuffer.Count > 0)
                {
                    int k = 0;
                    float topD = 0;//^^
                    for (int i =0; i< inputStatusMonitor.swipeBuffer.Count;i++)
                    {
                        float d = ((inputStatusMonitor.swipeBuffer[i] - oldp).magnitude + (inputStatusMonitor.swipeBuffer[i] - inputStatusMonitor.currentScreenPosition).magnitude);
                        if ( d > topD)
                        {
                            k = i;
                            topD = d;
                        }
                    }
                    newWP = inputStatusMonitor.swipeBuffer[k];
                    for (int i = 0; i <= k; i++)
                    {
                        inputStatusMonitor.swipeBuffer.RemoveAt(0);
                    }
                }

                inputStatusMonitor.currentSwipe.AddWP(newWP);
                WPAdded = true;
                inputStatusMonitor.highestDelta = 0;
                inputStatusMonitor.swipeDirection = (newWP - oldp).normalized;
            }
            else
            {
                inputStatusMonitor.swipeBuffer.Add(inputStatusMonitor.currentScreenPosition);
            }
        }


        UpdateLine(inputStatusMonitor);

        foreach (IInputListener c in PointerSwipeUpdateCalls)
        {
            c.OnPointerSwipeUpdate(new PointerInfo(inputStatusMonitor.inputSource, inputStatusMonitor.currentScreenPosition), inputStatusMonitor.currentSwipe, WPAdded);
        }

        //      //Debug.Log("");

    }
    void PointerSwipeEnd(InputStatusMonitor inputStatusMonitor)
    {
        inputStatusMonitor.currentSwipe.AddWP(inputStatusMonitor.currentScreenPosition);
        inputStatusMonitor.swipeBuffer.Clear();
        //Debug.Log("SwipeEnd");

        foreach (IInputListener c in PointerSwipeEndCalls)
        {
            c.OnPointerSwipeEnd(new PointerInfo(inputStatusMonitor.inputSource, inputStatusMonitor.currentScreenPosition), inputStatusMonitor.currentSwipe);
        }

    }
    void PointerHoldStart(InputStatusMonitor inputStatusMonitor)
    {
        inputStatusMonitor.holdStart = Time.timeSinceLevelLoad;
        //Debug.Log("HoldStart");
        foreach (IInputListener c in PointerHoldStartCalls)
        {
            c.OnPointerHoldStart(new PointerInfo(inputStatusMonitor.inputSource, inputStatusMonitor.currentScreenPosition));
        }
    }
    void PointerHoldUpdate(InputStatusMonitor inputStatusMonitor)
    {
        //     //Debug.Log("");
        foreach (IInputListener c in PointerHoldUpdateCalls)
        {
            c.OnPointerHoldUpdate(new PointerInfo(inputStatusMonitor.inputSource, inputStatusMonitor.currentScreenPosition), Time.timeSinceLevelLoad - inputStatusMonitor.holdStart);
        }
    }
    void PointerHoldEnd(InputStatusMonitor inputStatusMonitor)
    {
        //Debug.Log("HoldEnd");
        foreach (IInputListener c in PointerHoldEndCalls)
        {
            c.OnPointerHoldEnd(new PointerInfo(inputStatusMonitor.inputSource, inputStatusMonitor.currentScreenPosition), Time.timeSinceLevelLoad - inputStatusMonitor.holdStart);
        }
    }
    void PointerClick(InputStatusMonitor inputStatusMonitor)
    {
        tapFrame = true;
        //Debug.Log("Click");
        foreach (IInputListener c in PointerClickCalls)
        {
            c.OnPointerClick(new PointerInfo(inputStatusMonitor.inputSource, inputStatusMonitor.currentScreenPosition));
        }
    }
    void PointerDoubleClick(InputStatusMonitor inputStatusMonitor)
    {
        doubleTapFrame = true;
        //Debug.Log("DoubleClick");
        foreach (IInputListener c in PointerDoubleClickCalls)
        {
            c.OnPointerDoubleClick(new PointerInfo(inputStatusMonitor.inputSource, inputStatusMonitor.currentScreenPosition));
           
        }
    }

    public enum ePointerEvent
    {
        OnPointerDown,
        OnPointerUp,
        OnPointerMove,
        OnPointerClick,
        OnPointerDoubleClick,
        OnPointerHoldStart,
        OnPointerHoldUpdate,
        OnPointerHoldEnd,
        OnPointerSwipeStart,
        OnPointerSwipeUpdate,
        OnPointerSwipeEnd
    }
    public void Unregister(IInputListener listener)
    {
        if (PointerDownCalls.Contains(listener)) PointerDownCalls.Remove(listener);
        if (PointerUpCalls.Contains(listener)) PointerUpCalls.Remove(listener);
        if (PointerMoveCalls.Contains(listener)) PointerMoveCalls.Remove(listener);
        if (PointerClickCalls.Contains(listener)) PointerClickCalls.Remove(listener);
        if (PointerDoubleClickCalls.Contains(listener)) PointerDoubleClickCalls.Remove(listener);
        if (PointerHoldStartCalls.Contains(listener)) PointerHoldStartCalls.Remove(listener);
        if (PointerHoldUpdateCalls.Contains(listener)) PointerHoldUpdateCalls.Remove(listener);
        if (PointerHoldEndCalls.Contains(listener)) PointerHoldEndCalls.Remove(listener);
        if (PointerSwipeStartCalls.Contains(listener)) PointerSwipeStartCalls.Remove(listener);
        if (PointerSwipeUpdateCalls.Contains(listener)) PointerSwipeUpdateCalls.Remove(listener);
        if (PointerSwipeEndCalls.Contains(listener)) PointerSwipeEndCalls.Remove(listener);
    }

    public void UnRegisterForPointerEvent( IInputListener listener, ePointerEvent PointerEvent  )
    {
        
        switch (PointerEvent)
        {
            case ePointerEvent.OnPointerDown                    : if (PointerDownCalls          .Contains(listener)) PointerDownCalls          .Remove(listener);break;
            case ePointerEvent.OnPointerUp                      : if (PointerUpCalls            .Contains(listener)) PointerUpCalls            .Remove(listener);break;
            case ePointerEvent.OnPointerMove                    : if (PointerMoveCalls          .Contains(listener)) PointerMoveCalls          .Remove(listener);break;
            case ePointerEvent.OnPointerClick                   : if (PointerClickCalls         .Contains(listener)) PointerClickCalls         .Remove(listener);break;
            case ePointerEvent.OnPointerDoubleClick             : if (PointerDoubleClickCalls   .Contains(listener)) PointerDoubleClickCalls   .Remove(listener);break;
            case ePointerEvent.OnPointerHoldStart               : if (PointerHoldStartCalls     .Contains(listener)) PointerHoldStartCalls     .Remove(listener);break;
            case ePointerEvent.OnPointerHoldUpdate              : if (PointerHoldUpdateCalls    .Contains(listener)) PointerHoldUpdateCalls    .Remove(listener);break;
            case ePointerEvent.OnPointerHoldEnd                 : if (PointerHoldEndCalls       .Contains(listener)) PointerHoldEndCalls       .Remove(listener);break;
            case ePointerEvent.OnPointerSwipeStart              : if (PointerSwipeStartCalls    .Contains(listener)) PointerSwipeStartCalls    .Remove(listener);break;
            case ePointerEvent.OnPointerSwipeUpdate             : if (PointerSwipeUpdateCalls   .Contains(listener)) PointerSwipeUpdateCalls   .Remove(listener);break;
            case ePointerEvent.OnPointerSwipeEnd                : if (PointerSwipeEndCalls      .Contains(listener)) PointerSwipeEndCalls      .Remove(listener);break;
        }
    }

    public void RegisterForPointerEvent(IInputListener listener, ePointerEvent PointerEvent)
    {

        switch (PointerEvent)
        {
            case ePointerEvent.OnPointerDown: if (PointerDownCalls.Contains(listener)) return; PointerDownCalls.Add(listener); break;
            case ePointerEvent.OnPointerUp: if (PointerUpCalls.Contains(listener)) return; PointerUpCalls.Add(listener); break;
            case ePointerEvent.OnPointerMove: if (PointerMoveCalls.Contains(listener)) return; PointerMoveCalls.Add(listener); break;
            case ePointerEvent.OnPointerClick: if (PointerClickCalls.Contains(listener)) return; PointerClickCalls.Add(listener); break;
            case ePointerEvent.OnPointerDoubleClick: if (PointerDoubleClickCalls.Contains(listener)) return; PointerDoubleClickCalls.Add(listener); break;
            case ePointerEvent.OnPointerHoldStart: if (PointerHoldStartCalls.Contains(listener)) return; PointerHoldStartCalls.Add(listener); break;
            case ePointerEvent.OnPointerHoldUpdate: if (PointerHoldUpdateCalls.Contains(listener)) return; PointerHoldUpdateCalls.Add(listener); break;
            case ePointerEvent.OnPointerHoldEnd: if (PointerHoldEndCalls.Contains(listener)) return; PointerHoldEndCalls.Add(listener); break;
            case ePointerEvent.OnPointerSwipeStart: if (PointerSwipeStartCalls.Contains(listener)) return; PointerSwipeStartCalls.Add(listener); break;
            case ePointerEvent.OnPointerSwipeUpdate: if (PointerSwipeUpdateCalls.Contains(listener)) return; PointerSwipeUpdateCalls.Add(listener); break;
            case ePointerEvent.OnPointerSwipeEnd: if (PointerSwipeEndCalls.Contains(listener)) return; PointerSwipeEndCalls.Add(listener); break;
        }
        BootRegistrationCoroutine();
    }




    [Flags]
    public enum eFingerMask
    {
        None = 0x0,
        F1 = 0x1,
        F2 = 0x2,
        F3 = 0x4,
        F4 = 0x8,
        F5 = 0x10,
        F6 = 0x20,
        F7 = 0x40,
        F8 = 0x80,
        F9 = 0x100,
        F10 = 0x200,
        All = int.MaxValue
    }

    public eFingerMask fingerMask;


    public List<IInputListener> PointerDownCalls;
    public List<IInputListener> PointerUpCalls;
    public List<IInputListener> PointerHoldStartCalls;
    public List<IInputListener> PointerClickCalls;
    public List<IInputListener> PointerDoubleClickCalls;
    public List<IInputListener> PointerMoveCalls;
    public List<IInputListener> PointerSwipeStartCalls;
   
    public List<IInputListener>  PointerSwipeUpdateCalls;
    public List<IInputListener>  PointerSwipeEndCalls;
    public List<IInputListener>  PointerHoldUpdateCalls;
    public List<IInputListener>  PointerHoldEndCalls;
             

                                        
    void InitRegisters()
    {
        PointerDownCalls        = new List<IInputListener> ();
        PointerUpCalls          = new List<IInputListener> ();
        PointerHoldStartCalls   = new List<IInputListener> ();
        PointerClickCalls       = new List<IInputListener> ();
        PointerDoubleClickCalls = new List<IInputListener> ();
        PointerMoveCalls        = new List<IInputListener> ();
        PointerSwipeStartCalls  = new List<IInputListener> ();
                                      
        PointerSwipeUpdateCalls = new List<IInputListener> ();
        PointerSwipeEndCalls    = new List<IInputListener> ();
        PointerHoldUpdateCalls  = new List<IInputListener> ();
        PointerHoldEndCalls     = new List<IInputListener>();
    }




}




public interface IInputListener
{
    void OnPointerDown(PointerInfo pointerInfo);
    void OnPointerUp(PointerInfo pointerInfo);
    void OnPointerMove(PointerInfo pointerInfo);

    /// <summary>
    /// Sent at the start of a swipe
    /// </summary>
    /// <param name="pointerInfo"> information about the pointer position, source (mouse/touch fingers...)</param>
    /// <param name="currentSwipe"> contains data about the current swipe </param>
    void OnPointerSwipeStart(PointerInfo pointerInfo, Swipe currentSwipe);

    /// <summary>
    /// Sent every update while a swipe is in progress
    /// </summary>
    /// <param name="pointerInfo"> information about the pointer position, source (mouse/touch fingers...)</param>
    /// <param name="currentSwipe"> contains data about the current swipe </param>
    /// <param name="newWaypoint"> true if a new waypoint has been registered as per the monitors configuration </param>
    void OnPointerSwipeUpdate(PointerInfo pointerInfo, Swipe currentSwipe, bool newWaypoint);
    void OnPointerSwipeEnd(PointerInfo pointerInfo, Swipe currentSwipe);
    void OnPointerHoldStart(PointerInfo pointerInfo);

    /// <summary>
    /// Sent every update while the pointer or finger is Held down ( as per monitor configuration)
    /// </summary>
    /// <param name="pointerInfo"> position of cursor/touch and source of input</param>
    /// <param name="duration"> the duration since the start of the hold action </param>
    void OnPointerHoldUpdate(PointerInfo pointerInfo, float duration);
    void OnPointerHoldEnd(PointerInfo pointerInfo, float duration);
    void OnPointerClick(PointerInfo pointerInfo);
    void OnPointerDoubleClick(PointerInfo pointerInfo);
}




