using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CreatureCard : MonoBehaviour {

    /* creature_cards Format:
     * CreatureName: profile_file_name ID/stars/atk/def/special/ description
    */
    public static bool initialized { get { return _initialized; } }
    public static void initialize() {
        if (initialized) return;
        TextAsset ta = (Resources.Load("creature_cards_info") as TextAsset);
        Properties prop = new Properties(ta.text);
        initialize(prop);
    }
    public static void initialize(Properties prop) {
        cardInfos.Clear();
        cardInfosProp = prop;

        List<string> keys = prop.getKeys();
        foreach (string key in keys) {
            string info = prop.getString(key);
            if (info == "") continue;
            // get id
            int index2 = info.IndexOf('/');
            int index1 = info.LastIndexOf(' ', index2);
            int id = int.Parse(info.Substring(index1+1, index2 - index1 - 1));
            // make sure cardInfos can hold id
            if (id >= 1000) {
                Debug.LogError("Error: creature card id " + id + " is too high.");
            }
            while (cardInfos.Count <= id) {
                cardInfos.Add("");
            }
            if (cardInfos[id] != "") {
                Debug.LogError("Error: two creature cards have the same id: " + id);
            }
            cardInfos[id] = key + ": " + info.Trim();
        }

        _initialized = true;
    }
    public static string getCardNameFromID(int id) {
        if (!initialized)
            initialize();
        if (id < 0 || id >= cardInfos.Count) {
            Debug.LogError("Error: could not find creature card with id " + id);
            return "";
        }
        string info = cardInfos[id];
        if (info == "") {
            Debug.LogWarning("Creature id " + id + " does not have corresponding creature");
            return "";
        }
        int index = info.IndexOf(":");
        return info.Substring(0, index);
    }
    public static int getIDFromCardName(string cardName) {
        if (!initialized)
            initialize();
        string cardNameLower = cardName.ToLower();
        for (int i=0; i<cardInfos.Count; i++) {
            int index = cardInfos[i].IndexOf(":");
            if (index == -1) continue;
            if (cardInfos[i].Substring(0, index).ToLower() == cardNameLower) {
                return i;
            }
        }
        Debug.LogWarning("No id could be found for creature " + cardName);
        return 0;
    }
    public static int getNumCardsTotal() {
        if (!initialized)
            initialize();
        return cardInfos.Count - 1; // since there is no card with id 0
    }

    /////////////
    // PUBLIC //
    ////////////
    
    public GameObject starGameObject;
    public Vector2 starsCenterPos;
    public float starSpacing = 10;
    public Sprite cardFrontSprite;
    public Sprite cardBackSprite;

    public int numStars {
        get {
            return stars.Count;
        }
        set {
            while (stars.Count > value) {
                GameObject.Destroy(stars[stars.Count - 1]);
                stars.RemoveAt(stars.Count - 1);
            }
            while (stars.Count < value) {
                GameObject starGO = GameObject.Instantiate(starGameObject);
                starGO.transform.SetParent(transform, false);
                stars.Add(starGO);
            }
            for (int i=0; i<stars.Count; i++) {
                stars[i].transform.localPosition = new Vector3(starsCenterPos.x + starSpacing * (i + .5f - stars.Count / 2.0f), starsCenterPos.y, 0);
            }
        }
    }
    [HideInInspector]
    public string description;
    

    public void setCard(string creatureName) {
        if (!initialized)
            initialize();
        setCard(creatureName, cardInfosProp.getString(creatureName));
    }
    public void setCard(int creatureID) {
        if (!initialized)
            initialize();
        if (creatureID < 0 || creatureID >= cardInfos.Count) {
            Debug.LogError("Creature ID " + creatureID + " could not be found.");
        }
        string info = cardInfos[creatureID];
        if (info == "") {
            setCard("", "");
            return;
        }
        int index = info.IndexOf(":");
        setCard(info.Substring(0, index), info.Substring(index + 1));
    }
    public void setCard(string creatureName, string creatureInfo) {
        if (creatureInfo == "") {
            Debug.LogWarning("Warning: info for creature " + creatureName + " is blank.");
            setID(0);
            setName("");
            setStats(0, 0, 0);
            return;
        }
        setName(creatureName);

        string info = creatureInfo.Trim();
        int index1 = info.IndexOf(' ');
        string profileName = info.Substring(0, index1);
        setProfile(profileName);

        int index2 = info.IndexOf('/', index1);
        setID(int.Parse(info.Substring(index1 + 1, index2 - index1 - 1).Trim()));

        index1 = index2 + 1;
        index2 = info.IndexOf('/', index1);
        numStars = int.Parse(info.Substring(index1, index2 - index1));

        index1 = index2 + 1;
        index2 = info.IndexOf('/', index1);
        int attack = int.Parse(info.Substring(index1, index2-index1));
        index1 = index2 + 1;
        index2 = info.IndexOf('/', index1);
        int defense = int.Parse(info.Substring(index1, index2 - index1));
        index1 = index2 + 1;
        index2 = info.IndexOf('/', index1);
        int special = int.Parse(info.Substring(index1, index2 - index1));
        setStats(attack, defense, special);

        description = info.Substring(index2 + 1).Trim();
    }
    

    public void setProfile(string profileFileName) {
        Sprite sprite = Resources.Load<Sprite>(profileFileName);
        if (sprite == null) {
            Debug.LogWarning("Error: card sprite " + profileFileName + " could not be found.");
            return;
        }
        profile.sprite = sprite;
    }

    public void setID(int id) {
        string idStr = "";
        if (id < 100)
            idStr += "0";
        if (id < 10)
            idStr += "0";
        idStr += id;
        IDBox.setPlainText(idStr);
    }

    public void setName(string name) {
        nameBox.setPlainText(name.ToUpper());
    }

    public void setStats(int attack, int defense, int special) {
        attackBox.setPlainText("" + attack);
        defenseBox.setPlainText("" + defense);
        specialBox.setPlainText("" + special);
    }

    public void showFront() {
        image.enabled = true;
        image.sprite = cardFrontSprite;
        profile.enabled = true;
        nameBox.makeAllCharsVisible();
        attackBox.makeAllCharsVisible();
        defenseBox.makeAllCharsVisible();
        specialBox.makeAllCharsVisible();
        IDBox.makeAllCharsVisible();
        foreach (GameObject sGO in stars) {
            sGO.GetComponent<Image>().enabled = true;
        }
    }

    public void showBack() {
        image.enabled = true;
        image.sprite = cardBackSprite;
        profile.enabled = false;
        nameBox.makeAllCharsInvisible();
        attackBox.makeAllCharsInvisible();
        defenseBox.makeAllCharsInvisible();
        specialBox.makeAllCharsInvisible();
        IDBox.makeAllCharsInvisible();
        foreach (GameObject sGO in stars) {
            sGO.GetComponent<Image>().enabled = false;
        }
    }

    public void hide() {
        image.enabled = false;
        profile.enabled = false;
        nameBox.makeAllCharsInvisible();
        attackBox.makeAllCharsInvisible();
        defenseBox.makeAllCharsInvisible();
        specialBox.makeAllCharsInvisible();
        IDBox.makeAllCharsInvisible();
        foreach (GameObject sGO in stars) {
            sGO.GetComponent<Image>().enabled = false;
        }
    }

    /////////////
    // PRIVATE //
    /////////////

    private static Properties cardInfosProp = null;
    private static List<string> cardInfos = new List<string>(); // cardInfos[id] contains data for the card with the given id
    private static bool _initialized = false;

    void Awake() {
        image = GetComponent<Image>();
        profile = transform.Find("Profile").GetComponent<Image>();
        nameBox = transform.Find("NameBox").GetComponent<GlyphBox>();
        attackBox = transform.Find("AttackBox").GetComponent<GlyphBox>();
        defenseBox = transform.Find("DefenseBox").GetComponent<GlyphBox>();
        specialBox = transform.Find("SpecialBox").GetComponent<GlyphBox>();
        IDBox = transform.Find("IDBox").GetComponent<GlyphBox>();
    }
	
	void Update() {
		
	}

    Image image;
    Image profile;
    GlyphBox nameBox;
    GlyphBox attackBox;
    GlyphBox defenseBox;
    GlyphBox specialBox;
    GlyphBox IDBox;

    List<GameObject> stars = new List<GameObject>();
}
