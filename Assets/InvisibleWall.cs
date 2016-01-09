using UnityEngine;
using System.Collections;

public class InvisibleWall : MonoBehaviour {

    public bool useTimeUser = false;

    public bool wallEnabled {
        get {
            return bc2d.enabled;
        }
        set {
            bc2d.enabled = value;
        }
    }

    void EnableWall() {
        wallEnabled = true;
    }

    void DisableWall() {
        wallEnabled = false;
    }

	void Awake() {
        bc2d = GetComponent<BoxCollider2D>();
	}

    void Update() {

    }

    void OnSaveFrame(FrameInfo fi) {
        if (!useTimeUser) return;
        fi.bools["we"] = wallEnabled;
    }

    void OnRevert(FrameInfo fi) {
        if (!useTimeUser) return;
        wallEnabled = fi.bools["we"];
    }

    BoxCollider2D bc2d;
}
