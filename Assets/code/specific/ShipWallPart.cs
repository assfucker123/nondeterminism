using UnityEngine;
using System.Collections;

public class ShipWallPart : MonoBehaviour {

    public float fadeStartTime = 1.0f;
    public float fadeFinishTime = 2.0f;
    public int damage = 1;
    public AudioClip hitSound;
    public bool takeDownMessages = false;

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

            // get rid of tutorial messages
            if (takeDownMessages && ControlsMessageSpawner.instance != null) {
                ControlsMessageSpawner.instance.takeDownMessage(ControlsMessage.Control.RUN_AIM);
                ControlsMessageSpawner.instance.takeDownMessage(ControlsMessage.Control.JUMP);
                ControlsMessageSpawner.instance.takeDownMessage(ControlsMessage.Control.SHOOT);
            }
        }
	}

    void OnCollisionEnter2D(Collision2D c2d) {
        if (timeUser.shouldNotUpdate)
            return;
        if (c2d.relativeVelocity.magnitude > 4) {
            SoundManager.instance.playSFXRandPitchBend(hitSound, .02f);
        }

        if (c2d.gameObject == Player.instance.gameObject) {
            if (c2d.contacts[0].normal.y > .5f) {
                Player.instance.GetComponent<ReceivesDamage>().dealDamage(damage, true);
            }
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
        if (timeUser.shouldNotUpdate)
            return;
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
