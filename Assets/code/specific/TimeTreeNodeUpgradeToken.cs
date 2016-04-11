using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TimeTreeNodeUpgradeToken : MonoBehaviour {

    public Sprite healthSprite;
    public Sprite phaseSprite;
    public Sprite grayedSprite;

    public State state {
        get { return _state; }
        set {
            if (_state == value) return;
            _state = value;
            if (!grayed) {
                image.sprite = _state == State.HEALTH ? healthSprite : phaseSprite;
            }
        }
    }

    public bool grayed {
        get { return _grayed; }
        set {
            if (_grayed == value) return;
            _grayed = value;
            if (_grayed) {
                image.sprite = grayedSprite;
            } else {
                image.sprite = state == State.HEALTH ? healthSprite : phaseSprite;
            }
        }
    }

    public enum State {
        HEALTH,
        PHASE
    }

	void Awake() {
        image = GetComponent<Image>();
	}
	
	void Update() {
		
	}

    Image image;
    State _state = State.HEALTH;
    bool _grayed = false;

}
