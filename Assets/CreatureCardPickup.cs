using UnityEngine;
using System.Collections;

public class CreatureCardPickup : MonoBehaviour {

    public string creature = "";
    public float circlePeriod = 1.0f;
    public float circleRadius = .2f;
    public Material defaultMaterial;
    public Material grayscaleMaterial;

    public enum State {
        NOT_FOUND,
        FOUND,
        COLLECTED
    }

    public State state {
        get {
            return _state;
        }
        set {
            if (value == _state) return;

            if (value == State.NOT_FOUND) {
                animator.Play("default");
            }
            if (_state == State.NOT_FOUND) {
                animator.Play("found");
            }
            if (value == State.COLLECTED) {
                spriteRenderer.material = grayscaleMaterial;
            }
            if (_state == State.COLLECTED) {
                spriteRenderer.material = defaultMaterial;
            }

            _state = value;
        }
    }

	void Awake() {
        timeUser = GetComponent<TimeUser>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
	}

    void Start() {
        startPos = transform.localPosition;
        creatureID = CreatureCard.getIDFromCardName(creature);
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        time += Time.deltaTime;

        setPosition();


	}

    void setPosition() {
        if (!timeUser.exists) return;
        float angle = time / circlePeriod * Mathf.PI*2;
        Vector3 diff = new Vector2(Mathf.Cos(angle) * 0, Mathf.Sin(angle));
        diff *= circleRadius;
        transform.localPosition = startPos + diff;
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["t"] = time;
    }

    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["t"];
        setPosition();
    }

    void OnTriggerEnter2D(Collider2D c2d) {
        if (timeUser.shouldNotUpdate)
            return;
        if (c2d.gameObject != Player.instance.gameObject)
            return;
        if (Vars.currentNodeData == null || Vars.currentNodeData.creatureCardCollected(creatureID))
            return;

        Debug.Log("collect creature card.  WORK HERE");
        
    }

    Vector3 startPos = new Vector3();

    float time = 0;
    int creatureID = 0;
    State _state = State.NOT_FOUND;

    TimeUser timeUser;
    Animator animator;
    SpriteRenderer spriteRenderer;

}
