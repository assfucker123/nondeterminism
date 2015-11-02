using UnityEngine;
using System.Collections;

public class DeathLarvaOracle : MonoBehaviour {

    public Vector2 spawnPos = new Vector2(0, 0);
    public Vector2 explodeVel = new Vector2(0, 10);
    public float mercyFlashTime = 0;
    public float mercyFlashDuration = .5f;
    public float rotateSpeed = 300;
    public float rotateAccel = -1000;


    public State state = State.AIR;

    public enum State {
        AIR,
        GROUND
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
        colFinder = GetComponent<ColFinder>();
        timeUser = GetComponent<TimeUser>();
    }

    void Start() {
        animator.Play("pre_spin");
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        Vector2 v = rb2d.velocity;
        float rot = Utilities.get2DRot(spriteObject.transform.localRotation);

        switch (state) {
        case State.AIR:
            rotateSpeed = Mathf.Max(0, rotateSpeed + rotateAccel * Time.deltaTime);
            if (flippedHoriz)
                rot -= rotateSpeed * Time.deltaTime;
            else
                rot += rotateSpeed * Time.deltaTime;
            if (colFinder.hitBottom) {
                rot = 0;
                animator.Play("pre_waddle");
                state = State.GROUND;
            }
            break;
        case State.GROUND:
            break;
        }

        spriteObject.transform.localRotation = Utilities.setQuat(rot);
        rb2d.velocity = v;

        // color flashing
        if (mercyFlashTime < mercyFlashDuration) {
            mercyFlashTime += Time.deltaTime;

            if (mercyFlashTime >= mercyFlashDuration) {
                spriteRenderer.color = Color.white;
            } else {
                float mit = mercyFlashTime;
                float p = ReceivesDamage.MERCY_FLASH_PERIOD;
                float t = (mit - p * Mathf.Floor(mit / p)) / p; //t in [0, 1)
                if (t < .5) {
                    spriteRenderer.color = Color.Lerp(ReceivesDamage.MERCY_FLASH_COLOR, Color.white, t * 2);
                } else {
                    spriteRenderer.color = Color.Lerp(Color.white, ReceivesDamage.MERCY_FLASH_COLOR, (t - .5f) * 2);
                }
            }
        }

    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["time"] = time;
        fi.floats["rotateSpeed"] = rotateSpeed;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];
        rotateSpeed = fi.floats["rotateSpeed"];
    }

    float time;
    Segment segment;

    // components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    Animator animator;
    ColFinder colFinder;
    TimeUser timeUser;
	
}
