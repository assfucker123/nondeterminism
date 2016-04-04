using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MapPage : MonoBehaviour {

    public TextAsset textAsset;

    public void update() {

        /*
        if (Keys.instance.confirmPressed) {
            madeSelection = true;
            option = options[selectionIndex];
        }
        if (Keys.instance.backPressed) {
            if (quitSureNoText.hasVisibleChars) {
                madeSelection = true;
                option = quitSureNoText;
            }
        }
        */

    }

    void Awake() {
        glyphBox = transform.Find("GlyphBox").GetComponent<GlyphBox>();
        propAsset = new Properties(textAsset.text);
    }

    void Start() {
        hide();
    }

    void Update() {

    }

    public void show() {
        glyphBox.makeAllCharsVisible();
        
        MapUI.instance.showMap(true);
        MapUI.instance.inputEnabled = true;
        MapUI.instance.setMapPagePosition();
        glyphBox.setText("");
    }

    public void hide() {
        glyphBox.makeAllCharsInvisible();

        if (MapUI.instance != null) {
            MapUI.instance.hideMap();
        }
        
    }

    public void setChamberText(string positionCode) {
        setChamberText(positionCode, -1);
    }
    public void setChamberText(string positionCode, float visitedTime) {
        setChamberText(positionCode, visitedTime, -1);
    }
    public void setChamberText(string positionCode, float firstVisitedTime, float lastVisitedTime) {
        string str = propAsset.getString("chamber") + " " + positionCode;
        string line = "";
        CountdownTimer.Mode firstMode = CountdownTimer.Mode.NORMAL;
        int firstIndex = 0;
        CountdownTimer.Mode lastMode = CountdownTimer.Mode.NORMAL;
        int lastIndex = 0;
        if (firstVisitedTime != -1) {
            str += "\n";
            if (lastVisitedTime == -1) {
                line = propAsset.getString("visited");
            } else {
                line = propAsset.getString("first_visited");
            }
            line += ": ";
            firstIndex = line.Length;
            if (CountdownTimer.instance != null && CountdownTimer.instance.mode != CountdownTimer.Mode.NORMAL) {
                // use MELTDOWN or MELTDOWN_PERIL mode
                if (firstVisitedTime >= CountdownTimer.MELTDOWN_DURATION - CountdownTimer.MELTDOWN_PERIL_DURATION) {
                    firstMode = CountdownTimer.Mode.MELTDOWN_PERIL;
                } else {
                    firstMode = CountdownTimer.Mode.MELTDOWN;
                }
            }
            line += CountdownTimer.timeToStr(firstVisitedTime, firstMode);
            str += line;
        }
        if (lastVisitedTime != -1) {
            str += "\n";
            line = propAsset.getString("last_visited") + ": ";
            lastIndex = line.Length;
            if (CountdownTimer.instance != null && CountdownTimer.instance.mode != CountdownTimer.Mode.NORMAL) {
                // use MELTDOWN or MELTDOWN_PERIL mode
                if (lastVisitedTime >= CountdownTimer.MELTDOWN_DURATION - CountdownTimer.MELTDOWN_PERIL_DURATION) {
                    lastMode = CountdownTimer.Mode.MELTDOWN_PERIL;
                } else {
                    lastMode = CountdownTimer.Mode.MELTDOWN;
                }
            }
            line += CountdownTimer.timeToStr(lastVisitedTime, lastMode);
            str += line;
        }

        glyphBox.setColor(glyphBox.defaultStyle.color);
        glyphBox.setText(str);

        // set color of times
        Color color;
        if (firstVisitedTime != -1) {
            color = CountdownTimer.NORMAL_COLOR;
            switch (firstMode) {
            case CountdownTimer.Mode.MELTDOWN:
                color = CountdownTimer.MELTDOWN_COLOR;
                break;
            case CountdownTimer.Mode.MELTDOWN_PERIL:
                color = CountdownTimer.MELTDOWN_PERIL_COLOR;
                break;
            }
            glyphBox.setColor(color, 1, firstIndex, glyphBox.lineLength(1));
        }
        if (lastVisitedTime != -1) {
            color = CountdownTimer.NORMAL_COLOR;
            switch (lastMode) {
            case CountdownTimer.Mode.MELTDOWN:
                color = CountdownTimer.MELTDOWN_COLOR;
                break;
            case CountdownTimer.Mode.MELTDOWN_PERIL:
                color = CountdownTimer.MELTDOWN_PERIL_COLOR;
                break;
            }
            glyphBox.setColor(color, 2, lastIndex, glyphBox.lineLength(2));
        }

    }

    Properties propAsset;
    GlyphBox glyphBox;

}
