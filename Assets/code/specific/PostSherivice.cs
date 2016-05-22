using UnityEngine;
using System.Collections;

/// <summary>
/// Handles fading out and moving to the real game
/// </summary>
public class PostSherivice : MonoBehaviour {

    public float fadeDuration = 1.0f;
    public float postFadeDelay = .1f;
    public float descentDuration = 2.0f;
    public float landDuration = 1.0f;

    public AudioClip landingSound;
    public AudioClip landSound;

    enum State {
        NOT_STARTED,
        FADE,
        POST_FADE_DELAY,
        DESCENT_SOUND,
        LAND_SOUND,
        DONE
    }
    
    void Awake() {
        timeUser = GetComponent<TimeUser>();
	}

    void PostSheriviceStart() {
        time = 0;
        state = State.FADE;
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        switch (state) {
        case State.FADE:
            HUD.instance.blackScreen.color = new Color(0, 0, 0, Utilities.easeLinearClamp(time, 0, 1, fadeDuration));
            if (time >= fadeDuration) {
                CutsceneBars.instance.moveOff();
                state = State.POST_FADE_DELAY;
                time -= fadeDuration;
            }
            break;
        case State.POST_FADE_DELAY:
            if (time >= postFadeDelay) {
                state = State.DESCENT_SOUND;
                SoundManager.instance.playSFX(landingSound);
                time -= postFadeDelay;
            }
            break;
        case State.DESCENT_SOUND:
            if (time >= descentDuration) {
                state = State.LAND_SOUND;
                time -= descentDuration;
                SoundManager.instance.playSFX(landSound);
            }
            break;
        case State.LAND_SOUND:
            if (time >= landDuration) {
                startBaseLanding();
                state = State.DONE;
            }
            break;
        }

	}
    
    void startBaseLanding() {
        VarsLoadData.loadBaseLandingData();
        Level.doNotStartBGMusic = true;
        Vars.restartFromLastSave();
    }
    
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["t"] = time;
        fi.floats["bsa"] = HUD.instance.blackScreen.color.a;
    }

    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["t"];
        if (state != State.NOT_STARTED) {
            HUD.instance.blackScreen.color = new Color(0, 0, 0, fi.floats["bsa"]);
        }
    }

    TimeUser timeUser;
    
    State state = State.NOT_STARTED;
    float time = 0;
}
