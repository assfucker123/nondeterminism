using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShipWallLock : MonoBehaviour {

    public GameObject explosionGameObject;
    public AudioClip explosionSound;
    public bool takeDownMessages = false;

	void Awake() {
        receivesDamage = GetComponent<ReceivesDamage>();
        timeUser = GetComponent<TimeUser>();
        shipWallLocks.Add(this);

        
    }

    void Start() {
        if (Vars.currentNodeData.eventHappened(AdventureEvent.Physical.DESTROYED_TUTORIAL_WALL)) {
            GameObject.Destroy(gameObject);
        }
    }
	
	void Update() {
		
	}

    void OnDamage(AttackInfo ai) {
        if (receivesDamage.health <= 0) {
            // die
            GameObject.Instantiate(explosionGameObject, transform.localPosition, Quaternion.identity);
            SoundManager.instance.playSFX(explosionSound, .5f);
            timeUser.timeDestroy();

            // destroy ship wall
            bool destroyWall = true;
            foreach (ShipWallLock swl in shipWallLocks) {
                if (swl == null) continue;
                if (swl.GetComponent<TimeUser>().exists)
                    destroyWall = false;
            }
            if (destroyWall) {
                ShipWall shipWall = GameObject.Find("ShipWall").GetComponent<ShipWall>();
                shipWall.beDestroyed();
            }

        }

        if (takeDownMessages) {
            // get rid of tutorial messages
            if (ControlsMessageSpawner.instance != null) {
                ControlsMessageSpawner.instance.takeDownMessage(ControlsMessage.Control.RUN_AIM);
                ControlsMessageSpawner.instance.takeDownMessage(ControlsMessage.Control.JUMP);
                ControlsMessageSpawner.instance.takeDownMessage(ControlsMessage.Control.SHOOT);
            }
        }
    }

    void OnDestroy() {
        shipWallLocks.Remove(this);
    }

    static List<ShipWallLock> shipWallLocks = new List<ShipWallLock>();

    ReceivesDamage receivesDamage;
    TimeUser timeUser;

}
