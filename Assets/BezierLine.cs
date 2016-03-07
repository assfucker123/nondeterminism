using UnityEngine;
using System.Collections;

/// <summary>
/// Set endpoints and anchor points in EdgeCollider2D.  First point is the start point, last point is the end point, and the others are anchor points.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class BezierLine : MonoBehaviour {
    
    public string sortingLayer;
    public int orderInLayer;
    public float thickness = .0625f;
    public Color colorStart = Color.white;
    public Color colorEnd = Color.white;
    public float segmentPeriod = .3f;

    public float approximateDistance { get { return Vector2.Distance(startPoint, endPoint); } }
    public Mode mode { get; protected set; }
    public Vector2 startPoint { get; set; }
    public Vector2 controlPoint1 { get; protected set; }
    public Vector2 controlPoint2 { get; protected set; }
    public Vector2 endPoint { get; protected set; }
    public Vector2 center {  get { return new Vector2((right + left) / 2, (bottom + top) / 2); } }

    public Vector2 interpolate(float t) {
        switch (mode) {
        case Mode.QUADRATIC:
            return Utilities.quadraticBezier(startPoint, controlPoint1, endPoint, t);
        case Mode.CUBIC:
            return Utilities.cubicBezier(startPoint, controlPoint1, controlPoint2, endPoint, t);
        default:
            return startPoint + (endPoint - startPoint) * t;
        }
    }
    public Vector2 interpolateBounded(float t) {
        return interpolate(Mathf.Clamp(t, 0, 1));
    }
    public Vector2 getNormal(float t) {
        float L = .01f;
        Vector2 slope;
        if (t <= L) {
            slope = (interpolate(t + L) - interpolate(t)) / L;
        } else if (t >= 1 - L) {
            slope = (interpolate(t) - interpolate(t - L)) / L;
        } else {
            slope = (interpolate(t + L / 2) - interpolate(t - L / 2)) / L;
        }
        Vector2 ret = new Vector2(-slope.y, slope.x);
        ret.Normalize();
        return ret;
    }

    public enum Mode {
        LINEAR,
        QUADRATIC,
        CUBIC
    }

    public void setLinear(Vector2 p0, Vector2 p1) {
        lineRenderer.SetVertexCount(2);
        lineRenderer.SetPosition(0, p0);
        lineRenderer.SetPosition(1, p1);
        startPoint = p0;
        endPoint = p1;
        mode = Mode.LINEAR;
        left = Mathf.Min(p0.x, p1.x);
        right = Mathf.Max(p0.x, p1.x);
        bottom = Mathf.Min(p0.y, p1.y);
        top = Mathf.Max(p0.y, p1.y);
    }
    public void setQuad(Vector2 p0, Vector2 controlPoint, Vector2 p2) {
        startPoint = p0;
        controlPoint1 = controlPoint;
        endPoint = p2;
        mode = Mode.QUADRATIC;
        segmentPeriod = Mathf.Max(.05f, segmentPeriod);
        int numSegments = Mathf.CeilToInt(approximateDistance / segmentPeriod);
        lineRenderer.SetVertexCount(numSegments + 1);
        left = p0.x; right = p0.x;
        bottom = p0.y; top = p0.y;
        for (int i=0; i<=numSegments; i++) {
            Vector2 pos = interpolate(i * 1.0f / numSegments);
            lineRenderer.SetPosition(i, pos);
            left = Mathf.Min(left, pos.x);
            right = Mathf.Max(right, pos.x);
            bottom = Mathf.Min(bottom, pos.y);
            top = Mathf.Max(top, pos.y);
        }
    }
    public void setCubic(Vector2 p0, Vector2 controlPoint1, Vector2 controlPoint2, Vector2 p2) {
        startPoint = p0;
        this.controlPoint1 = controlPoint1;
        this.controlPoint2 = controlPoint2;
        endPoint = p2;
        mode = Mode.CUBIC;
        segmentPeriod = Mathf.Max(.05f, segmentPeriod);
        int numSegments = Mathf.CeilToInt(approximateDistance / segmentPeriod);
        lineRenderer.SetVertexCount(numSegments + 1);
        left = p0.x; right = p0.x;
        bottom = p0.y; top = p0.y;
        for (int i = 0; i <= numSegments; i++) {
            Vector2 pos = interpolate(i * 1.0f / numSegments);
            lineRenderer.SetPosition(i, pos);
            left = Mathf.Min(left, pos.x);
            right = Mathf.Max(right, pos.x);
            bottom = Mathf.Min(bottom, pos.y);
            top = Mathf.Max(top, pos.y);
        }
    }

    void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
        ec2d = GetComponent<EdgeCollider2D>();
        ec2d.enabled = false;
        lineRenderer.sortingLayerName = sortingLayer;
        lineRenderer.sortingOrder = orderInLayer;
        lineRenderer.SetWidth(thickness, thickness);
        lineRenderer.SetColors(colorStart, colorEnd);

        Vector2 thisPos = new Vector2(transform.localPosition.x, transform.localPosition.y);
        switch (ec2d.pointCount) {
        case 2:
            setLinear(ec2d.points[0] + thisPos, ec2d.points[1] + thisPos);
            break;
        case 3:
            setQuad(ec2d.points[0] + thisPos, ec2d.points[1] + thisPos, ec2d.points[2] + thisPos);
            break;
        case 4:
            setCubic(ec2d.points[0] + thisPos, ec2d.points[1] + thisPos, ec2d.points[2] + thisPos, ec2d.points[3] + thisPos);
            break;
        default:
            Debug.LogError("Error: BezierLine must have 2, 3, or 4 points in the EdgeCollider2D component.");
            break;
        }
    }

    void Start() {
        
    }

    void Update() {

    }

    LineRenderer lineRenderer;
    EdgeCollider2D ec2d;

    float left = 0;
    float top = 0;
    float right = 0;
    float bottom = 0;
}
