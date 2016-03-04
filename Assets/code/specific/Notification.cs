using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Notification : MonoBehaviour {

    public Color defaultColor = new Color(222 / 255.0f, 194/255.0f, 237/255.0f);
    public Color importantColor = new Color();
    public Color timeTreeColor = new Color();
    public Color deadlyColor = new Color();
    public float preDisplayDelay = .5f;
    public float textSpeed = 30;
    public float postDisplayDelay = 4f;
    public float bgXBorder = 2;
    public float bgYBorder = 1;
    public AudioClip textSound;
    public float textSoundPeriod = .08f;
    public int maxQueueSize = 5;

    public GameObject glyphBGGameObject;

    public static Notification instance {  get { return _instance; } }

    /* To use, call:
      Notification.instance.displayNotification("This just happened", Notification.NotifType.DEFAULT);
    */

    public enum NotifType {
        DEFAULT,
        IMPORTANT,
        TIME_TREE,
        DEADLY
    }

    public void displayNotification(string text, NotifType type = NotifType.DEFAULT) {
        if (notifs.Count >= maxQueueSize) {
            Debug.LogError("Error: notifs already reached max queue size");
            return;
        }
        Notif n = new Notif();
        n.text = text;
        n.type = type;
        notifs.Add(n);
        if (notifs.Count == 1) {
            // start displaying the new notif.  otherwise it was put in the back of the queue
            setNewDisplayingNotif();
            displayTime = -preDisplayDelay;
        }
        
    }

    public void clearAll() {
        textBox.setPlainText("");
        notifs.Clear();
        updateBGs();
    }
    
    class Notif {
        public string text = "";
        public NotifType type = NotifType.DEFAULT;
    }

	void Awake() {
        if (_instance != null) {
            GameObject.Destroy(_instance.gameObject);
        }
        _instance = this;

        timeUser = GetComponent<TimeUser>();
        textBox = transform.Find("TextBox").GetComponent<GlyphBox>();

        // make bg images
        for (int i=0; i<textBox.height; i++) {
            GameObject bgImageGO = GameObject.Instantiate(glyphBGGameObject);
            bgImageGO.transform.SetParent(textBox.transform, false);
            bgImageGO.GetComponent<RectTransform>().localPosition = new Vector3(
                -bgXBorder,
                bgYBorder - i * textBox.glyphGameObjects[0].GetComponent<GlyphSprite>().pixelHeight);
            bgImageGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, textBox.glyphGameObjects[0].GetComponent<GlyphSprite>().pixelHeight + bgYBorder*2);
            bgImages.Add(bgImageGO);
        }
	}
	
	void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        displayTime += Time.deltaTime;
        textSoundTime += Time.deltaTime;

        updateNotification();

	}

    void updateNotification() {
        if (!timeUser.exists) return;

        if (notifs.Count == 0) {
            // don't display anything
            return;
        }

        int prevVisibleChars = textBox.visibleChars;
        float numChars = Mathf.Clamp(displayTime * textSpeed, 0, textBox.totalChars);
        textBox.visibleChars = Mathf.FloorToInt(numChars);

        if (textBox.visibleChars > prevVisibleChars) {
            if (textSoundTime > textSoundPeriod) {
                if (!TimeUser.reverting) {
                    SoundManager.instance.playSFX(textSound);
                }
                textSoundTime = 0;
            }
        }

        if (displayTime >= textBox.totalChars / textSpeed + postDisplayDelay) {
            // done displaying text.  stop or move on to next notif
            notifs.RemoveAt(0);
            displayTime = -preDisplayDelay;
            setNewDisplayingNotif();
        }

        updateBGs();

    }

    // sets text and color for new notif
    void setNewDisplayingNotif() {
        if (notifs.Count <= 0) {
            textBox.setPlainText("", 0);
            return;
        }
        Notif n = notifs[0];

        Color col = defaultColor;
        switch (n.type) {
        case NotifType.IMPORTANT:
            col = importantColor;
            break;
        case NotifType.TIME_TREE:
            col = timeTreeColor;
            break;
        case NotifType.DEADLY:
            col = deadlyColor;
            break;
        }

        textBox.setColor(col, true);
        textBox.setPlainText(n.text, 0);
    }

    void updateBGs() {

        float charWidth = textBox.glyphGameObjects[0].GetComponent<GlyphSprite>().pixelWidth;
        float visibleChars = textBox.visibleChars * charWidth;

        for (int i=0; i<bgImages.Count; i++) {
            GameObject bgImageGO = bgImages[i];
            float lineWidth = textBox.getLineWidth(i);
            if (visibleChars < lineWidth) {
                lineWidth = visibleChars;
                visibleChars = 0;
            } else {
                visibleChars -= lineWidth;
            }

            bgImageGO.GetComponent<RectTransform>().sizeDelta = new Vector2(lineWidth + bgXBorder*2, bgImageGO.GetComponent<RectTransform>().sizeDelta.y);
            bgImageGO.GetComponent<UnityEngine.UI.Image>().enabled = (lineWidth > 0);
        }

    }


    void OnSaveFrame(FrameInfo fi) {
        fi.floats["dt"] = displayTime;
        fi.floats["tst"] = textSoundTime;
        fi.ints["nc"] = notifs.Count;
        for (int i=0; i<notifs.Count; i++) {
            if (i >= notifs.Count) {
                fi.strings["nt" + i] = "";
                fi.ints["nt" + i] = (int)NotifType.DEFAULT;
            } else {
                fi.strings["nt" + i] = notifs[i].text;
                fi.ints["nt" + i] = (int)notifs[i].type;
            }
        }
    }

    void OnRevert(FrameInfo fi) {
        string previousText = "";
        if (notifs.Count > 0) {
            previousText = notifs[0].text;
        }

        displayTime = fi.floats["dt"];
        textSoundTime = fi.floats["tst"];
        int nCount = fi.ints["nc"];
        while (notifs.Count > nCount) {
            notifs.RemoveAt(notifs.Count - 1);
        }
        while (notifs.Count < nCount) {
            notifs.Add(new Notif());
        }
        for (int i=0; i<nCount; i++) {
            notifs[i].text = fi.strings["nt" + i];
            notifs[i].type = (NotifType)fi.ints["nt" + i];
        }

        string newText = "";
        if (notifs.Count > 0) {
            newText = notifs[0].text;
        }
        if (previousText != newText) {
            setNewDisplayingNotif();
        }

        updateNotification();
    }

    void OnDestroy() {
        if (_instance == this)
            _instance = null;
    }

    TimeUser timeUser;
    GlyphBox textBox;

    float displayTime = 9999;
    float textSoundTime = 0;

    List<Notif> notifs = new List<Notif>(); // act as a queue.  Currently displayed notification is notifs[0]

    List<GameObject> bgImages = new List<GameObject>();

    static Notification _instance = null;

}
