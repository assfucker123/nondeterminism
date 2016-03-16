using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChamberScreen : MonoBehaviour {

    public string positionCode = "E5";
    public TextAsset textAsset;

    public bool simplifiedOptions {
        get {
            return false;
        }
    }

    public static Color DEFAULT_COLOR = new Color(200/255f, 45/255f, 100/255f); // 8CD7FF
    public static Color SELECTED_COLOR = new Color(22/255f, 33/255f, 40/255f); // 162128

    void Awake() {
        image = GetComponent<Image>();
        selection = transform.Find("Selection").GetComponent<Image>();
        selectionSmall = transform.Find("SelectionSmall").GetComponent<Image>();
        glyphBoxMain = transform.Find("GlyphBoxMain").GetComponent<GlyphBox>();
        propAsset = new Properties(textAsset.text);
	}

    void setMainMenu() {
        image.enabled = true;
        // what glyphBoxMain says
        string str = propAsset.getString("chamber") + " " + positionCode + "\n\n\n";
        if (simplifiedOptions) {
            str +=
                "\n" +
                propAsset.getString("chamber_save") + "\n" +
                propAsset.getString("back");
        } else {
            str +=
                propAsset.getString("chamber_save") + "\n" +
                propAsset.getString("wait") + "\n" +
                propAsset.getString("delete_nodes") + "\n" +
                propAsset.getString("back");
        }
        glyphBoxMain.clearText();
        glyphBoxMain.setText(str);
        // selections
        selection.enabled = true;
        selectionSmall.enabled = false;
    }

    void Start() {
        setMainMenu();
    }
	
	void Update() {
		
	}

    Image image;
    GlyphBox glyphBoxMain;
    Image selection;
    Image selectionSmall;
    Properties propAsset;

}
