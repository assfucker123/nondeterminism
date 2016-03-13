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
    public static List<PhysicalUpgrade.Orb> orbsFound = new List<PhysicalUpgrade.Orb>();
    public static bool orbFound(PhysicalUpgrade.Orb orb) {
        return orbsFound.IndexOf(orb) != -1;
    }
    public static void orbFind(PhysicalUpgrade.Orb orb) {
        if (orbFound(orb)) return;
        orbsFound.Add(orb);
    }
    public static bool boosterFound = false;
    public static List<PhysicalUpgrade.HealthUpgrade> healthUpgradesFound = new List<PhysicalUpgrade.HealthUpgrade>();
    public static bool healthUpgradeFound(PhysicalUpgrade.HealthUpgrade healthUpgrade) {
        return healthUpgradesFound.IndexOf(healthUpgrade) != -1;
    }
    public static void healthUpgradeFind(PhysicalUpgrade.HealthUpgrade healthUpgrade) {
        if (healthUpgradeFound(healthUpgrade)) return;
        healthUpgradesFound.Add(healthUpgrade);
    }
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
    public static string currentLevel {  get { return SceneManager.GetActiveScene().name; } }
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

    // TIME-DEPENDENT
    /* Keeps changing based on what the player does.  Note this is temporary, so it's not part of the time tree.
     * When resuming a game after saving or loading, make a new currentNodeData.
     * If the player dies before getting to a save point, delete this currentNodeData first (and player can pick on time tree where to resume) */
    public static NodeData currentNodeData = null;

    /* When a level starts, the currentNodeData at the time is copied to this */
    public static NodeData levelStartNodeData = null;

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
        if (HUD.instance != null) {
            HUD.instance.onUnloadLevel();
        }
        Time.timeScale = 1;
        SoundManager.instance.volumeScale = 1;

        // should be called at the title screen
        if (!startGameCalled) {
            startGame();
        }

        // have the black screen cover everything temporarily
        if (HUD.instance != null && HUD.instance.blackScreen != null) {
            HUD.instance.blackScreen.color = new Color(0, 0, 0, 1);
        }
        
        SceneManager.LoadScene(name);

        updateNodeData(currentNodeData);
        levelStartNodeData = NodeData.createNodeData(currentNodeData, true);
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
    }

    /* Restart level, using info from levelStartNodeData */
    public static void restartLevel() {

        currentNodeData.copyFrom(levelStartNodeData);

        if (Player.instance != null) {
            Player.instance.revertToFrameInfoOnLevelLoad();
        }

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
        // destroy current node data (which is temporary) if leaving main game
        NodeData.deleteNode(Vars.currentNodeData);
        NodeData.deleteNode(Vars.levelStartNodeData);
        Vars.currentNodeData = null;
        Vars.levelStartNodeData = null;

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

    /* Save data to a file */
    public static void saveData() {
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
        content += checksum;
        // encrypt
        string encrypted = StringEncrypt.encrypt(content);
        // add bonus text
        Properties propAsset = new Properties(Resources.Load<TextAsset>("save_data_bonus").text);
        content = propAsset.getString("Default") + "\n" + propAsset.getString("Divider") + "\n" + encrypted;

        byte[] bArr = Utilities.stringToBytes(content);

        #if !UNITY_WEBPLAYER
        File.WriteAllBytes(path, bArr);
        #endif
    }

    /* Load data from a file.
     * Returns false if there was a problem. */
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

        Debug.Log("loading encrypted data has not been tested yet");
        byte[] bArr = File.ReadAllBytes(path);
        string content = Utilities.bytesToString(bArr);
        // strip away bonus text and divider
        int index = content.IndexOf('\n', content.IndexOf('\n'));
        content = content.Substring(index + 1);
        // decrypt
        string decrypted = StringEncrypt.decrypt(content);
        // trim checksum
        index = decrypted.LastIndexOf('\n');
        if (index == -1) return false;
        string checksumStr = decrypted.Substring(index+1).Trim();
        decrypted = decrypted.Substring(0, index);
        // validate checksum
        int checksum = 0;
        bool worked = int.TryParse(checksumStr, out checksum);
        if (!worked) return false;
        int test = 0;
        for (int i=0; i<decrypted.Length; i++) {
            test += decrypted[i];
        }
        if (test != checksum) return false;
        
        loadDataFromString(Utilities.bytesToString(bArr));
        return true;

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
        currentNodeData = NodeData.createNodeData(null, true);
        currentNodeData.time = 0;
        currentNodeData.level = "tut_ship_1";
        currentNodeData.levelMapX = 1;
        currentNodeData.levelMapY = 0;
        currentNodeData.position.Set(33, 14);
        currentNodeData.orbs.Clear();
        currentNodeData.hasBooster = false;
        currentNodeData.healthUpgrades.Clear();
        currentNodeData.phaseReplacements = 0;
        currentNodeData.physicalEvents.Clear();
        currentNodeData.levelsAmbushesDefeated.Clear();
        currentNodeData.objectsDestroyed.Clear();
        // decryptors
        decryptors.Clear();
        // info events
        infoEvents.Clear();
        // current objective conversation
        TalkPage.setCurrentObjectiveFile("co_first_tutorial");
        // all talk conversations
        TalkPage.conversations.Clear();
        TalkPage.addConversationNoAlert("Standard Conversations", "c_finish_sentences", false, false);
        TalkPage.addConversationNoAlert("Oracle's Flashbacks", "c_oracle_vision", true, false);
        TalkPage.addConversationNoAlert("(help) Basic Controls", "help_basic_controls", false, true);
        TalkPage.addConversationNoAlert("(help) Visions", "help_vision", false, true);
        TalkPage.addConversationNoAlert("(help) Flashback", "help_flashback", false, true);
        // pause screen lastPageOpened, mode
        PauseScreen.lastPageOpened = PauseScreen.Page.TALK;
        PauseScreen.mode = PauseScreen.Mode.TUTORIAL;
        // orbs found
        orbsFound.Clear();
        // booster found
        boosterFound = false;
        // health upgrades found
        healthUpgradesFound.Clear();
        // creature cards found
        creatureCardsFound.Clear();


        // for testing
#if UNITY_EDITOR

        ///*
        collectDecryptor(Decryptor.ID.CHARGE_SHOT);
        //collectDecryptor(Decryptor.ID.ALTERED_SHOT);

        /*
        currentNodeData.creatureCardCollect("Sealime");
        currentNodeData.creatureCardCollect("Ciurivy");
        currentNodeData.creatureCardCollect("Smosey");
        currentNodeData.creatureCardCollect("Magoom");
        currentNodeData.creatureCardCollect("Pengrunt");
        creatureCardFind("Vengemole");
        currentNodeData.creatureCardCollect("Toucade");
        currentNodeData.creatureCardCollect("Sherivice");
        */
        //eventHappen(AdventureEvent.Info.FOUND_CREATURE_CARD);

        #endif


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
        // current objective conversation
        TalkPage.setCurrentObjectiveFile(strs[9]);
        // all talk conversations
        TalkPage.loadAllConversationsFromString(strs[10]);
        // pause screen lastPageOpened, mode
        string[] strsP = strs[11].Split(delims2);
        PauseScreen.lastPageOpened = (PauseScreen.Page)int.Parse(strsP[0]);
        PauseScreen.mode = (PauseScreen.Mode)int.Parse(strsP[1]);
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
        ret += "\n";
        // current objective conversation (9)
        ret += TalkPage.currentObjectiveFile;
        ret += "\n";
        // all talk conversations (10)
        ret += TalkPage.saveAllConversationsToString();
        ret += "\n";
        // pause screen lastPageOpened, mode (11)
        ret += PauseScreen.lastPageOpened + "," + PauseScreen.mode;
        ret += "/n";
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


        return ret;
    }

    /* SAVING SETTINGS */

    static string saveSettingsToString() {
        string str = "";
        str += "sfx_volume = " + sfxVolume + "\n";
        str += "music_volume = " + musicVolume + "\n";
        str += "vsync = " + QualitySettings.vSyncCount + "\n";
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
            } else if (name == "vsync") {
                int int0 = int.Parse(value);
                if (int0 != QualitySettings.vSyncCount)
                    QualitySettings.vSyncCount = int0;
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
