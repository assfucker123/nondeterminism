using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ProgressPage : MonoBehaviour {

    ////////////
    // STATIC //
    ////////////
    
    public static ProgressPage instance { get { return _instance; } }

    public class ProgressItem {
        public string name = ""; // name as it appears on the ProgressPage
        public Type type = Type.NONE;
        public int id = 0;
        public string description = "";
        public bool found = true;

        public enum Type {
            NONE,
            ORB_KEY,
            BOOST_UPGRADE,
            HEALTH_UPGRADE,
            DECRYPTOR_HEAD,
            DECRYPTOR,
            CREATURE_CARD_HEAD,
            CREATURE_CARD,
            DIVIDER
        }
    }

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

    // to get selected thing, call list[selectionIndex]

    /* Creates list, which will be the data used when displaying the list in displayList() */
    public void initializeList() {
        list.Clear();

        if (Vars.currentNodeData == null) {
            Debug.Log("ERROR: Vars.currentNodeData is null");
            return;
        }
        

        // add orb key item
        int numOrbsTotal = (int)PhysicalUpgrade.Orb.TOTAL_ORBS;
        int numOrbsCollected = Vars.currentNodeData.orbs.Count;
        int numOrbsFound = Vars.orbsFound.Count;
        ProgressItem orbKeyItem = new ProgressItem();
        orbKeyItem.type = ProgressItem.Type.ORB_KEY;
        orbKeyItem.name = propAsset.getString("orb_key") + " " + numOrbsCollected + "/" + numOrbsTotal + " " + propAsset.getString("collected") + " (" + numOrbsFound + " " + propAsset.getString("found") + ")";
        orbKeyItem.description = propAsset.getString("orb_description");
        orbKeyItem.found = true;
        list.Add(orbKeyItem);

        // add boost upgrade item
        if (Vars.boosterFound) {
            ProgressItem boostUpgradeItem = new ProgressItem();
            boostUpgradeItem.type = ProgressItem.Type.BOOST_UPGRADE;
            boostUpgradeItem.name = propAsset.getString("boost_upgrade") + " ";
            if (Vars.currentNodeData.hasBooster) {
                boostUpgradeItem.name += propAsset.getString("boost_upgrade_yes");
            } else {
                boostUpgradeItem.name += propAsset.getString("boost_upgrade_no");
            }
            boostUpgradeItem.description = propAsset.getString("boost_description");
            boostUpgradeItem.found = true;
            list.Add(boostUpgradeItem);
        }
        
        // add health upgrade item
        int numHUsTotal = (int)PhysicalUpgrade.HealthUpgrade.TOTAL_HEALTH_UPGRADES;
        int numHUsCollected = Vars.currentNodeData.healthUpgrades.Count;
        int numHUsFound = Vars.healthUpgradesFound.Count;
        ProgressItem healthUpgradeItem = new ProgressItem();
        healthUpgradeItem.type = ProgressItem.Type.HEALTH_UPGRADE;
        healthUpgradeItem.name = propAsset.getString("health_upgrade") + " " + numHUsCollected + "/" + numHUsTotal + " " + propAsset.getString("collected") + " (" + numHUsFound + " " + propAsset.getString("found") + ")";
        healthUpgradeItem.description = propAsset.getString("health_description");
        healthUpgradeItem.found = true;
        list.Add(healthUpgradeItem);

        // add divider
        if (dividerPItem == null) {
            dividerPItem = new ProgressItem();
            dividerPItem.type = ProgressItem.Type.DIVIDER;
            dividerPItem.name = propAsset.getString("divider");
            dividerPItem.found = false;
        }
        list.Add(dividerPItem);



        // add decryptor header item
        /*
        int numDecryptorsFound = Vars.decryptors.Count;
        ProgressItem decryptorHeaderItem = new ProgressItem();
        decryptorHeaderItem.type = ProgressItem.Type.DECRYPTOR_HEAD;
        decryptorHeaderItem.name = propAsset.getString("decryptor") + " " + numDecryptorsFound + " " + propAsset.getString("found");
        decryptorHeaderItem.found = true;
        list.Add(decryptorHeaderItem);
        */

        // add decryptor items
        int numDecryptorsFound = Vars.decryptors.Count;
        for (int i=0; i<numDecryptorsFound; i++) {
            ProgressItem decryptorItem = new ProgressItem();
            decryptorItem.type = ProgressItem.Type.DECRYPTOR;
            decryptorItem.name = propAsset.getString("decryptor_prefix") + " ";
            if (Decryptor.requiresBooster(Vars.decryptors[i])) {
                decryptorItem.name += propAsset.getString("decryptor_booster_prefix") + " ";
                decryptorItem.found = Vars.currentNodeData.hasBooster;
            } else {
                decryptorItem.found = true;
            }
            decryptorItem.name += Decryptor.getName(Vars.decryptors[i]);
            decryptorItem.id = (int)Vars.decryptors[i];
            decryptorItem.description = Decryptor.getDescription(Vars.decryptors[i]);
            list.Add(decryptorItem);
        }

        // add divider
        if (dividerPItem == null) {
            dividerPItem = new ProgressItem();
            dividerPItem.type = ProgressItem.Type.DIVIDER;
            dividerPItem.name = propAsset.getString("divider");
            dividerPItem.found = false;
        }
        list.Add(dividerPItem);

        // add creature card header item
        /*
        int numCreatureCardsTotal = CreatureCard.getNumCardsTotal();
        int numCreatureCardsCollected = Vars.currentNodeData.creatureCards.Count;
        int numCreatureCardsFound = Vars.creatureCardsFound.Count;
        ProgressItem creatureCardHeadItem = new ProgressItem();
        creatureCardHeadItem.type = ProgressItem.Type.CREATURE_CARD_HEAD;
        creatureCardHeadItem.name = propAsset.getString("creature_card") + " " + numCreatureCardsCollected + "/" + numCreatureCardsTotal + " " + propAsset.getString("collected") + " (" + numCreatureCardsFound + " " + propAsset.getString("found") + ")";
        creatureCardHeadItem.found = true;
        list.Add(creatureCardHeadItem);
        */

        // add creature cards
        int numCreatureCardsTotal = CreatureCard.getNumCardsTotal();
        for (int i=1; i<=numCreatureCardsTotal; i++) { // note: no card with id 0
            ProgressItem creatureCardItem = new ProgressItem();
            creatureCardItem.type = ProgressItem.Type.CREATURE_CARD;
            creatureCardItem.id = i;
            bool ccFound = Vars.creatureCardFound(i);
            bool ccCollected = Vars.currentNodeData.creatureCardCollected(i);
            creatureCardItem.name = propAsset.getString("creature_card_prefix") + " ";
            if (ccFound) {
                creatureCardItem.name += CreatureCard.getCardNameFromID(i);
                if (!ccCollected) {
                    creatureCardItem.name += " (" + propAsset.getString("not_collected") + ")";
                }
                creatureCardItem.found = true;
            } else {
                creatureCardItem.name += propAsset.getString("creature_card_not_found") + " ";
                if (i < 100) creatureCardItem.name += "0";
                if (i < 10) creatureCardItem.name += "0";
                creatureCardItem.name += i + "";
                creatureCardItem.found = false;
            }
            list.Add(creatureCardItem);
        }
        
    }

    /* Displays the list.  First line is the parameter listOffset */
    public void displayList(int listOffset) {

        // setting listOffset
        listOffset = Mathf.Clamp(listOffset, minListOffset, maxListOffset);
        this.listOffset = listOffset;

        listBox.setColor(listBox.defaultStyle.color); // reset color
        string text = "";
        for (int i = listOffset; i < list.Count; i++) {
            ProgressItem pItem = list[i];
            int lineIndex = i - listOffset;
            if (lineIndex >= listBox.height)
                break;

            string line = pItem.name;
            bool grayLine = !pItem.found;

            if (pItem.type != ProgressItem.Type.DIVIDER) {
                line = "¦" + line; // is mapped to a 'space' in the font
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
        bool foundItem = false;
        if (listOffset > 0) {
            // there are more items above
            scrollUpImage.enabled = true;
            for (int i = 0; i < listOffset && i < list.Count; i++) {
                if (list[i].found) {
                    foundItem = true; // there's at least one found item above
                    break;
                }
            }
            if (foundItem) {
                scrollUpImage.sprite = scrollUpSprite;
            } else {
                scrollUpImage.sprite = scrollUpGraySprite;
            }
        } else {
            // no items above
            scrollUpImage.enabled = false;
        }

        if (listOffset + listBox.height < list.Count) {
            // there are conversations below
            scrollDownImage.enabled = true;
            foundItem = false;
            for (int i = listOffset + listBox.height; i < list.Count; i++) {
                if (list[i].found) {
                    foundItem = true; // there's at least one found item below
                    break;
                }
            }
            if (foundItem) {
                scrollDownImage.sprite = scrollDownSprite;
            } else {
                scrollDownImage.sprite = scrollDownGraySprite;
            }
        } else {
            // no items below
            scrollDownImage.enabled = false;
        }

        hoverSelect(list[selectionIndex]);
    }

    void hoverSelect(ProgressItem pItem) {
        
        switch (pItem.type) {
        case ProgressItem.Type.ORB_KEY:
        case ProgressItem.Type.BOOST_UPGRADE:
        case ProgressItem.Type.HEALTH_UPGRADE:
        case ProgressItem.Type.DECRYPTOR:
            descriptionImage.enabled = true;
            descriptionBox.setText(pItem.description);
            creatureCard.hide();
            break;
        case ProgressItem.Type.CREATURE_CARD:
            if (pItem.found) {
                creatureCard.showFront();
                creatureCard.setCard(pItem.id);
                descriptionImage.enabled = true;
                descriptionBox.setText(creatureCard.description);
            } else {
                creatureCard.showBack();
                descriptionImage.enabled = false;
                descriptionBox.makeAllCharsInvisible();
            }
            break;
        default:
            descriptionImage.enabled = false;
            descriptionBox.makeAllCharsInvisible();
            creatureCard.hide();
            break;
        }

    }

    void updateStatBoxes() {
        // play time
        string str = "";
        float playTime = Vars.playTime;
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
        playTimeBox.setPlainText(str);

        // info percent
        float percent = Vars.infoPercentComplete();
        int percentInt = Mathf.FloorToInt(percent * 100);
        str = propAsset.getString("info_percent") + " ";
        if (percentInt < 10) str += "0";
        str += percentInt + "%";
        if (percentInt >= 100) {
            str = "|" + str + "|";
        }
        infoBox.setText(str);

        // phys percent
        percent = Vars.physPercentComplete();
        percentInt = Mathf.FloorToInt(percent * 100);
        str = propAsset.getString("phys_percent") + " ";
        if (percentInt < 10) str += "0";
        str += percentInt + "%";
        if (percentInt >= 100) {
            str = "|" + str + "|";
        }
        physBox.setText(str);

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
        
        
        // input
        if (Keys.instance.upPressed) {
            decrementSelection();
            SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
        } else if (Keys.instance.downPressed) {
            incrementSelection();
            SoundManager.instance.playSFXIgnoreVolumeScale(switchSound);
        } else if (Keys.instance.confirmPressed) {
            
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
        descriptionImage.enabled = true;
        descriptionBox.makeAllCharsVisible();
        playTimeBox.makeAllCharsVisible();
        infoBox.makeAllCharsVisible();
        physBox.makeAllCharsVisible();
        updateStatBoxes();

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
        descriptionImage.enabled = false;
        descriptionBox.makeAllCharsInvisible();
        playTimeBox.makeAllCharsInvisible();
        infoBox.makeAllCharsInvisible();
        physBox.makeAllCharsInvisible();
        creatureCard.hide();
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
        descriptionImage = transform.Find("DescriptionImage").GetComponent<Image>();
        descriptionBox = transform.Find("DescriptionBox").GetComponent<GlyphBox>();
        playTimeBox = transform.Find("PlayTimeBox").GetComponent<GlyphBox>();
        infoBox = transform.Find("InfoBox").GetComponent<GlyphBox>();
        physBox = transform.Find("PhysBox").GetComponent<GlyphBox>();
        creatureCard = transform.Find("CreatureCard").GetComponent<CreatureCard>();
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
    private static ProgressPage _instance = null;

    Image listBG;
    Image scrollUpImage;
    Image scrollDownImage;
    Image selectionImage;
    GlyphBox listBox;
    Image descriptionImage;
    GlyphBox descriptionBox;
    GlyphBox playTimeBox;
    GlyphBox infoBox;
    GlyphBox physBox;
    CreatureCard creatureCard;
    
    Properties propAsset;

    ProgressItem dividerPItem;

    int _listOffset = 0;
    int _selectionVisualIndex = 0;
    float selectionVisualTime = 9999;
    float selectionVisualDuration = .15f;
    Vector2 selectionVisualPos0 = new Vector2();
    Vector2 selectionVisualPos1 = new Vector2();
    bool markNotNewOnceTextBoxNotUsed = false;
    List<ProgressItem> list = new List<ProgressItem>();

}
