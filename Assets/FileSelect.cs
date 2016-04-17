using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FileSelect : MonoBehaviour {

    public Sprite normalDefaultSprite;
    public Sprite normalSelectedSprite;
    public Sprite newFileDefaultSprite;
    public Sprite newFileSelectedSprite;
    public TextAsset textAsset;

    public int index = 0;
    public Vector2 startPosition = new Vector2();
    public Vector2 endPosition = new Vector2();
    public float timeOffset = 0;

    public Vector2 position {
        get { return GetComponent<RectTransform>().localPosition; }
        set {
            GetComponent<RectTransform>().localPosition = value;
        }
    }

    public bool newFile {
        get { return _newFile; }
        set {
            if (_newFile == value) return;
            _newFile = value;
            if (newFile) {
                image.sprite = (selected ? newFileSelectedSprite : newFileDefaultSprite);
                nameBox.makeAllCharsInvisible();
                timeBox.makeAllCharsInvisible();
                difficultyBox.makeAllCharsInvisible();
                infoCompBox.makeAllCharsInvisible();
                physCompBox.makeAllCharsInvisible();
                newFileBox.makeAllCharsVisible();

                newFileBox.setText(properties.getString("new_file"));
                newFileBox.makeAllCharsVisible();
            } else {
                image.sprite = (selected ? normalSelectedSprite : normalDefaultSprite);
                nameBox.makeAllCharsVisible();
                timeBox.makeAllCharsVisible();
                difficultyBox.makeAllCharsVisible();
                infoCompBox.makeAllCharsVisible();
                physCompBox.makeAllCharsVisible();
                newFileBox.makeAllCharsInvisible();
            }
        }
    }
    public bool selected {
        get { return _selected; }
        set {
            if (_selected == value) return;
            _selected = value;
            if (newFile) {
                image.sprite = (selected ? newFileSelectedSprite : newFileDefaultSprite);
            } else {
                image.sprite = (selected ? normalSelectedSprite : normalDefaultSprite);
            }
        }
    }

    bool _newFile = false;
    bool _selected = false;

    public void setFileName(int fileIndex) {
        setFileName(properties.getString("file") + " " + fileIndex);
    }
    public void setFileName(string name) {
        nameBox.setText(name);
    }

    public void setPlayTime(float playTime) {
        string str = "";
        int hours = Mathf.FloorToInt(playTime / 3600);
        playTime -= hours * 3600;
        int mins = Mathf.FloorToInt(playTime / 60);
        playTime -= mins * 60;
        int secs = Mathf.FloorToInt(playTime);
        playTime -= secs;
        float centiseconds = Mathf.FloorToInt(playTime * 100);
        if (hours < 10) {
            str = "0:";
            if (mins < 10) str += "0";
            str += mins + ":";
            if (secs < 10) str += "0";
            str += secs + ".";
            if (centiseconds < 10) str += "0";
            str += centiseconds;
        } else if (hours >= 100) {
            str = "99:99:99";
        } else {
            if (hours < 10) str += "0";
            str += hours + ":";
            if (mins < 10) str += "0";
            str += mins + ":";
            if (secs < 10) str += "0";
            str += secs;
        }
        timeBox.setPlainText(str);
    }

    public void setDifficulty(string difficultyString) {
        string str = difficultyString;
        while (str.Length < 13) {
            str = "¦" + str; // ¦ char looks like a space
        }
        difficultyBox.setPlainText(str);
    }

    public void setInfoComplete(float infoCompletePercent) {
        int percentInt = Mathf.FloorToInt(infoCompletePercent * 100);
        string str = "";
        if (percentInt < 10) str += "0";
        str += percentInt + "%";
        if (percentInt >= 100) {
            str = "|" + str + "|";
        } else {
            str = "¦" + str; // ¦ char looks like a space
        }
        infoCompBox.setText(str);
    }

    public void setPhysComplete(float physCompletePercent) {
        int percentInt = Mathf.FloorToInt(physCompletePercent * 100);
        string str = "";
        if (percentInt < 10) str += "0";
        str += percentInt + "%";
        if (percentInt >= 100) {
            str = "|" + str + "|";
        } else {
            str = "¦" + str; // ¦ char looks like a space
        }
        physCompBox.setText(str);
    }

    void Awake() {
        image = GetComponent<Image>();
        nameBox = transform.Find("NameBox").GetComponent<GlyphBox>();
        timeBox = transform.Find("TimeBox").GetComponent<GlyphBox>();
        difficultyBox = transform.Find("DifficultyBox").GetComponent<GlyphBox>();
        infoCompBox = transform.Find("InfoCompBox").GetComponent<GlyphBox>();
        physCompBox = transform.Find("PhysCompBox").GetComponent<GlyphBox>();
        newFileBox = transform.Find("NewFileBox").GetComponent<GlyphBox>();
        
        properties = new Properties(textAsset.text);

    }

    void Start() {
        // set all colors
        nameBox.setColor(PauseScreen.DEFAULT_COLOR);
        timeBox.setColor(PauseScreen.DEFAULT_COLOR);
        difficultyBox.setColor(PauseScreen.DEFAULT_COLOR);
        infoCompBox.setColor(PauseScreen.DEFAULT_COLOR);
        physCompBox.setColor(PauseScreen.DEFAULT_COLOR);
    }
	
	void Update() {
		
	}

    Image image;
    GlyphBox nameBox;
    GlyphBox timeBox;
    GlyphBox difficultyBox;
    GlyphBox infoCompBox;
    GlyphBox physCompBox;
    GlyphBox newFileBox;

    Properties properties;
}
