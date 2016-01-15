using UnityEngine;
using System.Collections;

public class SpeedLines : MonoBehaviour {

    public void flashRed() {
        flash(new Color(1, 0, 0, .4f), new Color(1, 0, 0, .2f), .1f);
    }
    public void flashHeavyRed() {
        flash(new Color(1, 0, 0, .4f), new Color(1, 0, 0, .5f), .15f);
    }
    public void flashWhite() {
        flash(Color.clear, new Color(1, 1, 1, .3f), .15f, 2);
    }

    public void flash(Color imageColor, Color screenColor, float duration, int numFlashes = 1) {
        if (duration <= .001f) return;
        this.imageColor = imageColor;
        this.screenColor = screenColor;
        fadeTime = 0;
        fadeDuration = duration;
        count = 0;
        this.numFlashes = numFlashes;

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

            if (fadeTime >= fadeDuration) {
                count++;
                if (count < numFlashes)
                    fadeTime -= fadeDuration;
            }

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
        fi.floats["icr"] = imageColor.r;
        fi.floats["icg"] = imageColor.g;
        fi.floats["icb"] = imageColor.b;
        fi.floats["ica"] = imageColor.a;
        fi.floats["scr"] = screenColor.r;
        fi.floats["scg"] = screenColor.g;
        fi.floats["scb"] = screenColor.b;
        fi.floats["sca"] = screenColor.a;
        fi.floats["ft"] = fadeTime;
        fi.floats["fd"] = fadeDuration;
        fi.ints["c"] = count;
        fi.ints["nf"] = numFlashes;
    }

    void OnRevert(FrameInfo fi) {
        imageColor = new Color(fi.floats["icr"], fi.floats["icg"], fi.floats["icb"], fi.floats["ica"]);
        screenColor = new Color(fi.floats["scr"], fi.floats["scg"], fi.floats["scb"], fi.floats["sca"]);
        fadeTime = fi.floats["ft"];
        fadeDuration = fi.floats["fd"];
        count = fi.ints["c"];
        numFlashes = fi.ints["nf"];
        setColor();
    }

    Color imageColor = Color.white;
    Color screenColor = Color.white;
    float fadeTime = 0;
    float fadeDuration = 0;
    int count = 0;
    int numFlashes = 1;

    // components
    UnityEngine.UI.Image image;
    UnityEngine.UI.Image screen;
    RectTransform rt;
    TimeUser timeUser;
	
}
