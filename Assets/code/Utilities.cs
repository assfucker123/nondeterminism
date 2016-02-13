using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Utilities {

	/* 
	 * Range: [0, 360) */
	public static float get2DRot(Quaternion quaternion){
        // weird problem.  when setting quaternion to 180 degrees, changes y instead of z?  not sure what's going on
        if (Mathf.Abs(quaternion.eulerAngles.z) < .001f && Mathf.Abs(quaternion.eulerAngles.y) > .0001f) {
            return quaternion.eulerAngles.y;
        }
		return quaternion.eulerAngles.z;
	}
	public static Quaternion setQuat(float rotation2D){
        return Quaternion.Euler(new Vector3(0, 0, rotation2D));
	}
    /* rotates v around point */
	public static Vector2 rotateAroundPoint(Vector2 v, Vector2 point, float rotationRadians){
		Vector2 ret = new Vector2();
		float c = Mathf.Cos(rotationRadians);
		float s = Mathf.Sin(rotationRadians);
		ret.x = point.x + (v.x - point.x)*c - (v.y - point.y)*s;
		ret.y = point.y + (v.x - point.x)*s + (v.y - point.y)*c;
		return ret;
	}

    public static byte[] stringToBytes(string str) {
        byte[] bytes = new byte[str.Length * sizeof(char)];
        System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
        return bytes;
    }

    public static string bytesToString(byte[] bytes) {
        char[] chars = new char[bytes.Length / sizeof(char)];
        System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
        return new string(chars);
    }

    /* Unity stack trace */
    public static void debugLogCallStack() {
        Debug.Log(UnityEngine.StackTraceUtility.ExtractStackTrace());
    }

    /* calculates x % y */
    public static float fmod(float x, float y) {
        return x - Mathf.Floor(x / y) * y;
    }

    /* returns if v2 is positioned clockwise to v1 */
    public static bool isClockwise(Vector2 v1, Vector2 v2) {
        return -v1.x * v2.y + v1.y * v2.x > 0;
    }

    /* returns if point is contained in sector (angleSpread must be less than PI)  */
    public static bool pointInSector(Vector2 point, Vector2 center, float radius, float centerAngleRadians, float angleSpreadRadians) {
        Vector2 vPoint = point - center;
        if (vPoint.x*vPoint.x + vPoint.y+vPoint.y > radius * radius)
            return false;
        float a = centerAngleRadians - angleSpreadRadians / 2;
        Vector2 sectorStart = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        a = centerAngleRadians + angleSpreadRadians / 2;
        Vector2 sectorEnd = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        return !isClockwise(sectorStart, vPoint) && isClockwise(sectorEnd, vPoint);
    }

    /* returns projection of x onto v */
    public static Vector2 vectorProjection(Vector2 v, Vector2 x) {
        return v * Vector2.Dot(x, v) / Vector2.Dot(v, v);
    }

    /* returns the point on line defined by lineP0 and lineP1 that is closest to the given point */
    public static Vector2 closestPointOnLineToPoint(Vector2 lineP0, Vector2 lineP1, Vector2 point) {
        return lineP0 + vectorProjection(lineP1 - lineP0, point - lineP0);
    }
    
    public static float easeLinear(float t, float b, float c, float d){
        return c*t/d + b;
    }
    public static float easeLinearClamp(float t, float b, float c, float d) {
        t = Mathf.Min(d, Mathf.Max(0, t));
        return easeLinear(t, b, c, d);
    }
    public static float easeInQuad(float t, float b, float c, float d){
        t /= d;
        return c*t*t + b;
    }
    public static float easeInQuadClamp(float t, float b, float c, float d) {
        t = Mathf.Min(d, Mathf.Max(0, t));
        return easeInQuad(t, b, c, d);
    }
    public static float easeOutQuad(float t, float b, float c, float d) {
        t /= d;
        return -c * t * (t - 2) + b;
    }
    public static float easeOutQuadClamp(float t, float b, float c, float d) {
        t = Mathf.Min(d, Mathf.Max(0, t));
        return easeOutQuad(t, b, c, d);
    }
    public static float easeInOutQuad(float t, float b, float c, float d) {
        t /= d / 2;
        if (t < 1) return c / 2 * t * t + b;
        t--;
        return -c / 2 * (t * (t - 2) - 1) + b;
    }
    public static float easeInOutQuadClamp(float t, float b, float c, float d) {
        t = Mathf.Min(d, Mathf.Max(0, t));
        return easeInOutQuad(t, b, c, d);
    }
    public static float easeInCubic(float t, float b, float c, float d) {
        t /= d;
	    return c*t*t*t + b;
    }
    public static float easeInCubicClamp(float t, float b, float c, float d) {
        t = Mathf.Min(d, Mathf.Max(0, t));
        return easeInCubic(t, b, c, d);
    }
    public static float easeOutCubic(float t, float b, float c, float d) {
        t /= d;
        t--;
        return c*(t*t*t + 1) + b;
    }
    public static float easeOutCubicClamp(float t, float b, float c, float d) {
        t = Mathf.Min(d, Mathf.Max(0, t));
        return easeOutCubic(t, b, c, d);
    }
    public static float easeInOutCubic(float t, float b, float c, float d) {
        t /= d/2;
        if (t < 1) return c/2*t*t*t + b;
        t -= 2;
        return c/2*(t*t*t + 2) + b;
    }
    public static float easeInOutCubicClamp(float t, float b, float c, float d) {
        t = Mathf.Min(d, Mathf.Max(0, t));
        return easeInOutCubic(t, b, c, d);
    }

    public static float easeInCirc(float t, float b, float c, float d) {
        t /= d;
        return -c * (Mathf.Sqrt(1 - t * t) - 1) + b;
    }
    public static float easeInCircClamp(float t, float b, float c, float d) {
        t = Mathf.Min(d, Mathf.Max(0, t));
        return easeInCirc(t, b, c, d);
    }
    public static float easeOutCirc(float t, float b, float c, float d) {
        t /= d;
        t--;
        return c * Mathf.Sqrt(1 - t * t) + b;
    }
    public static float easeOutCircClamp(float t, float b, float c, float d) {
        t = Mathf.Min(d, Mathf.Max(0, t));
        return easeOutCirc(t, b, c, d);
    }
    public static float easeInOutCirc(float t, float b, float c, float d) {
        t /= d / 2;
        if (t < 1) return -c / 2 * (Mathf.Sqrt(1 - t * t) - 1) + b;
        t -= 2;
        return c / 2 * (Mathf.Sqrt(1 - t * t) + 1) + b;
    }
    public static float easeInOutCircClamp(float t, float b, float c, float d) {
        t = Mathf.Min(d, Mathf.Max(0, t));
        return easeOutCircClamp(t, b, c, d);
    }


};
