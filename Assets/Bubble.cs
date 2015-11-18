using UnityEngine;
using System.Collections;

public class Bubble : MonoBehaviour {

    public GameObject popGameObject;
    public AudioClip popSound;

    public void pop() {
        popOnFrameEnd = true;
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

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        Vector2 v = rb2d.velocity;



        rb2d.velocity = v;

        // travel across a segment:
        /*
        x = segment.travelClamp(rb2d.position.x, speed, Time.fixedDeltaTime);
        rb2d.MovePosition(new Vector2(x, rb2d.position.y));
        */

        // create a vision:
        /*
        GameObject vGO = visionUser.createVision(VisionUser.VISION_DURATION);
        */

        // spawn a bullet:
        /*
        GameObject bulletGO = GameObject.Instantiate(bulletGameObject,
            rb2d.position + relSpawnPosition,
            Utilities.setQuat(heading)) as GameObject;
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        bullet.heading = heading;
        if (visionUser.isVision) { //make bullet a vision if this is also a vision
            VisionUser bvu = bullet.GetComponent<VisionUser>();
            bvu.becomeVisionNow(visionUser.duration - visionUser.time, visionUser);
        }
        */

        // popping
        if (popOnFrameEnd) {
            // create pop
            GameObject.Instantiate(
                popGameObject,
                transform.localPosition,
                Quaternion.identity);
            SoundManager.instance.playSFX(popSound);
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
            pop();
        }
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.floats["time"] = time;
        fi.bools["pofe"] = popOnFrameEnd;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        time = fi.floats["time"];
        popOnFrameEnd = fi.bools["pofe"];
    }

    float time;
    bool popOnFrameEnd = false;

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
