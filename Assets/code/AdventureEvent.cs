using UnityEngine;
using System.Collections;

/* A bunch of events that can happen over the course of the game */
public class AdventureEvent {

    // info events are time-independent
	public enum Info {
        NONE = 0,
        COMPLETED_TUTORIAL = 1
    }

    // physical events are time-dependent
    public enum Physical {
        NONE = 0,
        DESTROYED_TUTORIAL_WALL = 1,
        FIRST_TALK = 2,
        HIT_PLAYER_WITH_TUTORIAL_WALL = 3,
        SPAWN_PAUSE_CONTROLS_MESSAGE = 4
    }

}
