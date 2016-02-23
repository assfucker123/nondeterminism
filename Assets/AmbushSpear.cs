using UnityEngine;
using System.Collections;

public class AmbushSpear : MonoBehaviour {

    public float raisedDist = 6;
    public float fallDuration = .8f;
    public float bounceDist = .5f;
    public float bounceDuration = .3f;
    public AudioClip dropSound1;
    public AudioClip dropSound2;

    public void fall() {
        if (state == State.FALLING) return;
        rb2d.position = raisedPos;
        state = State.FALLING;
        if (!SoundManager.instance.isSFXPlaying(dropSound1)) {
            SoundManager.instance.playSFX(dropSound1);
        }
    }
    public void rise() {
        if (state == State.RISING) return;
        rb2d.position = fellPos;
        state = State.RISING;
    }

    public enum State {
        RAISED,
        FALLING,
        FELL,
        RISING
    }

    public State state {
        get { return _state; }
        set {
            if (state == value) return;
            switch (value) {
            case State.RAISED:
                rb2d.position = raisedPos;
                spriteRenderer.enabled = false;
                c2d.isTrigger = true;
                break;
            case State.FALLING:
                spriteRenderer.enabled = true;
                c2d.isTrigger = false;
                break;
            case State.FELL:
                rb2d.position = fellPos;
                spriteRenderer.enabled = true;
                c2d.isTrigger = false;
                break;
            case State.RISING:
                spriteRenderer.enabled = true;
                c2d.isTrigger = false;
                break;
            }
            time = 0;
            _state = value;
        }
    }

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb2d = GetComponent<Rigidbody2D>();
        c2d = GetComponent<Collider2D>();
	}

    void Start() {
        fellPos = rb2d.position;
        raisedPos = fellPos + new Vector2(Mathf.Cos((rb2d.rotation + 90) * Mathf.PI / 180), Mathf.Sin((rb2d.rotation + 90) * Mathf.PI / 180)) * raisedDist;
        state = State.FALLING;
        state = State.RAISED;
    }
	
	void Update() {
        if (timeUser.shouldNotUpdate)
            return;
        
        time += Time.deltaTime;
        switch (state) {
        case State.FALLING:
            if (time < fallDuration) {
                float t = 0;
                if (time < fallDuration - bounceDuration) {
                    t = Utilities.easeInQuadClamp(time, 0, 1, fallDuration - bounceDuration);
                } else if (time < fallDuration - bounceDuration / 2) {
                    t = Utilities.easeOutQuadClamp(time - fallDuration + bounceDuration, 1, -bounceDist / raisedDist, bounceDuration / 2);
                    if (time - Time.deltaTime <= fallDuration + bounceDuration) {
                        if (!SoundManager.instance.isSFXPlaying(dropSound2)) {
                            SoundManager.instance.playSFX(dropSound2);
                        }
                    }
                } else {
                    t = Utilities.easeInQuadClamp(time - fallDuration + bounceDuration / 2, 1 - bounceDist / raisedDist, bounceDist / raisedDist, bounceDuration / 2);
                }
                rb2d.MovePosition(raisedPos + (fellPos - raisedPos) * t);
            } else {
                state = State.FELL;
            }
            break;
        case State.RISING:
            if (time < fallDuration) {
                float t = 0;
                t = Utilities.easeLinearClamp(time, 1, -1, fallDuration);
                rb2d.MovePosition(raisedPos + (fellPos - raisedPos) * t);
            } else {
                state = State.RAISED;
            }
            break;
        }

	}

    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["t"] = time;
    }

    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["t"];
    }

    Vector2 raisedPos = new Vector2();
    Vector2 fellPos = new Vector2();

    TimeUser timeUser;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb2d;
    Collider2D c2d;

    State _state;
    float time = 0;
}
