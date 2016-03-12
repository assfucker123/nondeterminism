using UnityEngine;
using System.Collections;

public class Bubble : MonoBehaviour {

    public GameObject popGameObject;
    public AudioClip popSound;

    public bool popOnContact {
        get {
            return _popOnContact;
        }
        set {
            if (value == popOnContact) return;
            _popOnContact = value;
            if (popOnContact) {
                gameObject.layer = LayerMask.NameToLayer("EnemyAttacks");
            } else {
                gameObject.layer = LayerMask.NameToLayer("Enemies");
            }
        }
    }

    public void pop() {
        popOnFrameEnd = true;
    }

    public void setHidden(bool hidden) {
        this.hidden = hidden;
        if (timeUser.exists) {
            spriteRenderer.enabled = !hidden;
        }
    }
    bool hidden = false;

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        spriteObject = transform.Find("spriteObject").gameObject;
        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();
        visionUser = GetComponent<VisionUser>();
    }

    void Start() { }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        // popping
        if (popOnFrameEnd) {
            // create pop
            GameObject.Instantiate(
                popGameObject,
                transform.localPosition,
                Quaternion.identity);
            if (!visionUser.isVision)
                SoundManager.instance.playSFXIfOnScreen(popSound, rb2d.position);
            // destroy this
            timeUser.timeDestroy();
        }

        // destroy if out of bounds
        Rect rect = CameraControl.getMapBounds();
        if (!rect.Contains(rb2d.position)) {
            timeUser.timeDestroy();
        }

    }

    /* called when this becomes a vision */
    void TimeSkip(float timeInFuture) {

        // Start() hasn't been called yet
        //Start();

        // increment time
        time += timeInFuture;
    }

    /* called when this takes damage */
    void OnDamage(AttackInfo ai) {
        if (receivesDamage.health <= 0) {
            pop();
        }
    }

    void OnCollisionEnter2D(Collision2D c2d) {
        if (popOnContact) {
            pop();
        }
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.floats["time"] = time;
        fi.bools["pofe"] = popOnFrameEnd;
        fi.bools["poc"] = popOnContact;
        fi.bools["hidden"] = hidden;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        time = fi.floats["time"];
        popOnFrameEnd = fi.bools["pofe"];
        popOnContact = fi.bools["poc"];
        hidden = fi.bools["hidden"];
        setHidden(hidden);
    }

    float time;
    bool popOnFrameEnd = false;
    bool _popOnContact = false;

    // components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;
    VisionUser visionUser;

}
