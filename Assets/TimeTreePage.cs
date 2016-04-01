using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TimeTreePage : MonoBehaviour {

    public GameObject selectorGameObject;
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

        
    }

    public void hide() {
        glyphBox.makeAllCharsInvisible();
        

    }
    
    Properties propAsset;
    GlyphBox glyphBox;

}
