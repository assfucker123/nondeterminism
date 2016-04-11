using UnityEngine;
using System.Collections;

public class CountdownTimerDot : MonoBehaviour {

    public Sprite normalSprite;
    public Sprite meltdownSprite;
    public Sprite meltdownPerilSprite;
    public Sprite weirdSprite;

    public CountdownTimer.Mode mode {
        get {
            return _mode;
        }
        set {
            if (_mode == value) return;
            _mode = value;
            switch (_mode) {
            case CountdownTimer.Mode.NORMAL:
                image.sprite = normalSprite;
                break;
            case CountdownTimer.Mode.MELTDOWN:
                image.sprite = meltdownSprite;
                break;
            case CountdownTimer.Mode.MELTDOWN_PERIL:
                image.sprite = meltdownPerilSprite;
                break;
            case CountdownTimer.Mode.WEIRD:
                image.sprite = weirdSprite;
                break;
            }
        }
    }

	void Awake() {
        image = GetComponent<UnityEngine.UI.Image>();
	}
	
	void Update() {
		
	}

    UnityEngine.UI.Image image;

    CountdownTimer.Mode _mode = CountdownTimer.Mode.NORMAL;
}
