using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlsMessageSpawner : MonoBehaviour {

    public GameObject controlsMessageGameObject;

    public static ControlsMessageSpawner instance {  get { return _instance; } }

    public void spawnMessage(ControlsMessage.Control message) {
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

	void Awake() {
		if (_instance != null) {
            GameObject.Destroy(this.gameObject);
            return;
        }
        _instance = this;
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
