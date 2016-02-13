using UnityEngine;
using System.Collections;

public class ImpactShard : MonoBehaviour {

    public Vector2 initialVelocity = new Vector2();
    public float gravity = 30;
    public float rotationMaxSpeed = 400;

	void Awake() {
        timeUser = GetComponent<TimeUser>();
	}
    void Start() {
        v = initialVelocity;
        rotationSpeed = (timeUser.randomValue() * 2 - 1) * rotationMaxSpeed;
    }
	
	void Update() {
        if (timeUser.shouldNotUpdate)
            return;

        v.y -= Time.deltaTime * gravity;
        transform.localPosition += new Vector3(v.x, v.y) * Time.deltaTime;
        transform.localRotation = Utilities.setQuat(Utilities.get2DRot(transform.localRotation) + rotationSpeed * Time.deltaTime);
	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["vx"] = v.x;
        fi.floats["vy"] = v.y;
    }
    void OnRevert(FrameInfo fi) {
        v.Set(fi.floats["vx"], fi.floats["vy"]);
    }

    float rotationSpeed = 0;
    Vector2 v = new Vector2();
    TimeUser timeUser;

}
