using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChamberScreen : MonoBehaviour {

    public string positionCode = "E5";
    public Vector2 mainSelectionPos = new Vector2();
    public float mainSelectionSpacing = 32;
    public Vector2 smallSelectionPos = new Vector2();
    public float smallSelectionSpacing = 32;
    public float selectionDuration = .2f;
    public float[] waitTimes = { (30-25)*60, (30-20)*60, (30-15)*60, (30-10)*60, (30-5)*60, 30*60-15 };
    public AudioClip switchSound;
    public AudioClip saveGameSound;
    public TextAsset textAsset;

    public bool simplifiedOptions {
        get {
            return true;
        }
    }

    public bool quitNow { get; private set; }

    public enum Option {
        NONE,
        SAVE,
        WAIT,
        DELETE,
        BACK,
        RESUME
    }

    public static Color DEFAULT_COLOR = new Color(140/255f, 215/255f, 255/255f); // 8CD7FF
    public static Color SELECTED_COLOR = new Color(22/255f, 33/255f, 40/255f); // 162128

    void Awake() {
        image = GetComponent<Image>();
        selection = transform.Find("Selection").GetComponent<Image>();
        selectionSmall = transform.Find("SelectionSmall").GetComponent<Image>();
        glyphBoxMain = transform.Find("GlyphBoxMain").GetComponent<GlyphBox>();
        glyphBoxSmall = transform.Find("GlyphBoxSmall").GetComponent<GlyphBox>();
        glyphBoxWait = transform.Find("GlyphBoxWait").GetComponent<GlyphBox>();
        propAsset = new Properties(textAsset.text);
	}

    void setMainMenu(Option startingSelection = Option.NONE) {
        image.enabled = true;
        glyphBoxWait.makeAllCharsInvisible();
        glyphBoxSmall.makeAllCharsInvisible();
        // what glyphBoxMain says
        string str = propAsset.getString("chamber") + " " + positionCode + "\n\n\n";
        if (simplifiedOptions) {
            str +=
                "\n" +
                propAsset.getString("chamber_save") + "\n" +
                propAsset.getString("back");
            indexMin = 4;
            indexMax = 5;
        } else {
            str +=
                propAsset.getString("chamber_save") + "\n" +
                propAsset.getString("wait") + "\n" +
                propAsset.getString("delete_nodes") + "\n" +
                propAsset.getString("back");
            indexMin = 3;
            indexMax = 6;
        }
        glyphBoxMain.alignment = GlyphBox.Alignment.LEFT;
        glyphBoxMain.clearText();
        glyphBoxMain.setText(str);
        glyphBoxMain.alignment = GlyphBox.Alignment.CENTER;
        // selections
        selection.enabled = true;
        selectionSmall.enabled = false;

        movingMain = true;
        saveSure = false;
        currentMainIndex = -1;
        if (startingSelection == Option.NONE)
            startingSelection = Option.SAVE;
        setMainSelection(startingSelection, true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="positionCodeStart"> Set to "" to indicate that no branch was added </param>
    /// <param name="timeStart"></param>
    /// <param name="positionCodeNow"></param>
    /// <param name="timeNow"></param>
    /// <param name="saveError"></param>
    void setSaveSure(string positionCodeStart, float timeStart, string positionCodeNow, float timeNow, bool saveError) {
        image.enabled = true;
        glyphBoxWait.makeAllCharsInvisible();
        glyphBoxSmall.makeAllCharsInvisible();
        string str = "";
        //if (positionCodeStart == "") {
        //    str += propAsset.getString("no_branch_added") + "\n";
        //} else {
        //    str += propAsset.getString("branch_added") + "\n";
        //    CountdownTimer.Mode mode = CountdownTimer.Mode.NORMAL; // change later
        //    char[] chars = { '¦' };
        //    string timeStartStr = CountdownTimer.timeToStr(timeStart, mode).Trim(chars);
        //    string timeNowStr = CountdownTimer.timeToStr(timeNow, mode).Trim(chars);
        //    str += positionCodeStart + " (" + timeStartStr + ") -> " + positionCodeNow + " (" + timeNowStr + ")";
        //}
        //str += "\n\n";
        //if (saveError) {
        //    str += propAsset.getString("game_save_error");
        //} else {
        //    str += propAsset.getString("game_saved");
        //}

        if (positionCodeStart != "") {
            str += propAsset.getString("time_tree_updated");
        }
        str += "\n\n";
        if (saveError) {
            str += propAsset.getString("game_save_error");
        } else {
            str += propAsset.getString("game_saved");
        }
        str += "\n";

        str += "\n\n";
        str += propAsset.getString("resume") + "\n";
        str += propAsset.getString("back");

        glyphBoxMain.alignment = GlyphBox.Alignment.LEFT;
        glyphBoxMain.clearText();
        glyphBoxMain.setText(str);
        glyphBoxMain.alignment = GlyphBox.Alignment.CENTER;
        // selections
        selection.enabled = true;
        selectionSmall.enabled = false;

        movingMain = true;
        saveSure = true;
        indexMin = 5;
        indexMax = 6;
        currentMainIndex = -1;
        setMainSelection(Option.RESUME, true);
    }

    void setMainSelection(Option option, bool immediately) {
        if (option == Option.NONE) return;
        selectionPos0 = getImagePosition(selection);
        int index = optionToIndexMain(option);
        selectionPos1.x = mainSelectionPos.x;
        selectionPos1.y = mainSelectionPos.y - mainSelectionSpacing * index;
        if (immediately) {
            selectionTime = 9999;
            setImagePosition(selection, selectionPos1);
        } else {
            selectionTime = 0;
        }
        // change text
        if (currentMainIndex != -1) {
            glyphBoxMain.setColor(DEFAULT_COLOR, currentMainIndex, 0, glyphBoxMain.width);
        }
        currentMainIndex = index;
        glyphBoxMain.setColor(SELECTED_COLOR, currentMainIndex, 0, glyphBoxMain.width);
    }

    void waitMenu() {

        glyphBoxMain.makeAllCharsInvisible();
        selection.enabled = false;

        // wait text
        glyphBoxWait.makeAllCharsVisible();
        glyphBoxWait.setText(propAsset.getString("wait_text"));

        // times (only use times past the current time)
        float currentTime = 0;
        if (CountdownTimer.instance != null) {
            currentTime = CountdownTimer.instance.time;
        }
        string str = "";
        int perilTimes = 0;
        numValidWaitTimes = 0;
        for (int i=0; i<waitTimes.Length; i++) {
            float t = waitTimes[i];
            if (t > currentTime) {
                numValidWaitTimes++;
            }
            if (t >= CountdownTimer.MELTDOWN_DURATION - CountdownTimer.MELTDOWN_PERIL_DURATION) {
                perilTimes++;
            }
        }
        indexMin = (7 - (numValidWaitTimes + 1)) / 2; // also have index for Back
        indexMax = indexMin + numValidWaitTimes; // also have index for Back
        for (int i=0; i<indexMin; i++) {
            str += "\n";
        }
        for (int i=waitTimes.Length-numValidWaitTimes; i < waitTimes.Length; i++) {
            str += CountdownTimer.timeToStr(waitTimes[i], CountdownTimer.Mode.MELTDOWN) + "\n";
        }
        str += propAsset.getString("back");
        glyphBoxSmall.alignment = GlyphBox.Alignment.LEFT;
        glyphBoxSmall.clearText();
        glyphBoxSmall.setColor(DEFAULT_COLOR);
        glyphBoxSmall.setText(str);
        glyphBoxSmall.alignment = GlyphBox.Alignment.CENTER;
        for (int i = indexMin; i < indexMax; i++) {
            if (i < indexMax - perilTimes) {
                glyphBoxSmall.setColor(CountdownTimer.MELTDOWN_COLOR, i, 0, glyphBoxSmall.width);
            } else {
                glyphBoxSmall.setColor(CountdownTimer.MELTDOWN_PERIL_COLOR, i, 0, glyphBoxSmall.width);
            }
        }

        // selections
        selection.enabled = false;
        selectionSmall.enabled = true;

        movingMain = false;
        saveSure = false;
        currentSmallIndex = -1;
        setSmallSelection(indexMin, true);

    }

    void setSmallSelection(int index, bool immediately) {
        if (index == -1) return;
        selectionPos0 = getImagePosition(selectionSmall);
        selectionPos1.x = smallSelectionPos.x;
        selectionPos1.y = smallSelectionPos.y - smallSelectionSpacing * index;
        if (immediately) {
            selectionTime = 9999;
            setImagePosition(selectionSmall, selectionPos1);
        } else {
            selectionTime = 0;
        }
        // change text color (only of the last element (Back))
        if (currentSmallIndex == indexMax) {
            glyphBoxSmall.setColor(DEFAULT_COLOR, currentSmallIndex, 0, glyphBoxSmall.width);
        }
        currentSmallIndex = index;
        if (currentSmallIndex == indexMax) {
            glyphBoxSmall.setColor(SELECTED_COLOR, currentSmallIndex, 0, glyphBoxSmall.width);
        }
        
    }

    void deleteNodes() {
        Debug.Log("delete nodes not implemented");
    }

    void Start() {
        setMainMenu();
    }
	
	void Update() {

        // input
        if (movingMain) {
            if (downPressed) {
                if (currentMainIndex >= indexMax) {
                    setMainSelection(indexToOptionMain(indexMin), false);
                } else {
                    setMainSelection(indexToOptionMain(currentMainIndex + 1), false);
                }
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            } else if (upPressed) {
                if (currentMainIndex <= indexMin) {
                    setMainSelection(indexToOptionMain(indexMax), false);
                } else {
                    setMainSelection(indexToOptionMain(currentMainIndex - 1), false);
                }
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            }
            if (confirmPressed || backPressed) {
                Option option = indexToOptionMain(currentMainIndex);
                //SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
                if (backPressed || option == Option.BACK) {
                    // back pressed
                    if (saveSure) {
                        setMainMenu(Option.SAVE);
                    } else {
                        quit();
                    }
                } else if (option == Option.SAVE) {
                    saveGame();
                } else if (option == Option.WAIT) {
                    waitMenu();
                } else if (option == Option.DELETE) {
                    deleteNodes();
                } else if (option == Option.RESUME) {
                    quit();
                }
            }
        } else {
            // moving small
            if (downPressed) {
                if (currentSmallIndex >= indexMax) {
                    setSmallSelection(indexMin, false);
                } else {
                    setSmallSelection(currentSmallIndex+1, false);
                }
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            } else if (upPressed) {
                if (currentSmallIndex <= indexMin) {
                    setSmallSelection(indexMax, false);
                } else {
                    setSmallSelection(currentSmallIndex-1, false);
                }
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            }
            if (confirmPressed || backPressed) {
                if (backPressed || currentSmallIndex == indexMax) {
                    // go back
                    SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
                    setMainMenu(Option.WAIT);
                } else {
                    // wait until specified time
                    float timeSelected = waitTimes[waitTimes.Length - (indexMax - currentSmallIndex)];
                    Debug.Log("wait until " + timeSelected);
                }
            }
        }
        

        // selection movement
        Image selImage = selection;
        if (!movingMain) selImage = selectionSmall;
        if (selectionTime >= selectionDuration) {
            setImagePosition(selImage, selectionPos1);
        } else {
            selectionTime += Time.unscaledDeltaTime;
            setImagePosition(selImage, new Vector2(
                Utilities.easeOutQuadClamp(selectionTime, selectionPos0.x, selectionPos1.x - selectionPos0.x, selectionDuration),
                Utilities.easeOutQuadClamp(selectionTime, selectionPos0.y, selectionPos1.y - selectionPos0.y, selectionDuration)));
        }

    }

    void saveGame() {

        bool redundant = false;
        string positionCodeStart = "";
        float timeStart = 0;
        if (Vars.currentNodeData == null) {
            Debug.LogWarning("WARNING: Vars.currentNodeData is null.");
        } else {
            // last minute update to current node data, to ensure that it's current to the second
            Vars.updateNodeData(Vars.currentNodeData);
            // see if current node data is redundant
            if (Vars.currentNodeData.parent == null) {
                redundant = false;
            } else {
                redundant = Vars.currentNodeData.redundant(Vars.currentNodeData.parent);
            }

            Debug.Log("redundant: " + redundant);

            if (!redundant) {
                // not redundant, so ensure currentNodeData will get saved (not temporary) and make a new currentNodeData
                if (Vars.currentNodeData.parent != null) {
                    positionCodeStart = Vars.currentNodeData.parent.chamberPositionCode;
                    timeStart = Vars.currentNodeData.parent.time;
                }
                Vars.currentNodeData.temporary = false;
                Vars.currentNodeData = NodeData.createNodeData(Vars.currentNodeData, true);
            }

            // set record of player position to perfectly on top of the chamber platform
            ChamberPlatform chamberPlatform = GameObject.FindObjectOfType<ChamberPlatform>();
            if (Vars.currentNodeData.parent != null) {
                Vars.currentNodeData.parent.position.Set(chamberPlatform.transform.localPosition.x, chamberPlatform.transform.localPosition.y + 1.6f);
            }
        }

        // save the game
        bool errorSaving = !Vars.saveData();

        // go to next screen
        string currentPositionCode = "";
        float currentTime = 0;
        if (Vars.currentNodeData != null) {
            currentPositionCode = Vars.currentNodeData.chamberPositionCode;
            currentTime = Vars.currentNodeData.time;
        }
        if (!errorSaving) {
            SoundManager.instance.playSFXIgnoreVolumeScale(saveGameSound);
        }
        setSaveSure(positionCodeStart, timeStart, currentPositionCode, currentTime, errorSaving);
    }

    void quit() {
        quitNow = true; // ChamberScreen will be destroyed by ChamberPlatform, as it detects when quitNow is set to true
    }

    //bool downPressed { get { return Input.GetKeyDown(KeyCode.DownArrow); } }
    //bool upPressed { get { return Input.GetKeyDown(KeyCode.UpArrow); } }
    //bool confirmPressed {  get { return Input.GetKeyDown(KeyCode.Z); } }
    //bool backPressed {  get { return Input.GetKeyDown(KeyCode.X); } }
    bool downPressed { get { return Keys.instance.downPressed; } }
    bool upPressed { get { return Keys.instance.upPressed; } }
    bool confirmPressed { get { return Keys.instance.confirmPressed; } }
    bool backPressed { get { return Keys.instance.backPressed; } }

    Vector2 getImagePosition(Image image) {
        return new Vector2(image.GetComponent<RectTransform>().localPosition.x, image.GetComponent<RectTransform>().localPosition.y);
    }
    void setImagePosition(Image image, Vector2 pos) {
        image.GetComponent<RectTransform>().localPosition = pos;
    }
    int optionToIndexMain(Option option) {
        if (saveSure) {
            switch (option) {
            case Option.RESUME:
                return 5;
            case Option.BACK:
                return 6;
            }
        }
        if (simplifiedOptions) {
            switch (option) {
            case Option.SAVE:
                return 4;
            case Option.BACK:
                return 5;
            }
        } else {
            switch (option) {
            case Option.SAVE:
                return 3;
            case Option.WAIT:
                return 4;
            case Option.DELETE:
                return 5;
            case Option.BACK:
                return 6;
            }
        }
        return -1;
    }
    Option indexToOptionMain(int index) {
        if (saveSure) {
            switch (index) {
            case 5:
                return Option.RESUME;
            case 6:
                return Option.BACK;
            }
        }
        if (simplifiedOptions) {
            switch (index) {
            case 4:
                return Option.SAVE;
            case 5:
                return Option.BACK;
            }
        } else {
            switch (index) {
            case 3:
                return Option.SAVE;
            case 4:
                return Option.WAIT;
            case 5:
                return Option.DELETE;
            case 6:
                return Option.BACK;
            }
        }
        return Option.NONE;
    }

    Image image;
    GlyphBox glyphBoxMain;
    Image selection;
    GlyphBox glyphBoxSmall;
    Image selectionSmall;
    GlyphBox glyphBoxWait;
    Properties propAsset;

    int indexMin = 0;
    int indexMax = 0;
    int currentMainIndex = 0;
    int currentSmallIndex = 0;

    Vector2 selectionPos0 = new Vector2();
    Vector2 selectionPos1 = new Vector2();
    float selectionTime = 99999;
    bool movingMain = true;
    bool saveSure = false;
    int numValidWaitTimes = 0;

}
