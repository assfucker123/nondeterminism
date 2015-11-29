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
        sampleText = transform.Find("SampleText").GetComponent<GlyphBox>();
        //propAsset = new Properties(textAsset.text);
    }

    void Start() {
        //sampleText.setPlainText(propAsset.getString("resume"));
        hide();
    }

    void Update() {

    }

    public void show() {
        sampleText.makeAllCharsVisible();

        MapUI.instance.showMap(true);
    }

    public void hide() {
        sampleText.makeAllCharsInvisible();

        MapUI.instance.hideMap();
    }

    Properties propAsset;
    GlyphBox sampleText;

}
