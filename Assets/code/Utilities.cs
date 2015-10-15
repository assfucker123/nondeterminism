using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Utilities {

	/* 
	 * Range: [0, 360) */
	public static float get2DRot(Quaternion quaternion){
		return quaternion.eulerAngles.z;
	}
	public static Quaternion setQuat(float rotation2D){
		Quaternion quat = Quaternion.identity;
		quat.SetFromToRotation(
			Vector3.right,
			new Vector3(Mathf.Cos(rotation2D*Mathf.PI/180), Mathf.Sin(rotation2D*Mathf.PI/180))
			);
		return quat;
	}
	public static Vector2 rotateAroundPoint(Vector2 v, Vector2 point, float rotationRadians){
		Vector2 ret = new Vector2();
		float c = Mathf.Cos(rotationRadians);
		float s = Mathf.Sin(rotationRadians);
		ret.x = point.x + (v.x - point.x)*c - (v.y - point.y)*s;
		ret.y = point.y + (v.x - point.x)*s + (v.y - point.y)*c;
		return ret;
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

    // Tiled map stuff
    public static Rect getMapBounds() {
        GameObject map = GameObject.FindWithTag("Map");
        Debug.Assert(map != null);
        Tiled2Unity.TiledMap tiledMap = map.GetComponent<Tiled2Unity.TiledMap>();
        Debug.Assert(tiledMap != null);
        return new Rect(
            map.transform.position.x,
            -map.transform.position.y,
            tiledMap.GetMapWidthInPixelsScaled(),
            tiledMap.GetMapHeightInPixelsScaled()
            );
    }
	
};
