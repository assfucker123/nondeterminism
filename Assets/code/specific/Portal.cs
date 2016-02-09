using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour {

    ///////////////////
    // SEND MESSAGES //
    ///////////////////

    /* Called when spawned. */
    // void OnSpawn(SpawnInfo spawnInfo);

    ////////////
    // PUBLIC //
    ////////////

    public Color color = new Color(255 / 255.0f, 141 / 255.0f, 141 / 255.0f); // color of the portal

    public GameObject gameObjectToSpawn = null; // the GameObject to spawn
    public AudioClip portalSound = null;
    public SpawnInfo spawnInfo = null; // optional info that is send to the GameObject through OnSpawn(SpawnInfo)
    public float visionTimeInFuture = 2.0f; // how far in advance the vision happens
    public float visionDuration = 1.5f; // how long the vision lasts
    [HideInInspector]
    public WaveSpawner waveSpawnerRef = null; // reference to the WaveSpawner that spawned it.

    /////////////
    // PRIVATE //
    /////////////

    private float enterDuration = 4 / 40.0f;
    private float idleDuration = 5 / 40.0f;
    private float exitDuration = 4 / 40.0f;

    enum State {
        PRE_VISION,
        VISION,
        ENTER,
        IDLE,
        EXIT,
        POST_EXIT
    }

    private State state = State.VISION;

    void spawnGameObject() {
        if (gameObjectToSpawn == null)
            return;
        GameObject sGO = GameObject.Instantiate(
            gameObjectToSpawn,
            transform.localPosition,
            Quaternion.identity) as GameObject;
        if (spawnInfo == null) {
            spawnInfo = new SpawnInfo();
        }
        TimeUser sTU = sGO.GetComponent<TimeUser>();
        sTU.setRandSeed((int)(int.MaxValue * timeUser.randomValue()));
        EnemyInfo eI = sGO.GetComponent<EnemyInfo>();
        eI.id = spawnInfo.variation;
        eI.waveSpanwerRef = waveSpawnerRef;
        
        sGO.SendMessage("OnSpawn", spawnInfo, SendMessageOptions.DontRequireReceiver);

        if (visionUser.isVision) {
            VisionUser sV = sGO.GetComponent<VisionUser>();
            sV.becomeVisionNow(visionDuration, visionUser);
        }
    }
	
	void Awake() {
		backSpriteObject = transform.Find("spriteObject").gameObject;
        backSpriteRenderer = backSpriteObject.GetComponent<SpriteRenderer>();
        backSpriteAnimator = backSpriteObject.GetComponent<Animator>();

        frontSpriteObject = transform.Find("frontSprite").gameObject;
        frontSpriteRenderer = frontSpriteObject.GetComponent<SpriteRenderer>();
        frontSpriteAnimator = frontSpriteObject.GetComponent<Animator>();

        timeUser = GetComponent<TimeUser>();
        visionUser = GetComponent<VisionUser>();
	}

    void Start() {
        backSpriteRenderer.enabled = false;
        frontSpriteRenderer.enabled = false;
        if (visionUser.isVision) {
            // set in TimeSkip
        } else { // not a vision
            time = 0;
            state = State.PRE_VISION;
            backSpriteRenderer.color = color;
            frontSpriteRenderer.color = color;
        }
    }
	
	void Update() {

        // update color (important for vision)
        frontSpriteRenderer.color = backSpriteRenderer.color;

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        switch (state) {
        case State.PRE_VISION:
            if (time > Time.deltaTime) { // create vision once exists for a frame
                Debug.Assert(!visionUser.isVision);
                GameObject vGO = visionUser.createVision(visionTimeInFuture, visionDuration);
                Portal vPortal = vGO.GetComponent<Portal>();
                vPortal.spawnInfo = spawnInfo;
                // move on to actual state
                time = 0;
                state = State.VISION;
            }
            break;
        case State.VISION:
            if (time >= visionTimeInFuture) {
                frontSpriteRenderer.enabled = true;
                frontSpriteAnimator.Play("enter");
                if (!visionUser.isVision && portalSound != null) {
                    if (!SoundManager.instance.isSFXPlaying(portalSound)) {
                        SoundManager.instance.playSFX(portalSound);
                    }
                }
                time = 0;
                state = State.ENTER;
            }
            break;
        case State.ENTER:
            if (time >= enterDuration) {
                spawnGameObject();

                backSpriteRenderer.enabled = true;
                backSpriteAnimator.Play("idle");
                frontSpriteAnimator.Play("exit");
                time = 0;
                state = State.IDLE;
            }
            break;
        case State.IDLE:
            if (time >= idleDuration) {
                backSpriteAnimator.Play("exit");
                frontSpriteRenderer.enabled = false;
                time = 0;
                state = State.EXIT;
            }
            break;
        case State.EXIT:
            if (time >= exitDuration) {
                backSpriteRenderer.enabled = false;
                time = 0;
                state = State.POST_EXIT;
            }
            break;
        case State.POST_EXIT:
            if (time + idleDuration + exitDuration >= visionDuration) {
                timeUser.timeDestroy();
            }
            break;
        }

        

	}

    void TimeSkip(float timeInFuture) {
        time += timeInFuture;
        state = State.VISION;

        // become vision
        Debug.Assert(visionUser.isVision);
        frontSpriteRenderer.material = backSpriteRenderer.material;
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.state = (int)state;

        // record animators
        fi.bools["bsrEnabled"] = backSpriteRenderer.enabled;
        fi.bools["fsrEnabled"] = frontSpriteRenderer.enabled;
        fi.ints["fsaFullPathHash"] = frontSpriteAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        fi.floats["fsaNormalizedTime"] = frontSpriteAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        state = (State)fi.state;

        // update animators
        backSpriteRenderer.enabled = fi.bools["bsrEnabled"];
        frontSpriteRenderer.enabled = fi.bools["fsrEnabled"];
        frontSpriteAnimator.Play(
            fi.ints["fsaFullPathHash"], 0,
            fi.floats["fsaNormalizedTime"]);
    }

    private float time = 0;

	// components
    GameObject backSpriteObject;
    SpriteRenderer backSpriteRenderer;
    Animator backSpriteAnimator;
    GameObject frontSpriteObject;
    SpriteRenderer frontSpriteRenderer;
    Animator frontSpriteAnimator;
    TimeUser timeUser;
    VisionUser visionUser;
}
