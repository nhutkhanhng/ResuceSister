using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CPC_Visual
{
    public Color pathColor = Color.green;
    public Color inactivePathColor = Color.gray;
    public Color frustrumColor = Color.white;
    public Color handleColor = Color.yellow;

    public float MaxRange = 1f;
}

public enum CPC_ECurveType
{
    EaseInAndOut,
    Linear,
    Custom
}

public enum CPC_EAfterLoop
{
    Continue,
    Stop
}

[System.Serializable]
public class CPC_Point
{
    public Vector3 position;

    public Vector3 Position { get { return position + currentWorldPoint; } set { position = value; } }
    public Quaternion rotation;
    public Vector3 handleprev;
    public Vector3 handlenext;
    public CPC_ECurveType curveTypeRotation;
    public AnimationCurve rotationCurve;
    public CPC_ECurveType curveTypePosition;
    public AnimationCurve positionCurve;
    public bool chained;
    public float delayToNextPoint = 0f;
    public float timePerSeg;

    public Vector3 currentWorldPoint;
    public CPC_Point(Vector3 pos, Quaternion rot)
    {
        Position = pos;
        rotation = rot;
        handleprev = Vector3.back;
        handlenext = Vector3.forward;
        curveTypeRotation = CPC_ECurveType.EaseInAndOut;
        rotationCurve = AnimationCurve.EaseInOut(0,0,1,1);
        curveTypePosition = CPC_ECurveType.Linear;
        positionCurve = AnimationCurve.Linear(0,0,1,1);
        chained = true;
    }
}

public class CPC_Path : MonoBehaviour
{

    public bool useMainCamera = true;
    public Transform selectedCamera;
    public bool useRelativePosition;

    public bool lookAtTarget = false;
    public Transform target;
    public bool playOnAwake = false;
    public float playOnAwakeTime = 10;
    public List<CPC_Point> points = new List<CPC_Point>();
    public int MaxPointFollow = 1;

    public CPC_Visual visual;
    public bool looped = false;
    public bool alwaysShow = true;
    public CPC_EAfterLoop afterLoop = CPC_EAfterLoop.Continue;

    protected int currentWaypointIndex;
    protected float currentTimeInWaypoint;
    protected float timePerSegment;
    protected float currentDelayTime = 0f;

    protected bool paused = false;
    protected bool playing = false;

    public CPC_Point StartPoint
    {
        get
        {
            if (points.Count > 0)
                return points[0];

            return null;
        }
    }

    public CPC_Point LastPoint
    {
        get
        {
            if (points.Count > 0)
                return points[points.Count - 1];

            return null;
        }
    }

    public delegate void dEndPath(CPC_Path path);
    public dEndPath OnEndPath;

    public delegate void dNextWayPoint(CPC_Path path);
    public dNextWayPoint OnNextPoint;

    void OnEnable ()
    {
#if UNITY_EDITOR
        if (Camera.main == null) { Debug.LogError("There is no main camera in the scene!"); }
#endif

        if (useMainCamera)
	        selectedCamera = Camera.main.transform;
	    else if (selectedCamera == null)
	    {
            selectedCamera = Camera.main.transform;
#if UNITY_EDITOR
            Debug.LogError("No camera selected for following path, defaulting to main camera");
#endif
        }

	    if (lookAtTarget && target == null)
	    {
	        lookAtTarget = false;
#if UNITY_EDITOR
            Debug.LogError("No target selected to look at, defaulting to normal rotation");
#endif
        }

        SetUpPath(useRelativePosition);

        if (playOnAwake)
            PlayPath(playOnAwakeTime);
    }

    public void ToggleRelative(bool isActive)
    {
        useRelativePosition = isActive;
        SetUpPath(isActive);
    }
    public bool IsUseRelative()
    {
        return useRelativePosition;
    }
    protected void SetUpPath(bool isRelative = false)
    {
        foreach (var index in points)
        {
            if (index.curveTypeRotation == CPC_ECurveType.EaseInAndOut) index.rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            if (index.curveTypeRotation == CPC_ECurveType.Linear) index.rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
            if (index.curveTypePosition == CPC_ECurveType.EaseInAndOut) index.positionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            if (index.curveTypePosition == CPC_ECurveType.Linear) index.positionCurve = AnimationCurve.Linear(0, 0, 1, 1);

            if (isRelative)
                index.currentWorldPoint = selectedCamera.transform.position;
            else
                index.currentWorldPoint = Vector3.zero;
        }
    }

    public void SetUpRelative(Transform target)
    {
        foreach (var index in points)
        {
            if (index.curveTypeRotation == CPC_ECurveType.EaseInAndOut) index.rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            if (index.curveTypeRotation == CPC_ECurveType.Linear) index.rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
            if (index.curveTypePosition == CPC_ECurveType.EaseInAndOut) index.positionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            if (index.curveTypePosition == CPC_ECurveType.Linear) index.positionCurve = AnimationCurve.Linear(0, 0, 1, 1);

            if (this.useRelativePosition)
                index.currentWorldPoint = target.position;
            else
                index.currentWorldPoint = Vector3.zero;
        }
    }

    public void PlayPath(Transform _Target)
    {
        SetUpRelative(_Target);
        PlayPath();
    }
    public void PlayPath()
    {
        paused = false;
        playing = true;
        StopAllCoroutines();
        StartCoroutine(FollowPath(this.playOnAwakeTime));
    }
    public void PlayPath(float time)
    {
        if (time <= 0) time = 0.001f;
        paused = false;
        playing = true;
        StopAllCoroutines();
        StartCoroutine(FollowPath(time));
    }

    public void StopPath()
    {
        playing = false;
        paused = false;
        if (this.gameObject.activeSelf)
            StopAllCoroutines();
    }

    public void UpdateTimeInSeconds(float seconds)
    {
        timePerSegment = seconds / ((looped) ? points.Count : points.Count - 1);
    }

    public void PausePath()
    {
        paused = true;
        playing = false;
    }

    public void ResumePath()
    {
        if (paused)
            playing = true;
        paused = false;
    }

    public bool IsPaused()
    {
        return paused;
    }

    public bool IsPlaying()
    {
        return playing;
    }

    public int GetCurrentWayPoint()
    {
        return currentWaypointIndex;
    }

    public float GetCurrentTimeInWaypoint()
    {
        return currentTimeInWaypoint;
    }

    public void SetCurrentWayPoint(int value)
    {
        currentWaypointIndex = value;
    }


    /// Waypoint time (Range is 0-1)
    public void SetCurrentTimeInWaypoint(float value)
    {
        currentTimeInWaypoint = value;
    }

    public void RefreshTransform()
    {
        selectedCamera.transform.position = GetBezierPosition(currentWaypointIndex, currentTimeInWaypoint);
        if (!lookAtTarget)
            selectedCamera.transform.rotation = GetLerpRotation(currentWaypointIndex, currentTimeInWaypoint);
        else
            selectedCamera.transform.rotation = Quaternion.LookRotation((target.transform.position - selectedCamera.transform.position).normalized);
    }

    IEnumerator FollowPath(float time)
    {
        if (points.Count > 1)
        {
            currentWaypointIndex = 0;
            currentDelayTime = 0f;

            timePerSegment = points[currentWaypointIndex].timePerSeg;

            while (currentWaypointIndex < points.Count)
            {
                currentTimeInWaypoint = 0;
                currentDelayTime = 0f;
                while (currentDelayTime < points[currentWaypointIndex].delayToNextPoint)
                {
                    if (!paused)
                    {
                        currentDelayTime += Time.deltaTime;
                    }
                    yield return currentDelayTime;
                }

                while (currentTimeInWaypoint < 1)
                {
                    if (!paused)
                    {
                        currentTimeInWaypoint += Time.deltaTime / timePerSegment;

                        Vector3 nextPoint = GetBezierPosition(currentWaypointIndex, currentTimeInWaypoint);
                        if (!(float.IsNaN(nextPoint.x) || float.IsNaN(nextPoint.y) || float.IsNaN(nextPoint.z)))
                            selectedCamera.transform.position = nextPoint;

                        if (!lookAtTarget)
                            selectedCamera.transform.rotation = GetLerpRotation(currentWaypointIndex, currentTimeInWaypoint);
                        else
                        {
                            selectedCamera.transform.rotation =
                                Quaternion.Slerp(
                                                    selectedCamera.transform.rotation,
                                                    Quaternion.LookRotation(target.transform.position - selectedCamera.transform.position),
                                                    8f * Time.deltaTime);

                            // Quaternion.LookRotation((target.transform.position - selectedCamera.transform.position).normalized);
                        }
                    }
                    yield return 0;
                }                

                ++currentWaypointIndex;
                currentTimeInWaypoint = 0f;
                OnNextPoint?.Invoke(this);

                if (currentWaypointIndex == points.Count - 1 && !looped)
                {
                    OnEndPath?.Invoke(this);
                    break;
                }

                if (currentWaypointIndex == points.Count && afterLoop == CPC_EAfterLoop.Continue)
                {
                    OnEndPath?.Invoke(this);
                    currentWaypointIndex = 0;
                }

                timePerSegment = points[currentWaypointIndex].timePerSeg;
            }
        }
        StopPath();
    }

    int GetNextIndex(int index)
    {
        if (index == points.Count-1)
            return 0;
        return index + 1;
    }

    Vector3 GetBezierPosition(int pointIndex, float time)
    {
        float t = points[pointIndex].positionCurve.Evaluate(time);
        int nextIndex = GetNextIndex(pointIndex);
        return
            Vector3.Lerp(
                Vector3.Lerp(
                    Vector3.Lerp(points[pointIndex].Position,
                        points[pointIndex].Position + points[pointIndex].handlenext, t),
                    Vector3.Lerp(points[pointIndex].Position + points[pointIndex].handlenext,
                        points[nextIndex].Position + points[nextIndex].handleprev, t), t),
                Vector3.Lerp(
                    Vector3.Lerp(points[pointIndex].Position + points[pointIndex].handlenext,
                        points[nextIndex].Position + points[nextIndex].handleprev, t),
                    Vector3.Lerp(points[nextIndex].Position + points[nextIndex].handleprev,
                        points[nextIndex].Position, t), t), t);
    }

    private Quaternion GetLerpRotation(int pointIndex, float time)
    {
        return Quaternion.LerpUnclamped(points[pointIndex].rotation, points[GetNextIndex(pointIndex)].rotation, points[pointIndex].rotationCurve.Evaluate(time));
    }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (UnityEditor.Selection.activeGameObject == gameObject || alwaysShow)
        {
            if (points.Count >= 2)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    if (i < points.Count - 1)
                    {
                        var index = points[i];
                        var indexNext = points[i + 1];
                        UnityEditor.Handles.DrawBezier(index.Position, indexNext.Position, index.Position + index.handlenext,
                            indexNext.Position + indexNext.handleprev,((UnityEditor.Selection.activeGameObject == gameObject) ? visual.pathColor : visual.inactivePathColor), null, 5);
                    }
                    else if (looped)
                    {
                        var index = points[i];
                        var indexNext = points[0];
                        UnityEditor.Handles.DrawBezier(index.Position, indexNext.Position, index.Position + index.handlenext,
                            indexNext.Position + indexNext.handleprev, ((UnityEditor.Selection.activeGameObject == gameObject) ? visual.pathColor : visual.inactivePathColor), null, 5);
                    }
                }
            }

            for (int i = 0; i < points.Count; i++)
            {
                var index = points[i];
                    Gizmos.matrix = Matrix4x4.TRS(index.Position, index.rotation, Vector3.one);
                    Gizmos.color = visual.frustrumColor;
                    Gizmos.DrawFrustum(Vector3.zero, Camera.main.fieldOfView, visual.MaxRange, 0.01f, Camera.main.aspect);


                //if (i < points.Count - 1)
                //{
                //    Gizmos.matrix = Matrix4x4.TRS(GetBezierPosition(i + 1, timePerSegment / 2),
                //        GetLerpRotation(i + 1, timePerSegment / 2), Vector3.one);

                //    Gizmos.DrawFrustum(Vector3.zero, Camera.main.fieldOfView, 10f, 0.01f, Camera.main.aspect);
                //}

                Gizmos.matrix = Matrix4x4.identity;
                
            }
        }
    }
#endif

#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {

        if (Application.isPlaying)
        {            
            var oldColor = Gizmos.color;
            Gizmos.color = visual.frustrumColor;
            Matrix4x4 worldMatrix = Gizmos.matrix;


            Gizmos.matrix = Matrix4x4.TRS(Camera.main.transform.position, Camera.main.transform.rotation, Vector3.one);
            Gizmos.DrawFrustum(Vector3.zero, Camera.main.fieldOfView, 10f, 0.01f, Camera.main.aspect);


            Gizmos.matrix = worldMatrix;
        }
    }
#endif

}
