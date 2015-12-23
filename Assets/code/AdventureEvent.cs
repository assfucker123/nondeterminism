using UnityEngine;
using System.Collections;

/* A bunch of events that can happen over the course of the game */
public class AdventureEvent {

    // info events are time-independent
	public enum Info {
        COMPLETED_TUTORIAL
    }

    // physical events are time-dependent
    public enum Physical {
        DEFEATED_GRYGOR
    }


}
