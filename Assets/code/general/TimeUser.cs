using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimeUser : MonoBehaviour {

    /* Will automatically record relevant info from the following components:
     *   - Rigidbody2D
     *   - SpriteRenderer (can optionally be a component of a child named 'spriteObject')
     *   - Animator (can optionally be a component of a child named 'spriteObject')
     * */

    ///////////////////
    // SEND MESSAGES //
    ///////////////////

    /* Will be called in LateUpdate just before a FrameInfo is recorded.
     * Use OnSaveFrame to save more info of the current frame to FrameInfo fi. */
    // void OnSaveFrame(FrameInfo fi);

    /* Called at the end of TimeUser.revert().
     * Parameter is the FrameInfo reverted to (do not edit the parameter) */
    // void OnRevert(FrameInfo fi);

    /* Called when a revert causes the exists variable to change
     * from false to true.  OnRevert is still called. */
    // void OnRevertExist();

    /* Will be called when this.timeDestroy is called,
     * causing the exists variable to change from true to false. */
    // void OnTimeDestroy();


    ///////////////////
    // PUBLIC STATIC //
    ///////////////////

    /* Once a TimeUser has been time destroyed for this amount of time, 
     * it will get destroyed for real. */
    public static float MAX_TIME_DESTROY_AGE = 12.0f;

    /* Begins a continuous revert.  The game will appear to go back in
     * time until endContinuousRevert() is called */
    public static void beginContinuousRevert(float speed = 1) {
        continuousRevertSpeed = speed;
        _reverting = true;
        _revertingTime = 0;
    }
    /* Ends a continuous revert. */
    public static void endContinuousRevert() {
        _reverting = false;
    }
    /* Gets if a continuous revert is happening.
     * All TimeUsers should check TimeUser.reverting in the beginning of
     * their Update() etc. functions, and return if TimeUser.reverting is true. */
    public static bool reverting { get { return _reverting; } }
    /* Gets how far back in time since the start of this continuous revert. */
    public static float revertingTime {
        get {
            if (!reverting) return 0;
            return _revertingTime;
        }
    }
    /* Speed for the continuous revert, minimum is 0.
     * Can be set without calling a function. */
    public static float continuousRevertSpeed = 1;
    /* RULE: TimeUser.time + TimeUser.timeDiff = Time.timeSinceLevelLoad
     * (timeDiff changes when revert is called) */
    public static float time { get { return Time.timeSinceLevelLoad - timeDiff; } }
    public static float timeDiff { get { return _timeDiff; } }
    /* Calls revert(time) for all TimeUsers.
     * This is called every frame during a continuous revert. */
    public static void revert(float time) {
        time = Mathf.Max(0, time);
        if (time >= TimeUser.time)
            return;
        _revertMutex = true;

        //adjust time
        TimeUser._timeDiff += TimeUser.time - time;
        
        //revert all TimeUsers
        foreach (TimeUser tu in allTimeUsers) {
            tu.revertThis(time);
        }

        _revertMutex = false;
    }
    public static bool revertMutex { get { return _revertMutex; } }

    /* Helper functions for converting Colors to strings and back.
     * Useful for storing color in OnSaveFrame() and OnRevert(). */
    public static string colorToString(Color c){
        return "" + c.r + "," + c.g + "," + c.b + "," + c.a;
    }
    public static Color stringToColor(string s) {
        char[] chars = { ',' };
        string[] colorParts = s.Split(chars);
        return new Color(float.Parse(colorParts[0]), float.Parse(colorParts[1]), float.Parse(colorParts[2]), float.Parse(colorParts[3]));
    }

    /* Called by Vars.loadLevel() */
    public static void onUnloadLevel() {
        _timeDiff = 0;
        _revertMutex = false;
        _reverting = false;
        _revertingTime = 0;
        continuousRevertAppliedThisFrame = false;
    }

    ////////////////
    // PROPERTIES //
    ////////////////

    public bool exists { get { return _exists; } }
    /* If shouldNotUpdate is true, then most TimeUsers should not 
     * do anything during Update() and similar functions. */
    public bool shouldNotUpdate {
        get {
            return TimeUser.reverting || !exists || 
                (PauseScreen.instance != null && PauseScreen.paused);
        }
    }
    public float timeCreated { get { return _timeCreated; } }
    public float age {  get { return TimeUser.time - timeCreated; } }
    public bool createdAtLevelLoad { get { return timeCreated < .1f; } }
    public int randSeed { get { return _randSeed; } }
    public FrameInfo getLastFrameInfo() {
        if (fis.Count == 0)
            return null;
        return fis[fis.Count - 1];
    }
    
    ///////////////
    // FUNCTIONS //
    ///////////////

    /* Attempts to revert back to a previous time.  Can only be called by TimeUser.revert()
     * If timeSinceLevelLoad is 0 or less and this was gameObject was created when the level loaded,
     * then will use the first FrameInfo recorded.
     * Otherwise, if timeSinceLevelLoad is less than this.timeSinceLevelLoadCreated,
     * then this gameObject will be destroyed (for real) */
    public void revertThis(float time) {

        //ensures was called by TimeUser.revert()
        if (!TimeUser.revertMutex)
            return;
        
        // does revert go back to before this gameObject was created?
        if (time < timeCreated &&
            !createdAtLevelLoad) {
            //then destroy this gameObject
            _exists = false;
            GameObject.Destroy(this.gameObject);
            return;
        }
        
        // find fis to revert back to
        if (fis.Count == 0)
            return;
        int fiRevertIndex = 0;
        for (fiRevertIndex = fis.Count - 1; fiRevertIndex > 0; fiRevertIndex--) {
            if (fis[fiRevertIndex].time <= time)
                break;
        }
        FrameInfo fiRevert = fis[fiRevertIndex];

        revertToFrameInfoUnsafe(fiRevert);

        //delete all FrameInfo following this revert
        for (int i = fiRevertIndex + 1; i < fis.Count; i++) {
            FrameInfo.destroy(fis[i]);
        }
        fis.RemoveRange(fiRevertIndex + 1, fis.Count - fiRevertIndex - 1);

    }

    /* Reverts to a specific FrameInfo.
     * No safety checking is done here.
     * Also sends OnRevert message. */
    public void revertToFrameInfoUnsafe(FrameInfo fiRevert) {

        // is this being brought into existance?
        if (!exists && fiRevert.exists) {
            _exists = true;
            if (rb2d != null) {
                rb2d.simulated = true;
            }
            if (spriteRenderer != null) {
                spriteRenderer.enabled = true;
            }
            SendMessage("OnRevertExist", SendMessageOptions.DontRequireReceiver);
        }
        Debug.Assert(!(exists && !fiRevert.exists));

        // apply other properties
        if (rb2d != null) {
            rb2d.position = fiRevert.position;
            rb2d.velocity = fiRevert.velocity;
            rb2d.rotation = fiRevert.rotation;
            rb2d.angularVelocity = fiRevert.angularVelocity;
        }
        if (transformSelf || spriteRenderer == null) {
            transform.localScale = new Vector3(
                fiRevert.spriteRendererLocalScaleX,
                fiRevert.spriteRendererLocalScaleY,
                transform.localScale.z);
            transform.localRotation = Utilities.setQuat(fiRevert.spriteRendererLocalRotation);
        } else {
            spriteRenderer.transform.localScale = new Vector3(
                fiRevert.spriteRendererLocalScaleX,
                fiRevert.spriteRendererLocalScaleY,
                spriteRenderer.transform.localScale.z);
            spriteRenderer.transform.localRotation = Utilities.setQuat(fiRevert.spriteRendererLocalRotation);
        }
        if (animator != null) {
            animator.Play(
                fiRevert.animatorFullPathHash, 0,
                fiRevert.animatorNormalizedTime);
        }
        _randSeed = fiRevert.randSeed;

        // manually adjust other stuff
        SendMessage("OnRevert", fiRevert, SendMessageOptions.DontRequireReceiver);

    }

    /* Creates a FrameInfo for this frame and adds it.
     * This is automatically called in LateUpdate(), so it doesn't have to be called manually. */
    public void addCurrentFrameInfo() {

        if (!this.exists) {
            //don't bother adding if already didn't exist
            if (fis.Count > 0) {
                FrameInfo lastFI = fis[fis.Count - 1];
                if (!lastFI.exists)
                    return;
            }
        }

        // create new FrameInfo for the current frame
        FrameInfo fi = FrameInfo.create();
        fi.time = TimeUser.time;
        fi.exists = exists;
        if (rb2d != null) {
            fi.position = rb2d.position;
            fi.velocity = rb2d.velocity;
            fi.rotation = rb2d.rotation;
            fi.angularVelocity = rb2d.angularVelocity;
        }
        if (transformSelf || spriteRenderer == null) {
            fi.spriteRendererLocalScaleX = transform.localScale.x;
            fi.spriteRendererLocalScaleY = transform.localScale.y;
            fi.spriteRendererLocalRotation = Utilities.get2DRot(transform.localRotation);
        } else {
            fi.spriteRendererLocalScaleX = spriteRenderer.transform.localScale.x;
            fi.spriteRendererLocalScaleY = spriteRenderer.transform.localScale.y;
            fi.spriteRendererLocalRotation = Utilities.get2DRot(spriteRenderer.transform.localRotation);
        }
        if (animator != null) {
            fi.animatorFullPathHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
            fi.animatorNormalizedTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }
        fi.randSeed = randSeed;

        // manually save other info
        this.SendMessage("OnSaveFrame", fi, SendMessageOptions.DontRequireReceiver);

        // add to fis
        fis.Add(fi);

    }

    /* Call to fake destroy the gameObject.  It won't actually be destroyed,
     * so it can be reverted back into existance later. */
    public void timeDestroy() {
        if (!exists) return;

        SendMessage("OnTimeDestroy", SendMessageOptions.DontRequireReceiver);

        if (rb2d != null) {
            rb2d.simulated = false;
        }
        if (spriteRenderer != null) {
            spriteRenderer.enabled = false;
        }

        _exists = false;
    }

    /* Call this to get repeatable random numbers, since it's based on a random seed.
     * The random seed gets reverted too.
     * Returns a number in [0, 1)
     * */
    public float randomValue() {
        Random.seed = randSeed;
        float val = Random.value; // in [0, 1)
        _randSeed = (int) (int.MaxValue * val);
        return val;
    }

    public void setRandSeed(int seed) {
        _randSeed = seed;
    }
    
    /////////////
    // PRIVATE //
    /////////////

    private static List<TimeUser> allTimeUsers = new List<TimeUser>();
    private static float _timeDiff = 0;
    private static bool _revertMutex = false;
    private static bool _reverting = false;
    private static float _revertingTime = 0;
    private static bool continuousRevertAppliedThisFrame = false;

    
    void Awake() {
        _timeCreated = TimeUser.time;
        allTimeUsers.Add(this);
        rb2d = this.GetComponent<Rigidbody2D>();
        Transform sot = this.transform.Find("spriteObject");
        if (sot == null){
            spriteRenderer = this.GetComponent<SpriteRenderer>();
            transformSelf = true;
            animator = this.GetComponent<Animator>();
        } else {
            spriteRenderer = sot.gameObject.GetComponent<SpriteRenderer>();
            transformSelf = false;
            animator = sot.gameObject.GetComponent<Animator>();
        }
        setRandSeed((int)(int.MaxValue * Random.value));
    }
    void OnLevelWasLoaded(int level) {
        //fixing odd bug.  For some TimeUsers that get created at the start, TimeUser.time isn't reset to 0 when Awake() is called
        _timeCreated = 0;

        // destroy all frameInfos that came from the previous level
        foreach (FrameInfo fi in fis) {
            FrameInfo.destroy(fi);
        }
        fis.Clear();
        // add current one
        addCurrentFrameInfo();
    }

    void Start() {
	    
	}

    void Update() {

        if (PauseScreen.instance != null && PauseScreen.paused)
            return;

        if (reverting && !continuousRevertAppliedThisFrame) {
            continuousRevertSpeed = Mathf.Max(0, continuousRevertSpeed);
            TimeUser.revert(TimeUser.time - Time.unscaledDeltaTime * (1 + continuousRevertSpeed));
            _revertingTime += Time.deltaTime * (1 + continuousRevertSpeed);
            continuousRevertAppliedThisFrame = true;
        }

    }

	void LateUpdate() {

        continuousRevertAppliedThisFrame = false;

        // no reason to save frame info when game is paused.  But still save frame info of the frame that caused the game to pause in the first place.
        if (screenPausedNextFrame) {
            screenPausedNextFrame = (PauseScreen.instance != null && PauseScreen.paused);
            return;
        }
        screenPausedNextFrame = (PauseScreen.instance != null && PauseScreen.paused);

        addCurrentFrameInfo();

        // if has been time destroyed, delete for good if it gets too old
        if (!this.exists) {
            if (TimeUser.time - getLastFrameInfo().time >= TimeUser.MAX_TIME_DESTROY_AGE) {
                GameObject.Destroy(gameObject);
            }
        }

        destroyEarlyFrameInfo();

    }

    void OnDestroy() {
        //destroy all frame infos
        foreach (FrameInfo fi in fis) {
            FrameInfo.destroy(fi);
        }
        fis.Clear();
        //
        allTimeUsers.Remove(this);
    }
    
    /* Destroys all frame info earlier than MAX_TIME_DESTROY_AGE */
    void destroyEarlyFrameInfo() {
        for (int i=0; i<fis.Count; i++) {
            FrameInfo fi = fis[i];
            if (!fi.preserve && fi.time < TimeUser.time - MAX_TIME_DESTROY_AGE) {
                FrameInfo.destroy(fi);
                fis.RemoveAt(i);
                i--;
            } else {
                break;
            }
        }
    }
    

    private Rigidbody2D rb2d;
    private SpriteRenderer spriteRenderer;
    private bool transformSelf = false; // if true, data is from gameObject.transform.  false is from spriteRenderer.transform
    private Animator animator;
    private bool _exists = true;
    private float _timeCreated = 0;
    private int _randSeed = 0;
    private List<FrameInfo> fis = new List<FrameInfo>(); //FrameInfos sorted in ascending timeSinceLevelLoad
    private bool screenPausedNextFrame = false;
}
