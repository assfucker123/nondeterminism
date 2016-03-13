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
        addConversationNoAlert(name, scriptFile, important, tutorial, order);
        manualAlert(1);
    }
    public static void addConversation(TalkConversation conversation) {
        addConversationNoAlert(conversation);
        manualAlert(1);
    }
    public static void addConversationNoAlert(string name, string scriptFile, bool important, bool tutorial, int order = 0) {
        TalkConversation tc = new TalkConversation();
        tc.name = name;
        tc.scriptFile = scriptFile;
        tc.newConversation = true;
        tc.important = important;
        tc.tutorial = tutorial;
        tc.order = order;
        addConversationNoAlert(tc);
    }
    public static void addConversationNoAlert(TalkConversation conversation) {
        if (hasConversation(conversation.name))
            return;
        conversations.Add(conversation);
    }
    public static void manualAlert(int numberOfConversationsAdded) {
        if (Notification.instance == null) return;
        if (numberOfConversationsAdded <= 0) return;
        if (s_propAsset == null) {
            TextAsset textAsset = Resources.Load("talk_page") as TextAsset;
            s_propAsset = new Properties(textAsset.text);
        }
        if (numberOfConversationsAdded == 1) {
            Notification.instance.displayNotification(s_propAsset.getString("conversation_add"), Notification.NotifType.DEFAULT);
        } else {
            Notification.instance.displayNotification(numberOfConversationsAdded + " " + s_propAsset.getString("conversation_multi_add"), Notification.NotifType.DEFAULT);
        }
    }
    private static Properties s_propAsset;


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

    private static int lastListOffset = 0;
    private static int lastSelectionVisualIndex = 0;

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
    public TalkConversation selectedConversation {  get { return list[selectionIndex]; } }

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
            } else if (convo.important) {
                listBox.setColor(listBox.importantColor, lineIndex, 0, line.Length, false);
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
        int prevSelectionVisualIndex = selectionVisualIndex;
        selectionVisualPos0.Set(selectionImage.rectTransform.localPosition.x, selectionImage.rectTransform.localPosition.y);

        if (selectionVisualIndex >= listBox.height - 1 /*2*/) {
            // scroll list offset instead, if possible
            if (listOffset < maxListOffset) {
                listOffset++;
            } else if (listOffset == maxListOffset) {
                listOffset = 0;
                selectionVisualIndex = 0;
            } else {
                selectionVisualIndex++;
            }
        } else {
            if (selectionVisualIndex == list.Count - 1) {
                selectionVisualIndex = 0;
            } else {
                selectionVisualIndex++;
            }
        }

        if (Mathf.Abs(selectionVisualIndex - prevSelectionVisualIndex) == 1) {
            selectionVisualPos1.Set(selectionImage.rectTransform.localPosition.x, selectionImage.rectTransform.localPosition.y);
            selectionVisualTime = 0;
            selectionImage.rectTransform.localPosition = new Vector3(selectionVisualPos0.x, selectionVisualPos0.y, selectionImage.rectTransform.localPosition.z);
        } else {
            selectionVisualTime = 99999;
        }
        
        displayList(listOffset);

        lastListOffset = listOffset;
        lastSelectionVisualIndex = selectionVisualIndex;
    }

    void decrementSelection() {
        int prevSelectionVisualIndex = selectionVisualIndex;
        selectionVisualPos0.Set(selectionImage.rectTransform.localPosition.x, selectionImage.rectTransform.localPosition.y);

        if (selectionVisualIndex <= 0 /*1*/) {
            // scroll list offset instead, if possible
            if (listOffset > 0) {
                listOffset--;
            } else if (listOffset == 0) {
                listOffset = maxListOffset;
                selectionVisualIndex = Mathf.Min(list.Count, listBox.height);
            } else {
                selectionVisualIndex--;
            }
        } else {
            selectionVisualIndex--;
        }

        if (Mathf.Abs(selectionVisualIndex - prevSelectionVisualIndex) == 1) {
            selectionVisualPos1.Set(selectionImage.rectTransform.localPosition.x, selectionImage.rectTransform.localPosition.y);
            selectionVisualTime = 0;
            selectionImage.rectTransform.localPosition = new Vector3(selectionVisualPos0.x, selectionVisualPos0.y, selectionImage.rectTransform.localPosition.z);
        } else {
            selectionVisualTime = 99999;
        }

        displayList(listOffset);

        lastListOffset = listOffset;
        lastSelectionVisualIndex = selectionVisualIndex;
    }
    
    public void update() {

        

        if (TextBox.instance.isBeingUsed) {
            // press pause to immediately end conversation
            if (Keys.instance.startPressed) {
                scriptRunner.stopScript();
                TextBox.instance.close();
            }
        } else {
            // input
            if (markNotNewOnceTextBoxNotUsed) {
                if (selectedConversation.newConversation &&
                    selectedConversation.name != propAsset.getString("current_objective")) {
                    selectedConversation.newConversation = false;
                    displayList(listOffset);
                }
                markNotNewOnceTextBoxNotUsed = false;
            }
            if (Keys.instance.upPressed) {
                decrementSelection();
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            } else if (Keys.instance.downPressed) {
                incrementSelection();
                SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
            } else if (Keys.instance.confirmPressed) {
                // play text
                if (selectedConversation.scriptFile != "") {
                    scriptRunner.runScript(Resources.Load(selectedConversation.scriptFile) as TextAsset);
                    markNotNewOnceTextBoxNotUsed = true;
                }
            }
        }

        // move selection
        if (selectionVisualTime < selectionVisualDuration) {
            selectionVisualTime += Time.unscaledDeltaTime;
            selectionImage.rectTransform.localPosition = new Vector3(
                Utilities.easeOutQuadClamp(selectionVisualTime, selectionVisualPos0.x, selectionVisualPos1.x - selectionVisualPos0.x, selectionVisualDuration),
                Utilities.easeOutQuadClamp(selectionVisualTime, selectionVisualPos0.y, selectionVisualPos1.y - selectionVisualPos0.y, selectionVisualDuration),
                selectionImage.rectTransform.localPosition.z);
        }
        
    }

    
    public void show() {
        listBG.enabled = true;
        selectionImage.enabled = true;
        listBox.makeAllCharsVisible();

        initializeList();
        displayList(Mathf.Clamp(lastListOffset, minListOffset, maxListOffset));
        selectionVisualIndex = lastSelectionVisualIndex;

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
        scriptRunner = GetComponent<ScriptRunner>();
        propAsset = new Properties(textAsset.text);
    }

    void Start() {
        
        hide();
        
    }

    void Update() {

        

    }

    void OnDestroy() {
        hide();
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
    ScriptRunner scriptRunner;

    Properties propAsset;

    TalkConversation currentObjectiveTalkConversation = null;
    TalkConversation dividerConversation = null;

    int _listOffset = 0;
    int _selectionVisualIndex = 0;
    float selectionVisualTime = 9999;
    float selectionVisualDuration = .15f;
    Vector2 selectionVisualPos0 = new Vector2();
    Vector2 selectionVisualPos1 = new Vector2();
    bool markNotNewOnceTextBoxNotUsed = false;
    List<TalkConversation> list = new List<TalkConversation>();

}
