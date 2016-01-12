using UnityEngine;
using System.Collections;

public class ShipWallPart : MonoBehaviour {

    public float fadeStartTime = 1.0f;
    public float fadeFinishTime = 2.0f;
    public int damage = 1;
    public AudioClip hitSound;
    public bool takeDownMessages = false;
    public float flashbackHaltMessageDelay = .3f;

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        eventHappener = GetComponent<EventHappener>();
	}
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        timeSinceHitPlayer += Time.deltaTime;
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

        if (timeSinceHitPlayer - Time.deltaTime < flashbackHaltMessageDelay &&
            timeSinceHitPlayer >= flashbackHaltMessageDelay) {

            if (triggerHaltScreen) {
                
                if (Player.instance.phase > 0) {
                    ControlsMessageSpawner.instance.spawnHaltScreen(HaltScreen.Screen.FLASHBACK);
                }
                triggerHaltScreen = false;

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
            if (c2d.contacts[0].normal.y > .4f) {
                Player.instance.GetComponent<ReceivesDamage>().dealDamage(damage, true);
                if (!Vars.currentNodeData.eventHappened(AdventureEvent.Physical.HIT_PLAYER_WITH_TUTORIAL_WALL)) {
                    eventHappener.physicalHappen(AdventureEvent.Physical.HIT_PLAYER_WITH_TUTORIAL_WALL, false);
                    triggerHaltScreen = true;
                }
                
                timeSinceHitPlayer = 0;
            }
        }

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.floats["tshp"] = timeSinceHitPlayer;
        fi.bools["ths"] = triggerHaltScreen;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        timeSinceHitPlayer = fi.floats["tshp"];
        triggerHaltScreen = fi.bools["ths"];
        setColor();
    }

    void setColor() {
        if (!timeUser.exists)
            return;
        if (time < fadeStartTime) {
            spriteRenderer.color = Color.white;
        } else {
            spriteRenderer.color = new Color(1, 1, 1, Utilities.easeLinearClamp(time - fadeStartTime, 1, -1, fadeFinishTime - fadeStartTime));
        }
        
    }

    float time = 0;
    float timeSinceHitPlayer = 9999;
    bool triggerHaltScreen = false;

    TimeUser timeUser;
    SpriteRenderer spriteRenderer;
    EventHappener eventHappener;
}
