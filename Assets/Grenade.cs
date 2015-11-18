using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour {


    public State state = State.STABLE;
    public Vector2 grenadePinVelocity = new Vector2();
    public float grenadePinAngularVelocity = 0;
    public float warningDuration = .6f; //how long in warning state before exploding
    public float thrownAngularVelocity = 0;
    public GameObject explosionGameObject;
    public GameObject grenadePinGameObject;
    public AudioClip pinSound;
    public AudioClip explodeSound;

    public enum State {
        STABLE,
        WARNING,
        EXPLODED
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

        SoundManager.instance.playSFX(pinSound);

        pinPopped = true;
    }

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        spriteObject = transform.Find("spriteObject").gameObject;
        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        animator = spriteObject.GetComponent<Animator>();
        colFinder = GetComponent<ColFinder>();
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();
        visionUser = GetComponent<VisionUser>();
    }

    void Start() {
        // attach to Segment
        /* segment = Segment.findBottom(rb2d.position); */
    }

    /* Called when being spawned from a Portal */
    void OnSpawn(SpawnInfo si) {

    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

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
            GameObject.Instantiate(
                explosionGameObject,
                transform.localPosition,
                Quaternion.identity);
            SoundManager.instance.playSFX(explodeSound);
            // destroy this
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
            explode();
        }
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["time"] = time;
        fi.bools["eofe"] = explodeOnFrameEnd;
        fi.bools["pp"] = pinPopped;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];
        explodeOnFrameEnd = fi.bools["eofe"];
        pinPopped = fi.bools["pp"];
    }

    float time;
    bool explodeOnFrameEnd = false;
    bool pinPopped = false;

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
