using UnityEngine;
using System.Collections;

/* add this PreFab to every level.
 * Its position in the scene is the default start position for Oracle when testing the level. */
public class Level : MonoBehaviour {

    public int mapX = 0;
    public int mapY = 0;
    public int mapWidth = 1;
    public int mapHeight = 1;
    public bool showOnMap = true;
    public bool bobbingCamera = false;

    public GameObject keysGameObject;
    public GameObject soundManagerGameObject;
    public GameObject canvasGameObject;
    public GameObject playerGameObject;

    public static Level currentLoadedLevel {  get { return _currentLoadedLevel; } } // reference to the last Level created
	
	void Awake() {

        // make sure level is ready to play

        // If currentNodeData hasn't been created yet (it should be created if game started at the title screen), then create something
        if (Vars.currentNodeData == null) {

            // loading data (which would have been done in the title screen)
            Vars.loadSettings();
            Vars.loadData(Vars.saveFileIndexLastUsed);

            // making currentNodeData from the level
            Vars.currentNodeData.level = Vars.currentLevel;
            Vars.currentNodeData.position = new Vector2(transform.localPosition.x, transform.localPosition.y);
            Vars.levelStartNodeData = NodeData.createNodeData(null, true);
            Vars.levelStartNodeData.copyFrom(Vars.currentNodeData);
        }

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
            Vector3 position = Vars.currentNodeData.position;
            playerGO = GameObject.Instantiate(playerGameObject, position, Quaternion.identity) as GameObject;
            playerGO.GetComponent<Player>().saveFrameInfoOnLevelLoad();
            // prevent player from getting destroyed when loading new level
            GameObject.DontDestroyOnLoad(playerGO);
        }
        
        _currentLoadedLevel = this;
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

        // optional bobbing for camera
        if (bobbingCamera) {
            CameraControl.instance.bobbing = true;
        }

    }
	
	void Update() {
		
	}

    private static Level _currentLoadedLevel = null;
	
}
