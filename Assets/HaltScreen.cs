using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/* Halts the game until the player does the specified action */
public class HaltScreen : MonoBehaviour {

    public enum Screen {
        FLASHBACK = 0
    }

    public Screen screen = Screen.FLASHBACK;

    public static List<HaltScreen> allScreens = new List<HaltScreen>();

    public Sprite flashbackSprite;

    public void end() {
        PauseScreen.instance.unpauseGame();
        GameObject.Destroy(gameObject);
    }

	void Awake() {
        image = GetComponent<Image>();

        allScreens.Add(this);
	}

    void Start() {
        if (transform.parent == null)
            return;

        switch (screen) {
        case Screen.FLASHBACK:
            image.sprite = flashbackSprite;
            break;
        }

        GetComponent<RectTransform>().localPosition = Vector3.zero;

        HUD.instance.createPauseScreen();
        PauseScreen.instance.pauseGameHaltScreen();
    }

    void Update() {

        if (transform.parent == null)
            return;
		
        switch (screen) {
        case Screen.FLASHBACK:
            if (Keys.instance.flashbackPressed) {
                Player.instance.flashbackNextFrameFlag = true;
                end();
            }
            break;
        }

	}

    void OnDestroy() {
        allScreens.Remove(this);
    }

    Image image;
}
