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
            (HUD.instance != null && HUD.instance.gameOverScreen != null && HUD.instance.gameOverScreen.activated))
            return;

        if (goToLevel) {
            
            // update map to indicate one room is open to another
            if (Level.currentLoadedLevel != null) {
                Rect mapBounds = CameraControl.getMapBounds();
                Vector2 trueSize = new Vector2(Level.currentLoadedLevel.mapWidth * CameraControl.ROOM_UNIT_WIDTH, Level.currentLoadedLevel.mapHeight * CameraControl.ROOM_UNIT_HEIGHT);
                Vector2 diff = mapBounds.size - trueSize;
                mapBounds.xMin += diff.x / 2;
                mapBounds.xMax -= diff.x / 2;
                mapBounds.yMin += diff.y / 2;
                mapBounds.yMax -= diff.y / 2;
                int mapX = Level.currentLoadedLevel.mapX;
                int mapY = Level.currentLoadedLevel.mapY;
                int mapWidth = Level.currentLoadedLevel.mapWidth;
                int mapHeight = Level.currentLoadedLevel.mapHeight;
                Vector2 plrPos = Player.instance.rb2d.position;
                Vector2 mapPlrPos = MapUI.instance.gridPositionFromWorldPosition(mapX, mapY, plrPos, mapBounds.xMin, mapBounds.yMin);
                int mapPlrPosX = Mathf.RoundToInt(mapPlrPos.x);
                int mapPlrPosY = Mathf.RoundToInt(mapPlrPos.y);
                if (mapBounds.xMin - plrPos.x > -2) { // left open
                    MapUI.instance.gridSetOpenLeftEdge(mapX, mapPlrPosY, true);
                } else if (mapBounds.yMin - plrPos.y > -2) { // bottom open
                    MapUI.instance.gridSetOpenBottomEdge(mapPlrPosX, mapY, true);
                } else if (plrPos.x - mapBounds.xMax > -2) { // right open
                    MapUI.instance.gridSetOpenRightEdge(mapX + mapWidth-1, mapPlrPosY, true);
                } else if (plrPos.y - mapBounds.yMax > -2) { // top open
                    MapUI.instance.gridSetOpenTopEdge(mapPlrPosX, mapY + mapHeight-1, true);
                }
            }

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
