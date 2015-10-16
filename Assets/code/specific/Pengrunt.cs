using UnityEngine;
using System.Collections;

public class Pengrunt : MonoBehaviour {

    // IDEA: Move by bouncing from spot to spot in defined arcs

    public enum State {
        IDLE,
        WALK, //walk for set duration (short or long).  Turn around at walls and edges
        PRE_SPRAY, //should be a short duration.  Possibly turn around?
        SPRAY //pushed backwards during this
    }

    public float idleDuration = .5f;
    public float walkShortDuration = .3f;
    public float walkLongDuration = .8f;
    public float preSprayDuration = .2f;
    public float sprayDuration = .7f;
    public float accel = 60;
    public float maxSpeed = 8;
    public float friction = 40;
    public float sprayAccel = 60;
    public float spraySpeed = 4;
    public State state = State.IDLE;

    public bool flippedHoriz {
        get { return spriteRenderer.transform.localScale.x < 0; }
        set {
            if (value == flippedHoriz)
                return;
            spriteRenderer.transform.localScale = new Vector3(
                -spriteRenderer.transform.localScale.x,
                spriteRenderer.transform.localScale.y,
                spriteRenderer.transform.localScale.z);
        }
    }

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        spriteObject = this.transform.Find("spriteObject").gameObject;
        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        animator = spriteObject.GetComponent<Animator>();
        colFinder = GetComponent<ColFinder>();
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();
        visionUser = GetComponent<VisionUser>();
    }

	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {

        if (timeUser.shouldNotUpdate)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            GameObject vision = visionUser.createVision();
            if (vision != null) {
                Rigidbody2D vRb2d = vision.GetComponent<Rigidbody2D>();
                vRb2d.position = rb2d.position + new Vector2(5, 0);
            }
        }

        Vector2 v = rb2d.velocity;
        time += Time.deltaTime;

        switch (state) {
        case State.IDLE:
            // apply friction
            if (v.x < 0)
                v.x = Mathf.Min(0, v.x + friction * Time.deltaTime);
            else
                v.x = Mathf.Max(0, v.x - friction * Time.deltaTime);
            // next state
            if (time >= idleDuration && colFinder.hitBottom) {
                state = State.WALK;
                time -= idleDuration;
                if (timeUser.randomValue() < .5f) {
                    duration = walkShortDuration;
                } else {
                    duration = walkLongDuration;
                }
            }
            break;
        case State.WALK:
            if (flippedHoriz) {
                // going left
                if (v.x > 0) { // if currently moving right
                    v.x = Mathf.Max(0, v.x - friction * Time.deltaTime);
                }
                v.x = Mathf.Max(-maxSpeed, v.x - accel * Time.deltaTime);
                // check if needs to go right
                if (colFinder.onLeftEdge() || colFinder.hitLeft)
                    flippedHoriz = false;
            } else {
                // going right
                if (v.x < 0) { // if currently moving left
                    v.x = Mathf.Min(0, v.x + friction * Time.deltaTime);
                }
                v.x = Mathf.Min(maxSpeed, v.x + accel * Time.deltaTime);
                // check if needs to go left
                if (colFinder.onRightEdge() || colFinder.hitRight)
                    flippedHoriz = true;
            }
            break;
        }

        rb2d.velocity = v;

	}

    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int) state;
        fi.floats["time"] = time;
        fi.floats["duration"] = duration;
    }
    void OnRevert(FrameInfo fi) {
        state = (State) fi.state;
        time = fi.floats["time"];
        duration = fi.floats["duration"];
    }

    float time = 0;
    float duration = 0;

    // components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    Animator animator;
    ColFinder colFinder;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;
    VisionUser visionUser;

}
