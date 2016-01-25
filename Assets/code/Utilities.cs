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

};
