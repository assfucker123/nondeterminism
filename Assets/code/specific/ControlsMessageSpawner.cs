using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlsMessageSpawner : MonoBehaviour {

    GameObject controlsMessageGameObject;
    GameObject haltScreenGameObject;

    public static ControlsMessageSpawner instance {  get { return _instance; } }

    public void spawnMessage(ControlsMessage.Control message) {
        if (controlsMessageGameObject == null) {
            Debug.LogError("Error: controlsMessageGameObject is null.  Put a ControlsMessage gameObject in the scene");
            return;
        }
        GameObject cmGO = GameObject.Instantiate(controlsMessageGameObject);
        cmGO.transform.SetParent(this.transform, false);
        ControlsMessage cm = cmGO.GetComponent<ControlsMessage>();
        cm.control = message;
    }

    public void takeDownMessage(ControlsMessage.Control message) {
        foreach (ControlsMessage cm in ControlsMessage.allMessages) {
            if (cm.control == message) {
                cm.fadeOut();
            }
        }
    }

    public void spawnHaltScreen(HaltScreen.Screen screen) {
        if (haltScreenGameObject == null) {
            Debug.LogError("Error: haltScreenGameObject is null.  Put a HaltScreen gameObject in the scene");
            return;
        }
        GameObject hsGO = GameObject.Instantiate(haltScreenGameObject);
        hsGO.transform.SetParent(this.transform, false);
        HaltScreen hs = hsGO.GetComponent<HaltScreen>();
        hs.screen = screen;
    }

    public void endHaltScreen(HaltScreen.Screen screen) {
        foreach (HaltScreen hs in HaltScreen.allScreens) {
            if (hs.screen == screen) {
                hs.end();
            }
        }
    }

    void Awake() {
		if (_instance != null) {
            GameObject.Destroy(this.gameObject);
            return;
        }
        _instance = this;

        if (controlsMessageGameObject == null) {
            controlsMessageGameObject = GameObject.Find("ControlsMessage");
        }
        if (haltScreenGameObject == null) {
            haltScreenGameObject = GameObject.Find("HaltScreen");
        }
	}
	
	void Update() {
		
    }

    void OnDestroy() {
        if (_instance == this) {
            _instance = null;
        }
    }

    private static ControlsMessageSpawner _instance = null;
}
