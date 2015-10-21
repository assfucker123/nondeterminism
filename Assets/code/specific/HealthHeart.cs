using UnityEngine;
using System.Collections;

public class HealthHeart : MonoBehaviour {

    public Sprite fullSprite;
    public Sprite halfSprite;
    public Sprite emptySprite;

    public Vector2 startPosition = new Vector2(-623, -50);
    public float spacing = 50;

    private float jostleMagnitude = 5f;
    private float jostleDuration = .4f;

    private bool shining = false;

    public State state {
        get {
            return _state;
        }
        set {
            if (value != _state) {
                _state = value;
                switch (state) {
                case State.FULL:
                    image.sprite = fullSprite;
                    break;
                case State.HALF:
                    image.sprite = halfSprite;
                    break;
                case State.EMPTY:
                    image.sprite = emptySprite;
                    break;
                }
            }
        }
    }
    private State _state = State.FULL;

    public enum State {
        FULL,
        HALF,
        EMPTY
    }

    public void setPosition(Vector2 position) {
        rt.anchorMin = new Vector2(.5f, 1);
        rt.anchorMax = new Vector2(.5f, 1);
        rt.anchoredPosition = position;
        centerPos = position;
    }

    public void jostle() {
        time = 0;
        jostleAngle = timeUser.randomValue() * Mathf.PI * 2;
        shining = false;
    }

    public void shine() {
        time = 0;
        shining = true;
    }
	
	void Awake() {
        image = GetComponent<UnityEngine.UI.Image>();
        timeUser = GetComponent<TimeUser>();
        rt = GetComponent<RectTransform>();
	}
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;


        if (time < jostleDuration) {
            time += Time.deltaTime;

            setJostle();
        }
        
	}

    void setJostle() {

        if (TimeUser.reverting) {
            rt.anchoredPosition = centerPos;
            image.color = Color.white;
            return;
        }
        
        //jostling
        float mag = Mathf.Sin(time * 20.0f);
        mag *= jostleMagnitude;
        mag *= Utilities.easeLinearClamp(time, 1, -1, jostleDuration);
        if (shining)
            mag = 0;
        rt.anchoredPosition = centerPos + mag * (new Vector2(Mathf.Cos(jostleAngle), Mathf.Sin(jostleAngle)));

        //setting color
        if (time >= jostleDuration) {
            image.color = Color.white;
        } else {
            float mit = time;
            float p = ReceivesDamage.MERCY_FLASH_PERIOD;
            Color color = ReceivesDamage.MERCY_FLASH_COLOR;
            if (shining) {
                color = Pickup.HEALTH_FLASH_COLOR;
            }
            float t = (mit - p * Mathf.Floor(mit / p)) / p; //t in [0, 1)
            if (t < .5) {
                image.color = Color.Lerp(color, Color.white, t * 2);
            } else {
                image.color = Color.Lerp(Color.white, color, (t - .5f) * 2);
            }
        }

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.floats["time"] = time;
        fi.floats["jostleAngle"] = jostleAngle;
    }
    void OnRevert(FrameInfo fi) {
        time = fi.floats["time"];
        jostleAngle = fi.floats["jostleAngle"];
        setJostle();
    }

    Vector2 centerPos = new Vector2();
    float time = 99999;
    float jostleAngle = 0;

	// components
    UnityEngine.UI.Image image;
    TimeUser timeUser;
    RectTransform rt;
}
