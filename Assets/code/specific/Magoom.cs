using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Magoom : MonoBehaviour {

    public float accel = 40;
    public float speed = 10;
    public float friction = 40;
    public bool moveOffLedges = true;
    public float walkDurationMin = 1.5f;
    public float walkDurationMax = 4.5f;
    public float teleportHalfDuration = .3f;
    public AudioClip magicSound;
    public AudioClip teleportSound;

    public State state = State.IDLE;
    public MagicAction magicAction = MagicAction.TELEPORT;

    public enum State {
        IDLE,
        WALK,
        MAGIC,
        TELEPORT_BEGIN, // fade out
        TELEPORT_END, // fade in
        DEAD //don't do anything; DefaultDeath takes care of this
    }

    public enum MagicAction {
        TELEPORT
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

        Vector2 v = rb2d.velocity;

        bool hitRight = colFinder.hitRight;
        bool hitLeft = colFinder.hitLeft;
        // hacky fix to collision bug
        if (state == State.WALK && timeSinceFlip > .4f &&
            Mathf.Abs(v.x) < .05f) {
            hitRight = true;
            hitLeft = true;
        }

        switch (state) {
        case State.IDLE:
            // go to walk when possible
            if (colFinder.hitBottom) {
                state = State.WALK;
                animator.Play("walk");
                time = 0;
                timeSinceFlip = 0;
                duration = walkDurationMin + timeUser.randomValue() * (walkDurationMax - walkDurationMin);
                state = State.WALK;
            }
            break;
        case State.WALK:
            timeSinceFlip += Time.deltaTime;
            if (flippedHoriz) { // moving left
                if (v.x > 0) { // apply friction
                    v.x = Mathf.Max(0, v.x - friction * Time.deltaTime);
                }
                v.x = Mathf.Max(-speed, v.x - accel * Time.deltaTime); // apply accel

                // turning around
                if (hitLeft ||
                    (!moveOffLedges && colFinder.onLeftEdge())) {
                    flippedHoriz = false;
                    v.x *= -1;
                    timeSinceFlip = 0;
                }

            } else { // moving right
                if (v.x < 0) { // apply friction
                    v.x = Mathf.Min(0, v.x + friction * Time.deltaTime);
                }
                v.x = Mathf.Min(speed, v.x + accel * Time.deltaTime); // apply accel

                // turning around
                if (hitRight ||
                    (!moveOffLedges && colFinder.onRightEdge())) {
                    flippedHoriz = true;
                    v.x *= -1;
                    timeSinceFlip = 0;
                }
            }

            if (time >= duration && colFinder.hitBottom) { // go to magic state

                switch (magicAction) {
                case MagicAction.TELEPORT:
                default:
                    // determine where teleport spot should be
                    List<Segment> segs = new List<Segment>();
                    segs.AddRange(Segment.bottomSegments);
                    if (segs.Count > 0) {
                        // remove segment currently on
                        Segment bottomSeg = Segment.findBottom(rb2d.position);
                        if (bottomSeg != null) {
                            segs.Remove(bottomSeg);
                        }
                        // remove segments that are offscreen
                        for (int i=0; i<segs.Count; i++) {
                            if (!segs[i].isOnScreen()) {
                                segs.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                    Segment.sortOnY(segs);
                    segs.RemoveRange(0, segs.Count / 2);
                    Segment seg = Segment.weightedRandom(segs, timeUser.randomValue());
                    teleportPos = seg.interpolate(timeUser.randomValue());
                    teleportPos.y += enemyInfo.spawnDist;

                    // determine how long magic duration will be
                    duration = VisionUser.VISION_DURATION - teleportHalfDuration;
                    break;
                }

                // spawn vision
                timeUser.addCurrentFrameInfo();
                visionUser.createVision(VisionUser.VISION_DURATION);

                animator.Play("sparkle_begin");
                time = 0;
                if (!visionUser.isVision) {
                    SoundManager.instance.playSFX(magicSound);
                }
                state = State.MAGIC;
            }

            break;

        case State.MAGIC:

            if (v.x > 0) { // apply friction
                v.x = Mathf.Max(0, v.x - friction * Time.deltaTime);
            } else {
                v.x = Mathf.Min(0, v.x + friction * Time.deltaTime);
            }

            if (time >= duration) {
                switch (magicAction) {
                case MagicAction.TELEPORT:
                default:
                    if (!visionUser.isVision) {
                        SoundManager.instance.playSFX(teleportSound);
                    }
                    time = 0;
                    state = State.TELEPORT_BEGIN;
                    break;
                }
            }
            break;

        case State.TELEPORT_BEGIN:

            if (time >= teleportHalfDuration) {
                // change location (do teleport)
                rb2d.position = teleportPos;
                time = 0;
                state = State.TELEPORT_END;
            } else {
                // fade out
                float startScale = 1;
                if (flippedHoriz)
                    startScale *= -1;
                spriteRenderer.transform.localScale = new Vector3(
                    Utilities.easeInOutQuad(time, startScale, -startScale, teleportHalfDuration),
                    spriteRenderer.transform.localScale.y,
                    spriteRenderer.transform.localScale.z);
            }
            break;

        case State.TELEPORT_END:

            float startScale2 = 1;
            if (flippedHoriz)
                startScale2 *= -1;
            spriteRenderer.transform.localScale = new Vector3(
                Utilities.easeInOutQuadClamp(time, 0, startScale2, teleportHalfDuration),
                spriteRenderer.transform.localScale.y,
                spriteRenderer.transform.localScale.z);

            if (time >= teleportHalfDuration) {
                // go to idle / walk
                time = 0;
                state = State.IDLE;
            }
            break;

        }

        if (colFinder.hitBottom) {
            //offset v.y to account for slope
            float gravity = 100;
            float a = colFinder.normalBottom - Mathf.PI / 2;
            v.y = v.x * Mathf.Atan(a) * SLOPE_RUN_MODIFIER;
            float g = gravity * Time.deltaTime;
            v.y -= g;

            //offset v.x to match gravity so object doesn't slide down when still
            v.x += g * SLOPE_STILL_MODIFIER * Mathf.Atan(a);

            if (state == State.WALK && isAnimatorCurrentState("falling")) {
                animator.Play("walk");
            }

        } else { //not touching ground
            //attempt to snap back to the ground
            if (!colFinder.raycastDownCorrection()) {
                if (state == State.WALK && !isAnimatorCurrentState("falling")) {
                    animator.Play("falling");
                }
            }
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
        //Start();

        // at this point, should be immediately after the magic was cast
        switch (magicAction) {
        case MagicAction.TELEPORT:
        default:
            time = teleportHalfDuration;
            state = State.TELEPORT_BEGIN; // trigger going to TELEPORT_END
            break;
        }

    }

    /* called when this takes damage */
    void OnDamage(AttackInfo ai) {
        if (receivesDamage.health <= 0) {
            spriteRenderer.transform.localScale = new Vector3(1, 1, 1);
            flippedHoriz = ai.impactToRight();
            animator.Play("damage");
        }
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["time"] = time;
        fi.floats["d"] = duration;
        fi.floats["tsf"] = timeSinceFlip;
        fi.floats["tpx"] = teleportPos.x;
        fi.floats["tpy"] = teleportPos.y;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];
        duration = fi.floats["d"];
        timeSinceFlip = fi.floats["tsf"];
        teleportPos.Set(fi.floats["tpx"], fi.floats["tpy"]);
    }

    // helper function
    bool isAnimatorCurrentState(string stateString) {
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash(stateString);
    }

    float time;
    float duration = 1.0f;
    Segment segment;
    float timeSinceFlip = 0;
    Vector2 teleportPos = new Vector2();

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

    //collision stuff
    private float SLOPE_RUN_MODIFIER = 1f;
    private float SLOPE_STILL_MODIFIER = 1.2f;

}
