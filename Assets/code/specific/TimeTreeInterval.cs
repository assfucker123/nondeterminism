using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimeTreeInterval : MonoBehaviour {

    public void show() {
        image.enabled = true;
        timeBox.makeAllCharsVisible();
    }

    public void hide() {
        image.enabled = false;
        timeBox.makeAllCharsInvisible();
    }

    public void setTime(float time) {
        string str = "";
        int mins = Mathf.RoundToInt(time / 60);
        if (CountdownTimer.instance != null && CountdownTimer.instance.mode != CountdownTimer.Mode.NORMAL) {
            mins -= Mathf.RoundToInt(CountdownTimer.MELTDOWN_DURATION / 60);
            if (mins > 0) {
                str = "+" + mins;
            } else {
                str = "" + mins;
            }
        } else {
            str = "" + mins;
        }
        if (mins < -99) {
            str = "-99";
        } else if (mins > 99) {
            str = "+99";
        }
        timeBox.setPlainText(str);
    }

	void Awake() {
        image = GetComponent<Image>();
        timeBox = transform.Find("TimeBox").GetComponent<GlyphBox>();
	}
	
	void Update() {
		
	}

    Image image;
    GlyphBox timeBox;

}
