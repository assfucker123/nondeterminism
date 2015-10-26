using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CountdownTimer : MonoBehaviour {

    public float time = 0; // in seconds (as usual)
    public bool countUp = false;
    public Format format = Format.SECONDS;
    public Vector2 position = new Vector2(532, -49);
    
    public enum Format {
        SECONDS,
        CENTISECONDS
    }

    public bool visible {
        get { return _visible; }
        set {
            if (value == visible) return;
            _visible = value;
            text.enabled = visible;
            textDrop.enabled = visible;
        }
    }

    public void setUp() {
        rt.anchorMin = new Vector2(.5f, 1);
        rt.anchorMax = new Vector2(.5f, 1);
        rt.anchoredPosition = position;
        if (Vars.arcadeMode) {
            visible = false;
        }
    }

	void Awake() {
        rt = GetComponent<RectTransform>();
		text = transform.Find("Text").GetComponent<Text>();
        textDrop = transform.Find("TextDrop").GetComponent<Text>();
        timeUser = GetComponent<TimeUser>();
	}
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        if (countUp)
            time += Time.deltaTime;
        else
            time -= Time.deltaTime;
        displayTime(time);
	}

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["t"] = time;
    }
    void OnRevert(FrameInfo fi) {
        time = fi.floats["t"];
        displayTime(time);
    }

    void displayTime(float time) {
        string str = timeToStr(time);
        text.text = str;
        textDrop.text = str;
    }

    string timeToStr(float time) {
        
        if (time < 0){
            switch (format) {
            case Format.SECONDS: return "00:00";
            case Format.CENTISECONDS: return "00:00.00";
            }
        }
        if (time >= 100*60){
            switch (format) {
            case Format.SECONDS: return "99:99";
            case Format.CENTISECONDS: return "99:99.99";
            }
        }

        string str = "";
        int mins = Mathf.FloorToInt(time / 60);
        time -= mins * 60;
        int secs = Mathf.FloorToInt(time);
        time -= secs;
        int centiseconds = Mathf.FloorToInt(time * 100);
        if (mins < 10)
            str = "0" + mins;
        else
            str = "" + mins;
        str += ":";
        if (secs < 10)
            str += "0" + secs;
        else
            str += "" + secs;
        if (format == Format.CENTISECONDS) {
            str += ".";
            if (centiseconds < 10)
                str += "0" + centiseconds;
            else
                str += "" + centiseconds;
        }
        return str;
    }

    bool _visible = true;

	// components
    RectTransform rt;
    Text text;
    Text textDrop;
    TimeUser timeUser;
}
