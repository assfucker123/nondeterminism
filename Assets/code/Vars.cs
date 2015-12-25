using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public class Vars {

    public static bool arcadeMode = false;
    public static int highScore = 0;
    public static bool screenshotMode = false;

    // TIME-INDEPENDENT
    public static int saveFileIndex = 0;
    public static string username = "";
    public static DateTime createdDate = new DateTime();
    public static DateTime modifiedDate = new DateTime();
    public static float playTime = 0; // this is NOT the same as currentNodeData.time
    public static List<Decryptor.ID> decryptors = new List<Decryptor.ID>();
    public static List<AdventureEvent.Info> infoEvents = new List<AdventureEvent.Info>();
    public static bool eventHappened(AdventureEvent.Info eventID) {
        return infoEvents.IndexOf(eventID) != -1;
    }
    public static void eventHappen(AdventureEvent.Info eventID) {
        if (eventHappened(eventID)) return;
        infoEvents.Add(eventID);
    }

    // TIME-DEPENDENT
    /* Keeps changing based on what the player does.
     * When resuming a game after saving or loading, make a new currentNodeData.
     * If the player dies before getting to a save point, delete this currentNodeData first (and player can pick on time tree where to resume) */
    public static NodeData currentNodeData = null;

    // SETTINGS

    public static float sfxVolume = 1;
    public static float musicVolume { get { return _musicVolume; }
        set {
            _musicVolume = value;
        }
    }
    public static int saveFileIndexLastUsed = 0;
    
    public static void loadDefaultSettings() {
        sfxVolume = 1;
        musicVolume = 1;
        saveFileIndexLastUsed = 0;
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    public static void startGame() {
        if (startGameCalled) return;
        loadSettings();
        loadData(saveFileIndexLastUsed);

        startGameCalled = true;
    }

    /* Loads the given level after doing some stuff first. */
    public static void loadLevel(string name) {

        TimeUser.onUnloadLevel();
        VisionUser.onUnloadLevel();
        Time.timeScale = 1;
        SoundManager.instance.volumeScale = 1;

        // should be called at the title screen
        if (!startGameCalled) {
            startGame();
        }

        // have the black screen cover everything temporarily
        if (HUD.instance != null && HUD.instance.blackScreen != null) {
            HUD.instance.blackScreen.color = Color.white;
        }

        SceneManager.LoadScene(name);
    }

    public static void restartLevel() {
        loadLevel(SceneManager.GetActiveScene().name);
    }

    public static void goToTitleScreen() {
        // destroy canvas if leaving main game
        GameObject canvasGO = GameObject.FindGameObjectWithTag("Canvas");
        if (canvasGO != null) {
            GameObject.Destroy(canvasGO);
        }
        // destroy player if leaving main game
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) {
            GameObject.Destroy(playerGO);
        }

        loadLevel("title_scene");
    }

    public static void quitGame() {
        #if UNITY_EDITOR
        // set the PlayMode to stop
        Debug.Log("quit game");
        #else
        Application.Quit();
        #endif
    }

    /* Save data to a file */
    public static void saveData() {
        string path = Application.persistentDataPath + "/data" + saveFileIndex + ".sav";

        byte[] bArr = Utilities.stringToBytes(saveDataToString());

        #if !UNITY_WEBPLAYER
        File.WriteAllBytes(path, bArr);
        #endif
    }

    /* Load data from a file */
    public static void loadData(int saveFileIndex = 0) {

        if (saveFileIndex != saveFileIndexLastUsed) {
            saveFileIndexLastUsed = saveFileIndex;
            saveSettings();
        }

        #if UNITY_WEBPLAYER
        loadDefaultData(saveFileIndex);
        return;
        #endif

        #if !UNITY_WEBPLAYER

        string path = Application.persistentDataPath + "/data" + saveFileIndex + ".sav";

        if (!File.Exists(path)) {
            loadDefaultData(saveFileIndex);
            return;
        }
        byte[] bArr = File.ReadAllBytes(path);
        loadDataFromString(Utilities.bytesToString(bArr));

        #endif
    }

    /* Save settings to a file */
    public static void saveSettings() {
        string path = Application.persistentDataPath + "/settings.ini";

        byte[] bArr = Utilities.stringToBytes(saveSettingsToString());

        #if !UNITY_WEBPLAYER
        File.WriteAllBytes(path, bArr);
        #endif
    }

    /* Load settings from a file */
    public static void loadSettings() {
        #if UNITY_WEBPLAYER
        loadDefaultSettings();
        return;
        #endif

        #if !UNITY_WEBPLAYER

        string path = Application.persistentDataPath + "/settings.ini";

        if (!File.Exists(path)){
            loadDefaultSettings();
            return;
        }
        byte[] bArr = File.ReadAllBytes(path);
        loadSettingsFromString(Utilities.bytesToString(bArr));

        #endif
    }

    /* Take a screenshot */
    public static void takeScreenshot() {
        System.DateTime dateTime = System.DateTime.Now;
        string str = "screenie_";
        str += dateTime.ToString("MM-dd_HH-mm-ss");
        str += ".png";
        Application.CaptureScreenshot(str);
    }

    /* Adds decrptor to the list, checking to see it wasn't added yet */
    public static void collectDecryptor(Decryptor.ID decryptor) {
        if (decryptors.IndexOf(decryptor) == -1)
            decryptors.Add(decryptor);
    }

    /* Returns if the ability described by the given descryptor can currently be used. */
    public static bool abilityKnown(Decryptor.ID decryptor) {
        if (decryptors.IndexOf(decryptor) == -1) return false;
        bool hasBooster = false;
        if (currentNodeData != null)
            hasBooster = currentNodeData.hasBooster;
        return Decryptor.canUse(decryptor, hasBooster, decryptors);
    }

    /////////////
    // PRIVATE //
    /////////////

    private static bool startGameCalled = false;
    private static float _musicVolume = 1;

    /* LOADING DEFAULT DATA */
    static void loadDefaultData(int saveFileIndex = 0) {
        // save file index
        Vars.saveFileIndex = saveFileIndex;
        // username
        username = "";
        // created date
        createdDate = DateTime.Now;
        // modified date
        modifiedDate = DateTime.Now;
        // play time
        playTime = 0;
        // all node data
        NodeData.clearAllNodes();
        // current node data
        currentNodeData = NodeData.createNodeData();
        currentNodeData.time = 0;
        currentNodeData.level = "tut_ship_1";
        currentNodeData.levelMapX = 1;
        currentNodeData.levelMapY = 0;
        currentNodeData.position.Set(25, 14);
        currentNodeData.orbs.Clear();
        currentNodeData.hasBooster = false;
        currentNodeData.healthUpgrades.Clear();
        currentNodeData.phaseReplacements = 0;
        currentNodeData.physicalEvents.Clear();
        currentNodeData.levelsAmbushesDefeated.Clear();
        // decryptors
        decryptors.Clear();
        // info events
        infoEvents.Clear();

    }

    /* LOADING DATA */
    static void loadDataFromString(string str) {
        char[] delims = {'\n' };
        char[] delims2 = {','};
        string[] strs = str.Split(delims);
        // save file index
        saveFileIndex = int.Parse(strs[0]);
        // username
        username = strs[1];
        // created date
        createdDate = DateTime.Parse(strs[2]);
        // modified date
        modifiedDate = DateTime.Parse(strs[3]);
        // play time
        playTime = float.Parse(strs[4]);
        // all node data
        NodeData.loadAllNodesFromString(strs[5]);
        // current node data
        currentNodeData = NodeData.nodeDataFromID(int.Parse(strs[6]));
        // decryptors
        decryptors.Clear();
        string[] dStrs = strs[7].Split(delims2);
        for (int i=0; i<dStrs.Length; i++) {
            if (dStrs[i] == "") continue;
            decryptors.Add((Decryptor.ID)int.Parse(dStrs[i]));
        }
        // info events
        infoEvents.Clear();
        string[] iStrs = strs[8].Split(delims2);
        for (int i = 0; i < iStrs.Length; i++) {
            if (iStrs[i] == "") continue;
            infoEvents.Add((AdventureEvent.Info)int.Parse(iStrs[i]));
        }

    }

    /* SAVING DATA */
    static string saveDataToString() {
        string ret = "";
        // save file index (0)
        ret += saveFileIndex + "\n";
        // username (1)
        ret += username + "\n";
        // created date (2)
        ret += createdDate.ToString() + "\n";
        // modified date (3)
        ret += modifiedDate.ToString() + "\n";
        // play time (4)
        ret += playTime + "\n";
        // all node data (5)
        ret += NodeData.saveAllNodesToString() + "\n";
        // current node data (6)
        if (currentNodeData == null) ret += "0\n";
        else ret += currentNodeData.id + "\n";
        // decryptors (7)
        for (int i=0; i<decryptors.Count; i++) {
            ret += decryptors[i];
            if (i < decryptors.Count - 1)
                ret += ",";
        }
        ret += "\n";
        // info events (8)
        for (int i = 0; i < infoEvents.Count; i++) {
            ret += infoEvents[i];
            if (i < infoEvents.Count - 1)
                ret += ",";
        }

        return ret;
    }

    /* SAVING SETTINGS */

    static string saveSettingsToString() {
        string str = "";
        str += "sfx_volume = " + sfxVolume + "\n";
        str += "music_volume = " + musicVolume + "\n";
        str += "save_file_last_used = " + saveFileIndexLastUsed + "\n";
        return str;
    }
    static void loadSettingsFromString(string str) {
        char[] nl = { '\n' };
        string[] lines = str.Split(nl);
        foreach (string line in lines) {
            int index = line.IndexOf('=');
            if (index == -1) continue;
            string name = line.Substring(0, index).Trim().ToLower();
            string value = line.Substring(index + 1).Trim();
            // set settings
            if (name == "sfx_volume") {
                sfxVolume = float.Parse(value);
            } else if (name == "music_volume") {
                musicVolume = float.Parse(value);
            } else if (name == "save_file_last_used") {
                saveFileIndexLastUsed = int.Parse(value);
            }

        }
        
    }

    // using serialize:
    /*
    public static void Save() {
        SaveLoad.savedGames.Add(Game.current);
        BinaryFormatter bf = new BinaryFormatter();
        //Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
        FileStream file = File.Create (Application.persistentDataPath + "/savedGames.gd"); //you can call it anything you want
        bf.Serialize(file, SaveLoad.savedGames);
        file.Close();
    }   
     
    public static void Load() {
        if(File.Exists(Application.persistentDataPath + "/savedGames.gd")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
            SaveLoad.savedGames = (List<Game>)bf.Deserialize(file);
            file.Close();
        }
    }
    */

}
