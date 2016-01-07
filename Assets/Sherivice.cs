using UnityEngine;
using System.Collections;

public class Sherivice : MonoBehaviour {


    public State state = State.IDLE;

    public float flyInDuration = .8f;
    public Vector2 flyInStartPosition = new Vector2(47, 16);
    public Vector2 flyInPosition = new Vector2(33, 14);

    public AudioClip wingFlapSound;
    public AudioClip screamSound;

    public enum State {
        IDLE,
        FLY_IN,
        CUTSCENE_IDLE,
        DEAD //don't do anything; DefaultDeath takes care of this
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
        receivesDamage = GetComponent<ReceivesDamage>();
        visionUser = GetComponent<VisionUser>();
        defaultDeath = GetComponent<DefaultDeath>();
        enemyInfo = GetComponent<EnemyInfo>();
    }

    void Start() {
        // attach to Segment
        /* segment = Segment.findBottom(rb2d.position); */
    }

    /* Called when being spawned from a Portal */
    void OnSpawn(SpawnInfo si) {
        flippedHoriz = !si.faceRight;
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;
        if (defaultDeath.activated)
            return;

        time += Time.deltaTime;

        Vector2 pos = rb2d.position;

        switch (state) {
        case State.IDLE:
            break;
        case State.FLY_IN:
            pos.x = Utilities.easeInOutQuadClamp(time, pos0.x, pos1.x - pos0.x, flyInDuration);
            pos.y = Utilities.easeInOutQuadClamp(time, pos0.y, pos1.y - pos0.y, flyInDuration);

            wingFlapPlayTime += Time.deltaTime;
            if (wingFlapPlayTime > .45f && time < flyInDuration / 2) {
                SoundManager.instance.playSFXRandPitchBend(wingFlapSound);
                wingFlapPlayTime = 0;
            }

            if (time >= flyInDuration) {
                state = State.CUTSCENE_IDLE;
                time = 0;
            }
            break;
        case State.CUTSCENE_IDLE:
            // nothing happens
            break;
        }

        rb2d.MovePosition(pos);

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

        

    }

    /* called from script */
    void ScriptFlyIn() {
        state = State.FLY_IN;
        time = 0;
        flippedHoriz = true;
        pos0 = flyInStartPosition;
        pos1 = flyInPosition;
        rb2d.position = pos0;
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
            flippedHoriz = ai.impactToRight();
            //animator.Play("damage");
        }
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["time"] = time;
        fi.floats["wfpt"] = wingFlapPlayTime;
        fi.floats["p0x"] = pos0.x;
        fi.floats["p0y"] = pos0.y;
        fi.floats["p1x"] = pos1.x;
        fi.floats["p1y"] = pos1.y;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];
        wingFlapPlayTime = fi.floats["wfpt"];
        pos0.Set(fi.floats["p0x"], fi.floats["p0y"]);
        pos1.Set(fi.floats["p1x"], fi.floats["p1y"]);
    }

    float time;
    Segment segment;
    float wingFlapPlayTime = 0;
    Vector2 pos0 = new Vector2();
    Vector2 pos1 = new Vector2();

    // components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    Animator animator;
    ColFinder colFinder;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;
    VisionUser visionUser;
    DefaultDeath defaultDeath;
    EnemyInfo enemyInfo;

}
