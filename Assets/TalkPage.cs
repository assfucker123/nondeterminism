using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TalkPage : MonoBehaviour {

    ////////////////////////////
    // STATIC / CONVERSATIONS //
    ////////////////////////////

    public class TalkConversation {
        public string scriptFile = ""; // name of the script to be obtained through Resources.Load()
        public string name = ""; // name as it appears on the TalkPage
        public bool newConversation = false;
        public bool important = false;
        public bool tutorial = false;
        public int order = 0; // not implemented yet

        public void loadFromString(string str) {
            char[] delims = {'@' };
            string[] strs = str.Split(delims);
            scriptFile = strs[0];
            name = strs[1];
            newConversation = (strs[2] == "1");
            important = (strs[3] == "1");
            tutorial = (strs[4] == "1");
            order = int.Parse(strs[5]);
        }
        public string saveToString() {
            string ret = scriptFile + "@" + name + "@";
            if (newConversation) ret += "1";
            else ret += "0";
            ret += "@";
            if (important) ret += "1";
            else ret += "0";
            ret += "@";
            if (tutorial) ret += "1";
            else ret += "0";
            ret += "@" + order;

            return ret;
        }
    }

    public static void setCurrentObjectiveFile(string file) {
        _currentObjectiveFile = file;
        if (instance != null) {
            instance.currentObjectiveTalkConversation.scriptFile = currentObjectiveFile;
        }
    }
    public static string currentObjectiveFile {  get { return _currentObjectiveFile; } }
    
    public static void addConversation(string name, string scriptFile, bool important, bool tutorial, int order = 0) {
        TalkConversation tc = new TalkConversation();
        tc.name = name;
        tc.scriptFile = scriptFile;
        tc.newConversation = true;
        tc.important = important;
        tc.tutorial = tutorial;
        tc.order = order;
        addConversation(tc);
    }
    public static void addConversation(TalkConversation conversation) {
        if (hasConversation(conversation.name))
            return;
        conversations.Add(conversation);
    }

    public static bool hasConversation(string conversationName) {
        foreach (TalkConversation tc in conversations) {
            if (tc.name == conversationName)
                return true;
        }
        return false;
    }

    public static void loadAllConversationsFromString(string str) {
        conversations.Clear();
        char[] delims = {'|' };
        string[] strs = str.Split(delims);
        for (int i=0; i<strs.Length; i++) {
            TalkConversation tc = new TalkConversation();
            tc.loadFromString(strs[i]);
            addConversation(tc);
        }
    }
    public static string saveAllConversationsToString() {
        string ret = "";
        for (int i=0; i<conversations.Count; i++) {
            ret += conversations[i].saveToString();
            if (i != conversations.Count - 1)
                ret += "|";
        }
        return ret;
    }

    public static List<TalkConversation> conversations = new List<TalkConversation>();
    public static TalkPage instance {  get { return _instance; } }

    ////////////
    // PUBLIC //
    ////////////

    public Sprite scrollUpSprite;
    public Sprite scrollUpGraySprite;
    public Sprite scrollDownSprite;
    public Sprite scrollDownGraySprite;
    public AudioClip switchSound;
    public Color grayColor = Color.gray;
    public Vector2 selectionOffset = new Vector2();
    public float selectionSpacing = 16;
    public TextAsset textAsset;

    public int listOffset {
        get { return _listOffset; }
        set {
            _listOffset = value;
        }
    }
    public int minListOffset { get { return 0; } }
    public int maxListOffset { get { return Mathf.Max(0, list.Count - listBox.height); } }

    public int selectionVisualIndex {
        get { return _selectionVisualIndex; }
        set {
            _selectionVisualIndex = Mathf.Clamp(value, 0, Mathf.Min(listBox.height, list.Count) - 1);
            selectionImage.rectTransform.localPosition = listBox.rectTransform.localPosition + new Vector3(selectionOffset.x, selectionOffset.y - selectionVisualIndex * selectionSpacing, 0);
        }
    }
    public int selectionIndex {
        get { return selectionVisualIndex + listOffset; }
        set {
            selectionVisualIndex = value - listOffset;
        }
    }

    /* Creates list, which will be the data used when displaying the list in displayList() */
    public void initializeList() {
        list.Clear();
        // first part is (current objective) and possibly the strip suit command
        if (currentObjectiveTalkConversation == null) {
            currentObjectiveTalkConversation = new TalkConversation();
        }
        currentObjectiveTalkConversation.name = propAsset.getString("current_objective");
        currentObjectiveTalkConversation.scriptFile = currentObjectiveFile;
        currentObjectiveTalkConversation.important = false;
        currentObjectiveTalkConversation.newConversation = true;
        currentObjectiveTalkConversation.tutorial = false;
        list.Add(currentObjectiveTalkConversation);

        //testing
        list.Add(currentObjectiveTalkConversation);
        list.Add(currentObjectiveTalkConversation);

        // (here add strip suit command)

        // add divider
        if (dividerConversation == null) {
            dividerConversation = new TalkConversation();
            dividerConversation.name = propAsset.getString("divider");
            dividerConversation.scriptFile = "";
            dividerConversation.important = false;
            dividerConversation.newConversation = false;
            dividerConversation.tutorial = false;
        }
        list.Add(dividerConversation);

        // add the actual conversations (that aren't tutorials, save those for the bottom)
        for (int i=0; i<conversations.Count; i++) {
            if (!conversations[i].tutorial) {
                list.Add(conversations[i]);
            }
        }

        // add divider
        list.Add(dividerConversation);

        // add tutorials
        for (int i = 0; i < conversations.Count; i++) {
            if (conversations[i].tutorial) {
                list.Add(conversations[i]);
            }
        }


        //testing
        list.Add(currentObjectiveTalkConversation);
        list.Add(currentObjectiveTalkConversation);
        list.Add(currentObjectiveTalkConversation);
        list.Add(currentObjectiveTalkConversation);
        list.Add(currentObjectiveTalkConversation);
        list.Add(currentObjectiveTalkConversation);


    }

    /* Displays the list.  First line is the parameter listOffset */
    public void displayList(int listOffset) {

        // setting listOffset
        listOffset = Mathf.Clamp(listOffset, minListOffset, maxListOffset);
        this.listOffset = listOffset;
        
        listBox.setColor(listBox.defaultStyle.color); // reset color
        string text = "";
        for (int i = listOffset; i<list.Count; i++) {
            TalkConversation convo = list[i];
            int lineIndex = i - listOffset;
            if (lineIndex >= listBox.height)
                break;

            string line = "";
            bool grayLine = false;
            if (convo.scriptFile == "") {
                // is divider
                line += convo.name;
                grayLine = true;
            } else {
                if (convo.important) {
                    line += "*";
                } else {
                    line += "¦"; // this character maps to a space in the font
                }
                line += convo.name;
                grayLine = !convo.newConversation;
            }
            
            // set color of line
            if (grayLine) {
                listBox.setColor(grayColor, lineIndex, 0, line.Length, false);
            }

            text += line;
            if (lineIndex < listBox.height - 1)
                text += "\n";
            
        }
        listBox.setPlainText(text);
        listBox.updateGlyphs(true);

        // update scrollUp and scrollDown
        bool newConvo = false;
        if (listOffset > 0) {
            // there are more conversations above
            scrollUpImage.enabled = true;
            for (int i = 0; i < listOffset && i < list.Count; i++) {
                if (list[i].newConversation) {
                    newConvo = true; // there's at least one new conversation above
                    break;
                }
            }
            if (newConvo) {
                scrollUpImage.sprite = scrollUpSprite;
            } else {
                scrollUpImage.sprite = scrollUpGraySprite;
            }
        } else {
            // no conversations above
            scrollUpImage.enabled = false;
        }

        if (listOffset+listBox.height < list.Count) {
            // there are conversations below
            scrollDownImage.enabled = true;
            newConvo = false;
            for (int i = listOffset + listBox.height; i < list.Count; i++) {
                if (list[i].newConversation) {
                    newConvo = true; // there's at least one new conversation below
                    break;
                }
            }
            if (newConvo) {
                scrollDownImage.sprite = scrollDownSprite;
            } else {
                scrollDownImage.sprite = scrollDownGraySprite;
            }
        } else {
            // no conversations below
            scrollDownImage.enabled = false;
        }
    }

    void incrementSelection() {
        if (selectionIndex >= list.Count)
            return;
        if (selectionVisualIndex >= listBox.height - 2) {
            // scroll list offset instead, if possible
            if (listOffset < maxListOffset) {
                listOffset++;
            } else {
                selectionVisualIndex++;
            }
        } else {
            selectionVisualIndex++;
        }
        displayList(listOffset);
    }

    void decrementSelection() {
        if (selectionIndex <= 0)
            return;
        if (selectionVisualIndex <= 1) {
            // scroll list offset instead, if possible
            if (listOffset > 0) {
                listOffset--;
            } else {
                selectionVisualIndex--;
            }
        } else {
            selectionVisualIndex--;
        }
        displayList(listOffset);
    }
    
    public void update() {
        
    }

    
    public void show() {
        listBG.enabled = true;
        selectionImage.enabled = true;
        listBox.makeAllCharsVisible();
        displayList(listOffset);
    }

    public void hide() {
        listBG.enabled = false;
        scrollUpImage.enabled = false;
        scrollDownImage.enabled = false;
        selectionImage.enabled = false;
        listBox.makeAllCharsInvisible();
    }



    /////////////
    // PRIVATE //
    /////////////

    void Awake() {
        if (instance != null) {
            GameObject.Destroy(instance.gameObject);
        }
        _instance = this;

        listBG = transform.Find("ListBG").GetComponent<Image>();
        scrollUpImage = transform.Find("ScrollUp").GetComponent<Image>();
        scrollDownImage = transform.Find("ScrollDown").GetComponent<Image>();
        selectionImage = transform.Find("Selection").GetComponent<Image>();
        listBox = transform.Find("ListBox").GetComponent<GlyphBox>();
        propAsset = new Properties(textAsset.text);
    }

    void Start() {
        
        //hide();

        //testing
        initializeList();
        displayList(1);

        selectionVisualIndex = 1;
    }

    void Update() {


        //testing
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            decrementSelection();
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            incrementSelection();
        }

    }

    void OnDestroy() {
        if (_instance == this)
            _instance = null;
    }

    private static string _currentObjectiveFile = "co_first_tutorial";
    private static TalkPage _instance = null;

    Image listBG;
    Image scrollUpImage;
    Image scrollDownImage;
    Image selectionImage;
    GlyphBox listBox;

    Properties propAsset;

    TalkConversation currentObjectiveTalkConversation = null;
    TalkConversation dividerConversation = null;

    int _listOffset = 0;
    int _selectionVisualIndex = 0;
    List<TalkConversation> list = new List<TalkConversation>();

}
