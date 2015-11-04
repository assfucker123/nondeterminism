using UnityEngine;
using System.Collections;

public class SpeedLines : MonoBehaviour {

    public void flashRed() {
        flash(new Color(1, 0, 0, .4f), Color.clear, .1f);
    }
    public void flashHeavyRed() {
        flash(new Color(1, 0, 0, .4f), new Color(1, 0, 0, .5f), .15f);
    }

    public void flash(Color imageColor, Color screenColor, float duration) {
        if (duration <= .001f) return;
        this.imageColor = imageColor;
        this.screenColor = screenColor;
        fadeTime = 0;
        fadeDuration = duration;

        image.color = imageColor;
        image.enabled = true;
        screen.color = screenColor;
        screen.enabled = true;
        Vector3 localScale = new Vector3(1, 1, 1);
        if (timeUser.randomValue() < .5f) localScale.x = -1;
        if (timeUser.randomValue() < .5f) localScale.y = -1;
        rt.localScale = localScale;
    }

    ///////////////

    public void setUp() {
        image.enabled = false;
        screen.enabled = false;
        fadeTime = 0;
        fadeDuration = 0;
    }

    /////////////
    // PRIVATE //
    /////////////

	void Awake() {
        image = GetComponent<UnityEngine.UI.Image>();
        screen = transform.Find("screen").GetComponent<UnityEngine.UI.Image>();
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
            if (!screen.enabled)
                screen.enabled = true;
            Color c = Color.Lerp(imageColor, Color.clear, fadeTime / fadeDuration);
            image.color = c;
            c = Color.Lerp(screenColor, Color.clear, fadeTime / fadeDuration);
            screen.color = c;
        } else {
            if (image.enabled)
                image.enabled = false;
            if (screen.enabled)
                screen.enabled = false;
        }
    }

    void OnSaveFrame(FrameInfo fi) {
        fi.strings["ic"] = TimeUser.colorToString(imageColor);
        fi.strings["sc"] = TimeUser.colorToString(screenColor);
        fi.floats["ft"] = fadeTime;
        fi.floats["fd"] = fadeDuration;
    }

    void OnRevert(FrameInfo fi) {
        imageColor = TimeUser.stringToColor(fi.strings["ic"]);
        screenColor = TimeUser.stringToColor(fi.strings["sc"]);
        fadeTime = fi.floats["ft"];
        fadeDuration = fi.floats["fd"];
        setColor();
    }

    Color imageColor = Color.white;
    Color screenColor = Color.white;
    float fadeTime = 0;
    float fadeDuration = 0;

    // components
    UnityEngine.UI.Image image;
    UnityEngine.UI.Image screen;
    RectTransform rt;
    TimeUser timeUser;
	
}
