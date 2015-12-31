using UnityEngine;
using System.Collections;

public class Wally : MonoBehaviour {

    public Mode mode = Mode.TUTORIAL_SHIP_NPC;
    public float walkSpeed = 10;

    public enum Mode {
        TUTORIAL_SHIP_NPC
    }

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
        spriteObject = transform.Find("spriteObject").gameObject;
        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        animator = spriteObject.GetComponent<Animator>();
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();
    }

    void Start() {
        switch (mode) {
        case Mode.TUTORIAL_SHIP_NPC:
            animator.Play("idle");
            flippedHoriz = false;
            scriptStep = 0;
            duration = 1 + timeUser.randomValue() * 1;
            break;
        }
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        switch (mode) {
        case Mode.TUTORIAL_SHIP_NPC:

            Vector2 v = rb2d.velocity;

            switch (scriptStep) {
            case 0: // stay still
                v.Set(0, 0);
                if (time >= duration) {
                    animator.Play("tap");
                    scriptStep++;
                    time = 0;
                    duration = 5.0f + timeUser.randomValue() * 4.0f;
                }
                break;
            case 1: // tap foot
                v.Set(0, 0);

                // hacky hack that keeps Wally tapping his foot when the first talk is happening
                if (ScriptRunner.scriptsPreventPausing) {
                    time = 0;
                }

                if (time >= duration) {
                    animator.Play("walk");
                    flippedHoriz = true;
                    scriptStep++;
                    time = 0;
                    walkDur = 1.0f + timeUser.randomValue() * 1.0f;
                    duration = walkDur;
                }
                break;
            case 2: // walk left
                v.Set(-walkSpeed, 0);
                if (time >= duration) {
                    animator.Play("idle");
                    scriptStep++;
                    time = 0;
                    duration = 2.0f + timeUser.randomValue() * 1.0f;
                }
                break;
            case 3: // stay still
                v.Set(0, 0);
                if (time >= duration) {
                    animator.Play("walk");
                    flippedHoriz = false;
                    scriptStep++;
                    time = 0;
                    duration = walkDur;
                }
                break;
            case 4: // walk right
                v.Set(walkSpeed, 0);
                if (time >= duration) {
                    animator.Play("idle");
                    scriptStep = 0;
                    time = 0;
                    duration = 2.0f + timeUser.randomValue() * 1.0f;
                }
                break;
            }

            rb2d.velocity = v;

            break;
        }

	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["time"] = time;
        fi.floats["duration"] = duration;
        fi.ints["ss"] = scriptStep;
        fi.floats["walkDur"] = walkDur;
    }
    
    void OnRevert(FrameInfo fi) {
        time = fi.floats["time"];
        duration = fi.floats["duration"];
        scriptStep = fi.ints["ss"];
        walkDur = fi.floats["walkDur"];
    }
    
    float time = 0;
    float duration = 0;
    int scriptStep = 0;
    float walkDur = 0;

    // components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    Animator animator;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;

}
