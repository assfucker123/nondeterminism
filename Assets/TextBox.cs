using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/* IMPORTANT: PLAYER SHOULD NOT BE ABLE TO MOVE TO ANOTHER LEVEL WHILE TEXT IS ON THE SCREEN.
 * IT'S IMPOSSIBLE FOR THE TEXT TO PRESERVE IN THE NEXT SCENE BECAUSE OF HOW TIMEUSER WORKS.
 * SOLUTION: PLAYER SHOULD ALWAYS BE STILL OR LOCKED IN AN AREA WHILE TEXTBOX IS VISIBLE. */

public class TextBox : MonoBehaviour {

    ////////////
    // STATIC //
    ////////////

    public static TextBox instance {  get { return _instance; } }

    ////////////
    // PUBLIC //
    ////////////

    public GlyphBox messageBox {  get { return _messageBox; } }
    public GlyphBox nameBox {  get { return _nameBox; } }

    public enum State {
        CLOSED,
        OPENING,
        OPEN,
        CLOSING
    }

    public State state {  get { return _state; } }
    public bool isOpen {  get { return state == State.OPEN; } }
    public bool isClosed { get { return state == State.CLOSED; } }

    public bool doneDisplaying {  get { return _doneDisplaying; } }

    public void open(bool clear = true) {
        if (state == State.OPEN || state == State.OPENING)
            return;
        allVisible();
        animator.Play("open");
        _state = State.OPENING;
        time = 0;

        if (clear) {
            this.clear();
        }
    }

    public void close() {
        if (state == State.CLOSED || state == State.CLOSING)
            return;
        animator.Play("close");
        clear();
        _state = State.CLOSING;
        time = 0;
    }

    public void closeImmediately() {
        allInvisible();
        _state = State.CLOSED;
    }

    public void setProfile(string profile) {
        profileAnimator.Play(profile);
    }
    public string currentProfile {  get { return _currentProfile; } }

    public void clear() {
        setProfile("none");
        messageBox.setPlainText("");
        nameBox.setPlainText("");
        continueImage.enabled = false;
    }

    public void displayText(string name, string formattedText) {
        _doneDisplaying = false;
        textIndex = 0;

        nameBox.setPlainText(name);
        profileImage.enabled = true;
        messageBox.setText(formattedText, 0);
        continueImage.enabled = false;

    }

    /////////////
    // PRIVATE //
    /////////////

	void Awake() {
        if (instance != null) {
            GameObject.Destroy(gameObject);
            return;
        }
        _instance = this;
        timeUser = GetComponent<TimeUser>();
        image = GetComponent<Image>();
        animator = GetComponent<Animator>();
        _messageBox = transform.Find("Message").GetComponent<GlyphBox>();
        _nameBox = transform.Find("Name").GetComponent<GlyphBox>();
        profileAnimator = transform.Find("Profile").GetComponent<Animator>();
        profileImage = profileAnimator.GetComponent<Image>();
        continueImage = transform.Find("Continue").GetComponent<Image>();
    }
	
	void Update() {

        if (timeUser.shouldNotUpdate && !PauseScreen.paused)
            return;

        if (PauseScreen.paused) {
            time += Time.unscaledDeltaTime;
        } else {
            time += Time.deltaTime;
        }

        switch (state) {
        case State.OPENING:
            if (time >= openDuration) {
                _state = State.OPEN;
                time = 0;
            }
            break;
        case State.CLOSING:
            if (time >= closeDuration) {
                closeImmediately();
            }
            break;
        }
        
        if (doneDisplaying) {
            if (isOpen) {

            }
        } else {

            if (Keys.instance.confirmPressed) {
                speedyText = true;
            }

            float speed = textDisplaySpeed;
            if (speedyText)
                speed = textFastDisplaySpeed;
            
            if (PauseScreen.paused) {
                textIndex += Time.unscaledDeltaTime * speed;
            } else {
                textIndex += Time.deltaTime * speed;
            }

            if (textIndex > messageBox.totalChars) {
                // finished displaying text
                _doneDisplaying = true;
                speedyText = false;
                continueImage.enabled = true;
                textIndex = messageBox.totalChars;
            }
            messageBox.visibleChars = Mathf.FloorToInt(textIndex);
        }

    }

    void OnSaveFrame(FrameInfo fi) {

        fi.state = (int)state;
        fi.floats["t"] = time;
        fi.ints["profileFullPathHash"] = profileAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        fi.floats["profileNormalizedTime"] = profileAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        fi.strings["profile"] = currentProfile;
        fi.bools["ie"] = image.enabled;
        fi.bools["pie"] = profileImage.enabled;
        fi.bools["cie"] = continueImage.enabled;
        fi.bools["dd"] = doneDisplaying;
        fi.floats["ti"] = textIndex;
        fi.bools["st"] = speedyText;

    }

    void OnRevert(FrameInfo fi) {

        _state = (State)fi.state;
        time = fi.floats["t"];
        profileAnimator.Play(fi.ints["profileFullPathHash"], 0, fi.floats["profileNormalizedTime"]);
        _currentProfile = fi.strings["profile"];
        image.enabled = fi.bools["ie"];
        profileImage.enabled = fi.bools["pie"];
        continueImage.enabled = fi.bools["cie"];
        _doneDisplaying = fi.bools["dd"];
        textIndex = fi.floats["ti"];
        speedyText = fi.bools["st"];

    }

    void OnDestroy() {
        if (_instance == this)
            _instance = null;
    }

    public float openDuration = .134f;
    public float closeDuration = .134f;
    public float textDisplaySpeed = 20;
    public float textFastDisplaySpeed = 60;

    void allVisible() {
        image.enabled = true;
        messageBox.makeAllCharsVisible();
        nameBox.makeAllCharsVisible();
        profileImage.enabled = true;
        continueImage.enabled = true;
    }

    void allInvisible() {
        image.enabled = false;
        messageBox.makeAllCharsInvisible();
        nameBox.makeAllCharsInvisible();
        profileImage.enabled = false;
        continueImage.enabled = false;
    }

    State _state = State.CLOSED;
    string _currentProfile;
    float time = 0;
    bool _doneDisplaying = true;
    float textIndex = 0;
    bool speedyText = false;

    TimeUser timeUser;
    Image image;
    Animator animator;
    GlyphBox _messageBox;
    GlyphBox _nameBox;
    Animator profileAnimator;
    Image profileImage;
    Image continueImage;

    private static TextBox _instance = null;



}
