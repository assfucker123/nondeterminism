using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapIcon : MonoBehaviour {

    public MapUI.Icon icon;
    public int x = 0;
    public int y = 0;
    public bool wideSprite = false;
    public bool canBeFound = false;
    public Sprite foundSprite;

    public bool found {
        get { return _found; }
        set {
            if (!canBeFound) return;
            if (found == value) return;
            _found = value;
            if (found) {
                originalSprite = image.sprite;
                image.sprite = foundSprite;
            } else {
                image.sprite = originalSprite;
            }
        }
    }

    public string toString() {
        string foundStr = "0";
        if (found) foundStr = "1";
        return "i" + ((int)icon) + "x" + x + "y" + y + "f" + foundStr;
    }

	void Awake() {
        image = GetComponent<Image>();
	}
	
	void Update() {
		
	}

    Image image;

    bool _found = false; // important this is false by default
    Sprite originalSprite;
	
}
