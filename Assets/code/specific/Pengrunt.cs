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
    public float speed = 8;
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

	void Start() {
        // attach to Segment
        segment = Segment.findBottom(rb2d.position);
	}

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        if (state == State.WALK && Input.GetKeyDown(KeyCode.Alpha1)) {
            visionUser.createVision(VisionUser.VISION_DURATION);
        }

        time += Time.deltaTime;

        switch (state) {
        case State.IDLE:
            rb2d.velocity = new Vector2(0, rb2d.velocity.y);
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

            float s = speed;
            if (flippedHoriz)
                s *= -1;
            float x = segment.travel(rb2d.position.x, s, Time.fixedDeltaTime);
            bool turnaround = segment.travelTurnsAround(rb2d.position.x, s, Time.fixedDeltaTime);
            rb2d.MovePosition(new Vector2(x, rb2d.position.y));
            if (turnaround) {
                flippedHoriz = !flippedHoriz;
            }
            break;
        }

	}

    // called when this becomes a vision
    void TimeSkip(float timeInFuture) {
        Debug.Assert(state == State.WALK); //vision should only be made when Pengrunt is walking

        // Start() hasn't been called yet
        Start();

        // increment time
        time += timeInFuture;

        // change position after duration
        float s = speed;
        if (flippedHoriz)
            s *= -1;
        float x = segment.travel(rb2d.position.x, s, VisionUser.VISION_DURATION);
        bool turnaround = segment.travelTurnsAround(rb2d.position.x, s, VisionUser.VISION_DURATION);
        rb2d.position = new Vector2(x, rb2d.position.y);
        if (turnaround) {
            flippedHoriz = !flippedHoriz;
        }
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
    Segment segment = null;

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
