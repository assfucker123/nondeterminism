using UnityEngine;
using System.Collections;

public class ShipWallPart : MonoBehaviour {

    public float fadeStartTime = 1.0f;
    public float fadeFinishTime = 2.0f;
    public AudioClip hitSound;

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        spriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        setColor();

        if (time >= fadeFinishTime) {
            timeUser.timeDestroy();
        }
	}

    void OnCollisionEnter2D(Collision2D c2d) {
        if (timeUser.shouldNotUpdate)
            return;
        if (c2d.relativeVelocity.magnitude > 4) {
            SoundManager.instance.playSFXRandPitchBend(hitSound, .02f);
        }
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        setColor();
    }

    void setColor() {
        if (time < fadeStartTime) {
            spriteRenderer.color = Color.white;
        } else {
            spriteRenderer.color = new Color(1, 1, 1, Utilities.easeLinearClamp(time - fadeStartTime, 1, -1, fadeFinishTime - fadeStartTime));
        }
        
    }

    float time = 0;

    TimeUser timeUser;
    SpriteRenderer spriteRenderer;
}
