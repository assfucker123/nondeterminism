using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Calls Vars.eventHappen() and Vars.currentNodeData.eventHappen() to make events happen.
 * Will automatically undo physical events if reverting to before the event happened.
*/

public class EventHappener : MonoBehaviour {

    public void infoHappen(AdventureEvent.Info infoEventID) {
        Vars.eventHappen(infoEventID);
    }
    public void physicalHappen(AdventureEvent.Physical physicalEventID, bool undoOnFlashback = true) {
        if (Vars.currentNodeData == null) {
            Debug.LogError("Error: Vars.currentNodeData is currently null");
            return;
        }
        if (Vars.currentNodeData.eventHappened(physicalEventID)) {
            return;
        }
        Vars.currentNodeData.eventHappen(physicalEventID);

        if (!undoOnFlashback)
            return;

        if (timeUser == null) {
            Debug.LogError("ERROR: Using EventHappener.physicalHappen with undoOnFlashback==true when timeUser == null");
            return;
        }

        ids.Add("" + ((int)physicalEventID));
        events.Add(true);
        
    }
    
	protected virtual void Awake() {
        timeUser = GetComponent<TimeUser>();
	}
	
    protected virtual void Update() {

        if (timeUser != null && timeUser.shouldNotUpdate)
            return;

        

    }

    protected virtual void OnSaveFrame(FrameInfo fi) {

        if (TimeUser.reverting)
            return;

        for (int i=0; i<ids.Count; i++) {
            fi.bools[ids[i]] = events[i];
        }
        
    }

    protected virtual void OnRevert(FrameInfo fi) {
        
        // undo all events that happened this frame
        if (Vars.currentNodeData != null) {

            for (int i=0; i<ids.Count; i++) {
                if (events[i]) {
                    if (!fi.bools.ContainsKey(ids[i]) || !fi.bools[ids[i]])
                        Vars.currentNodeData.eventUndo((AdventureEvent.Physical)int.Parse(ids[i]));
                }
            }

        }

        for (int i = 0; i < ids.Count; i++) {
            if (fi.bools.ContainsKey(ids[i]))
                events[i] = fi.bools[ids[i]];
            else
                events[i] = false;
        }
        

    }
    
    protected TimeUser timeUser;

    // these are the keys of the events to track.  They are "constant" once set, so they won't change with flashbacks
    List<string> ids = new List<string>();

    List<bool> events = new List<bool>();
    
}
