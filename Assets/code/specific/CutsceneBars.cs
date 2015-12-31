using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/* Should not be used when the game is paused. */
public class CutsceneBars : MonoBehaviour {

    public float moveOnDuration = 1.0f;
    public float moveOffDuration = 1.0f;

    public static CutsceneBars instance {  get { return _instance; } }
    
    public enum State {
        OFF,
        MOVING_ON,
        ON,
        MOVING_OFF
    }

    public State state {  get { return _state; } }
    public bool areOn {  get { return state == State.ON; } }
    public bool areOff {  get { return state == State.OFF; } }

    public void moveOn() {
        if (state == State.MOVING_ON || state == State.ON) return;
        time = 0;
        _state = State.MOVING_ON;
    }

    public void moveOff() {
        if (state == State.MOVING_OFF || state == State.OFF) return;
        time = 0;
        _state = State.MOVING_OFF;
    }

    public void moveOnImmediately() {
        topBar.rectTransform.localScale = new Vector3(1, 1, 1);
        bottomBar.rectTransform.localScale = new Vector3(-1, 1, 1);
        _state = State.ON;
    }

    public void moveOffImmediately() {
        topBar.rectTransform.localScale = new Vector3(0, 1, 1);
        bottomBar.rectTransform.localScale = new Vector3(0, 1, 1);
        _state = State.OFF;
    }

	void Awake() {
        if (instance != null) {
            GameObject.Destroy(instance.gameObject);
        }
        _instance = this;
        
        timeUser = GetComponent<TimeUser>();
        topBar = transform.Find("TopBar").GetComponent<Image>();
        bottomBar = transform.Find("BottomBar").GetComponent<Image>();
	}
    
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;
        
        time += Time.deltaTime;

        setBarScales();
	}

    void setBarScales() {
        switch (state) {
        case State.MOVING_ON:
            topBar.rectTransform.localScale = new Vector3(Utilities.easeOutQuadClamp(time, 0, 1, moveOnDuration), 1, 1);
            bottomBar.rectTransform.localScale = new Vector3(Utilities.easeOutQuadClamp(time, 0, -1, moveOnDuration), 1, 1);
            if (time >= moveOnDuration) {
                _state = State.ON;
            }
            break;
        case State.MOVING_OFF:
            topBar.rectTransform.localScale = new Vector3(Utilities.easeOutQuadClamp(time, 1, -1, moveOnDuration), 1, 1);
            bottomBar.rectTransform.localScale = new Vector3(Utilities.easeOutQuadClamp(time, -1, 1, moveOnDuration), 1, 1);
            if (time >= moveOnDuration) {
                _state = State.OFF;
            }
            break;
        }
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["t"] = time;
    }

    void OnRevert(FrameInfo fi) {
        _state = (State)fi.state;
        time = fi.floats["t"];
        setBarScales();
    }

    void OnDestroy() {
        if (instance == this)
            _instance = null;
    }

    private static CutsceneBars _instance = null;

    Image topBar;
    Image bottomBar;
    TimeUser timeUser;

    State _state = State.ON;
    float time = 0;
}
