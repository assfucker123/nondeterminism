using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Segment {

    ////////////
    // STATIC //
    ////////////

    public static List<Segment> bottomSegments = new List<Segment>();
    public static List<Segment> leftSegments = new List<Segment>();
    public static List<Segment> topSegments = new List<Segment>();
    public static List<Segment> rightSegments = new List<Segment>();

    /* Finds bottomSegment that most closely matches the position.
     * Returns null if none found. */
    public static Segment findBottom(Vector2 pos) {
        Segment ret = null;
        float dist = 0;
        foreach (Segment seg in bottomSegments) {
            if (seg.p0.x <= pos.x && pos.x <= seg.p1.x) {
                if (ret == null ||
                    (Mathf.Abs(pos.y - seg.p0.y) < dist)) {
                    ret = seg;
                    dist = Mathf.Abs(pos.y - seg.p0.y);
                }
            }
        }
        return ret;
    }

    /* Finds topSegment that most closely matches the position.
     * Returns null if none found. */
    public static Segment findTop(Vector2 pos) {
        Segment ret = null;
        float dist = 0;
        foreach (Segment seg in topSegments) {
            if (seg.p0.x <= pos.x && pos.x <= seg.p1.x) {
                if (ret == null ||
                    (Mathf.Abs(pos.y - seg.p0.y) < dist)) {
                    ret = seg;
                    dist = Mathf.Abs(pos.y - seg.p0.y);
                }
            }
        }
        return ret;
    }

    /* Finds leftSegment that most closely matches the position.
     * Returns null if none found. */
    public static Segment findLeft(Vector2 pos) {
        Segment ret = null;
        float dist = 0;
        foreach (Segment seg in leftSegments) {
            if (seg.p0.y <= pos.y && pos.y <= seg.p1.y) {
                if (ret == null ||
                    (Mathf.Abs(pos.x - seg.p0.x) < dist)) {
                    ret = seg;
                    dist = Mathf.Abs(pos.x - seg.p0.x);
                }
            }
        }
        return ret;
    }

    /* Finds rightSegment that most closely matches the position.
     * Returns null if none found. */
    public static Segment findRight(Vector2 pos) {
        Segment ret = null;
        float dist = 0;
        foreach (Segment seg in rightSegments) {
            if (seg.p0.y <= pos.y && pos.y <= seg.p1.y) {
                if (ret == null ||
                    (Mathf.Abs(pos.x - seg.p0.x) < dist)) {
                    ret = seg;
                    dist = Mathf.Abs(pos.x - seg.p0.x);
                }
            }
        }
        return ret;
    }

    /* Selects a random Segment from segments, provided value is a random number in [0, 1).
     * Segments with a greater length are more likely to be picked. */
    public static Segment weightedRandom(List<Segment> segments, float value) {
        if (value < 0 || value >= 1 || segments.Count == 0)
            return null;
        float len = 0;
        for (int i = 0; i < segments.Count; i++) {
            len += segments[i].length;
        }
        float target = len * value;
        len = 0;
        for (int i = 0; i < segments.Count; i++) {
            len += segments[i].length;
            if (len > target)
                return segments[i];
        }
        return segments[segments.Count-1];
    }

    public static void sortOnY(List<Segment> segments) {
        segments.Sort(delegate(Segment seg1, Segment seg2) {
            return (int)((seg1.p0.y + seg1.p1.y) / 2 - (seg2.p0.y + seg2.p1.y) / 2);
        });
    }

    /* Attempts to add segment to one of the above lists.
     * Prereq: apply snapToWall() beforehand. */
    public static bool addSegment(Segment seg) {
        switch (seg.wall) {
        case Wall.BOTTOM:
            bottomSegments.Add(seg);
            break;
        case Wall.LEFT:
            leftSegments.Add(seg);
            break;
        case Wall.TOP:
            topSegments.Add(seg);
            break;
        case Wall.RIGHT:
            rightSegments.Add(seg);
            break;
        default:
            return false;
        }
        return true;
    }

    /* Removes all stores segments */
    public static void removeAll() {
        bottomSegments.Clear();
        leftSegments.Clear();
        topSegments.Clear();
        rightSegments.Clear();
    }

    ////////////
    // PUBLIC //
    ////////////

    public enum Wall {
        NONE,
        BOTTOM,
        LEFT,
        TOP,
        RIGHT
    }

    public string name = ""; // will be set to the name of the GameObject in the inspector

    public Vector2 p0 = new Vector2();
    public Vector2 p1 = new Vector2();
    public Wall wall = Wall.NONE;
    public bool horizontal {
        get { return Mathf.Abs(p0.y - p1.y) < .00001f; }
    }
    public bool vertical {
        get { return Mathf.Abs(p0.x - p1.x) < .00001f; }
    }
    public float length {
        get {
            if (horizontal) {
                return Mathf.Abs(p0.x - p1.x);
            } else if (vertical) {
                return Mathf.Abs(p0.y - p1.y);
            } else {
                return Vector2.Distance(p0, p1);
            }
        }
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    /* Attempts to snap segment to a wall. */
    public bool snapToWall() {
        if (!smoothen())
            return false;

        if (horizontal) {
            //can snap to top or bottom wall
            bool toTop = false;
            RaycastHit2D topResult = Physics2D.Raycast(p0, new Vector2(0, 1), 1.0f, 1 << LayerMask.NameToLayer("Default"));
            RaycastHit2D bottomResult = Physics2D.Raycast(p0, new Vector2(0, -1), 1.0f, 1 << LayerMask.NameToLayer("Default"));
            if (topResult.collider == null) {
                if (bottomResult.collider == null)
                    return false;
                toTop = false;
            } else {
                if (bottomResult.collider == null)
                    toTop = true;
                else {
                    toTop = (topResult.distance < bottomResult.distance);
                }
            }
            if (toTop) {
                p0.y = topResult.point.y;
                wall = Wall.TOP;
            } else {
                p0.y = bottomResult.point.y;
                wall = Wall.BOTTOM;
            }
            p1.y = p0.y;
            return true;
        } else {
            //can snap to left or right wall
            bool toRight = false;
            RaycastHit2D rightResult = Physics2D.Raycast(p0, new Vector2(1, 0), 1.0f, 1 << LayerMask.NameToLayer("Default"));
            RaycastHit2D leftResult = Physics2D.Raycast(p0, new Vector2(-1, 0), 1.0f, 1 << LayerMask.NameToLayer("Default"));
            if (rightResult.collider == null) {
                if (leftResult.collider == null)
                    return false;
                toRight = false;
            } else {
                if (leftResult.collider == null)
                    toRight = true;
                else {
                    toRight = (rightResult.distance < leftResult.distance);
                }
            }
            if (toRight) {
                p0.x = rightResult.point.x;
                wall = Wall.RIGHT;
            } else {
                p0.x = leftResult.point.x;
                wall = Wall.LEFT;
            }
            p1.x = p0.x;
            return true;
        }
    }

    /* Travel along the segment.
     * If hits an end, will bounce back.
     * start:
     *    for horizontal segment: p0.x <= start <= p1.x, returns x coordinate
     *    for vertical segment: p0.y <= start <= p1.y, returns y coordinate
     *    doesn't work for other segments
     * velocity: speed travelling at.  Be negative to go backwards.
     * time: duration spent travelling. */
    public float travel(float start, float velocity, float time) {
        if (!horizontal && !vertical)
            return 0;
        if (horizontal)
            start -= p0.x;
        else
            start -= p0.y;
        float dist = start + velocity * time;
        float inter = 0;
        int halfLaps = Mathf.FloorToInt(dist / length);
        inter = dist - halfLaps * length;
        if (halfLaps % 2 != 0) {
            inter = length - inter;
        }
        Vector2 ret = interpolate(inter / length);
        if (horizontal)
            return ret.x;
        else
            return ret.y;
    }

    /* Retruns if the object would turn around when travelling the segment
     * start:
     *    for horizontal segment: p0.x <= start <= p1.x
     *    for vertical segment: p0.y <= start <= p1.y
     *    doesn't work for other segments
     * velocity: speed travelling at.  Be negative to go backwards.
     * time: duration spent travelling. */
    public bool travelTurnsAround(float start, float velocity, float time) {
        if (!horizontal && !vertical)
            return false;
        if (horizontal)
            start -= p0.x;
        else
            start -= p0.y;
        float dist = start + velocity * time;
        int halfLaps = Mathf.FloorToInt(dist / length);
        if (halfLaps % 2 != 0)
            return true;
        return false;
    }

    /* Travel along the segment.
     * But if hits an end, will stay there and not bounce back.
     * start:
     *    for horizontal segment: p0.x <= start <= p1.x, returns x coordinate
     *    for vertical segment: p0.y <= start <= p1.y, returns y coordinate
     *    doesn't work for other segments
     * velocity: speed travelling at.  Be negative to go backwards.
     * time: duration spent travelling. */
    public float travelClamp(float start, float velocity, float time) {
        if (!horizontal && !vertical)
            return 0;
        if (horizontal)
            start -= p0.x;
        else
            start -= p0.y;
        float dist = start + velocity * time;
        float inter = Mathf.Max(0, Mathf.Min(length, dist));
        Vector2 ret = interpolate(inter / length);
        if (horizontal)
            return ret.x;
        else
            return ret.y;
    }

    /* val is in [0, 1] */
    public Vector2 interpolate(float val) {
        return p0 + (p1 - p0) * val;
    }

    /* Attempts to make segment perfectly horizontal or vertical.
     * Also goes left->right and down->up. */
    public bool smoothen() {
        if (Mathf.Abs(p0.y - p1.y) < .2f) {
            p0.y = (p0.y + p1.y) / 2;
            p1.y = p0.y;
            if (p1.x < p0.x) {
                float temp = p1.x;
                p1.x = p0.x;
                p0.x = temp;
            }
            return true;
        }
        if (Mathf.Abs(p0.x - p1.x) < .2f) {
            p0.x = (p0.x + p1.x) / 2;
            p1.x = p0.x;
            if (p1.y < p0.y) {
                float temp = p1.y;
                p1.y = p0.y;
                p0.y = temp;
            }
            return true;
        }
        return false;
    }

    
}
