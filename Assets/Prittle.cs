using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Prittle : MonoBehaviour {


    public State state = State.IDLE;
    public int vineShootDamage = 2;
    public int vineDamage = 1;
    public float idleDurationMin = .4f;
    public float idleDurationMax = .7f;
    public float shootSpeed = 20;
    public float shootDuration = .4f;
    public float retractDurationMax = .4f; // retract speed = vineMaxLength / retractDurationMax
    public float vineSpacingMin = 20;
    public float vineSpacingMax = 30;
    public float maxSpread = 180;
    public float hitShakeMagnitude = .3f;
    public float hitShakeDuration = .4f;
    public AudioClip growSound;
    public AudioClip vineHitSound;
    public AudioClip retractSound;
    
    public GameObject prittleVineGameObject;

    public float vineMaxLength {
        get { return prittleVineGameObject.GetComponent<PrittleVine>().maxLength; }
    }

    public enum State {
        IDLE, // also retracts during idle state
        SHOOTING,
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

        vines.Clear();
        PrittleVine[] childVines = spriteObject.transform.GetComponentsInChildren<PrittleVine>();
        foreach (PrittleVine pv in childVines) {
            vines.Add(pv);
        }

        stateQueue = new VisionUser.StateQueue(0);

        

    }

    void Start() {
        // attach to Segment
        /* segment = Segment.findBottom(rb2d.position); */

        if (!visionUser.isVision) {
            foreach (PrittleVine vine in vines) {
                vine.length = 0;
            }
        }
    }

    /* Called when being spawned from a Portal */
    void OnSpawn(SpawnInfo si) {
        flippedHoriz = !si.faceRight;
    }

    /// <summary>
    /// Going to idle state
    /// </summary>
    void idle(float duration) {
        state = State.IDLE;
        time = 0;
        
        bool playSound = false;
        for (int i = 0; i < vines.Count; i++) {
            vines[i].complete = true;
            if (vines[i].length > .5f) {
                playSound = true;
            }
        }
        if (playSound) {
            if (!visionUser.isVision) {
                SoundManager.instance.playSFXIfOnScreen(retractSound, transform.position);
            }
        }
    }

    /// <summary>
    /// Going to shooting state
    /// </summary>
    void shooting(int stateQueueIndex) {
        state = State.SHOOTING;
        time = 0;

        for (int i=0; i<vines.Count; i++) {
            vines[i].length = 0;
            vines[i].complete = false;
            vines[i].localRotation = stateQueue.getFloatVal(i, stateQueueIndex);
        }

        if (!visionUser.isVision) {
            SoundManager.instance.playSFXIfOnScreen(growSound, transform.position);
        }

    }

    void addIdleStateToQueue() {
        float duration = timeUser.randomRange(idleDurationMin, idleDurationMax);
        stateQueue.addState((int)State.IDLE, duration, 0, 0, false);
    }

    void addShootingStateToQueue() {
        // get rotation of vines
        int numVines = vines.Count;
        float spacing = timeUser.randomRange(vineSpacingMin, vineSpacingMax);
        float spreadRange = (maxSpread - spacing*(numVines-1));
        float center = timeUser.randomRange(-spreadRange/2, spreadRange/2) + 90;
        float[] angles = new float[numVines];
        for (int i=0; i<numVines; i++) {
            angles[i] = Utilities.centeredSpacing(spacing, i, numVines) + center;
        }

        stateQueue.addState((int)State.SHOOTING, shootDuration, 0, 0, true, angles);
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;
        //if (defaultDeath.activated) return;

        time += Time.deltaTime;

        Vector2 v = rb2d.velocity;
        
        // adding states to state queue, alternate between idle and shooting
        float planDuration = VisionUser.VISION_DURATION * 2;
        if (stateQueue.planAheadDuration < planDuration) {
            if (stateQueue.empty || (State)stateQueue.getLastState() == State.SHOOTING) {
                addIdleStateToQueue();
            } else {
                addShootingStateToQueue();
            }
        }

        // handle states changing this frame
        int statesPopped = stateQueue.numStatesPoppedByIncrementingTime(Time.deltaTime);
        if (statesPopped >= 1 && receivesDamage.health > 0) {
            State nextState = (State)stateQueue.getState(statesPopped);
            switch (nextState) {
            case State.IDLE:
                idle(stateQueue.getDuration(statesPopped));
                break;
            case State.SHOOTING:
                shooting(statesPopped);
                break;
            }
        }

        // increment time
        stateQueue.incrementTime(Time.deltaTime);

        // create visions if needed
        if (!visionUser.isVision && stateQueue.shouldCreateVisionThisFrame(Time.deltaTime, VisionUser.VISION_DURATION) != -1) {
            timeUser.addCurrentFrameInfo();
            Prittle vision = visionUser.createVision(VisionUser.VISION_DURATION).GetComponent<Prittle>();
        }

        // state actions
        switch (state) {
        case State.IDLE:
            // retract vines
            float retractSpeed = vineMaxLength / retractDurationMax;
            for (int i = 0; i < vines.Count; i++) {
                PrittleVine vine = vines[i];
                if (vine.length > 0) {
                    vine.length = Mathf.Max(0, vine.length - retractSpeed * Time.deltaTime);
                }
            }
            break;
        case State.SHOOTING:
            for (int i=0; i<vines.Count; i++) {
                PrittleVine vine = vines[i];
                if (!vine.complete) {
                    float distTravelled = shootSpeed * Time.deltaTime;
                    Vector2 origin = new Vector2(vine.transform.position.x, vine.transform.position.y);
                    float radius = vine.girth / 2;
                    Vector2 direction = new Vector2(Mathf.Cos(vine.rotation * Mathf.Deg2Rad), Mathf.Sin(vine.rotation * Mathf.Deg2Rad));
                    RaycastHit2D rh2d = Physics2D.CircleCast(origin, radius, direction, vine.length + distTravelled, LayerMask.GetMask("Default"));
                    if (rh2d) {
                        // hit wall
                        vine.length = rh2d.distance;
                        vine.complete = true;
                        if (!visionUser.isVision) {
                            if (vineHitSound != null && !SoundManager.instance.isSFXPlaying(vineHitSound)) {
                                SoundManager.instance.playSFXIfOnScreenRandPitchBend(vineHitSound, rh2d.point);
                            }
                            CameraControl.instance.shake(hitShakeMagnitude, hitShakeDuration);
                        }
                    } else {
                        float newLength = vine.length + distTravelled;
                        if (newLength >= vine.maxLength) {
                            // reached edge of vine
                            vine.length = vine.maxLength;
                            vine.complete = true;
                        } else {
                            vine.length = newLength;
                        }
                    }
                }
            }
            break;
        }

        // vines dealing damage
        if (!visionUser.isVision) {
            foreach (PrittleVine vine in vines) {
                RaycastHit2D hitPlayer = vine.hitPlayer();
                if (hitPlayer) {
                    ReceivesDamage rd = Player.instance.GetComponent<ReceivesDamage>();
                    if (vine.complete) {
                        // not shooting
                        rd.dealDamage(vineDamage, hitPlayer.point.x < Player.instance.rb2d.position.x);
                    } else {
                        // shooting
                        rd.dealDamage(vineShootDamage, Mathf.Cos(vine.rotation * Mathf.Deg2Rad) > 0);
                    }
                }
            }
        }
        
        rb2d.velocity = v;

        // aware of player
        /*
        if (playerAwareness.awareOfPlayer) { }

        */

    }

    void LateUpdate() {

        // manually update vision graphics for vines because SpriteMask can't handle the material
        if (visionUser.isVision) {
            foreach (PrittleVine vine in vines) {
                vine.alpha = spriteRenderer.color.a;
            }
        }
    }

    /* called when this becomes a vision */
    void TimeSkip(float timeInFuture) {

        // Start() hasn't been called yet
        //Start();
        
        // increment time
        time += timeInFuture;
        int statesPopped = stateQueue.numStatesPoppedByIncrementingTime(timeInFuture);
        if (statesPopped >= 1) {
            State nextState = (State)stateQueue.getState(statesPopped);
            if (nextState == State.SHOOTING) {
                shooting(statesPopped);
            } else {
                Debug.Log("Something's wrong, this state should be SHOOTING");
            }
        }
        stateQueue.incrementTime(timeInFuture);

        // manually update vision graphics for vines because SpriteMask can't handle the material
        if (visionUser.isVision) {
            foreach (PrittleVine vine in vines) {
                vine.vision = true;
            }
        }
    }

    /* called when this takes damage */
    void OnDamage(AttackInfo ai) {
        // be aware of player
        playerAwareness.alwaysAware = true;
        // die
        if (receivesDamage.health <= 0) {
            flippedHoriz = ai.impactToRight();
            idle(9999); // go to idle state to retract vines
            animator.Play("damage");
        }
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["time"] = time;
        stateQueue.OnSaveFrame(fi);

        for (int i=0; i<vines.Count; i++) {
            PrittleVine vine = vines[i];
            fi.floats["vl" + i] = vine.length;
            fi.floats["vlr" + i] = vine.localRotation;
            fi.bools["vc" + i] = vine.complete;
        }
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];
        stateQueue.OnRevert(fi);

        for (int i=0; i<vines.Count; i++) {
            PrittleVine vine = vines[i];
            vine.length = fi.floats["vl" + i];
            vine.localRotation = fi.floats["vlr" + i];
            vine.complete = fi.bools["vc" + i];
        }
    }

    void OnTimeDestroy() {
        foreach (PrittleVine vine in vines) {
            vine.spriteObject.GetComponent<SpriteRenderer>().color = Color.clear;
        }
    }

    void OnRevertExist() {
        foreach (PrittleVine vine in vines) {
            vine.spriteObject.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }



    float time;
    Segment segment;

    VisionUser.StateQueue stateQueue;

    List<PrittleVine> vines = new List<PrittleVine>();

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
