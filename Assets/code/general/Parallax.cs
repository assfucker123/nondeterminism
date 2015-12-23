using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Parallax : MonoBehaviour {

    public float xFactor = 1;
    public float yFactor = 1;

    [HideInInspector]
    public Vector2 position = new Vector2(); // the "real" position.  Set this in-game instead of the transform

    /*
    When camera is in the middle of the level, position appears normal.
    But when camera moves 10 to the left, xFactor of .2 means object appeared to only move 2 to the right

    */

    public static List<Parallax> parallaxs = new List<Parallax>();

    public void updateTransform(Vector2 cameraDistanceFromCenter) {
        transform.localPosition = new Vector3(
            position.x + cameraDistanceFromCenter.x * (1 - xFactor),
            position.y + cameraDistanceFromCenter.y * (1 - yFactor),
            transform.localPosition.z);
    }

    public void setInitialPositionFromTransform() {
        position.x = transform.localPosition.x;
        position.y = transform.localPosition.y;
    }

    void Awake() {
        parallaxs.Add(this);
    }

	void Start() {
        setInitialPositionFromTransform();
	}
	
	// Update is called once per frame
	void Update() {
	    
	}

    void OnDestroy() {
        parallaxs.Remove(this);
    }


}
