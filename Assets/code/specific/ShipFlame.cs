using UnityEngine;
using System.Collections;

public class ShipFlame : MonoBehaviour {


    public float scaleMin = .3f;
    public float scaleMax = .95f;
    public float scalePeriod = 4.0f;
    public float scaleVibrateMagnitude = .05f;
    public float scaleVibratePeriod = .1f;
    public float timeOffset = 0;
    public GameObject flameParticleGameObject;
    public float flameParticleX = -2;
    public float flameParticleSpread = 3;
    public float flameParticleMaxSpeed = 10;
    public float flameParticleFrequency = 2;

    public bool triggerFlashbackHaltScreen = false;
    public float triggerFlashbackHaltScreenDelay = .25f;

	void Awake() {
        timeUser = GetComponent<TimeUser>();
	}

    void Start() {
        time = timeOffset;
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        particleTime += Time.deltaTime;
        while (particleTime > 1 / flameParticleFrequency) {

            Vector3 pos = new Vector3(flameParticleX, (timeUser.randomValue() * 2 - 1)/2 * flameParticleSpread, 0);
            FlameParticle fp = (GameObject.Instantiate(flameParticleGameObject, transform.localPosition + pos, Quaternion.identity) as GameObject).GetComponent<FlameParticle>();
            fp.speed = flameParticleMaxSpeed * Mathf.Abs(transform.localScale.x);
            fp.heading = 180;

            particleTime -= 1 / flameParticleFrequency;
        }

        updateScale();

        if (time > timeToTriggerHaltScreen) {
            if (!Vars.currentNodeData.eventHappened(AdventureEvent.Physical.HIT_PLAYER_WITH_TUTORIAL_WALL)) {
                Vars.currentNodeData.eventHappen(AdventureEvent.Physical.HIT_PLAYER_WITH_TUTORIAL_WALL);
                if (Player.instance.phase > 0) {
                    ControlsMessageSpawner.instance.spawnHaltScreen(HaltScreen.Screen.FLASHBACK);
                }
            }
            timeToTriggerHaltScreen = 999999;
        }
        
	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.floats["pt"] = particleTime;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        particleTime = fi.floats["pt"];
        updateScale();
    }

    void OnDealDamage(ReceivesDamage rd) {
        if (triggerFlashbackHaltScreen) {
            timeToTriggerHaltScreen = time + triggerFlashbackHaltScreenDelay;
        }
    }

    void updateScale() {
        float scale = Mathf.Sin(time / scalePeriod * Mathf.PI*2) * (scaleMax - scaleMin) / 2 + (scaleMax + scaleMin) / 2;

        float scaleVibrate = Mathf.Sin(time / scaleVibratePeriod * Mathf.PI*2) * scaleVibrateMagnitude * scale;

        transform.localScale = new Vector3(scale + scaleVibrate, transform.localScale.y, transform.localScale.z);
    }

    float time = 0;
    float particleTime = 0;
    float timeToTriggerHaltScreen = 999999;

    TimeUser timeUser;

}
