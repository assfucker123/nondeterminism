using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {

    public Type type = Type.HEALTH;
    public float amount = 0;
    public float hopSpeed = 50f;
    public float hopRandomOffset = .1f;
    public float friction = 50f;
    public float spinMaxSpeed = 300f;

    public enum Type {
        HEALTH,
        PHASE
    }

    public bool pickedUp { get { return _pickedUp; } }

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

        rb2d.velocity = v;

	}

    void OnTriggerEnter2D(Collider2D c2d) {
        if (pickedUp) return;
        if (timeUser.shouldNotUpdate) return;
        if (c2d.gameObject == null) return;
        Player plr = c2d.gameObject.GetComponent<Player>();
        if (plr == null) return;

        switch (type) {
        case Type.HEALTH:
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
    }
    void OnRevert(FrameInfo fi) {
        bool prevPickedUp = pickedUp;
        spinSpeed = fi.floats["spinSpeed"];
        _pickedUp = fi.bools["pickedUp"];

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
	
	// components
    Rigidbody2D rb2d;
    SpriteRenderer spriteRenderer;
    ColFinder colFinder;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;
}
