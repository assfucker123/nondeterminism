using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public class Vars {
    
    public static bool screenshotMode = false;
    public static bool encryptSaveData = false; // encryption isn't consistent for some reason.  Need to fix
    public static bool buildIndexLevelIDs = false; // When true: some instances of a level name will be replaced with its build index, which reduces the size of the save file, but can mess things up if a level changes build index
    public static bool promptDifficulty = false; // should be true in the full game when difficulty and tutorial toggle is enabled

    // TIME-INDEPENDENT
    public static int saveFileIndex = 0;
    public static string username = "";
    public static DateTime createdDate = new DateTime();
    public static DateTime modifiedDate = new DateTime();
    #region Difficulty
    public enum Difficulty {
        NONE,
        EASY,
        TRADITIONAL,
        STANDARD,
        HARD,
        CRUEL
    }
    public static Difficulty difficulty = Difficulty.STANDARD;
    #endregion
    public static bool tutorialsEnabled = true;
    public static List<Decryptor.ID> decryptors = new List<Decryptor.ID>();
    #region Info Events
    public static List<AdventureEvent.Info> infoEvents = new List<AdventureEvent.Info>();
    public static bool eventHappened(AdventureEvent.Info eventID) {
        return infoEvents.IndexOf(eventID) != -1;
    }
    public static void eventHappen(AdventureEvent.Info eventID) {
        if (eventHappened(eventID)) return;
        infoEvents.Add(eventID);
    }
    #endregion
    #region Orbs
    public static List<PhysicalUpgrade.Orb> orbsFound = new List<PhysicalUpgrade.Orb>();
    public static bool orbFound(PhysicalUpgrade.Orb orb) {
        return orbsFound.IndexOf(orb) != -1;
    }
    public static void orbFind(PhysicalUpgrade.Orb orb) {
        if (orbFound(orb)) return;
        orbsFound.Add(orb);
    }
    #endregion
    public static bool boosterFound = false;
    #region Health Upgrades
    public static List<PhysicalUpgrade.HealthUpgrade> healthUpgradesFound = new List<PhysicalUpgrade.HealthUpgrade>();
    public static bool healthUpgradeFound(PhysicalUpgrade.HealthUpgrade healthUpgrade) {
        return healthUpgradesFound.IndexOf(healthUpgrade) != -1;
    }
    public static void healthUpgradeFind(PhysicalUpgrade.HealthUpgrade healthUpgrade) {
        if (healthUpgradeFound(healthUpgrade)) return;
        healthUpgradesFound.Add(healthUpgrade);
    }
    #endregion
    #region Creature Cards
    public static List<int> creatureCardsFound = new List<int>();
    public static bool creatureCardFound(string creatureName) {
        return creatureCardFound(CreatureCard.getIDFromCardName(creatureName));
    }
    public static bool creatureCardFound(int creatureID) {
        return creatureCardsFound.IndexOf(creatureID) != -1;
    }
    public static void creatureCardFind(string creatureName) {
        creatureCardFind(CreatureCard.getIDFromCardName(creatureName));
    }
    public static void creatureCardFind(int creatureID) {
        if (creatureCardFound(creatureID)) return;
        creatureCardsFound.Add(creatureID);
    }
    #endregion
    #region Completion Progress
    public static float playTime = 0; // this is NOT the same as currentNodeData.time
    public static float infoPercentComplete() {
        int stuff = 0;
        int totalStuff = 0;

        stuff += orbsFound.Count;
        totalStuff += (int)PhysicalUpgrade.Orb.TOTAL_ORBS;

        if (boosterFound) stuff += 1;
        totalStuff += 1;

        stuff += healthUpgradesFound.Count;
        totalStuff += (int)PhysicalUpgrade.HealthUpgrade.TOTAL_HEALTH_UPGRADES;

        stuff += decryptors.Count;
        totalStuff += (int)Decryptor.ID.LAST_ID - 1;

        stuff += creatureCardsFound.Count;
        totalStuff += CreatureCard.getNumCardsTotal();

        return stuff * 1.0f / totalStuff;
    }
    public static float physPercentComplete() {
        if (currentNodeData == null)
            return 0;
        int stuff = 0;
        int totalStuff = 0;

        stuff += currentNodeData.orbs.Count;
        totalStuff += (int)PhysicalUpgrade.Orb.TOTAL_ORBS;

        if (currentNodeData.hasBooster) stuff += 1;
        totalStuff += 1;

        stuff += currentNodeData.healthUpgrades.Count;
        totalStuff += (int)PhysicalUpgrade.HealthUpgrade.TOTAL_HEALTH_UPGRADES;

        stuff += decryptors.Count;
        totalStuff += (int)Decryptor.ID.LAST_ID - 1;

        stuff += currentNodeData.creatureCards.Count;
        totalStuff += CreatureCard.getNumCardsTotal();

        return stuff * 1.0f / totalStuff;
    }
    #endregion
    public static string currentLevel {  get { return SceneManager.GetActiveScene().name; } }
    public static int currentLevelBuildIndex {  get { return SceneManager.GetActiveScene().buildIndex; } }
    
    // TIME-DEPENDENT
    /* Keeps changing based on what the player does.  Note this is temporary, so it's not part of the time tree.
     * When resuming a game after saving or loading, make a new currentNodeData.
     * If the player dies before getting to a save point, delete this currentNodeData first (and player can pick on time tree where to resume) */
    public static NodeData currentNodeData = null;

    /* When a level starts, the currentNodeData at the time is copied to this */
    public static NodeData levelStartNodeData = null;

    #region Settings
    public static float sfxVolume = 1;
    public static float musicVolume {
        get { return _musicVolume; }
        set {
            _musicVolume = value;
        }
    }
    public static int saveFileIndexLastUsed = 0;
    public static bool hardModesUnlocked = false;
    public static void loadDefaultSettings() {
        sfxVolume = 1;
        musicVolume = 1;
        saveFileIndexLastUsed = 0;
        hardModesUnlocked = false;
    }
    #endregion
    
    ///////////////
    // FUNCTIONS //
    ///////////////
    
    /* Loads the given level after doing some stuff first. */
    public static void loadLevel(string name) {

        TimeUser.onUnloadLevel();
        VisionUser.onUnloadLevel();
        if (HUD.instance != null) {
            HUD.instance.onUnloadLevel();
        }
        Time.timeScale = 1;
        SoundManager.instance.volumeScale = 1;
        
        // have the black screen cover everything temporarily
        if (HUD.instance != null && HUD.instance.blackScreen != null) {
            HUD.instance.blackScreen.color = new Color(0, 0, 0, 1);
        }
        
        SceneManager.LoadScene(name);

        updateNodeData(currentNodeData);
        levelStartNodeData = NodeData.createNodeData(null, true);
        levelStartNodeData.copyFrom(currentNodeData);
    }

    /* updates the given NodeData with information about the current level */
    public static void updateNodeData(NodeData nodeData) {
        Scene activeScene = SceneManager.GetActiveScene();
        nodeData.level = activeScene.name;
        if (Level.currentLoadedLevel != null) {
            nodeData.levelMapX = Level.currentLoadedLevel.mapX;
            nodeData.levelMapY = Level.currentLoadedLevel.mapY;
        }
        if (Player.instance != null) {
            nodeData.position = Player.instance.rb2d.position;
        }
        if (CountdownTimer.instance != null) {
            nodeData.time = CountdownTimer.instance.time;
        }
    }

    /// <summary>
    /// Called when doing a Chamber Flashback.  Reverts back to the given nodeData, and goes to that level specified.
    /// </summary>
    /// <param name="nodeData">The nodeData to revert to</param>
    public static void revertToNodeData(NodeData nodeData) {
        // get rid of current node data
        NodeData.deleteNode(currentNodeData);
        currentNodeData = null;
        NodeData.deleteNode(levelStartNodeData);
        levelStartNodeData = null;

        // make new current node data from what was given
        currentNodeData = NodeData.createNodeData(nodeData, true);

        // now that the last save was altered, restart from last save
        restartFromLastSave();

        // start room correctly
        Player.instantiateMode = Player.InstantiateMode.CHAMBER_FLASHBACK;
        

    }

    /* Restart level, using info from levelStartNodeData */
    public static void restartLevel() {

        currentNodeData.copyFrom(levelStartNodeData);
        currentNodeData.temporary = true;

        if (Player.instance != null) {
            Player.instance.revertToFrameInfoOnLevelLoad();
        }
        if (CountdownTimer.instance != null) {
            CountdownTimer.instance.time = currentNodeData.time;
        }

        loadLevel(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Restores the last save data and restarts from that level
    /// </summary>
    public static void restartFromLastSave() {
        // restores last node data
        NodeData parent = currentNodeData.parent;
        currentNodeData.copyFrom(parent);
        currentNodeData.parent = parent;
        currentNodeData.temporary = true;
        currentNodeData.children.Clear();
        if (CountdownTimer.instance != null) {
            CountdownTimer.instance.time = currentNodeData.time;
        }
        // destroy player so it's cleanly created when the level loads
        if (Player.instance != null) {
            GameObject.DestroyImmediate(Player.instance.gameObject);
        }
        // goes to the level from the last save
        loadLevel(currentNodeData.level);
    }

    /// <summary>
    /// This is the first function called in the program, called by FirstScene
    /// </summary>
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
        // destroy current node data (which is temporary) if leaving main game
        NodeData.deleteNode(Vars.currentNodeData);
        NodeData.deleteNode(Vars.levelStartNodeData);
        currentNodeData = null;
        levelStartNodeData = null;

        TimeUser.onUnloadLevel();
        VisionUser.onUnloadLevel();
        if (HUD.instance != null) {
            HUD.instance.onUnloadLevel();
        }
        Time.timeScale = 1;
        SoundManager.instance.volumeScale = 1;

        SceneManager.LoadScene("title_scene");
    }

    public static void quitGame() {
        #if UNITY_EDITOR
        // set the PlayMode to stop
        Debug.Log("quit game");
        #else
        Application.Quit();
        #endif
    }

    /* Save data to a file.  Returns true if save was successful */
    public static bool saveData() {
        string path = Application.persistentDataPath + "/data" + saveFileIndex + ".sav";

        string content = "";

        // find checksum
        string data = saveDataToString();
        int checksum = 0;
        for (int i=0; i<data.Length; i++) {
            checksum += data[i];
        }
        // add data and checksum
        content += data;
        content += "\n" + checksum;
        // encrypt
        string encrypted;
        if (encryptSaveData) {
            encrypted = StringEncrypt.encrypt(content);
        } else {
            encrypted = content;
        }
        // add bonus text
        Properties propAsset = new Properties(Resources.Load<TextAsset>("save_data_bonus").text);
        content = propAsset.getString("Default") + "\n" + propAsset.getString("Divider") + "\n" + encrypted;

        byte[] bArr = Utilities.stringToBytes(content);

#if UNITY_WEBPLAYER
        return false;
#else
        try {
            File.WriteAllBytes(path, bArr);
        } catch (Exception e) {
            Debug.LogError("ERROR while saving: " + e.Message);
            return false;
        }

        FileSelectScreen.saveToQuickdata(saveFileIndex, "FILE " + saveFileIndex, difficulty, playTime, infoPercentComplete(), physPercentComplete());

        return true;
#endif

    }

    /// <summary>
    /// Loads data from a file.  If no file is present, calls loadDefaultData() instead.
    /// Also creates Vars.currentNodeData.
    /// This is called in Start() of TitleScreen and Awake() of Level if currentNodeData is null
    /// </summary>
    /// <param name="saveFileIndex">Index of the save file to use.</param>
    /// <returns>false if there was a problem. </returns>
    public static bool loadData(int saveFileIndex = 0) {

        if (saveFileIndex != saveFileIndexLastUsed) {
            saveFileIndexLastUsed = saveFileIndex;
            saveSettings();
        }

        #if UNITY_WEBPLAYER
        loadDefaultData(saveFileIndex);
        return true;
        #endif

        #if !UNITY_WEBPLAYER

        string path = Application.persistentDataPath + "/data" + saveFileIndex + ".sav";

        if (!File.Exists(path)) {
            loadDefaultData(saveFileIndex);
            return true;
        }

        byte[] bArr = File.ReadAllBytes(path);
        string content = Utilities.bytesToString(bArr);
        // strip away bonus text and divider (to be more accurate, stripping away the first two lines)
        int index = content.IndexOf('\n', content.IndexOf('\n')+1);
        content = content.Substring(index + 1);
        // decrypt
        string decrypted;
        if (encryptSaveData) {
            decrypted = StringEncrypt.decrypt(content);
        } else {
            decrypted = content;
        }
        // trim checksum
        index = decrypted.LastIndexOf('\n');
        if (index == -1) return false;
        string checksumStr = decrypted.Substring(index+1).Trim();
        decrypted = decrypted.Substring(0, index);
        // validate checksum
        int checksum = 0;
        bool worked = int.TryParse(checksumStr, out checksum);
        if (!worked) {
            Debug.Log("Extacting checksum didn't work");
            return false;
        }
        int test = 0;
        for (int i=0; i<decrypted.Length; i++) {
            test += decrypted[i];
        }
        if (test != checksum) {
            Debug.Log("Save data does not match checksum");
            Debug.Log("test: " + test + " checksum: " + checksum);
            return false;
        }
        
        loadDataFromString(decrypted);
        return true;

        #endif
    }

    public static void deleteData(int saveFileIndex) {

#if !UNITY_WEBPLAYER
        string path = Application.persistentDataPath + "/data" + saveFileIndex + ".sav";
        if (File.Exists(path)) {
            File.Delete(path);
        }
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

    private static float _musicVolume = 1;

    /* LOADING DEFAULT DATA */
    /// <summary>
    /// Loads the default data, what the player gets if no save data is already created.  Should be the start of the game.
    /// </summary>
    /// <param name="saveFileIndex"></param>
    static void loadDefaultData(int saveFileIndex = 0) {
        // save file index
        Vars.saveFileIndex = saveFileIndex;

        // moved everything to its own file for easier editing
        VarsLoadData.loadDefaultSaveData();
        
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
        // play time, difficulty, tutorials enabled
        string[] miscStrs = strs[4].Split(delims2);
        playTime = float.Parse(miscStrs[0]);
        difficulty = (Difficulty)int.Parse(miscStrs[1]);
        tutorialsEnabled = miscStrs[2] == "1";
        // all node data
        NodeData.loadAllNodesFromString(strs[5]);
        // current node data
        NodeData savedCurrentNodeData = NodeData.nodeDataFromID(int.Parse(strs[6]));
        if (savedCurrentNodeData == null) {
            currentNodeData = null;
            Debug.LogWarning("WARNING: currentNodeData is null");
        } else {
            // make currentNodeData a temporary clone of the saved current node data
            if (currentNodeData != null && currentNodeData != savedCurrentNodeData) {
                NodeData.deleteNode(currentNodeData);
            }
            currentNodeData = NodeData.createNodeData(savedCurrentNodeData, true);
        }
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
        // current objective conversation
        TalkPage.setCurrentObjectiveFile(strs[9]);
        // all talk conversations
        TalkPage.loadAllConversationsFromString(strs[10]);
        // pause screen lastPageOpened, mode, countdown timer visible, mode
        string[] strsP = strs[11].Split(delims2);
        PauseScreen.lastPageOpened = (PauseScreen.Page)int.Parse(strsP[0]);
        PauseScreen.mode = (PauseScreen.Mode)int.Parse(strsP[1]);
        CountdownTimer.staticVisible = strsP[2] == "1";
        CountdownTimer.staticMode = (CountdownTimer.Mode)int.Parse(strsP[3]);
        // orbs found
        orbsFound.Clear();
        string[] ofStrs = strs[12].Split(delims2);
        for (int i = 0; i < ofStrs.Length; i++) {
            if (ofStrs[i] == "") continue;
            orbsFound.Add((PhysicalUpgrade.Orb)int.Parse(ofStrs[i]));
        }
        // booster found
        boosterFound = (strs[13] == "1");
        // health upgrades found
        healthUpgradesFound.Clear();
        string[] huStrs = strs[14].Split(delims2);
        for (int i = 0; i < huStrs.Length; i++) {
            if (huStrs[i] == "") continue;
            healthUpgradesFound.Add((PhysicalUpgrade.HealthUpgrade)int.Parse(huStrs[i]));
        }
        // creature cards found
        creatureCardsFound.Clear();
        string[] ccStrs = strs[15].Split(delims2);
        for (int i = 0; i < ccStrs.Length; i++) {
            if (ccStrs[i] == "") continue;
            creatureCardsFound.Add(int.Parse(ccStrs[i]));
        }
        // map
        if (MapUI.instance == null) {
            MapUI.tempGridString = strs[16];
        } else {
            MapUI.instance.gridFromString(strs[16]);
        }
        // map icons
        if (MapUI.instance == null) {
            MapUI.tempIconString = strs[17];
        } else {
            MapUI.instance.iconsFromString(strs[17]);
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
        modifiedDate = DateTime.Now;
        ret += modifiedDate.ToString() + "\n";
        // play time, difficulty, tutorials enabled (4)
        ret += playTime + "," + ((int)difficulty) + "," + (tutorialsEnabled ? "1" : "0") + "\n";
        // all node data (5)
        ret += NodeData.saveAllNodesToString() + "\n";
        // current node data (6)
        if (currentNodeData == null) ret += "0\n";
        else {
            if (currentNodeData.temporary) { // temporary node datas aren't saved.  We're interested in its parent
                if (currentNodeData.parent == null || currentNodeData.parent.temporary) {
                    Debug.LogError("ERROR: Error saving currentNodeData");
                } else {
                    ret += currentNodeData.parent.id + "\n";
                }
            } else {
                ret += currentNodeData.id + "\n";
            }
        }
        // decryptors (7)
        for (int i=0; i<decryptors.Count; i++) {
            ret += (int)decryptors[i];
            if (i < decryptors.Count - 1)
                ret += ",";
        }
        ret += "\n";
        // info events (8)
        for (int i = 0; i < infoEvents.Count; i++) {
            ret += (int)infoEvents[i];
            if (i < infoEvents.Count - 1)
                ret += ",";
        }
        ret += "\n";
        // current objective conversation (9)
        ret += TalkPage.currentObjectiveFile;
        ret += "\n";
        // all talk conversations (10)
        ret += TalkPage.saveAllConversationsToString();
        ret += "\n";
        // pause screen lastPageOpened, mode, countdown timer visible, mode (11)
        ret += (int)PauseScreen.lastPageOpened + "," + (int)PauseScreen.mode + "," + (CountdownTimer.staticVisible ? "1" : "0") + "," + (int)CountdownTimer.staticMode;
        ret += "\n";
        // orbs found (12)
        for (int i = 0; i < orbsFound.Count; i++) {
            ret += ((int)orbsFound[i]);
            if (i < orbsFound.Count - 1)
                ret += ",";
        }
        ret += "\n";
        // booster found (13)
        if (boosterFound) ret += "1"; else ret += "0";
        ret += "\n";
        // health upgrades found (14)
        for (int i = 0; i < healthUpgradesFound.Count; i++) {
            ret += ((int)healthUpgradesFound[i]);
            if (i < healthUpgradesFound.Count - 1)
                ret += ",";
        }
        ret += "\n";
        // creature cards found (15)
        for (int i = 0; i < creatureCardsFound.Count; i++) {
            ret += creatureCardsFound[i];
            if (i < creatureCardsFound.Count - 1)
                ret += ",";
        }
        ret += "\n";
        // map (16)
        if (MapUI.instance == null) {
            ret += "";
        } else {
            ret += MapUI.instance.gridToString();
        }
        ret += "\n";
        // map icons (17)
        if (MapUI.instance == null) {
            ret += "";
        } else {
            ret += MapUI.instance.iconsStr;
        }

        return ret;
    }

    /* SAVING SETTINGS */

    static string saveSettingsToString() {
        Properties prop = new Properties();
        prop.setFloat("sfx_volume", sfxVolume);
        prop.setFloat("music_volume", musicVolume);
        prop.setInt("vsync", QualitySettings.vSyncCount);
        prop.setInt("save_file_last_used", saveFileIndexLastUsed);
        if (hardModesUnlocked) {
            prop.setBool("hard_modes", hardModesUnlocked);
        }
        return prop.convertToString();
    }
    
    static void loadSettingsFromString(string str) {

        Properties prop = new Properties(str);
        sfxVolume = prop.getFloat("sfx_volume", sfxVolume);
        musicVolume = prop.getFloat("music_volume", musicVolume);
        int int0 = prop.getInt("vsync", QualitySettings.vSyncCount);
        if (int0 != QualitySettings.vSyncCount)
            QualitySettings.vSyncCount = int0;
        saveFileIndexLastUsed = prop.getInt("save_file_last_used", saveFileIndexLastUsed);
        hardModesUnlocked = prop.getBool("hard_modes", hardModesUnlocked);
        
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
