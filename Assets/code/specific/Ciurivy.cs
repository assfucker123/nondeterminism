using UnityEngine;
using System.Collections;

public class Ciurivy : MonoBehaviour {

    public bool startFacingLeft = false;
    public bool neverChangeDirection = false;
    public State state = State.START;
    public float idleDurationMin = .7f;
    public float idleDurationMax = 1.5f;
    public float runSpeed = 10f;
    public float getBombDuration = .3f;
    public float throwBombDuration = .3f;
    public float postThrowBombDuration = .3f;
    public Vector2 throwPosition = new Vector2(.56f, .05f);
    public float throwInitialYVelocity = 18;
    public float throwMinXDistance = 3;
    public float throwMaxXDistance = 8;
    public float playerLeadMaxOffset = 1; // adjusting throw distance based on player speed
    public float sightRange = 30;
    public float sightSpread = 80;
    public AudioClip throwSound;
    public AudioClip stepSound;
    public float stepSoundOffset = .091f;
    public float stepSoundPeriod = .455f;

    public GameObject acornBombGameObject;

    public enum State {
        START,
        IDLE,
        RUN,
        GET_BOMB,
        THROW_BOMB,
        POST_THROW_BOMB,

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
    }

    void Start() {
        // attach to Segment
        segment = Segment.findBottom(rb2d.position);
        
        if (!visionUser.isVision || state == State.START) {
            idleState();
        }

        if (!visionUser.isVision && startFacingLeft) {
            flippedHoriz = true;
        }
    }

    /* Called when being spawned from a Portal */
    void OnSpawn(SpawnInfo si) {
        flippedHoriz = !si.faceRight;
    }

    void idleState() {
        state = State.IDLE;
        animator.Play("idle");
        time = 0;
        duration = timeUser.randomValue() * (idleDurationMax - idleDurationMin) + idleDurationMin;

        runAfterIdle = Mathf.Abs(getNextRunPosition().x - rb2d.position.x) > .5f;
        if (!playerAwareness.awareOfPlayer || enemyInfo.id == EnemyInfo.ID.CIURIVY_PASSIVE)
            runAfterIdle = true;
    }

    void runState() {
        state = State.RUN;
        pos0 = rb2d.position;

        // decide which direction to run

        // run to edge of segment towards player
        pos1 = getNextRunPosition();
        flippedHoriz = pos1.x < pos0.x;
        time = 0;
        duration = Mathf.Abs(pos1.x - pos0.x) / runSpeed;
        animator.Play("run");

        // getting ready to throw bomb
        throwBombAfterRun = duration + getBombDuration > VisionUser.VISION_DURATION + .1f;
        if (!playerAwareness.awareOfPlayer || enemyInfo.id == EnemyInfo.ID.CIURIVY_PASSIVE)
            throwBombAfterRun = false;
        
    }

    void getBombState(bool setNextThrowDistance) {
        state = State.GET_BOMB;
        time = 0;
        duration = getBombDuration;
        animator.Play("get_bomb");
        
        if (setNextThrowDistance) {
            throwXDistance = getNextThrowXDistance(rb2d.position.x, flippedHoriz);
        }
    }

    Vector2 getNextRunPosition() {
        Vector2 ret = new Vector2();
        bool toRight = false;
        if (neverChangeDirection) {
            toRight = !flippedHoriz;
        } else if (playerAwareness.awareOfPlayer && enemyInfo.id != EnemyInfo.ID.CIURIVY_PASSIVE) {
            toRight = Player.instance.rb2d.position.x > (segment.p1.x + segment.p0.x) / 2;
        } else {
            toRight = rb2d.position.x < (segment.p1.x + segment.p0.x) / 2;
        }
        if (toRight) {
            ret.x = segment.p1.x;
        } else {
            ret.x = segment.p0.x;
        }
        ret.y = rb2d.position.y;

        return ret;
    }

    /* Distance where the acorn aims at the player */
    float getNextThrowXDistance(float currentX, bool currentFlippedHoriz) {
        float ret = 0;
        float posX = currentX;
        float plrX = Player.instance.rb2d.position.x;
        plrX += playerLeadMaxOffset * Player.instance.rb2d.velocity.x / Player.instance.groundMaxSpeed;
        if (currentFlippedHoriz) {
            ret = Mathf.Clamp(plrX - posX, -throwMaxXDistance, -throwMinXDistance);
        } else {
            ret = Mathf.Clamp(plrX - posX, throwMinXDistance, throwMaxXDistance);
        }
        return ret;
    }

    void FixedUpdate() {

        if (timeUser.shouldNotUpdate)
            return;
        if (defaultDeath.activated)
            return;

        time += Time.deltaTime;
        
        Vector2 v = rb2d.velocity;

        switch (state) {
        case State.IDLE:
            v.Set(0, v.y); // stop

            if (!runAfterIdle) {
                // create vision of throwing bomb at the right time
                if (visionUser.shouldCreateVisionThisFrame(time - Time.deltaTime, time, duration + getBombDuration, VisionUser.VISION_DURATION)) {
                    // prepare where the bomb will be thrown
                    throwXDistance = getNextThrowXDistance(pos1.x, flippedHoriz);
                    timeUser.addCurrentFrameInfo();
                    // make vision
                    visionUser.createVision(VisionUser.VISION_DURATION);
                }
            }

            if (time >= duration) {
                time -= duration;
                if (runAfterIdle) {
                    if (Mathf.Abs(getNextRunPosition().x - rb2d.position.x) < .1f) {
                        // already at where it would run to
                        idleState();
                    } else {
                        runState();
                    }
                } else {
                    // not running after idle, throw bomb
                    getBombState(false); // already set throw distance when creating vision
                }

            }
            break;
        case State.RUN:

            Vector2 pos = pos0;
            if (duration < .0001f) {
                pos = pos1;
            } else {
                pos.x = Utilities.easeLinearClamp(time, pos0.x, pos1.x - pos0.x, duration);
            }
            pos.y = rb2d.position.y; //Utilities.easeLinearClamp(time, pos0.y, pos1.y - pos0.y, duration);
            rb2d.MovePosition(pos);

            if (throwBombAfterRun) {
                // create vision of throwing bomb at the right time
                if (visionUser.shouldCreateVisionThisFrame(time - Time.deltaTime, time, duration + getBombDuration, VisionUser.VISION_DURATION)) {
                    // prepare where the bomb will be thrown
                    throwXDistance = getNextThrowXDistance(pos1.x, flippedHoriz);
                    timeUser.addCurrentFrameInfo();
                    // make vision
                    visionUser.createVision(VisionUser.VISION_DURATION);
                }
            }

            // step sound
            if (!visionUser.isVision && visionUser.shouldHaveEventThisFrame(time - Time.deltaTime + stepSoundOffset, time + stepSoundOffset, stepSoundPeriod)) {
                SoundManager.instance.playSFXIfOnScreenRandPitchBend(stepSound, rb2d.position);
            }
            
            if (time >= duration) {
                time -= duration;

                if (throwBombAfterRun) {
                    getBombState(false); // don't set throw position, was already set when making vision
                } else {
                    idleState();
                }

            }
            break;
        case State.GET_BOMB:
            if (time >= duration) {
                state = State.THROW_BOMB;
                animator.Play("throw_bomb");
                time -= duration;
                duration = throwBombDuration;
            }
            break;
        case State.THROW_BOMB:
            if (time >= duration) {

                // throw bomb
                Vector2 bombPos = rb2d.position;
                if (flippedHoriz) {
                    bombPos += new Vector2(-throwPosition.x, throwPosition.y);
                } else {
                    bombPos += throwPosition;
                }
                GameObject bombGO = GameObject.Instantiate(acornBombGameObject, new Vector3(bombPos.x, bombPos.y), Quaternion.identity) as GameObject;
                AcornBomb ab = bombGO.GetComponent<AcornBomb>();
                ab.spinCW = !flippedHoriz;
                ab.initialYVelocity = throwInitialYVelocity;
                ab.finalX = rb2d.position.x + throwXDistance;
                if (visionUser.isVision) {
                    ab.GetComponent<VisionUser>().becomeVisionNow(visionUser.timeLeft, visionUser);
                }
                if (!visionUser.isVision) {
                    SoundManager.instance.playSFXIfOnScreenRandPitchBend(throwSound, rb2d.position);
                }
                
                // go to post throw state
                state = State.POST_THROW_BOMB;
                time -= duration;
                duration = postThrowBombDuration;
            }
            break;
        case State.POST_THROW_BOMB:
            if (time >= duration) {
                idleState();
                animator.Play("thrown_to_idle");
            }
            break;

        }

        rb2d.velocity = v;

    }

    /* called when this becomes a vision */
    void TimeSkip(float timeInFuture) {
        
        // Start() hasn't been called yet

        // at position moving towards
        rb2d.position = new Vector2(pos1.x, rb2d.position.y);

        // will be throwing a bomb in this vision
        state = State.THROW_BOMB;
        animator.Play("throw_bomb");
        time = 0;
        duration = throwBombDuration;

    }

    /* called when this takes damage */
    void OnDamage(AttackInfo ai) {
        // become always aware of player
        playerAwareness.alwaysAware = true;
        // death
        if (receivesDamage.health <= 0) {
            flippedHoriz = ai.impactToRight();
            animator.Play("damage");
        }
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["time"] = time;
        fi.floats["d"] = duration;
        fi.floats["p0x"] = pos0.x;
        fi.floats["p0y"] = pos0.y;
        fi.floats["p1x"] = pos1.x;
        fi.floats["p1y"] = pos1.y;
        fi.floats["txd"] = throwXDistance;
        fi.bools["tbar"] = throwBombAfterRun;
        fi.bools["rai"] = runAfterIdle;
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];
        duration = fi.floats["d"];
        pos0.x = fi.floats["p0x"];
        pos0.y = fi.floats["p0y"];
        pos1.x = fi.floats["p1x"];
        pos1.y = fi.floats["p1y"];
        throwXDistance = fi.floats["txd"];
        throwBombAfterRun = fi.bools["tbar"];
        runAfterIdle = fi.bools["rai"];
    }

    float time;
    float duration;
    Vector2 pos0 = new Vector2();
    Vector2 pos1 = new Vector2();
    float throwXDistance = 0;
    bool throwBombAfterRun = false;
    bool runAfterIdle = false;

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

}
