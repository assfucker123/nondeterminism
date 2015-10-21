using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pickup : MonoBehaviour {

    public Type type = Type.HEALTH;
    public float amount = 0;
    public float hopSpeed = 7f;
    public float hopRandomOffset = .1f;
    public float friction = 0f;
    public float spinMaxSpeed = 400f;
    public float fadeDuration = 6.5f; //how long until pickup disappears
    public float fadeMercyDuration = 2.5f; //how long before pickup disappears will it start flashing

    public enum Type {
        HEALTH,
        PHASE
    }

    public bool pickedUp { get { return _pickedUp; } }
    public bool faded { get { return _faded; } }

    public static Color HEALTH_FLASH_COLOR = new Color(.79f, 0, .79f);
    public static float HEALTH_FLASH_DURATION = .5f;
    public static Color PHASE_FLASH_COLOR = new Color(0, .58f, 1);
    public static float PHASE_FLASH_DURATION = .3f;

    void Awake() {
		rb2d = GetComponent<Rigidbody2D>();
        GameObject spriteObject = this.transform.Find("spriteObject").gameObject;
        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        colFinder = GetComponent<ColFinder>();
        timeUser = GetComponent<TimeUser>();
	}

    void Start() {
        setSpinSpeed();
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;
        if (pickedUp || faded)
            return;

        Vector2 v = rb2d.velocity;

        //friction
        if (v.x > 0) {
            v.x = Mathf.Max(0, v.x - friction * Time.deltaTime);
        } else {
            v.x = Mathf.Min(0, v.x + friction * Time.deltaTime);
        }

        //hopping
        if (colFinder.hitBottom) {
            v.y = hopSpeed * (1 + (timeUser.randomValue()*2-1) * hopRandomOffset);
            setSpinSpeed();
        }

        //spinning
        spriteRenderer.transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime);

        //fading
        fadeTime += Time.deltaTime;
        if (fadeTime > fadeDuration - fadeMercyDuration) {
            bool visible = (fadeTime - Mathf.Floor(fadeTime / .1f) * .1f > .05f);
            if (visible) {
                spriteRenderer.color = Color.white;
            } else {
                spriteRenderer.color = new Color(1, 1, 1, 0);
            }
        } else {
            if (spriteRenderer.color != Color.white)
                spriteRenderer.color = Color.white;
        }

        rb2d.velocity = v;

        if (fadeTime >= fadeDuration) {
            _faded = true;
            timeUser.timeDestroy();
        }

	}

    void OnTriggerEnter2D(Collider2D c2d) {
        if (pickedUp || faded) return;
        if (timeUser.shouldNotUpdate) return;
        if (c2d.gameObject == null) return;
        Player plr = c2d.gameObject.GetComponent<Player>();
        if (plr == null) return;

        switch (type) {
        case Type.HEALTH:
            if (plr.health >= plr.maxHealth)
                return;
            plr.healthPickup(Mathf.RoundToInt(amount));
            break;
        case Type.PHASE:
            plr.phasePickup(amount);
            break;
        }

        _pickedUp = true;

        timeUser.timeDestroy();
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["spinSpeed"] = spinSpeed;
        fi.bools["pickedUp"] = pickedUp;
        fi.floats["fadeTime"] = fadeTime;
        fi.bools["faded"] = faded;
        fi.strings["color"] = TimeUser.colorToString(spriteRenderer.color);
    }
    void OnRevert(FrameInfo fi) {
        bool prevPickedUp = pickedUp;
        spinSpeed = fi.floats["spinSpeed"];
        _pickedUp = fi.bools["pickedUp"];
        fadeTime = fi.floats["fadeTime"];
        _faded = fi.bools["faded"];
        spriteRenderer.color = TimeUser.stringToColor(fi.strings["color"]);

        if (prevPickedUp && !pickedUp && type == Type.PHASE) {
            //went back before phase was picked up, need to remove the phase gained
            //(phase is special because it does not go back to a previous value when reverting)
            Player.instance.revertBeforePhasePickup(amount);
        }
    }

    void setSpinSpeed() {
        spinSpeed = (timeUser.randomValue() * 2 - 1) * spinMaxSpeed;
    }

    float spinSpeed = 0;
    bool _pickedUp = false;
    float fadeTime = 0;
    bool _faded = false;
    
	
	// components
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;
    ColFinder colFinder;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;
}
