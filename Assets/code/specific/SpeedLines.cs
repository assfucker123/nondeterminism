using UnityEngine;
using System.Collections;

public class SpeedLines : MonoBehaviour {

    public void flashRed() {
        flash(new Color(1, 0, 0, .5f), .1f);
    }

    public void flash(Color color, float duration) {
        if (duration <= .001f) return;
        this.color = color;
        fadeTime = 0;
        fadeDuration = duration;

        image.color = color;
        image.enabled = true;
        Vector3 localScale = new Vector3(1, 1, 1);
        if (timeUser.randomValue() < .5f) localScale.x = -1;
        if (timeUser.randomValue() < .5f) localScale.y = -1;
        rt.localScale = localScale;
    }

    ///////////////

    public void setUp() {
        image.enabled = false;
        fadeTime = 0;
        fadeDuration = 0;
    }

    /////////////
    // PRIVATE //
    /////////////

	void Awake() {
        image = GetComponent<UnityEngine.UI.Image>();
        rt = GetComponent<RectTransform>();
        timeUser = GetComponent<TimeUser>();
	}
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        if (fadeTime < fadeDuration) {
            fadeTime += Time.unscaledDeltaTime;

            setColor();
        }

	}

    void setColor() {
        if (fadeTime < fadeDuration) {
            if (!image.enabled)
                image.enabled = true;
            Color c = Color.Lerp(color, Color.clear, fadeTime / fadeDuration);
            image.color = c;
        } else {
            if (image.enabled)
                image.enabled = false;
        }
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.strings["c"] = TimeUser.colorToString(color);
        fi.floats["ft"] = fadeTime;
        fi.floats["fd"] = fadeDuration;
    }

    void OnRevert(FrameInfo fi) {
        color = TimeUser.stringToColor(fi.strings["c"]);
        fadeTime = fi.floats["ft"];
        fadeDuration = fi.floats["fd"];
        setColor();
    }

    Color color = Color.white;
    float fadeTime = 0;
    float fadeDuration = 0;

    // components
    UnityEngine.UI.Image image;
    RectTransform rt;
    TimeUser timeUser;
	
}
