using UnityEngine;
using System.Collections;

public class LevelSensor : MonoBehaviour {

    public string levelTo = "";

	void Awake() {
        bc2d = GetComponent<BoxCollider2D>();
	}
	
	void Update() {

        if (levelTo == "") return;
        if (Player.instance == null) return;
        Vector3 plrPos = Player.instance.rb2d.position;

        if (bc2d.bounds.Contains(plrPos)) {
            goToLevel = true;
        }

	}

    void LateUpdate() {

        if (TimeUser.reverting ||
            Time.timeScale < .0001f ||
            HUD.instance.gameOverScreen.activated)
            return;

        if (goToLevel) {
            // update player position
            Vars.currentNodeData.levelMapX = Level.currentLoadedLevel.mapX;
            Vars.currentNodeData.levelMapY = Level.currentLoadedLevel.mapY;
            Vars.currentNodeData.position = Player.instance.rb2d.position;
            // get player ready to reposition
            Player.instance.getReadyToReposition(Level.currentLoadedLevel.mapX, Level.currentLoadedLevel.mapY, Player.instance.rb2d.position);
            // go to level
            Vars.loadLevel(levelTo);
        }
    }

    bool goToLevel = false;
    BoxCollider2D bc2d;

}
