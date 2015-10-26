using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Vars {

    public static bool arcadeMode = true;

    public static float sfxVolume = 1;
    public static float musicVolume { get { return _musicVolume; }
        set {
            _musicVolume = value;
        }
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    /* Loads the given level after doing some stuff first. */
    public static void loadLevel(string name) {

        TimeUser.onUnloadLevel();
        VisionUser.onUnloadLevel();

        Application.LoadLevel(name);
    }

    public static void restartLevel() {
        loadLevel(Application.loadedLevelName);
    }

    /////////////
    // PRIVATE //
    /////////////

    private static float _musicVolume = 1;

}
