using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Vengemole : MonoBehaviour {

    /*
Vengemole plan:

- idle, waits

- Goes underground
	- At this instant, create vision of Vengemole popping out of the ground somewhere
- waits underground
- Pops out of the ground
	- back to idle OR
	- swings hammer OR
	- throws hammer
- back to waits
    */

    public State state = State.IDLE;
    public float idleMinDuration = .5f;
    public float idleMaxDuration = 4;
    public float undergroundMaxDuration = 3f; // min duration is VISION_DURATION
    public float undergroundGetHammerMinDuration = 2f;
    public float undergroundGetHammerMaxDuration = 4f;
    public float throwHammerChance = .7f;
    public float riseSwingDelay = .2f;
    public float throwDelay = .08f;
    public float postSwingDuration = .6f;

    public GameObject hammerGameObject;

    public enum State {
        IDLE,
        UNDERGROUND,
        RISE,
        RISE_SWING,
        RISE_THROW,
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
        playerAwareness = GetComponent<PlayerAwareness>();

        holeObject = transform.Find("hole").gameObject;
        holeAnimator = holeObject.GetComponent<Animator>();
    }

    void Start() {

        if (visionUser.isVision)
            return;

        // attach to Segment
        segment = Segment.findBottom(rb2d.position);

        idleState();
        animator.Play("idle");
    }

    void idleState() {
        state = State.IDLE;
        time = 0;
        duration = timeUser.randomValue() * (idleMaxDuration - idleMinDuration) + idleMinDuration;
    }

    void undergroundState() {
        // decide where to rise next
        bool attackingPlayer = playerAwareness.awareOfPlayer;
        Vector2 plrPos = Player.instance.rb2d.position;
        Segment nextSegment;
        if (attackingPlayer) {
            nextSegment = Segment.segmentClosestToPoint(Segment.bottomSegments, plrPos);
            posAfterUnderground = nextSegment.interpolate(timeUser.randomValue());
            flippedHorizAfterUnderground = (posAfterUnderground.x < plrPos.x);
        } else {
            List<Segment> segs = Segment.bottomSegments;
            if (segs.Count > 1 && segment != null) {
                segs.Remove(segment);
            }
            nextSegment = Segment.weightedRandom(segs, timeUser.randomValue());
            posAfterUnderground = nextSegment.interpolate(timeUser.randomValue());
            flippedHorizAfterUnderground = (timeUser.randomValue() < .5f);
        }
        posAfterUnderground.y += enemyInfo.spawnDist; // adjusting for spawn distance

        // action to take after rising
        if (attackingPlayer) {
            if (enemyInfo.id == EnemyInfo.ID.VENGEMOLE_CAN_THROW) {
                if (timeUser.randomValue() < throwHammerChance) {
                    stateAfterUnderground = State.RISE_THROW;
                } else {
                    stateAfterUnderground = State.RISE_SWING;
                }
            } else {
                stateAfterUnderground = State.RISE_SWING;
            }
        } else {
            stateAfterUnderground = State.RISE;
        }

        // state transition
        time -= duration;
        if (hasHammer) {
            animator.Play("fall");
        } else {
            animator.Play("fall_nohammer");
        }
        state = State.UNDERGROUND;

        // how long underground?
        if (hasHammer) {
            duration = timeUser.randomValue() * (undergroundMaxDuration - VisionUser.VISION_DURATION + .1f) + VisionUser.VISION_DURATION + .1f;
        } else {
            duration = timeUser.randomValue() * (undergroundGetHammerMaxDuration - undergroundGetHammerMinDuration) + undergroundGetHammerMinDuration;
        }

    }

    void riseState() {
        state = stateAfterUnderground;
        time = 0;
        rb2d.position = posAfterUnderground;
        flippedHoriz = flippedHorizAfterUnderground;
        hasHammer = true;
        state = stateAfterUnderground;
        switch (state) {
        case State.RISE:
            idleState();
            animator.Play("rise");
            break;
        case State.RISE_SWING:
        case State.RISE_THROW:
            animator.Play("rise_to_swing");
            duration = postSwingDuration;
            break;
        }
    }

    void hammerThrow() {
        Debug.Log("Throw hammer");
        hasHammer = false;
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

        Vector2 v = rb2d.velocity;

        switch (state) {
        case State.IDLE:
            if (time >= duration) {
                // go underground
                undergroundState();
                
            }
            break;
        case State.UNDERGROUND:

            // creating vision of rising
            if (visionUser.shouldCreateVisionThisFrame(time-Time.deltaTime, time, duration, VisionUser.VISION_DURATION)) {
                visionUser.createVision(VisionUser.VISION_DURATION);

            }

            if (time >= duration) {
                // rise from the ground
                riseState(); // uses properties already set when undergroundState() was called
            }

            break;
        case State.RISE:
        case State.RISE_SWING:
        case State.RISE_THROW:
            // swinging hammer
            if ((state == State.RISE_SWING || state == State.RISE_THROW) &&
                time-Time.deltaTime < riseSwingDelay && time >= riseSwingDelay) {
                if (state == State.RISE_SWING) {
                    animator.Play("swing");
                } else {
                    animator.Play("throw");
                }
            }
            // throwing hammer
            if (state == State.RISE_THROW &&
                time-Time.deltaTime < riseSwingDelay+throwDelay && time >= riseSwingDelay + throwDelay) {
                hammerThrow();
            }
            // going back underground
            if (time >= postSwingDuration) {
                undergroundState();
            }
            break;


        }

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

    }

    /* called when this becomes a vision */
    void TimeSkip(float timeInFuture) {

        // Start() hasn't been called yet

        time += timeInFuture;

        // rise from the ground
        riseState(); // uses properties already set in idle

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
        fi.floats["d"] = duration;
        fi.floats["paux"] = posAfterUnderground.x;
        fi.floats["pauy"] = posAfterUnderground.y;
        fi.ints["sau"] = (int)stateAfterUnderground;
        fi.bools["fhau"] = flippedHorizAfterUnderground;
        fi.bools["hh"] = hasHammer;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];
        duration = fi.floats["d"];
        posAfterUnderground.Set(fi.floats["paux"], fi.floats["pauy"]);
        stateAfterUnderground = (State)fi.ints["sau"];
        flippedHorizAfterUnderground = fi.bools["fhau"];
        hasHammer = fi.bools["hh"];
    }

    float time = 0;
    float duration = 0;
    Vector2 posAfterUnderground = new Vector2();
    State stateAfterUnderground = State.RISE;
    bool flippedHorizAfterUnderground = false;
    bool hasHammer = true;
    Segment segment;

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
    PlayerAwareness playerAwareness;

    GameObject holeObject;
    Animator holeAnimator;

}
