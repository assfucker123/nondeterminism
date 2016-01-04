using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/* Halts the game until the player does the specified action */
public class HaltScreen : MonoBehaviour {

    public enum Screen {
        FLASHBACK = 0
    }

    public Screen screen = Screen.FLASHBACK;

    public Sprite flashbackSprite;

	void Awake() {
        image = GetComponent<Image>();
	}

    void Start() {
        switch (screen) {
        case Screen.FLASHBACK:
            image.sprite = flashbackSprite;
            break;
        }
        image.color = Color.clear;
    }

    void Update() {
		
	}

    Image image;
}
