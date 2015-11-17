using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Vars {

    public static bool arcadeMode = true;
    public static int highScore = 0;
    public static bool screenshotMode = false;

    // SETTINGS

    public static float sfxVolume = 1;
    public static float musicVolume { get { return _musicVolume; }
        set {
            _musicVolume = value;
        }
    }
    
    public static void loadDefaultSettings() {
        sfxVolume = 1;
        musicVolume = 1;
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    public static void startGame() {
        if (startGameCalled) return;
        loadSettings();

        startGameCalled = true;
    }

    /* Loads the given level after doing some stuff first. */
    public static void loadLevel(string name) {

        TimeUser.onUnloadLevel();
        VisionUser.onUnloadLevel();
        Time.timeScale = 1;
        SoundManager.instance.volumeScale = 1;

        // should be called at the title screen
        startGame();
        

        Application.LoadLevel(name);
    }

    public static void restartLevel() {
        loadLevel(Application.loadedLevelName);
    }

    public static void goToTitleScreen() {
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

    /////////////
    // PRIVATE //
    /////////////

    private static bool startGameCalled = false;
    private static float _musicVolume = 1;

    /* SAVING SETTINGS
     * */

    static string saveSettingsToString() {
        string str = "";
        str += "sfx_volume = " + sfxVolume + "\n";
        str += "music_volume = " + musicVolume + "\n";
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
