using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TitleScreen : MonoBehaviour {

    public AudioClip switchSound;
	
	void Awake() {
        clockText = transform.Find("ClockText").GetComponent<Text>();
        selection = transform.Find("Selection").GetComponent<Image>();
        playGameText = transform.Find("PlayGameText").GetComponent<Text>();
        optionsText = transform.Find("OptionsText").GetComponent<Text>();
        quitText = transform.Find("QuitText").GetComponent<Text>();
	}

    void Start() {
        // start game
        Vars.startGame();

        // set options
        options.Add(playGameText);
        options.Add(optionsText);
        options.Add(quitText);
    }
	
	void Update() {

        menuUpdate();

	}

    void menuUpdate() {

        int prevSelectionIndex = selectionIndex;
        bool newSelection = false;
        if (Keys.instance.upPressed) {
            selectionIndex--;
            if (selectionIndex < 0) selectionIndex = options.Count - 1;
            newSelection = true;
        } else if (Keys.instance.downPressed) {
            selectionIndex++;
            if (selectionIndex >= options.Count) selectionIndex = 0;
            newSelection = true;
        }
        if (newSelection) {
            options[prevSelectionIndex].color = PauseScreen.DEFAULT_COLOR;
            options[selectionIndex].color = PauseScreen.SELECTED_COLOR;
            selectionPos0.Set(selection.rectTransform.localPosition.x, selection.rectTransform.localPosition.y);
            selectionPos1.Set(options[selectionIndex].rectTransform.localPosition.x, options[selectionIndex].rectTransform.localPosition.y);
            selectionPos1 = selectionPos1 + selectionOffset;
            selectionTime = 0;
            SoundManager.instance.playSFX(switchSound);
        }

        float dur = .1f;
        if (selectionTime < dur) {
            selectionTime += Time.unscaledDeltaTime;
            float sx = Utilities.easeOutQuadClamp(selectionTime, selectionPos0.x, selectionPos1.x - selectionPos0.x, dur);
            float sy = Utilities.easeOutQuadClamp(selectionTime, selectionPos0.y, selectionPos1.y - selectionPos0.y, dur);
            selection.rectTransform.localPosition = new Vector3(sx, sy);
        }

        // confirming an option
        if (Keys.instance.confirmPressed) {
            Text selection = options[selectionIndex];

            if (selection == playGameText) {
                playGameSelected();
            } else if (selection == optionsText) {
                optionsSelected();
            } else if (selection == quitText) {
                quitGameSelected();
            }

        }

    }

    void playGameSelected() {
        Vars.loadLevel("mapScene");
    }

    void optionsSelected() {

    }

    void quitGameSelected() {
        Vars.quitGame();
    }

    int selectionIndex = 0;
    float selectionTime = 9999f;
    Vector2 selectionPos0 = new Vector2();
    Vector2 selectionPos1 = new Vector2();
    List<Text> options = new List<Text>();

    Text clockText;
    Image selection;
    Vector2 selectionOffset = new Vector2(0, 2);
    Text playGameText;
    Text optionsText;
    Text quitText;

}
