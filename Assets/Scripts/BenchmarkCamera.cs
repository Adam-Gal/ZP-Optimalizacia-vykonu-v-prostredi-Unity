using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BenchmarkCamera : MonoBehaviour
{
    [System.Serializable]
    public class CameraPoint
    {
        public Transform point;
        public Transform lookTarget;
    }

    [Header("Path")]
    public List<CameraPoint> points = new List<CameraPoint>();
    public float duration = 20f;
    public bool loop = true;
    public bool playOnStart = true;

    [Header("Rotation")]
    public float rotationSmooth = 8f;
    
    [Header("Rotation Blending")]
    [Range(0f, 0.5f)]
    public float rotationBlendZone = 0.2f;
    
    [Header("Start Delay")]
    public float startDelay = 10f;

    private float time;
    private bool playing;

    private void Start()
    {
        if (playOnStart && points.Count >= 2)
            StartCoroutine(StartWithDelay());
    }

    private IEnumerator StartWithDelay()
    {
        yield return new WaitForSeconds(startDelay);
        playing = true;
    }

    private void Update()
    {
        if (!playing || points.Count < 2)
            return;

        time += Time.deltaTime;

        // Ak necyklujeme a čas vypršal
        if (!loop && time >= duration)
        {
            time = duration;
            MoveAlongSpline(1f);
            playing = false;
            Application.Quit();
            return;
        }

        float t = time / duration;

        if (loop)
            t %= 1f;
        else
            t = Mathf.Clamp01(t);

        MoveAlongSpline(t);
    }

    private void MoveAlongSpline(float t)
    {
        int count = points.Count;

        float scaledT = t * count;
        int i = Mathf.FloorToInt(scaledT);
        float localT = scaledT - i;

        int p0 = WrapIndex(i - 1);
        int p1 = WrapIndex(i);
        int p2 = WrapIndex(i + 1);
        int p3 = WrapIndex(i + 2);

        Vector3 pos = CatmullRom(
            points[p0].point.position,
            points[p1].point.position,
            points[p2].point.position,
            points[p3].point.position,
            localT
        );

        Vector3 tangent = CatmullRomTangent(
            points[p0].point.position,
            points[p1].point.position,
            points[p2].point.position,
            points[p3].point.position,
            localT
        );

        transform.position = pos;

        Quaternion currentRot = GetRotationForPoint(p1, pos, tangent);
        Quaternion nextRot = GetRotationForPoint(p2, pos, tangent);

        float blend = 0f;

        if (localT > 1f - rotationBlendZone)
        {
            float blendT = (localT - (1f - rotationBlendZone)) / rotationBlendZone;
            blend = Mathf.SmoothStep(0f, 1f, blendT);
        }

        Quaternion blendedTarget = Quaternion.Slerp(currentRot, nextRot, blend);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            blendedTarget,
            rotationSmooth * Time.deltaTime
        );
    }
    
    private Quaternion GetRotationForPoint(int index, Vector3 pos, Vector3 tangent)
    {
        if (points[index].lookTarget != null)
        {
            Vector3 dir = points[index].lookTarget.position - pos;
            return Quaternion.LookRotation(dir.normalized);
        }
        else
        {
            return Quaternion.LookRotation(tangent.normalized);
        }
    }

    private int WrapIndex(int i)
    {
        int count = points.Count;
        if (loop)
            return (i % count + count) % count;
        return Mathf.Clamp(i, 0, count - 1);
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

    private Vector3 CatmullRomTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            (-p0 + p2) +
            2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t +
            3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t * t
        );
    }
}