using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class FileSelectScreen : MonoBehaviour {

    public float fadeInDuration = .4f;
    public float fadeOutDuration = .4f;
    public float fadeDistance = 600;
    public Vector2 centerPos = new Vector2();
    public float verticalSpacing = 100;
    public float timeDiff = .2f;
    public float whiteScreenDuration = .4f;
    public Vector2 settingsSelectionOffset = new Vector2();
    public GameObject fileSelectGameObject;
    public Sprite startButtonDefaultSprite;
    public Sprite startButtonSelectedSprite;
    public AudioClip switchSound;
    public AudioClip selectConfirmSound;
    public AudioClip fadeInSound;
    public AudioClip whiteScreenSound;
    public TextAsset textAsset;

    public const int NUM_SAVE_FILES = 4;

    public State state { get; private set; }

    public int selectionIndex {
        get { return _selectionIndex; }
        set {
            if (_selectionIndex == value) return;
            if (fileSelects.Count == 0) return;
            int val = value;
            if (val < 0) val = fileSelects.Count - 1;
            if (val >= fileSelects.Count) val = 0;
            // unselect previous
            if (_selectionIndex >= 0 && _selectionIndex < fileSelects.Count) {
                fileSelects[_selectionIndex].selected = false;
            }
            // select new
            _selectionIndex = val;
            fileSelects[_selectionIndex].selected = true;
            selection.GetComponent<RectTransform>().localPosition = fileSelects[_selectionIndex].position;
            if (fileSelects[_selectionIndex].newFile) {
                selection.GetComponent<Animator>().Play("newFile");
                deleteFile.enabled = false;
            } else {
                selection.GetComponent<Animator>().Play("normal");
                deleteFile.enabled = true;
            }
        }
    }

    public enum State {
        HIDE,
        FADE_IN,
        SHOW,
        FADE_OUT,
        WHITE_SCREEN_TRANSITION
    }

    public void hide(bool immediately) {
        state = State.FADE_OUT;
        if (immediately) {
            time = fadeOutDuration - .01f;
        } else {
            time = 0;
        }
    }

    public void fadeIn() {
        if (state != State.HIDE) return;
        time = 0;
        state = State.FADE_IN;
        image.enabled = true;
        image.color = new Color(1, 1, 1, 0);
        createFileSelects();
        SoundManager.instance.playSFX(fadeInSound);
    }

    public void fadeOut() {
        if (state != State.SHOW) return;
        time = 0;
        state = State.FADE_OUT;
        selection.enabled = false;
        deleteFile.enabled = false;
        deleteHoldTime = 0;
        _selectionIndex = -1;
    }

    public void showError(string message) {
        errorImage.enabled = true;
        errorImage.transform.SetAsLastSibling();
        errorText.setText(message);
    }

    public void hideError() {
        errorImage.enabled = false;
        errorText.makeAllCharsInvisible();
    }

    public void showSettings() {
        if (settingsShown) return;
        settingsHeaderBox.setPlainText(properties.getString("new_file_settings") + "\n" + properties.getString("no_change"));
        setSettingsDifficulty(Vars.Difficulty.STANDARD);
        setSettingsTutorials(true);
        settingsStartButton.enabled = true;
        settingsStartButton.sprite = startButtonDefaultSprite;
        settingsShown = true;

        // also hide file selects
        foreach (FileSelect fs in fileSelects) {
            fs.GetComponent<RectTransform>().localPosition = new Vector2(fs.GetComponent<RectTransform>().localPosition.x, fs.GetComponent<RectTransform>().localPosition.y - 1000);
        }
        selection.GetComponent<RectTransform>().localPosition = new Vector2(selection.GetComponent<RectTransform>().localPosition.x, selection.GetComponent<RectTransform>().localPosition.y - 1000);

        // set settings selection
        setSettingsSelection(SettingsSelection.DIFFICULTY);
    }

    public void hideSettings() {
        if (!settingsShown) return;
        settingsHeaderBox.makeAllCharsInvisible();
        settingsDifficultyBox.makeAllCharsInvisible();
        settingsDiffDescripBox.makeAllCharsInvisible();
        settingsTutorialsBox.makeAllCharsInvisible();
        settingsTutDescripBox.makeAllCharsInvisible();
        settingsStartButton.enabled = false;
        settingsSelection.enabled = false;
        settingsShown = false;
        deleteHoldTime = 0;

        // also show file selects
        foreach (FileSelect fs in fileSelects) {
            fs.GetComponent<RectTransform>().localPosition = new Vector2(fs.GetComponent<RectTransform>().localPosition.x, fs.GetComponent<RectTransform>().localPosition.y + 1000);
        }
        selection.GetComponent<RectTransform>().localPosition = new Vector2(selection.GetComponent<RectTransform>().localPosition.x, selection.GetComponent<RectTransform>().localPosition.y + 1000);
    }

    public void setSettingsDifficulty(Vars.Difficulty difficulty) {
        string diffName = difficultyProperties.getString(((int)difficulty)+"name");
        // apply padding to name
        while (diffName.Length < 14) {
            if (diffName.Length % 2 == 0)
                diffName = " " + diffName;
            else
                diffName = diffName + " ";
        }
        // carats
        string left = "<";
        //if (difficulty == Vars.Difficulty.EASY) left = " ";
        string right = ">";
        //if (difficulty == Vars.Difficulty.CRUEL || (!Vars.hardModesUnlocked && difficulty == Vars.Difficulty.STANDARD)) right = " ";

        string diffStr = properties.getString("difficulty") + ": " + left + diffName + right;
        settingsDifficultyBox.setPlainText(diffStr);

        // description
        string diffDescrip = difficultyProperties.getString(((int)difficulty)+"description");
        settingsDiffDescripBox.setPlainText(diffDescrip);

        settingsDifficulty = difficulty;
    }

    public void setSettingsTutorials(bool enabled) {
        string tutName = "";
        if (enabled) tutName = properties.getString("enabled");
        else tutName = properties.getString("disabled");
        // apply padding to name
        while (tutName.Length < 12) {
            if (tutName.Length % 2 == 0)
                tutName = " " + tutName;
            else
                tutName = tutName + " ";
        }
        // carats
        string left = "<";
        //if (enabled) left = " ";
        string right = ">";
        //if (!enabled) right = " ";

        string tutStr = properties.getString("tutorials") + ": " + left + tutName + right;
        settingsTutorialsBox.setPlainText(tutStr);

        // description
        string tutDescrip = "";
        if (enabled) tutDescrip = properties.getString("tutorial_on");
        else tutDescrip = properties.getString("tutorial_off");
        settingsTutDescripBox.setPlainText(tutDescrip);

        settingsTutorials = enabled;
    }

    void setSettingsSelection(SettingsSelection selection) {
        // previous selection
        switch (settingsSelectionIndex) {
        case SettingsSelection.DIFFICULTY:
            settingsDifficultyBox.setColor(PauseScreen.DEFAULT_COLOR);
            break;
        case SettingsSelection.TUTORIALS:
            settingsTutorialsBox.setColor(PauseScreen.DEFAULT_COLOR);
            break;
        case SettingsSelection.START:
            settingsStartButton.sprite = startButtonDefaultSprite;
            break;
        }
        // current selection
        settingsSelectionIndex = selection;
        switch (selection) {
        case SettingsSelection.DIFFICULTY:
            settingsDifficultyBox.setColor(PauseScreen.SELECTED_COLOR);
            settingsSelection.enabled = true;
            settingsSelection.GetComponent<RectTransform>().localPosition = settingsDifficultyBox.GetComponent<RectTransform>().localPosition + new Vector3(settingsSelectionOffset.x, settingsSelectionOffset.y);
            break;
        case SettingsSelection.TUTORIALS:
            settingsTutorialsBox.setColor(PauseScreen.SELECTED_COLOR);
            settingsSelection.enabled = true;
            settingsSelection.GetComponent<RectTransform>().localPosition = settingsTutorialsBox.GetComponent<RectTransform>().localPosition + new Vector3(settingsSelectionOffset.x, settingsSelectionOffset.y);
            break;
        case SettingsSelection.START:
            settingsStartButton.sprite = startButtonSelectedSprite;
            settingsSelection.enabled = false;
            break;
        }
    }

	void Awake() {
        image = GetComponent<Image>();
        selection = transform.Find("Selection").GetComponent<Image>();
        errorImage = transform.Find("Error").GetComponent<Image>();
        errorText = errorImage.transform.Find("GlyphBox").GetComponent<GlyphBox>();
        deleteFile = transform.Find("DeleteFile").GetComponent<Image>();
        Transform settingsT = transform.Find("Settings");
        settingsHeaderBox = settingsT.Find("HeaderBox").GetComponent<GlyphBox>();
        settingsDifficultyBox = settingsT.Find("DifficultyBox").GetComponent<GlyphBox>();
        settingsDiffDescripBox = settingsT.Find("DiffDescripBox").GetComponent<GlyphBox>();
        settingsTutorialsBox = settingsT.Find("TutorialsBox").GetComponent<GlyphBox>();
        settingsTutDescripBox = settingsT.Find("TutDescripBox").GetComponent<GlyphBox>();
        settingsStartButton = settingsT.Find("StartButton").GetComponent<Image>();
        settingsSelection = settingsT.Find("Selection").GetComponent<Image>();
        whiteScreen = transform.Find("WhiteScreen").GetComponent<Image>();
        whiteScreen.enabled = false;
        properties = new Properties(textAsset.text);
        difficultyProperties = new Properties((Resources.Load("difficulty_info") as TextAsset).text);

        image.enabled = false;
        selection.enabled = false;
        deleteFile.enabled = false;
        errorImage.enabled = false;
        errorText.makeAllCharsInvisible();

        settingsHeaderBox.makeAllCharsInvisible();
        settingsDifficultyBox.makeAllCharsInvisible();
        settingsDiffDescripBox.makeAllCharsInvisible();
        settingsTutorialsBox.makeAllCharsInvisible();
        settingsTutDescripBox.makeAllCharsInvisible();
        settingsSelection.enabled = false;
        settingsStartButton.enabled = false;
	}

    void Start() {
        settingsHeaderBox.setColor(PauseScreen.DEFAULT_COLOR);
        settingsDifficultyBox.setColor(PauseScreen.DEFAULT_COLOR);
        settingsDiffDescripBox.setColor(PauseScreen.DEFAULT_COLOR);
        settingsTutorialsBox.setColor(PauseScreen.DEFAULT_COLOR);
        settingsTutDescripBox.setColor(PauseScreen.DEFAULT_COLOR);
    }
	
	void Update() {
        
        time += Time.unscaledDeltaTime;
        float duration = 0;
        FileSelect fs;

        switch (state) {
        case State.HIDE:
            break;
        case State.FADE_IN:
            duration = fadeInDuration + timeDiff * (fileSelects.Count - 1);
            // fade in screen
            image.color = new Color(1, 1, 1, Utilities.easeLinearClamp(time, 0, 1, duration));
            // animate file selects
            for (int i=0; i<fileSelects.Count; i++) {
                fs = fileSelects[i];
                fs.position = Utilities.easeOutQuadClamp(time - fs.timeOffset, fs.startPosition, fs.endPosition - fs.startPosition, fadeInDuration);
            }
            // end state
            if (time >= duration) {
                state = State.SHOW;
                deleteHoldTime = 0;
                // make selection
                selection.enabled = true;
                int firstSelection = Vars.saveFileIndexLastUsed;
                if (firstSelection < 0 || firstSelection >= fileSelects.Count) {
                    firstSelection = 0;
                }
                selectionIndex = firstSelection;
            }
            break;
        case State.SHOW:

            if (errorShown) {
                if (backPressed || confirmPressed) {
                    hideError();
                }
            } else if (settingsShown) {

                if (Keys.instance.leftPressed) {
                    switch (settingsSelectionIndex) {
                    case SettingsSelection.DIFFICULTY:
                        switch (settingsDifficulty) {
                        case Vars.Difficulty.EASY:
                            if (Vars.hardModesUnlocked) setSettingsDifficulty(Vars.Difficulty.CRUEL);
                            else setSettingsDifficulty(Vars.Difficulty.STANDARD);
                            break;
                        case Vars.Difficulty.TRADITIONAL: setSettingsDifficulty(Vars.Difficulty.EASY); break;
                        case Vars.Difficulty.STANDARD: setSettingsDifficulty(Vars.Difficulty.TRADITIONAL); break;
                        case Vars.Difficulty.HARD: setSettingsDifficulty(Vars.Difficulty.STANDARD); break;
                        case Vars.Difficulty.CRUEL: setSettingsDifficulty(Vars.Difficulty.HARD); break;
                        }
                        SoundManager.instance.playSFX(switchSound);
                        break;
                    case SettingsSelection.TUTORIALS:
                        setSettingsTutorials(!settingsTutorials);
                        SoundManager.instance.playSFX(switchSound);
                        break;
                    }
                } else if (Keys.instance.rightPressed) {
                    switch (settingsSelectionIndex) {
                    case SettingsSelection.DIFFICULTY:
                        switch (settingsDifficulty) {
                        case Vars.Difficulty.EASY: setSettingsDifficulty(Vars.Difficulty.TRADITIONAL);  break;
                        case Vars.Difficulty.TRADITIONAL: setSettingsDifficulty(Vars.Difficulty.STANDARD); break;
                        case Vars.Difficulty.STANDARD:
                            if (Vars.hardModesUnlocked) setSettingsDifficulty(Vars.Difficulty.HARD);
                            else setSettingsDifficulty(Vars.Difficulty.EASY);
                            break;
                        case Vars.Difficulty.HARD: setSettingsDifficulty(Vars.Difficulty.CRUEL); break;
                        case Vars.Difficulty.CRUEL: setSettingsDifficulty(Vars.Difficulty.EASY); break;
                        }
                        SoundManager.instance.playSFX(switchSound);
                        break;
                    case SettingsSelection.TUTORIALS:
                        setSettingsTutorials(!settingsTutorials);
                        SoundManager.instance.playSFX(switchSound);
                        break;
                    }
                } else if (Keys.instance.upPressed) {
                    switch (settingsSelectionIndex) {
                    case SettingsSelection.DIFFICULTY:
                        setSettingsSelection(SettingsSelection.START);
                        break;
                    case SettingsSelection.TUTORIALS:
                        setSettingsSelection(SettingsSelection.DIFFICULTY);
                        break;
                    case SettingsSelection.START:
                        setSettingsSelection(SettingsSelection.TUTORIALS);
                        break;
                    }
                    SoundManager.instance.playSFX(switchSound);
                } else if (Keys.instance.downPressed) {
                    switch (settingsSelectionIndex) {
                    case SettingsSelection.DIFFICULTY:
                        setSettingsSelection(SettingsSelection.TUTORIALS);
                        break;
                    case SettingsSelection.TUTORIALS:
                        setSettingsSelection(SettingsSelection.START);
                        break;
                    case SettingsSelection.START:
                        setSettingsSelection(SettingsSelection.DIFFICULTY);
                        break;
                    }
                    SoundManager.instance.playSFX(switchSound);
                } else if (Keys.instance.backPressed) {
                    hideSettings();
                } else if (confirmPressed && settingsSelectionIndex == SettingsSelection.START) {
                    beginNewFile(selectionIndex, settingsDifficulty, settingsTutorials);
                }

            } else { // nothing shown

                if (deleteFile.enabled && Input.GetKey(KeyCode.A)) {
                    deleteHoldTime += Time.unscaledDeltaTime;
                    if (deleteHoldTime > 4) {
                        deleteFileAction(selectionIndex);
                        deleteFile.enabled = false;
                        deleteHoldTime = 0;
                    }
                } else {
                    deleteHoldTime = 0;
                }
                
                // move selection
                if (Keys.instance.upPressed) {
                    selectionIndex--;
                    deleteHoldTime = 0;
                    SoundManager.instance.playSFX(switchSound);
                } else if (Keys.instance.downPressed) {
                    selectionIndex++;
                    deleteHoldTime = 0;
                    SoundManager.instance.playSFX(switchSound);
                } else if (confirmPressed) {
                    deleteHoldTime = 0;
                    // selecting file
                    if (fileSelects[selectionIndex].newFile) {
                        SoundManager.instance.playSFX(selectConfirmSound);
                        if (Vars.promptDifficulty) {
                            showSettings();
                        } else {
                            beginNewFile(selectionIndex, Vars.Difficulty.STANDARD, true);
                        }
                    } else {
                        beginFile(selectionIndex);
                    }
                } else if (Keys.instance.backPressed) {
                    // back
                    fadeOut();
                }
            }
            
            

            break;
        case State.FADE_OUT:

            duration = fadeOutDuration + timeDiff * (fileSelects.Count - 1);
            // fade in screen
            image.color = new Color(1, 1, 1, Utilities.easeLinearClamp(time, 1, -1, duration));
            // animate file selects
            for (int i = 0; i < fileSelects.Count; i++) {
                fs = fileSelects[i];
                fs.position = Utilities.easeOutQuadClamp(time - fileSelects[fileSelects.Count-1-i].timeOffset, fs.endPosition, fs.startPosition - fs.endPosition, fadeOutDuration);
            }
            // end state
            if (time >= duration) {
                state = State.HIDE;
                image.enabled = false;
            }
            
            break;
        case State.WHITE_SCREEN_TRANSITION:
            // data is loaded.  Go to the game when the transition ends
            whiteScreen.color = new Color(1, 1, 1, Utilities.easeLinearClamp(time, 0, 1, whiteScreenDuration));
            if (time >= whiteScreenDuration) {
                string levelName = Vars.currentNodeData.level;
                Vars.loadLevel(levelName);
            }
            break;
        }

	}

    void OnDestroy() {
        clearFileSelects();
    }

    void deleteFileAction(int fileIndex) {
        Vars.deleteData(fileIndex);
        removeFromQuickdata(fileIndex);
        fileSelects[fileIndex].newFile = true;
        deleteFile.enabled = false;
        selection.GetComponent<Animator>().Play("newFile");
    }

    void beginNewFile(int fileIndex, Vars.Difficulty difficulty, bool tutorials) {
        Vars.saveFileIndexLastUsed = fileIndex;
        Vars.loadData(fileIndex);
        Vars.difficulty = difficulty;
        Vars.tutorialsEnabled = tutorials;
        whiteScreenTransition();
    }

    void beginFile(int fileIndex) {
        Vars.saveFileIndexLastUsed = fileIndex;
        bool error = !Vars.loadData(fileIndex);
        
        if (error) {
            showError(properties.getString("error") + " " + properties.getString("file") + " " + fileIndex);
        } else {
            whiteScreenTransition();
        }
    }

    void whiteScreenTransition() {
        if (state != State.SHOW) return;
        state = State.WHITE_SCREEN_TRANSITION;
        time = 0;
        whiteScreen.enabled = true;
        whiteScreen.color = new Color(1, 1, 1, 0);
        whiteScreen.transform.SetAsLastSibling();
        SoundManager.instance.playSFX(whiteScreenSound);
        SoundManager.instance.fadeOutMusic();
    }

    public static void saveToQuickdata(int fileIndex, string fileName, Vars.Difficulty difficulty, float time, float infoPercent, float physPercent) {

#if UNITY_WEBPLAYER
        return;
#endif

        Properties quickdataProp = new Properties();
        string path = Application.persistentDataPath + "/quickData.sav";
        bool fileExists = File.Exists(path);
        if (fileExists) {
            byte[] bArr = File.ReadAllBytes(path);
            string content = Utilities.bytesToString(bArr);
            quickdataProp.parse(content);
        }

        quickdataProp.setString("fn" + fileIndex, fileName);
        quickdataProp.setInt("diff" + fileIndex, ((int)difficulty));
        quickdataProp.setFloat("t" + fileIndex, time);
        quickdataProp.setFloat("info" + fileIndex, infoPercent);
        quickdataProp.setFloat("phys" + fileIndex, physPercent);

        byte[] bArr2 = Utilities.stringToBytes(quickdataProp.convertToString());

        File.WriteAllBytes(path, bArr2);

    }

    /// <summary>
    /// removes data from the quickData file, then saves it
    /// </summary>
    /// <param name="fileIndex"></param>
    public static void removeFromQuickdata(int fileIndex) {
#if UNITY_WEBPLAYER
        return;
#endif

        Properties quickdataProp = new Properties();
        string path = Application.persistentDataPath + "/quickData.sav";
        bool fileExists = File.Exists(path);
        if (fileExists) {
            byte[] bArr = File.ReadAllBytes(path);
            string content = Utilities.bytesToString(bArr);
            quickdataProp.parse(content);
        }

        quickdataProp.remove("fn" + fileIndex);
        quickdataProp.remove("diff" + fileIndex);
        quickdataProp.remove("t" + fileIndex);
        quickdataProp.remove("info" + fileIndex);
        quickdataProp.remove("phys" + fileIndex);

        byte[] bArr2 = Utilities.stringToBytes(quickdataProp.convertToString());

        File.WriteAllBytes(path, bArr2);
    }

    void createFileSelects() {
        clearFileSelects();

        string path = Application.persistentDataPath + "/quickData.sav";
        bool fileExists = false;
        List<QuickData> quickDatas = new List<QuickData>();

#if !UNITY_WEBPLAYER
        fileExists = File.Exists(path);
#endif

        if (fileExists) {
            byte[] bArr = File.ReadAllBytes(path);
            string content = Utilities.bytesToString(bArr);
            Properties prop = new Properties(content);
            for (int i = 0; i < NUM_SAVE_FILES; i++) {
                QuickData qd = new QuickData();
                qd.create(
                    prop.getString("fn" + i, "-1"),
                    ((Vars.Difficulty)prop.getInt("diff" + i, 0)),
                    prop.getFloat("t" + i, -1),
                    prop.getFloat("info" + i, -1),
                    prop.getFloat("phys" + i, -1));
                quickDatas.Add(qd);
            }
        } else {
            for (int i = 0; i < NUM_SAVE_FILES; i++) {
                quickDatas.Add(new QuickData());
            }
        }


        
        for (int i=0; i<quickDatas.Count; i++) {
            QuickData qd = quickDatas[i];
            GameObject fsGO = GameObject.Instantiate(fileSelectGameObject);
            fsGO.transform.SetParent(transform, false);
            FileSelect fs = fsGO.GetComponent<FileSelect>();
            fs.index = i;
            fs.selected = false;
            fs.startPosition.Set(centerPos.x, centerPos.y - verticalSpacing * fs.index - fadeDistance);
            fs.GetComponent<RectTransform>().localPosition = fs.startPosition;
            fs.endPosition.Set(centerPos.x, centerPos.y - verticalSpacing * fs.index);
            fs.timeOffset = timeDiff * fs.index;
            if (qd.created) {
                fs.newFile = false;
                fs.setFileName(fs.index);
                fs.setPlayTime(qd.time);
                fs.setDifficulty(difficultyProperties.getString(((int)qd.difficulty)+"name"));
                fs.setInfoComplete(qd.infoPercent);
                fs.setPhysComplete(qd.physPercent);
            } else {
                fs.newFile = true;
            }
            fileSelects.Add(fs);
        }

    }

    void clearFileSelects() {
        foreach (FileSelect fs in fileSelects) {
            GameObject.Destroy(fs.gameObject);
        }
        fileSelects.Clear();
    }

    class QuickData {
        public QuickData() { }
        public bool created = false;
        public string fileName = "";
        public Vars.Difficulty difficulty = Vars.Difficulty.STANDARD;
        public float time = 0;
        public float infoPercent = 0;
        public float physPercent = 0;
        public void create(string fileName, Vars.Difficulty difficulty, float time, float infoPercent, float physPercent) {
            if (fileName == "-1" || difficulty == Vars.Difficulty.NONE || time < 0 || infoPercent < 0 || physPercent < 0) {
                created = false;
                return;
            }
            created = true;
            this.fileName = fileName;
            this.difficulty = difficulty;
            this.time = time;
            this.infoPercent = infoPercent;
            this.physPercent = physPercent;
        }
    }

    enum SettingsSelection {
        DIFFICULTY,
        TUTORIALS,
        START
    }

    bool confirmPressed {
        get {
            return Keys.instance.confirmPressed ||
                Keys.instance.startPressed;
        }
    }
    bool backPressed {
        get {
            return Keys.instance.backPressed ||
                Keys.instance.escapePressed;
        }
    }

    Image image;
    Image selection;

    Image errorImage;
    GlyphBox errorText;

    GlyphBox settingsHeaderBox;
    GlyphBox settingsDifficultyBox;
    GlyphBox settingsDiffDescripBox;
    GlyphBox settingsTutorialsBox;
    GlyphBox settingsTutDescripBox;
    Image settingsStartButton;
    Image settingsSelection;
    Image deleteFile;

    Properties properties;
    Properties difficultyProperties;
    
    float time = 0;
    bool settingsShown = false;
    bool errorShown {  get { return errorImage.enabled; } }
    Vars.Difficulty settingsDifficulty = Vars.Difficulty.STANDARD;
    bool settingsTutorials = false;
    SettingsSelection settingsSelectionIndex = SettingsSelection.DIFFICULTY;
    float deleteHoldTime = 0;

    List<FileSelect> fileSelects = new List<FileSelect>();

    Image whiteScreen;



    int _selectionIndex = -1;

}
