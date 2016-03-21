using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Ambush : MonoBehaviour {

    public float camMoveDuration = .4f;
    public float camResumeDuration = .4f;
    public float spearFallDelay = .3f;
    public float waveSpawnDelay = 0;
    public float waveSpawnFinishedDelay = .4f;
    public float spearRiseDelay = .4f;
    public float notificationDelay = .5f;
    public float escapedNotificationDelay = 1.0f;
    public bool useIncludedSensor = false; // it's preferred to use AmbushTrigger
    public List<GameObject> ambushSpearRefs = new List<GameObject>(); // if null, will just activate all of them in a room
    public AudioClip startSound;
    public Color startFlashColor = new Color(0, 0, 0, .3f);
    public AudioClip defeatedSound;
    public Color defeatedFlashColor = new Color(0, .9f, .1f, .4f);
    public AudioClip escapedSound;
    public Color escapedFlashColor = new Color(.5f, .5f, .5f, 1);
    public TextAsset textAsset;
    public float bgOverlayFadeInDuration = .4f;
    public float bgOverlayFadeOutDuration = .4f;
    public GameObject bgOverlayGameObject;

    public bool activated {  get { return state == State.ACTIVATED || state == State.DEFEATED; } }

    public enum State {
        NOT_ACTIVE,
        ACTIVATED,
        DEFEATED,
        ESCAPED
    }

    public void activate() {
        if (activated) return;

        // move camera
        CameraControl.instance.moveToPosition(new Vector2(transform.localPosition.x, transform.localPosition.y), camMoveDuration);

        HUD.instance.speedLines.flash(Color.clear, startFlashColor, .3f);
        bgOverlay.fadeIn(bgOverlayFadeInDuration);
        SoundManager.instance.playSFX(startSound);

        state = State.ACTIVATED;
        time = 0;
        waveSpawnFinishedTime = 0;
        waveSpawnDelay = Mathf.Max(.01f, waveSpawnDelay);
    }

    public void ambushDefeated() {
        if (state != State.ACTIVATED) return;

        // move camera back to following player
        CameraControl.instance.followPlayer(camResumeDuration);

        // record that ambush was defeated
        Vars.currentNodeData.defeatAmbush(Vars.currentLevel);

        SoundManager.instance.playSFX(defeatedSound);
        bgOverlay.fadeOut(bgOverlayFadeOutDuration);
        HUD.instance.speedLines.flash(Color.clear, defeatedFlashColor, .4f);

        state = State.DEFEATED;
        time = 0;
        toDisplayNotification = true;
        toRiseSpears = true;
    }

    public void ambushEscaped() {
        if (state != State.ACTIVATED) return;

        // very "bright" flash to hide enemies being destroyed
        HUD.instance.speedLines.flash(Color.clear, escapedFlashColor, .8f);
        bgOverlay.fadeOut(bgOverlayFadeOutDuration);
        SoundManager.instance.playSFX(escapedSound);

        // end spawner and destroy all spawned enemies
        waveSpawner.finishSpawner(true);
        CameraControl.instance.followPlayer(camResumeDuration);
        // raise spears
        foreach (GameObject spearGO in ambushSpearRefs) {
            spearGO.GetComponent<AmbushSpear>().riseImmediately();
        }
        
        state = State.ESCAPED;
        time = 0;
        toDisplayNotification = true;
        toRiseSpears = false;
    }

	void Awake() {
        waveSpawner = GetComponent<WaveSpawner>();
        timeUser = GetComponent<TimeUser>();
        propAsset = new Properties(textAsset.text);
        bgOverlay = GameObject.Instantiate(bgOverlayGameObject).GetComponent<AmbushBGOverlay>();
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
            } else {
                // detect if player has escaped
                if (!CameraControl.pointContainedInScreen(Player.instance.rb2d.position, -1f) &&
                    SceneManager.GetActiveScene().isDirty) {

                    ambushEscaped();
                }
            }
            break;
        case State.DEFEATED:
            // rise spears
            if (toRiseSpears && time >= spearRiseDelay) {
                // raise spears
                foreach (GameObject spearGO in ambushSpearRefs) {
                    spearGO.GetComponent<AmbushSpear>().rise();
                }
                toRiseSpears = false;
            }
            // display notification that ambush was defeated
            if (toDisplayNotification && time >= notificationDelay) {
                Notification.instance.displayNotification(propAsset.getString("defeated"), Notification.NotifType.DEFAULT);
                toDisplayNotification = false;
            }
            break;
        case State.ESCAPED:
            if (time >= escapedNotificationDelay && time - Time.deltaTime < escapedNotificationDelay) {
                Notification.instance.displayNotification(propAsset.getString("escaped"), Notification.NotifType.DEFAULT);
            }
            break;
        }
	}

    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["t"] = time;
        fi.bools["tdn"] = toDisplayNotification;
        fi.bools["trs"] = toRiseSpears;
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
        toRiseSpears = fi.bools["trs"];
    }

    void OnTriggerEnter2D(Collider2D c2d) {
        if (!useIncludedSensor) return;
        if (c2d.gameObject == null) return;
        if (c2d.gameObject != Player.instance.gameObject) return;

        activate();
    }

    void OnDestroy() {
        if (bgOverlay != null) {
            GameObject.Destroy(bgOverlay.gameObject);
            bgOverlay = null;
        }
    }

    WaveSpawner waveSpawner;
    TimeUser timeUser;
    Properties propAsset;

    State state = State.NOT_ACTIVE;
    float time = 0;
    float waveSpawnFinishedTime = 0;
    bool toRiseSpears = false;
    bool toDisplayNotification = false;
    AmbushBGOverlay bgOverlay;
}
