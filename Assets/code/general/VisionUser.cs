using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    public static float VISION_DURATION = 1.0f;

    public bool isVision { get { return _isVision; } }
    public float fadeInDuration = .2f; //how long for the vision to fade in
    public float time { get { return _time; } } //how long this visionUser has been a vision
    public float duration { get { return _duration; } } //how long this visionUser will be a vision for
    
    public Material material;

    /* Creates a clone of this gameObject, converted into a vision. */
    public GameObject createVision(float timeInFuture, float visionDuration) {
        if (isVision) return null; //visions cannot create visions of themselves
        if (timeUser.getLastFrameInfo() == null) //this should only happen if this hasn't existed for more than 1 frame
            return null;
        GameObject vision = GameObject.Instantiate(gameObject) as GameObject;
        VisionUser vu = vision.GetComponent<VisionUser>();
        vu.becomeVision(timeInFuture, visionDuration, timeUser.getLastFrameInfo(), this);
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

        _duration = visionDuration;

        //be more like the original
        if (currentFI != null) {
            timeUser.revertToFrameInfoUnsafe(currentFI);
        }

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

    /* Immediately converts a gameObject into a vision.
     * Useful for when visions spawn other visions (like bullets) */
    public void becomeVisionNow(float visionDuration, VisionUser visionCreator) {
        becomeVision(0, visionDuration, null, visionCreator);
    }

	void Awake() {
		rb2d = GetComponent<Rigidbody2D>();
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
        Debug.Assert(timeUser != null);
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
        float a = 1;
        if (timeUser != null && !timeUser.exists){
            a = 0;
        } else if (TimeUser.reverting){
            //should be invisible while reverting
            a = Utilities.easeLinearClamp(TimeUser.revertingTime, 1, -1, fadeInDuration);
        } else {
            //normally should be visible, except when fading in at the start and end
            if (duration - time < fadeInDuration) {
                a = Utilities.easeLinearClamp(duration - time, 0, 1, fadeInDuration);
            } else {
                a = Utilities.easeLinearClamp(time, 0, 1, fadeInDuration);
            }
        }
        spriteRenderer.color = new Color(1, 1, 1, a);
    }

    bool _isVision = false;
    List<VisionUser> visionsMade = new List<VisionUser>();
    VisionUser visionCreator;
    private float _time = 0;
    private float _duration = 1.0f; // how long the vision lasts before diappearing
	
	// components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    TimeUser timeUser;
}
