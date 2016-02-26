using UnityEngine;
using System.Collections;

public class Ambush : MonoBehaviour {

    public bool useIncludedSensor = false; // it's preferred to use AmbushTrigger

    public bool activated {  get { return state == State.ACTIVATED; } }

    public enum State {
        NOT_ACTIVE,
        ACTIVATED
    }

    public void activate() {
        if (activated) return;

        Debug.Log("WORK HERE on ambush");

        state = State.ACTIVATED;
    }

	void Awake() {
        timeUser = GetComponent<TimeUser>();
	}
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;
	}

    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
    }

    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
    }

    void OnTriggerEnter2D(Collider2D c2d) {
        if (!useIncludedSensor) return;
        if (c2d.gameObject == null) return;
        if (c2d.gameObject != Player.instance.gameObject) return;

        activate();
    }

    TimeUser timeUser;

    State state = State.NOT_ACTIVE;
}
