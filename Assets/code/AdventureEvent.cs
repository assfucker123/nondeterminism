using UnityEngine;
using System.Collections;

/* A bunch of events that can happen over the course of the game */
public class AdventureEvent {

    // info events are time-independent
	public enum Info {
        NONE = 0,
        COMPLETED_TUTORIAL = 1,
        FOUND_CREATURE_CARD = 2,

    }

    // physical events are time-dependent
    public enum Physical {
        NONE = 0,
        DESTROYED_TUTORIAL_WALL = 1,
        FIRST_TALK = 2,
        HIT_PLAYER_WITH_TUTORIAL_WALL = 3,
        SPAWN_PAUSE_CONTROLS_MESSAGE = 4,
        SHERIVICE_ENCOUNTER = 5,
        VISION_TUTORIAL_SCREEN = 6,
        SHERIVICE_FLASHBACK_CONTROL_REMINDER = 7

    }

}
