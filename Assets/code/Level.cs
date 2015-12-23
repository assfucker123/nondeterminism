using UnityEngine;
using System.Collections;

/* add this PreFab to every level */
public class Level : MonoBehaviour {

    public int mapX = 0;
    public int mapY = 0;
    public int mapWidth = 1;
    public int mapHeight = 1;

    public GameObject keysGameObject;
    public GameObject soundManagerGameObject;
    public GameObject canvasGameObject;
    public GameObject playerGameObject;
	
	void Awake() {

        // make sure level is ready to play

        // check if Keys has been set up yet (it should if being played from first_scene).  If not, do so.
        GameObject keysGO = GameObject.FindGameObjectWithTag("Keys");
        if (keysGO == null) {
            keysGO = GameObject.Instantiate(keysGameObject);
        }

        // check if SoundManager has been set up yet (it should if being played from first_scene).  If not, do so.
        GameObject soundManagerGO = GameObject.FindGameObjectWithTag("SoundManager");
        if (soundManagerGO == null) {
            soundManagerGO = GameObject.Instantiate(soundManagerGameObject);
        }
		
        // check if Canvas has been created.  If not, create it
        GameObject canvasGO = GameObject.FindGameObjectWithTag("Canvas");
        if (canvasGO == null) {
            canvasGO = GameObject.Instantiate(canvasGameObject);
            // prevent canvas from getting destroyed when loading new level.
            GameObject.DontDestroyOnLoad(canvasGO);
        }

        // check if Player has been created.  If not, create it
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null) {
            playerGO = GameObject.Instantiate(playerGameObject);
            // prevent player from getting destroyed when loading new level
            GameObject.DontDestroyOnLoad(playerGO);
        }

	}

    void Start() {

        // set bounds of camera to the entire level
        Rect mapBounds = CameraControl.getMapBounds();
        Vector2 trueSize = new Vector2(mapWidth * CameraControl.ROOM_UNIT_WIDTH, mapHeight * CameraControl.ROOM_UNIT_HEIGHT);
        Vector2 diff = mapBounds.size - trueSize;
        mapBounds.xMin += diff.x / 2;
        mapBounds.xMax -= diff.x / 2;
        mapBounds.yMin += diff.y / 2;
        mapBounds.yMax -= diff.y / 2;
        CameraControl.instance.enableBounds(mapBounds);

    }
	
	void Update() {
		
	}
	
}
