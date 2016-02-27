using UnityEngine;
using System.Collections;

public class AmbushBGOverlay : MonoBehaviour {

    public void fadeIn(float duration) {
        fadingIn = true;
        time = 0;
        this.duration = duration;
    }

    public void fadeOut(float duration) {
        fadingIn = false;
        time = 0;
        this.duration = duration;
    }

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1, 1, 1, 0);
    }

    void Start() {
        GetComponent<Parallax>().position = CameraControl.getMapBounds().center; // ensure will always be in the center of the screen
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        setColor();
	}

    void setColor() {
        Color color = spriteRenderer.color;
        if (!timeUser.exists) {
            color.a = 0;
        } else if (fadingIn) {
            if (time >= duration) {
                color.a = 1;
            } else {
                color.a = Utilities.easeLinearClamp(time, 0, 1, duration);
            }
        } else {
            if (time >= duration) {
                color.a = 0;
            } else {
                color.a = Utilities.easeLinearClamp(time, 1, -1, duration);
            }
        }
        spriteRenderer.color = color;
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.floats["d"] = duration;
        fi.bools["fi"] = fadingIn;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        duration = fi.floats["d"];
        fadingIn = fi.bools["fi"];
        setColor();
    }

    TimeUser timeUser;
    SpriteRenderer spriteRenderer;

    float time = 0;
    float duration = 0;
    bool fadingIn = false;
}
