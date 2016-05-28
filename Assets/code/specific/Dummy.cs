using UnityEngine;
using System.Collections;

// surpresses "assigned but never used" warnings.  Get rid of this line when making the actual enemy
#pragma warning disable 0414

public class Dummy : MonoBehaviour {

    
    public State state = State.IDLE;

    public enum State {
        IDLE,
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

        stateQueue = new VisionUser.StateQueue(0);
    }

    void Start() {
        // attach to Segment
        /* segment = Segment.findBottom(rb2d.position); */
    }

    /* Called when being spawned from a Portal */
    void OnSpawn(SpawnInfo si) {
        flippedHoriz = !si.faceRight;
    }

    void addIdleStateToQueue() {
        float duration = 1.5f;
        stateQueue.addState((int)State.IDLE, duration, 0, 0, false);
    }

    /// <summary>
    /// Going to idle state
    /// </summary>
    void idle(float duration) {
        state = State.IDLE;
        time = 0;
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;
        if (defaultDeath.activated)
            return;

        time += Time.deltaTime;

        Vector2 v = rb2d.velocity;

        // adding states to state queue, alternate between idle and shooting
        float planDuration = VisionUser.VISION_DURATION * 2;
        if (stateQueue.planAheadDuration < planDuration) {

            addIdleStateToQueue();

            // if (stateQueue.empty || (State)stateQueue.getLastState() == State.SHOOTING) {
        }

        // handle states changing this frame
        int statesPopped = stateQueue.numStatesPoppedByIncrementingTime(Time.deltaTime);
        if (statesPopped >= 1) {
            State nextState = (State)stateQueue.getState(statesPopped);
            switch (nextState) {
            case State.IDLE:
                idle(stateQueue.getDuration(statesPopped));
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
            bvu.becomeVisionNow(visionUser.timeLeft, visionUser);
        }
        */

        // aware of player
        /*
        if (playerAwareness.awareOfPlayer) { }

        */

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
            /*if (nextState == State.SHOOTING) {
                shooting(statesPopped);
            } else {
                Debug.Log("Something's wrong, this state should be SHOOTING");
            }*/
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
            //animator.Play("damage");
        }
    }

    /* called at the end of a frame to record information */
    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["time"] = time;
        stateQueue.OnSaveFrame(fi);
    }
    /* called when reverting back to a certain time */
    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["time"];
        stateQueue.OnRevert(fi);
    }

    float time;
    Segment segment;

    VisionUser.StateQueue stateQueue;

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
