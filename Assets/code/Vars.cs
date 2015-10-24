using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Vars {

    public static bool arcadeMode = true;

    /* Loads the given level after doing some stuff first. */
    public static void loadLevel(string name) {

        TimeUser.onUnloadLevel();
        VisionUser.onUnloadLevel();

        Application.LoadLevel(name);
    }

    public static void restartLevel() {
        loadLevel(Application.loadedLevelName);
    }

}
