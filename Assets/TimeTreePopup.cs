using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimeTreePopup : MonoBehaviour {

    public float selectionDuration = .1f;
    public AudioClip switchSound;

    public State state { get; private set; }
    public bool visible {
        get {
            return image.enabled;
        }
    }

    public enum State {
        NONE_PRESSED,
        YES_PRESSED,
        NO_PRESSED
    }

	void Awake() {
        image = GetComponent<Image>();
        titleBox = transform.Find("TitleBox").GetComponent<GlyphBox>();
        yesBox = transform.Find("YesBox").GetComponent<GlyphBox>();
        noBox = transform.Find("NoBox").GetComponent<GlyphBox>();
        selection = transform.Find("Selection").GetComponent<Image>();
    }
	
    public void show(string titleBoxText, string yesText, string noText, Color titleColor, bool selectYesStart=true) {
        image.enabled = true;
        titleBox.setText(titleBoxText);
        titleBox.setColor(titleColor);
        yesBox.setText(yesText);
        noBox.setText(noText);
        selection.enabled = true;
        state = State.NONE_PRESSED;
        yesSelected = selectYesStart;

        if (yesSelected) {
            selectionPos1 = yesBox.GetComponent<RectTransform>().localPosition;
            yesBox.setColor(PauseScreen.SELECTED_COLOR);
            noBox.setColor(PauseScreen.DEFAULT_COLOR);
        } else {
            selectionPos1 = noBox.GetComponent<RectTransform>().localPosition;
            yesBox.setColor(PauseScreen.DEFAULT_COLOR);
            noBox.setColor(PauseScreen.SELECTED_COLOR);
        }

        setSelectionPos(selectionPos1);
        selectionPos0 = selectionPos1;

        afterShowTime = 0;
    }

    public void hide() {
        image.enabled = false;
        titleBox.makeAllCharsInvisible();
        yesBox.makeAllCharsInvisible();
        noBox.makeAllCharsInvisible();
        selection.enabled = false;
        state = State.NONE_PRESSED;
    }

	void Update() {
        if (!visible) return;
        if (Keys.instance.startPressed) return;

        afterShowTime += Time.unscaledDeltaTime;
        
        // moving selection
        if (selectionTime < selectionDuration) {
            selectionTime += Time.unscaledDeltaTime;
            Vector2 pos = Utilities.easeOutQuadClamp(selectionTime, selectionPos0, selectionPos1-selectionPos0, selectionDuration);
            setSelectionPos(pos);
        }

        // input
        if (afterShowTime-Time.unscaledDeltaTime > .01f && state == State.NONE_PRESSED) {

            if (Keys.instance.rightPressed || Keys.instance.leftPressed) {
                if (yesSelected) {
                    // select no
                    selectionPos0 = getPos(yesBox.gameObject);
                    selectionPos1 = getPos(noBox.gameObject);
                    yesBox.setColor(PauseScreen.DEFAULT_COLOR);
                    noBox.setColor(PauseScreen.SELECTED_COLOR);
                    yesSelected = false;
                } else {
                    // select yes
                    selectionPos0 = getPos(noBox.gameObject);
                    selectionPos1 = getPos(yesBox.gameObject);
                    yesBox.setColor(PauseScreen.SELECTED_COLOR);
                    noBox.setColor(PauseScreen.DEFAULT_COLOR);
                    yesSelected = true;
                }
                selectionTime = 0;
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            } else if (Keys.instance.backPressed || (Keys.instance.confirmPressed && !yesSelected)) {
                // no pressed
                state = State.NO_PRESSED;
            } else if (Keys.instance.confirmPressed && yesSelected) {
                // yes pressed
                state = State.YES_PRESSED;
            }

        }

        
	}

    void setSelectionPos(Vector3 pos) {
        selection.GetComponent<RectTransform>().localPosition = pos;
    }
    Vector2 getPos(GameObject gameObject) {
        return new Vector2(gameObject.GetComponent<RectTransform>().localPosition.x, gameObject.GetComponent<RectTransform>().localPosition.y);
    }

    Image image;
    GlyphBox titleBox;
    GlyphBox yesBox;
    GlyphBox noBox;
    Image selection;

    bool yesSelected = false;
    float selectionTime = 9999;
    float afterShowTime = 0;
    Vector2 selectionPos0 = new Vector2();
    Vector2 selectionPos1 = new Vector2();
}
