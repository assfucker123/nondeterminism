using UnityEngine;
using System.Collections;

public class Pengrunt : MonoBehaviour {



    public enum State {
        IDLE,
        WALK, //walk for set duration (short or long)
        PRE_SPRAY, //should be a short duration.  Possibly turn around?
        SPRAY //pushed backwards during this
    }

    public float idleDuration = .5f;
    public float walkShortDuration = .3f;
    public float walkLongDuration = .8f;
    public float preSprayDuration = .2f;
    public float sprayDuration = .7f;

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        spriteObject = this.transform.Find("spriteObject").gameObject;
        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
        animator = spriteObject.GetComponent<Animator>();
        colFinder = GetComponent<ColFinder>();
        timeUser = GetComponent<TimeUser>();
        receivesDamage = GetComponent<ReceivesDamage>();
    }

	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // components
    Rigidbody2D rb2d;
    GameObject spriteObject;
    SpriteRenderer spriteRenderer;
    Animator animator;
    ColFinder colFinder;
    TimeUser timeUser;
    ReceivesDamage receivesDamage;

}
