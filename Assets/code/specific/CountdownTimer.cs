using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CountdownTimer : MonoBehaviour {

    public float time = 0; // in seconds (as usual)
    public Vector2 position = new Vector2(532, -49);
    public Rect dotBox = new Rect(-66, -17, 132, 34);
    public float dotPeriod = 60;
    public Sprite normalSprite;
    public Sprite meltdownSprite;
    public Sprite meltdownPerilSprite;
    public Sprite weirdSprite;
    public Color weirdColor = Color.magenta;
    public float flashPeriodVisible = .5f;
    public float flashPeriodInvisible = .2f;

    public static CountdownTimer instance { get; private set; }
    public static Mode staticMode {
        get {
            return instance == null ? _staticMode : instance.mode;
        }
        set {
            if (instance == null) _staticMode = value;
            else instance.mode = value;
        }
    }
    public static bool staticVisible {
        get {
            return instance == null ? _staticVisible : instance.visible;
        }
        set {
            if (instance == null) _staticVisible = value;
            else instance.visible = value;
        }
    }
    
    public static float MELTDOWN_DURATION {
        get {
            return 30 * 60;
        }
    }
    public static float MELTDOWN_PERIL_DURATION {
        get {
            return 5 * 60;
        }
    }

    public static Color NORMAL_COLOR = new Color(231/255f, 231/255f, 231/255f);
    public static Color MELTDOWN_COLOR = new Color(253/255f, 255/255f, 138/255f);
    public static Color MELTDOWN_PERIL_COLOR = new Color(234/255f, 0, 0);
    
    public enum Mode {
        NORMAL,
        MELTDOWN,
        MELTDOWN_PERIL,
        WEIRD
    }

    public Mode mode {
        get { return _mode; }
        set {
            if (_mode == value) return;
            _mode = value;
            switch (mode) {
            case Mode.NORMAL:
                image.sprite = normalSprite;
                glyphBox.setColor(NORMAL_COLOR);
                break;
            case Mode.MELTDOWN:
                image.sprite = meltdownSprite;
                glyphBox.setColor(MELTDOWN_COLOR);
                break;
            case Mode.MELTDOWN_PERIL:
                image.sprite = meltdownPerilSprite;
                glyphBox.setColor(MELTDOWN_PERIL_COLOR);
                break;
            case Mode.WEIRD:
                image.sprite = weirdSprite;
                glyphBox.setColor(weirdColor);
                break;
            }
            foreach (CountdownTimerDot dot in dots) {
                dot.mode = mode;
            }
        }
    }

    public bool visible {
        get { return _visible; }
        set {
            if (value == visible) return;
            _visible = value;
            image.enabled = visible;
            if (visible) {
                glyphBox.makeAllCharsVisible();
            } else {
                glyphBox.makeAllCharsInvisible();
            }
            foreach (CountdownTimerDot dot in dots) {
                dot.GetComponent<UnityEngine.UI.Image>().enabled = visible;
            }
        }
    }
    public bool flashing {
        get { return _flashing; }
        set {
            if (flashing == value) return;
            _flashing = value;
        }
    }
    /// <summary>
    /// When frozen, time won't increase
    /// </summary>
    public bool frozen {
        get { return _frozen; }
        set { _frozen = value; }
    }

    public float dotBoxPerimeter {
        get {
            return dotBox.width * 2 + dotBox.height * 2;
        }
    }

    public void setUp() {
        //rt.anchorMin = new Vector2(.5f, 1);
        //rt.anchorMax = new Vector2(.5f, 1);
        //rt.anchoredPosition = position;
        foreach (CountdownTimerDot dot in dots) {
            dot.mode = Mode.NORMAL;
        }
        glyphBox.setColor(NORMAL_COLOR);

        Mode tempMode = mode;
        _mode = Mode.NORMAL;
        mode = tempMode;
    }

	void Awake() {
        if (instance != null) {
            GameObject.Destroy(instance.gameObject);
        }
        instance = this;

        image = GetComponent<Image>();
		glyphBox = transform.Find("GlyphBox").GetComponent<GlyphBox>();
        timeUser = GetComponent<TimeUser>();

        dots = transform.GetComponentsInChildren<CountdownTimerDot>();

        mode = _staticMode;
        visible = _staticVisible;
	}

    void Start() {
        if (Vars.currentNodeData != null) {
            time = Vars.currentNodeData.time;
        }
    }
	
	void Update() {
        
        if (timeUser.shouldNotUpdate)
            return;

        if (!frozen) {
            time += Time.deltaTime;
        }
        flashTime += Time.deltaTime;
        displayTime(time);
	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
        fi.floats["ft"] = flashTime;
    }
    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        flashTime = fi.floats["ft"];
        displayTime(time);
    }

    void OnDestroy() {
        if (instance == this) {
            instance = null;
        }
    }

    void displayTime(float time) {
        // detect switching mode from MELTDOWN to MELTDOWN_PERIL
        if (mode == Mode.MELTDOWN && time > MELTDOWN_DURATION - MELTDOWN_PERIL_DURATION) {
            mode = Mode.MELTDOWN_PERIL;
        } else if (mode == Mode.MELTDOWN_PERIL && time <= MELTDOWN_DURATION - MELTDOWN_PERIL_DURATION) {
            mode = Mode.MELTDOWN;
        }
        // displaying time
        string str = timeToStr(time, mode);
        glyphBox.setPlainText(str);
        moveDots(time);
        // visible
        if (visible) {
            // flashing
            if (flashing) {
                bool vis = Utilities.fmod(flashTime, flashPeriodVisible+flashPeriodInvisible) > flashPeriodInvisible;
                if (vis && glyphBox.visibleChars == 0) {
                    glyphBox.makeAllCharsVisible();
                }
                if (!vis && glyphBox.visibleChars > 0) {
                    glyphBox.makeAllCharsInvisible();
                }
            }
        } else {
            glyphBox.makeAllCharsInvisible();
        }
        
    }

    void moveDots(float time) {
        time *= -1; // for reverse direction
        float widthPerim = dotBox.width / dotBoxPerimeter;
        for (int i=0; i<dots.Length; i++) {
            Vector2 pos = new Vector2();
            CountdownTimerDot dot = dots[i];
            float t = time + i * dotPeriod / dots.Length;
            t = Utilities.fmod(t, dotPeriod) / dotPeriod; // t in [0, 1)
            if (t < widthPerim) {
                pos.x = dotBox.x + dotBox.width * (t / widthPerim);
                pos.y = dotBox.y;
            } else if (t < .5f) {
                t -= widthPerim;
                pos.x = dotBox.x + dotBox.width;
                pos.y = dotBox.y + dotBox.height * (t / (.5f - widthPerim));
            } else if (t < .5f + widthPerim) {
                t -= .5f;
                pos.x = dotBox.x + dotBox.width * (1 - t / widthPerim);
                pos.y = dotBox.y + dotBox.height;
            } else {
                t -= widthPerim + .5f;
                pos.x = dotBox.x;
                pos.y = dotBox.y + dotBox.height * (1 - t / (.5f - widthPerim));
            }
            dot.GetComponent<RectTransform>().localPosition = pos;
        }
    }

    public static string timeToStr(float time, Mode mode) {

        string str = "";
        float t = time;
        bool meltdownInvert = false;
        if (mode == Mode.MELTDOWN || mode == Mode.MELTDOWN_PERIL) {
            t = MELTDOWN_DURATION - time;
            if (t < 0) {
                t *= -1;
                meltdownInvert = true;
            }
        }
        int mins = 0;
        int secs = 0;
        if (t <= 0) {
            str = "00:00";
        } else if (t >= 100 * 60) {
            str = "99:59";
        } else {
            mins = Mathf.FloorToInt(t / 60);
            secs = Mathf.FloorToInt(t - mins * 60);
            if (mins < 10) str += "0";
            str += mins + ":";
            if (secs < 10) str += "0";
            str += secs;
        }
        if (mode == Mode.MELTDOWN || mode == Mode.MELTDOWN_PERIL) {
            if (meltdownInvert) {
                str = "+" + str;
            } else {
                str = "-" + str;
            }
            //str = "¦" + str + "¦";
        } else {
            str = "¦" + str + "¦"; // this character maps to a space in the font
        }
        return str;
    }

    bool _visible = true;
    Mode _mode = Mode.NORMAL;
    bool _flashing = false;
    float flashTime = 0;
    bool _frozen = false;

    private static Mode _staticMode = Mode.NORMAL;
    private static bool _staticVisible = true;

    CountdownTimerDot[] dots;

    Image image;
    GlyphBox glyphBox;
    TimeUser timeUser;
}
