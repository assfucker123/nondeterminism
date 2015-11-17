using UnityEngine;
using System.Collections;

/* Spawns trails, using the sprite in the spriteRenderer attached to this gameObject or a child called spriteObject
 * */

public class SpriteTrailSpawner : MonoBehaviour {

    public GameObject spriteTrailGameObject;
    public float trailDuration = .2f;
    public float period = .1f;
    public bool trailFadeOut = true;
    public bool startActivated = false;

    public bool activated { get { return _activated; } }
    public void activate() {
        if (activated) return;
        _activated = true;
        time = trailDuration;
    }
    public void deactivate() {
        if (!activated) return;
        _activated = false;
    }

	
	void Awake() {
        timeUser = GetComponent<TimeUser>();
        //visionUser = GetComponent<VisionUser>();
        Transform sot = transform.Find("spriteObject");
        if (sot == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
            usingSpriteObject = false;
        } else {
            spriteRenderer = sot.GetComponent<SpriteRenderer>();
            usingSpriteObject = true;
        }
	}
    void Start() {
        if (startActivated) {
            activate();
        } else {
            deactivate();
        }
    }
	
	void Update() {

        if (timeUser != null && timeUser.shouldNotUpdate)
            return;

        if (activated) {
            time += Time.deltaTime;
            if (time >= period) {
                spawnTrail();
                time -= period;
            }
        }

	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["time"] = time;
        fi.bools["a"] = activated;
    }
    void OnRevert(FrameInfo fi) {
        time = fi.floats["time"];
        _activated = fi.bools["a"];
    }

    public void spawnTrail() {

        Vector3 pos = transform.localPosition;
        Vector3 scale = transform.localScale;
        float rot = Utilities.get2DRot(transform.localRotation);
        if (usingSpriteObject) {
            pos = transform.TransformPoint(spriteRenderer.transform.localPosition);
            scale.x *= spriteRenderer.transform.localScale.x;
            scale.y *= spriteRenderer.transform.localScale.y;
            rot += Utilities.get2DRot(spriteRenderer.transform.localRotation);
        }
        
        GameObject GO = GameObject.Instantiate(spriteTrailGameObject, pos, Utilities.setQuat(rot)) as GameObject;
        GO.transform.SetParent(transform.parent, false);
        GO.transform.localScale = scale;
        SpriteRenderer stsr = GO.GetComponent<SpriteRenderer>();
        stsr.sprite = spriteRenderer.sprite;
        stsr.material = spriteRenderer.material;
        stsr.color = spriteRenderer.color;
        VisualEffect stve = GO.GetComponent<VisualEffect>();
        stve.duration = trailDuration;
        stve.fadeOut = trailFadeOut;
    }

    TimeUser timeUser;
    //VisionUser visionUser;
    SpriteRenderer spriteRenderer = null;
    bool usingSpriteObject = false;
    float time = 0;
    bool _activated = false;

}
