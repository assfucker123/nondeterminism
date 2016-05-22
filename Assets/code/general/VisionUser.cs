using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(TimeUser))]
public class VisionUser : MonoBehaviour {
	
    /* Component for all GameObjects that can produce visions of themselves.
     * By default this gameObject is real.  Call becomeVision() to convert this gameObject
     * into a vision. */

    /* Properties of a vision:
     * - Colored in grayscale
     * - Colliders are tagged as "Vision", which only hit walls (tag: "Default")
     * - Colliders do not deal damage.
     * - Are invisible during flashbacks
     * */

    ///////////////////
    // SEND MESSAGES //
    ///////////////////

    /* Skips ahead the given amount of time.
     * Called when this gameObject becomes a vision. */
    // void TimeSkip(float timeInFuture);

    ////////////
    // STATIC //
    ////////////

    public static float VISION_DURATION = .9f; //recommended duration for all visions (for consistency)

    /* Activates Oracle's vision ability, which makes these visions visible. */
    public static void activateAbility() {
        if (abilityActive) return;
        _abilityActive = true;
    }
    /* Deactivates Oracle's vision ability, which will cause all visions to be invisible. */
    public static void deactivateAbility() {
        if (!abilityActive) return;
        cutAllVisions();
        _abilityActive = false;
    }
    /* If Oracle's vision ability is currently active. */
    public static bool abilityActive { get { return _abilityActive; } }
    /* Cuts all visions. */
    public static void cutAllVisions() {
        foreach (VisionUser vu in allVisionUsers) {
            vu.cutVisions();
        }
    }

    /* Called by Vars.loadLevel() */
    public static void onUnloadLevel() {
        _abilityActive = true;
    }

    
    ////////////
    // PUBLIC //
    ////////////

    private float flickerPeriod = .2f;
    private float flickerAlpha = .7f;
    private float baseAlpha = .6f;
    public bool isVision { get { return _isVision; } }
    private float fadeInDuration = .35f; //how long for the vision to fade in
    private float flashbackAlpha = .15f; // alpha of a vision during a flashback (used to be 0)
    public float time { get { return _time; } } //how long this visionUser has been a vision
    public float duration { get { return _duration; } } //how long this visionUser will be a vision for
    public float timeLeft { get { return duration - time; } } //how much longer until vision goes away
    public bool createdWhenAbilityDeactivated { get { return _createdWhenAbilityDeactivated; } }
    
    public Material material; //material for the vision's sprite.
    public AudioClip visionSound; //sound made when a vision is created.
    public static bool PLAY_VISION_SOUND = false;

    /* Creates a clone of this gameObject, converted into a vision. */
    public GameObject createVision(float timeInFuture, float visionDuration) {
        if (isVision) {
            Debug.LogError("ERROR: Visions cannot make visions of themselves");
            return null; //visions cannot create visions of themselves
        }
        if (timeUser.getLastFrameInfo() == null) { //this should only happen if this hasn't existed for more than 1 frame
            Debug.Log("ERROR: This timeUser must exist for more than 1 frame to create a vision");
            return null;
        }
        GameObject vision = GameObject.Instantiate(gameObject) as GameObject;
        VisionUser vu = vision.GetComponent<VisionUser>();
        vu.becomeVision(timeInFuture, visionDuration, timeUser.getLastFrameInfo(), this);
        if (PLAY_VISION_SOUND) {
            if (visionSound != null && VisionUser.abilityActive) {
                SoundManager.instance.playSFX(visionSound);
            }
        }
        return vision;
    }
    /* Automatically sets visionDuration to timeInFuture. */
    public GameObject createVision(float timeInFuture) {
        return createVision(timeInFuture, timeInFuture);
    }

    /* Tells all the visions created to immediately fade out.
     * This is useful if this gameObject died while a vision is still being displayed. */
    public void cutVisions() {
        foreach (VisionUser vu in visionsMade) {
            vu._time = Mathf.Max(vu.time, vu.duration - vu.fadeInDuration);
        }
    }

    /* When creating a vision, this function is called after the vision gameObject is instansiated.
     * currentFI parameter is used to help make this vision be as close to the original as possible.
     * This should only be called by createVision().
     * Becoming a vision is permenant. */
    public void becomeVision(float timeInFuture, float visionDuration, FrameInfo currentFI, VisionUser visionCreator) {
        if (isVision) return;
        _isVision = true;

        _createdWhenAbilityDeactivated = !VisionUser.abilityActive;
        
        //be more like the original
        if (currentFI != null) {
            timeUser.revertToFrameInfoUnsafe(currentFI);
        }

        _duration = visionDuration;

        //color diffently
        spriteRenderer.material = material;
        setSpriteRendererAlpha();

        //set collider layer
        gameObject.layer = LayerMask.NameToLayer("Visions");
        for (int i=0; i<gameObject.transform.childCount; i++){
            GameObject child = gameObject.transform.GetChild(i).gameObject;
            child.layer = LayerMask.NameToLayer("Visions");
        }

        //add vision to list
        if (visionCreator != null) {
            visionCreator.visionsMade.Add(this);
        }
        this.visionCreator = visionCreator;

        //call TimeSkip
        if (timeInFuture > 0) {
            SendMessage("TimeSkip", timeInFuture, SendMessageOptions.DontRequireReceiver);
        }

    }

    /// <summary>
    /// Immediately converts a gameObject into a vision.  Useful for when visions spawn other visions(like bullets)
    /// </summary>
    /// <param name="visionDuration">How long the gameObject will be a vision</param>
    /// <param name="visionCreator">Creator of the vision (can be null).  Note that when a creator calls cutVisions(), this vision will be cut too.</param>
    public void becomeVisionNow(float visionDuration, VisionUser visionCreator = null) {
        becomeVision(0, visionDuration, null, visionCreator);
    }

    /* Math helper:
     * An event happens every eventPeriod seconds.  For each event, we want a vision to be created visionDuration seconds before it.
     * If time was timePreviousFrame the previous frame, and is currently timeThisFrame, should we create a vision? */
    public bool shouldCreateVisionThisFrame(float timePreviousFrame, float timeThisFrame, float eventPeriod, float visionDuration) {
        float dt = timeThisFrame - timePreviousFrame;
        float firstVisionTime = -visionDuration + eventPeriod * (Mathf.Floor(visionDuration / eventPeriod) + 1); // time the first vision should happen after t=0.  Every following vision should happen at eventPeriod intervals
        timeThisFrame -= firstVisionTime;
        timeThisFrame = Utilities.fmod(timeThisFrame, eventPeriod);
        return (timeThisFrame >= 0 && timeThisFrame - dt < 0);
    }
    /* Math helper:
     * An event happens every eventPeriod seconds.
     * If time was timePreviousFrame the previous frame, and is currently timeThisFrame, should we trigger the event? */
     public bool shouldHaveEventThisFrame(float timePreviousFrame, float timeThisFrame, float eventPeriod) {
        float dt = timeThisFrame - timePreviousFrame;
        timeThisFrame = Utilities.fmod(timeThisFrame, eventPeriod);
        return (timeThisFrame >= 0 && timeThisFrame - dt < 0);
    }

    /////////////
    // PRIVATE //
    /////////////

    private static bool _abilityActive = true;
    private static List<VisionUser> allVisionUsers = new List<VisionUser>();

	void Awake() {
        allVisionUsers.Add(this);
		Transform sot = this.transform.Find("spriteObject");
        if (sot == null) {
            spriteObject = gameObject;
            spriteRenderer = this.GetComponent<SpriteRenderer>();
        } else {
            spriteObject = sot.gameObject;
            spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        }
        Debug.Assert(spriteRenderer != null);
        timeUser = GetComponent<TimeUser>();
	}
	
	void Update() {

        setSpriteRendererAlpha(); //called before flashback check because fades out during a flashback

        if (timeUser.shouldNotUpdate)
            return;
        if (!isVision)
            return;
        
        _time += Time.deltaTime;
        if (time >= duration) {
            timeUser.timeDestroy();
        }
	}

    void OnDestroy() {
        allVisionUsers.Remove(this);
        if (visionCreator != null) {
            visionCreator.visionsMade.Remove(this);
            visionCreator = null;
        }
        while (visionsMade.Count > 0) {
            VisionUser vu = visionsMade[0];
            visionsMade.Remove(vu);
            GameObject.Destroy(vu);
        }
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["visionTime"] = time;
        fi.floats["visionDuration"] = duration;
    }
    void OnRevert(FrameInfo fi) {
        _time = fi.floats["visionTime"];
        _duration = fi.floats["visionDuration"];
    }
    void OnTimeDestroy() {
        cutVisions();
    }

    void setSpriteRendererAlpha() {
        if (!isVision)
            return;
        float a = baseAlpha;
        if (timeUser != null && !timeUser.exists){
            a = 0;
        } else if (createdWhenAbilityDeactivated) {
            a = 0;
        } else if (TimeUser.reverting) {
            //should be invisible while reverting
            a = Utilities.easeLinearClamp(TimeUser.revertingTime, baseAlpha, flashbackAlpha - baseAlpha, fadeInDuration);
        } else {
            //normally should be visible, except when fading in at the start and end
            if (duration - time < fadeInDuration) {
                a = Utilities.easeLinearClamp(duration - time, 0, baseAlpha, fadeInDuration);
            } else {
                a = Utilities.easeLinearClamp(time, 0, baseAlpha, fadeInDuration);
            }
        }

        float flickerT = time - flickerPeriod * Mathf.Floor(time / flickerPeriod);
        a *= Utilities.easeOutQuad(flickerT, flickerAlpha, 1 - flickerAlpha, flickerPeriod);

        spriteRenderer.color = new Color(1, 1, 1, a);
    }

    bool _isVision = false;
    List<VisionUser> visionsMade = new List<VisionUser>();
    VisionUser visionCreator;
    private float _time = 0;
    private float _duration = 1.0f; // how long the vision lasts before diappearing
    private bool _createdWhenAbilityDeactivated = false;
	
	// components
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    TimeUser timeUser;

    public class StateQueue {

        /// <summary>
        /// Pass in a unique id to separate it from other StateStacks when saving in TimeUser
        /// </summary>
        public StateQueue(int uniqueID = 0) {
            this.uniqueID = uniqueID;
        }

        public void addState(int state, float duration, float x, float y, bool shouldCreateVision, float[] floatVals=null) {
            StateObject stateObject = new StateObject();
            stateObject.state = state;
            stateObject.duration = duration;
            stateObject.x = x;
            stateObject.y = y;
            stateObject.shouldCreateVision = shouldCreateVision;
            if (floatVals != null) {
                stateObject.floatVals = new float[floatVals.Length];
                for (int i=0; i<floatVals.Length; i++) {
                    stateObject.floatVals[i] = floatVals[i];
                }
            }
            stateObjects.Add(stateObject);
            queueDurationSum += stateObject.duration;
        }

        /// <summary>
        /// Manually removes a state from the queue.  Not recommended.
        /// </summary>
        public void removeState(int queueIndex) {
            if (queueIndex < 0 || queueIndex >= stateObjects.Count) return;
            queueDurationSum -= stateObjects[queueIndex].duration;
            stateObjects.RemoveAt(queueIndex);
        }

        /// <summary>
        /// Returns if a vision should be created this frame.  It's expected that incrementTime() is called before calling this function.
        /// It's true if the time an attack happens - visionDuration is in (this.time-deltaTime, this.time]
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="visionDuration"></param>
        /// <returns>-1 if should not create vision.  If should, then return is the queueIndex of the event that has the vision </returns>
        public int shouldCreateVisionThisFrame(float deltaTime, float visionDuration) {
            float totalDuration = 0;
            for (int i=0; i<stateObjects.Count; i++) {
                StateObject so = stateObjects[i];
                if (so.shouldCreateVision) {
                    if (time - deltaTime < totalDuration - visionDuration && totalDuration - visionDuration <= time) {
                        return i;
                    }
                }
                totalDuration += so.duration;
            }
            return -1;
        }

        /// <summary>
        /// time is always 0 if queue is empty
        /// </summary>
        public float time { get; private set; }
        public bool empty {  get { return queueCount == 0; } }
        public int queueCount {  get { return stateObjects.Count; } }
        public float queueDurationSum { get; private set; }
        public float planAheadDuration {  get { return Mathf.Max(0, queueDurationSum - time); } }
        public int getState(int queueIndex=0) {
            if (queueIndex < 0 || queueIndex >= stateObjects.Count) return -1;
            return stateObjects[queueIndex].state;
        }
        public float getDuration(int queueIndex = 0) {
            if (queueIndex < 0 || queueIndex >= stateObjects.Count) return -1;
            return stateObjects[queueIndex].duration;
        }
        public float getX(int queueIndex = 0) {
            if (queueIndex < 0 || queueIndex >= stateObjects.Count) return -1;
            return stateObjects[queueIndex].x;
        }
        public float getY(int queueIndex = 0) {
            if (queueIndex < 0 || queueIndex >= stateObjects.Count) return -1;
            return stateObjects[queueIndex].y;
        }
        public bool getShouldCreateVision(int queueIndex = 0) {
            if (queueIndex < 0 || queueIndex >= stateObjects.Count) return false;
            return stateObjects[queueIndex].shouldCreateVision;
        }
        public int getFloatValsLength(int queueIndex = 0) {
            if (queueIndex < 0 || queueIndex >= stateObjects.Count) return -1;
            return stateObjects[queueIndex].floatVals.Length;
        }
        public float getFloatVal(int floatValIndex, int queueIndex = 0) {
            if (queueIndex < 0 || queueIndex >= stateObjects.Count) return -1;
            return stateObjects[queueIndex].floatVals[floatValIndex];
        }
        /// <summary>
        /// Increments time by given amount, and pops states off queue as necessary.  Should be called in update, with additionalTime being Time.deltaTime
        /// </summary>
        public void incrementTime(float additionalTime) {
            time += additionalTime;
            while (stateObjects.Count > 0 && time >= stateObjects[0].duration) {
                time -= stateObjects[0].duration;
                // pop state from queue
                removeState(0);
            }
            if (stateObjects.Count == 0)
                time = 0;
        }
        /// <summary>
        /// Gets the number of states that would be popped off the queue if the given time was added
        /// </summary>
        public int numStatesPoppedByIncrementingTime(float additionalTime) {
            int count=0;
            float totalTime = time + additionalTime;
            for (int i=0; i<stateObjects.Count; i++) {
                totalTime -= stateObjects[i].duration;
                if (totalTime < 0)
                    return count;
                count++;
            }
            return count;
        }

        /// <summary>
        /// Call within TimeUser's OnSaveFrame()
        /// </summary>
        public void OnSaveFrame(FrameInfo fi) {
            string prefix = "ss"+uniqueID+"_";
            fi.floats[prefix + "t"] = time;
            fi.ints[prefix + "qc"] = queueCount;
            for (int i = 0; i < queueCount; i++) {
                string objPrefix = prefix + "q" + i + "_";
                StateObject so = stateObjects[i];
                fi.ints[objPrefix + "s"] = so.state;
                fi.floats[objPrefix + "d"] = so.duration;
                fi.floats[objPrefix + "x"] = so.x;
                fi.floats[objPrefix + "y"] = so.y;
                fi.bools[objPrefix + "scv"] = so.shouldCreateVision;
                if (so.floatVals == null)
                    fi.ints[objPrefix + "fvl"] = 0;
                else {
                    fi.ints[objPrefix + "fvl"] = so.floatVals.Length;
                    for (int j=0; j<so.floatVals.Length; j++) {
                        fi.floats[objPrefix + "fv" + j] = so.floatVals[j];
                    }
                }
            }
        }
        /// <summary>
        /// Call within TimeUser's OnRevert()
        /// </summary>
        public void OnRevert(FrameInfo fi) {
            string prefix = "ss"+uniqueID+"_";
            time = fi.floats[prefix + "t"];
            // resize stateObjects
            int soCount = fi.ints[prefix + "qc"];
            while (stateObjects.Count > soCount) stateObjects.RemoveAt(stateObjects.Count - 1);
            while (stateObjects.Count < soCount) stateObjects.Add(new StateObject());
            // fill stateObjects
            queueDurationSum = 0;
            for (int i=0; i<soCount; i++) {
                string objPrefix = prefix + "q" + i + "_";
                StateObject so = stateObjects[i];
                so.state = fi.ints[objPrefix + "s"];
                so.duration = fi.floats[objPrefix + "d"];
                so.x = fi.floats[objPrefix + "x"];
                so.y = fi.floats[objPrefix + "y"];
                so.shouldCreateVision = fi.bools[objPrefix + "scv"];
                queueDurationSum += so.duration;
                int floatValsLength = fi.ints[objPrefix + "fvl"];
                if (floatValsLength == 0)
                    so.floatVals = null;
                else {
                    so.floatVals = new float[floatValsLength];
                    for (int j=0; j<floatValsLength; j++) {
                        so.floatVals[j] = fi.floats[objPrefix + "fv" + j];
                    }
                }
            }
        }

        int uniqueID = 0;

        List<StateObject> stateObjects = new List<StateObject>();

        protected class StateObject {
            public StateObject() { }
            public int state;
            public float duration;
            public float x;
            public float y;
            public bool shouldCreateVision;
            public float[] floatVals = null;
        }
        

    }

}
