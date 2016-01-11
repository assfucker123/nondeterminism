using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/* Halts the game until the player does the specified action */
public class HaltScreen : MonoBehaviour {

    public enum Screen {
        FLASHBACK = 0,
        VISION = 1
    }

    public Screen screen = Screen.FLASHBACK;

    public static List<HaltScreen> allScreens = new List<HaltScreen>();

    public Sprite flashbackSprite;
    public Sprite visionSprite;

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
        case Screen.VISION:
            image.sprite = visionSprite;
            break;
        }

        GetComponent<RectTransform>().localPosition = Vector3.zero;

        HUD.instance.createPauseScreen();
        PauseScreen.instance.pauseGameHaltScreen();
        time = 0;
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
        case Screen.VISION:
            time += Time.unscaledDeltaTime;
            if (time > .2f) {
                if (Keys.instance.jumpPressed) {
                    end();
                }
            }
            break;
        }

	}

    void OnDestroy() {
        allScreens.Remove(this);
    }

    Image image;
    float time = 0;
}
