using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour {


    public State state = State.STABLE;
    public Vector2 grenadePinVelocity = new Vector2();
    public float grenadePinAngularVelocity = 0;
    public float warningDuration = .6f; //how long in warning state before exploding
    public float thrownAngularVelocity = 0;
    public bool sway = false; // can be set while running
    public float swayPeriod = 1.0f;
    public float swayAngle = 30;
    public GameObject explosionGameObject;
    public GameObject grenadePinGameObject;
    public AudioClip pinSound;
    public AudioClip explodeSound;
    
    public enum State {
        STABLE,
        WARNING,
        EXPLODED
    }

    public bool explodeOnContact {
        get {
            return _explodeOnContact;
        }
        set {
            if (value == explodeOnContact) return;
            _explodeOnContact = value;
            if (explodeOnContact) {
                gameObject.layer = LayerMask.NameToLayer("EnemyAttacks");
            } else {
                gameObject.layer = LayerMask.NameToLayer("Enemies");
            }
        }
    }
    

    public void throwGrenade(Vector2 velocity) {
        rb2d.velocity = velocity;
        if (velocity.x > 0) {
            rb2d.angularVelocity = -thrownAngularVelocity;
        } else {
            rb2d.angularVelocity = thrownAngularVelocity;
        }
    }

    public void explode() {
        if (state == State.EXPLODED) return;
        state = State.EXPLODED;
        explodeOnFrameEnd = true;
    }

    public void toWarning() {
        if (state == State.WARNING) return;
        popPin();
        time = 0;
        state = State.WARNING;
        animator.Play("warning");
    }

    public void popPin() {
        if (pinPopped) return;

        GrenadePin gpGOGP = grenadePinGameObject.GetComponent<GrenadePin>();
        Vector2 spawnPos = new Vector3(gpGOGP.spawnPos.x, gpGOGP.spawnPos.y);
        spawnPos = transform.TransformPoint(spawnPos);
        float spawnRot = gpGOGP.spawnRot;
        spawnRot += Utilities.get2DRot(transform.localRotation);

        GameObject gpGO = GameObject.Instantiate(
            grenadePinGameObject,
            spawnPos,
            Utilities.setQuat(spawnRot)) as GameObject;

        Rigidbody2D gpRB2D = gpGO.GetComponent<Rigidbody2D>();
        gpRB2D.velocity = Utilities.rotateAroundPoint(grenadePinVelocity, Vector2.zero, Utilities.get2DRot(transform.localRotation) * Mathf.PI / 180);
        gpRB2D.angularVelocity = grenadePinAngularVelocity;

        if (visionUser.isVision) {
            gpGO.GetComponent<VisionUser>().becomeVisionNow(visionUser.duration - visionUser.time, visionUser);
        } else {
            SoundManager.instance.playSFXIfOnScreen(pinSound, rb2d.position);
        }

        pinPopped = true;
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
        animator = spriteObject.GetComponent<Animator>();
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();
        visionUser = GetComponent<VisionUser>();
    }

    void Start() { }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        if (sway) {
            swayTime += Time.deltaTime;
            rb2d.rotation = Mathf.Sin(swayTime * Mathf.PI * 2 / swayPeriod) * swayAngle;
        }

        switch (state) {
        case State.STABLE:
            break;
        case State.WARNING:
            if (time >= warningDuration) {
                explode();
            }
            break;
        }
        // exploding
        if (explodeOnFrameEnd) {
            // create explosion
            GameObject eGO = GameObject.Instantiate(
                explosionGameObject,
                transform.localPosition,
                Quaternion.identity) as GameObject;
            if (visionUser.isVision) {
                eGO.GetComponent<VisionUser>().becomeVisionNow(visionUser.duration - visionUser.time, visionUser);
            } else {
                SoundManager.instance.playSFXIfOnScreen(explodeSound, rb2d.position);
            }
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
        swayTime += timeInFuture;
    }

    /* called when this takes damage */
    void OnDamage(AttackInfo ai) {
        if (receivesDamage.health <= 0) {
            explode();
        }
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["time"] = time;
        fi.bools["eofe"] = explodeOnFrameEnd;
        fi.bools["pp"] = pinPopped;
        fi.bools["eoc"] = explodeOnContact;
        fi.bools["sway"] = sway;
        fi.floats["swayT"] = swayTime;
        fi.bools["hidden"] = hidden;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];
        explodeOnFrameEnd = fi.bools["eofe"];
        pinPopped = fi.bools["pp"];
        explodeOnContact = fi.bools["eoc"];
        sway = fi.bools["sway"];
        swayTime = fi.floats["swayT"];
        hidden = fi.bools["hidden"];
        setHidden(hidden);
    }

    void OnCollisionEnter2D(Collision2D c2d) {
        if (explodeOnContact) {
            explode();
        }
    }

    float time;
    bool explodeOnFrameEnd = false;
    bool pinPopped = false;
    bool _explodeOnContact = false;
    float swayTime = 0;

    // components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    Animator animator;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;
    VisionUser visionUser;

}
