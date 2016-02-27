using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ambush : MonoBehaviour {

    public float camMoveDuration = .4f;
    public float camResumeDuration = .4f;
    public float spearFallDelay = .3f;
    public float waveSpawnDelay = 0;
    public float waveSpawnFinishedDelay = .4f;
    public float notificationDelay = .5f;
    public bool useIncludedSensor = false; // it's preferred to use AmbushTrigger
    public List<GameObject> ambushSpearRefs = new List<GameObject>(); // if null, will just activate all of them in a room

    public bool activated {  get { return state != State.NOT_ACTIVE; } }

    public enum State {
        NOT_ACTIVE,
        ACTIVATED,
        DEFEATED
    }

    public void activate() {
        if (activated) return;

        // move camera
        CameraControl.instance.moveToPosition(new Vector2(transform.localPosition.x, transform.localPosition.y), camMoveDuration);

        HUD.instance.speedLines.flash(new Color(0, 0, 0, 0f), new Color(0, 0, 0, .3f), .3f);

        state = State.ACTIVATED;
        time = 0;
        waveSpawnFinishedTime = 0;
        waveSpawnDelay = Mathf.Max(.01f, waveSpawnDelay);
    }

    public void ambushDefeated() {
        if (state != State.ACTIVATED) return;

        // move camera back to following player
        CameraControl.instance.followPlayer(camResumeDuration);

        // raise spears
        foreach (GameObject spearGO in ambushSpearRefs) {
            spearGO.GetComponent<AmbushSpear>().rise();
        }

        // record that ambush was defeated
        Vars.currentNodeData.defeatAmbush(Vars.currentLevel);

        Debug.Log("Ambush defeated.  Now need to add sound effects, escaping an ambush, etc.");

        state = State.DEFEATED;
        time = 0;
        toDisplayNotification = true;
    }

	void Awake() {
        waveSpawner = GetComponent<WaveSpawner>();
        timeUser = GetComponent<TimeUser>();
	}

    void Start() {
        // deactivate if already defeated
        if (Vars.currentNodeData.ambushDefeated(Vars.currentLevel)) {
            state = State.DEFEATED;
        }
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;
        switch (state) {
        case State.ACTIVATED:
            // start wave spawner
            if (time >= waveSpawnDelay && time-Time.deltaTime < waveSpawnDelay) {
                waveSpawner.startSpawner();
            }
            // have spears fall
            if (time >= spearFallDelay && time-Time.deltaTime < spearFallDelay) {
                if (ambushSpearRefs.Count == 0) {
                    AmbushSpear[] spears = GameObject.FindObjectsOfType<AmbushSpear>();
                    foreach (AmbushSpear spear in spears) {
                        ambushSpearRefs.Add(spear.gameObject);
                    }
                }
                foreach (GameObject spearGO in ambushSpearRefs) {
                    spearGO.GetComponent<AmbushSpear>().fall();
                }
            }

            if (waveSpawner.finished) {
                waveSpawnFinishedTime += Time.deltaTime;
                if (waveSpawnFinishedTime > waveSpawnFinishedDelay) {
                    ambushDefeated();
                }
            }
            break;
        case State.DEFEATED:
            time += Time.deltaTime;
            // display notification that ambush was defeated
            if (toDisplayNotification && time >= notificationDelay) {
                Notification.instance.displayNotification("Ambush defeated.", Notification.NotifType.DEFAULT);
                toDisplayNotification = false;
            }
            break;
        }
	}

    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["t"] = time;
        fi.bools["tdn"] = toDisplayNotification;
    }

    void OnRevert(FrameInfo fi) {
        State prevState = state;
        state = (State)fi.state;
        if (prevState == State.DEFEATED && state != State.DEFEATED) {
            // undo record of ambush defeated
            Vars.currentNodeData.defeatAmbushUndo(Vars.currentLevel);
        }
        time = fi.floats["t"];
        toDisplayNotification = fi.bools["tdn"];
    }

    void OnTriggerEnter2D(Collider2D c2d) {
        if (!useIncludedSensor) return;
        if (c2d.gameObject == null) return;
        if (c2d.gameObject != Player.instance.gameObject) return;

        activate();
    }

    WaveSpawner waveSpawner;
    TimeUser timeUser;

    State state = State.NOT_ACTIVE;
    float time = 0;
    float waveSpawnFinishedTime = 0;
    bool toDisplayNotification = false;
}
