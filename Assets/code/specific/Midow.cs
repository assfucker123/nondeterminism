using UnityEngine;
using System.Collections;

public class Midow : MonoBehaviour {

    public float speed = 10;
    public float segmentEdgePadding = 1.5f;
    public float idleDurationMin = .4f;
    public float idleDurationMax = 1.5f;
    public float weakIdleDurationMultiplier = 2.1f;
    public Vector2 bulletSpawnPos = new Vector2();
    public float shootingDuration = .3f;
    public float bulletLeftHeading = -70;
    public float bulletCenterHeading = 0;
    public float bulletRightHeading = 70;
    public float bulletSlowSpeedMultiplier = .3f;
    public GameObject bulletGameObject;
    public AudioClip shootSound;
    public State state = State.IDLE;
    public bool flippedOnSegment = false;

    public enum State {
        IDLE,
        MOVE,
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
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();
        visionUser = GetComponent<VisionUser>();
        defaultDeath = GetComponent<DefaultDeath>();
        enemyInfo = GetComponent<EnemyInfo>();
        playerAwareness = GetComponent<PlayerAwareness>();
    }

    void Start() {
        if (!visionUser.isVision) {
            attachToSegment();
            setRB2D(true);
            updateStateQueue(0);
        }
        
    }

    void idle(float duration) {
        rb2d.velocity = Vector2.zero;
        state = State.IDLE;
        time = 0;
        this.duration = duration;
        animator.Play("idle");
    }

    /// <summary>
    /// Sets state to State.MOVE and begins to move Midow.  Duration is automatically calculated from speed
    /// </summary>
    /// <param name="inter">interpolation in [0,1]</param>
    void moveTo(float inter) {
        duration = getTravelDuration(interpolation, inter);
        inter0 = interpolation;
        inter1 = inter;

        animator.Play("walk");
        flippedHoriz = (inter1 > inter0) == flippedOnSegment;
        state = State.MOVE;
        time = 0;
    }

    void shoot() {
        rb2d.velocity = Vector2.zero;
        animator.Play("shoot");
        state = State.SHOOTING;
        time = 0;

        if (enemyInfo.id == EnemyInfo.ID.MIDOW) {
            shootBullets(false);
        }
        shootBullets(true);
        if (!visionUser.isVision) {
            SoundManager.instance.playSFXIfOnScreenRandPitchBend(shootSound, rb2d.position, .02f);
        }
    }

    void shootBullets(bool slow) {
        Vector2 pos = rb2d.position + Utilities.rotateAroundPoint(bulletSpawnPos, Vector2.zero, rb2d.rotation * Mathf.PI/180);
        Bullet bullet;
        // left direction
        bullet = (GameObject.Instantiate(bulletGameObject, pos, Quaternion.identity) as GameObject).GetComponent<Bullet>();
        bullet.heading = bulletLeftHeading + rb2d.rotation;
        if (slow) bullet.speed *= bulletSlowSpeedMultiplier;
        if (visionUser.isVision) bullet.GetComponent<VisionUser>().becomeVisionNow(visionUser.timeLeft, visionUser);
        // up direction
        bullet = (GameObject.Instantiate(bulletGameObject, pos, Quaternion.identity) as GameObject).GetComponent<Bullet>();
        bullet.heading = bulletCenterHeading + rb2d.rotation;
        if (slow) bullet.speed *= bulletSlowSpeedMultiplier;
        if (visionUser.isVision) bullet.GetComponent<VisionUser>().becomeVisionNow(visionUser.timeLeft, visionUser);
        // right direction
        bullet = (GameObject.Instantiate(bulletGameObject, pos, Quaternion.identity) as GameObject).GetComponent<Bullet>();
        bullet.heading = bulletRightHeading + rb2d.rotation;
        if (slow) bullet.speed *= bulletSlowSpeedMultiplier;
        if (visionUser.isVision) bullet.GetComponent<VisionUser>().becomeVisionNow(visionUser.timeLeft, visionUser);
    }

    float getTravelDuration(float interStart, float interEnd) {
        float dist = Vector2.Distance(positionFromTrueInterpolation(getTrueInterpolation(interStart)), positionFromTrueInterpolation(getTrueInterpolation(interEnd)));
        return dist / speed;
    }

    /// <summary>
    /// Tries to find a MidowWebSegment and attach to it.  It that doesn't work, find a Segment and attach to that
    /// </summary>
    void attachToSegment() {
        // finding webSegments
        BezierLine[] webSegments = GameObject.FindObjectsOfType<BezierLine>();
        webSegment = null;
        Vector2 webSegmentClosePoint = new Vector2();
        float dist2 = 999999;
        foreach (BezierLine ws in webSegments) {
            Vector2 closePoint = Utilities.closestPointOnLineToPoint(ws.startPoint, ws.endPoint, rb2d.position);
            if ((closePoint - rb2d.position).SqrMagnitude() < dist2) {
                dist2 = (closePoint - rb2d.position).SqrMagnitude();
                webSegment = ws;
                webSegmentClosePoint = closePoint;
            }
        }
        
        // finding Segment
        segment = Segment.findAny(rb2d.position);
        // choosing between webSegment and segment
        if (segment != null && webSegment != null) {
            if (segment.distanceFromPoint(rb2d.position) < Mathf.Sqrt(dist2)) {
                webSegment = null;
            } else {
                segment = null;
            }
        }
        if (webSegment == null && segment == null) {
            Debug.LogError("Midow not attached to a segment");
        } else if (webSegment != null) {
            // set to web segment
            interpolation = Utilities.inverseInterpolate(webSegment.startPoint, webSegment.endPoint, webSegmentClosePoint, false);
        } else {
            // set to segment
            interpolation = segment.inverseInterpolate(rb2d.position);
        }
    }

    /// <summary>
    /// in [0,1], represents how far across the segment Midow is, assuming segment was trimmed by segmentEdgePadding
    /// set this for movement
    /// </summary>
    float interpolation = 0;
    /// <summary>
    /// Gets true interpolation over entire segment, not trimmed
    /// </summary>
    float trueInterpolation {
        get {
            return getTrueInterpolation(interpolation);
        }
    }
    float getTrueInterpolation(float interpolation) {
        float scaledPadding;
        if (webSegment != null) {
            scaledPadding = segmentEdgePadding / webSegment.approximateDistance;
            return scaledPadding + interpolation * (webSegment.approximateDistance - segmentEdgePadding * 2) / webSegment.approximateDistance;
        }
        if (segment != null) {
            scaledPadding = segmentEdgePadding / segment.length;
            return scaledPadding + interpolation * (segment.length - segmentEdgePadding * 2) / segment.length;
        }
        return interpolation;
    }

    /// <summary>
    /// Gets position in local space based on the interpolation property
    /// </summary>
    Vector2 positionFromTrueInterpolation(float trueInter) {
        if (webSegment != null) {
            return webSegment.interpolate(trueInter);
        }
        if (segment != null) {
            return segment.interpolate(trueInter);
        }
        return Vector2.zero;
    }

    /// <summary>
    /// Gets normal based on the interpolation property
    /// </summary>
    Vector2 normalFromTrueInterpolation(float trueInter) {
        if (webSegment != null) {
            return webSegment.getNormal(trueInter);
        }
        if (segment != null) {
            return segment.normal;
        }
        return Vector2.zero;
    }

    /// <summary>
    /// Sets rb2d from trueInterpolation and flippedOnSegment
    /// </summary>
    /// <param name="immediately">if true, will set the position.  if false, will call rb2d.MovePosition()</param>
    void setRB2D(bool immediately) {
        Vector2 pt = positionFromTrueInterpolation(trueInterpolation);
        Vector2 n = normalFromTrueInterpolation(trueInterpolation);
        float rotation = Mathf.Atan2(-n.x, n.y);
        if (flippedOnSegment)
            rotation += Mathf.PI;
        Vector2 rotDiff = Utilities.rotateAroundPoint(new Vector2(0,-enemyInfo.spawnDist), Vector2.zero, rotation);
        pt -= rotDiff;
        if (immediately) {
            rb2d.rotation = rotation * 180 / Mathf.PI;
            rb2d.position = pt;
        } else {
            rb2d.MovePosition(pt);
            rb2d.rotation = rotation * 180 / Mathf.PI;
        }
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
            // state handling is done in updateStateQueue
            break;
        case State.MOVE:
            interpolation = Utilities.easeLinearClamp(time, inter0, inter1 - inter0, duration);
            setRB2D(false);
            // state handing is done is updateStateQueue
            break;
        case State.SHOOTING:
            // state handling is done in updateStateQueue
            break;
        }

        rb2d.velocity = v;

        // update state queue
        updateStateQueue(Time.deltaTime);


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
            bvu.becomeVisionNow(visionUser.timeLeft, visionUser);
        }
        */

        // aware of player
        /*
        if (playerAwareness.awareOfPlayer) { }

        */

    }
    /// <summary>
    /// Called each frame to make sure stateQueue stays full
    /// </summary>
    void updateStateQueue(float deltaTime) {
        
        // handle states changing this frame
        int statesPopped = stateQueue.numStatesPoppedByIncrementingTime(deltaTime);
        if (statesPopped >= 1) {
            State nextState = (State)stateQueue.getState(statesPopped);
            switch (nextState) {
            case State.IDLE:
                idle(stateQueue.getDuration(statesPopped));
                break;
            case State.MOVE:
                moveTo(stateQueue.getX(statesPopped));
                break;
            case State.SHOOTING:
                shoot();
                break;
            }
        }

        // increment time
        stateQueue.incrementTime(deltaTime);

        // create visions if needed
        if (!visionUser.isVision && stateQueue.shouldCreateVisionThisFrame(deltaTime, VisionUser.VISION_DURATION) != -1) {
            timeUser.addCurrentFrameInfo();
            visionUser.createVision(VisionUser.VISION_DURATION).GetComponent<Midow>();
        }

        // plan ahead stateQueue
        float planDuration = VisionUser.VISION_DURATION * 2;
        bool testDone = false;
        while (stateQueue.planAheadDuration < planDuration && !testDone) {
            float idleDur;
            if (stateQueue.empty) {
                // adding idle state
                idleDur = timeUser.randomRange(idleDurationMin, idleDurationMax);
                if (enemyInfo.id == EnemyInfo.ID.MIDOW_WEAK)
                    idleDur *= weakIdleDurationMultiplier;
                stateQueue.addState((int)State.IDLE, idleDur, interpolation, 0, false);
            } else {
                State lastState = (State)stateQueue.getState(stateQueue.queueCount-1);
                float lastInter = stateQueue.getX(stateQueue.queueCount-1);
                switch (lastState) {
                case State.IDLE:
                    // adding move state
                    float nextInter;
                    if (lastInter < .5f) nextInter = timeUser.randomRange(.5f, 1);
                    else nextInter = timeUser.randomRange(0, .5f);
                    stateQueue.addState((int)State.MOVE, getTravelDuration(lastInter, nextInter), nextInter, lastInter, false);
                    break;
                case State.MOVE:
                    if (stateQueue.planAheadDuration < VisionUser.VISION_DURATION + .2f) { // don't add shoot state too early or else there won't be a vision for it
                        // adding idle state
                        idleDur = timeUser.randomRange(idleDurationMin, idleDurationMax);
                        if (enemyInfo.id == EnemyInfo.ID.MIDOW_WEAK)
                            idleDur *= weakIdleDurationMultiplier;
                        stateQueue.addState((int)State.IDLE, idleDur, lastInter, 0, false);
                    } else {
                        // adding shoot state
                        stateQueue.addState((int)State.SHOOTING, shootingDuration, lastInter, 0, true);
                    }
                    break;
                case State.SHOOTING:
                    // adding idle state
                    idleDur = timeUser.randomRange(idleDurationMin, idleDurationMax);
                    if (enemyInfo.id == EnemyInfo.ID.MIDOW_WEAK)
                        idleDur *= weakIdleDurationMultiplier;
                    stateQueue.addState((int)State.IDLE, idleDur, lastInter, 0, false);
                    break;
                }
            }
            testDone = true;
        }
    }

    /* called when this becomes a vision */
    void TimeSkip(float timeInFuture) {

        // Start() hasn't been called yet
        //Start();
        attachToSegment();

        // increment time
        time += timeInFuture;
        int statesPopped = stateQueue.numStatesPoppedByIncrementingTime(timeInFuture);
        if (statesPopped >= 1) {
            State nextState = (State)stateQueue.getState(statesPopped);
            if (nextState == State.SHOOTING) {
                interpolation = stateQueue.getX(statesPopped);
                setRB2D(true);
                shoot();
            } else {
                Debug.Log("Something's wrong, this state should be SHOOTING");
            }
        }
        stateQueue.incrementTime(timeInFuture);

    }

    /* called when this takes damage */
    void OnDamage(AttackInfo ai) {
        // be aware of player
        playerAwareness.alwaysAware = true;
        // die
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
        fi.floats["in"] = interpolation;
        fi.floats["i0"] = inter0;
        fi.floats["i1"] = inter1;
        stateQueue.OnSaveFrame(fi);
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];
        duration = fi.floats["d"];
        interpolation = fi.floats["in"];
        inter0 = fi.floats["i0"];
        inter1 = fi.floats["i1"];
        stateQueue.OnRevert(fi);
    }

    float time;
    float duration;
    float inter0 = 0;
    float inter1 = 0;
    Segment segment;
    BezierLine webSegment;

    VisionUser.StateQueue stateQueue = new VisionUser.StateQueue();

    // components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    Animator animator;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;
    VisionUser visionUser;
    DefaultDeath defaultDeath;
    EnemyInfo enemyInfo;
    PlayerAwareness playerAwareness;

}
