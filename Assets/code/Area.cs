using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Area {

    ////////////
    // STATIC //
    ////////////

    public static List<Area> areas = new List<Area>();

    /* Finds area that contains the position given */
    public static Area findArea(Vector2 pos) {
        Area ret = null;
        foreach (Area a in areas) {
            if (a.contains(pos)) {
                ret = a;
            }
        }
        return ret;
    }

    /* Selects a random Area from areas, provided value is a random number in [0, 1).
     * Areas with a greater area are more likely to be picked. */
    public static Area weightedRandom(List<Area> areas, float value) {
        if (value < 0 || value >= 1 || areas.Count == 0)
            return null;
        float a = 0;
        for (int i = 0; i < areas.Count; i++) {
            a += areas[i].area;
        }
        float target = a * value;
        a = 0;
        for (int i = 0; i < areas.Count; i++) {
            a += areas[i].area;
            if (a > target)
                return areas[i];
        }
        return areas[areas.Count - 1];
    }

    public static void sortOnY(List<Area> areas) {
        areas.Sort(delegate(Area a1, Area a2) {
            return (int)(a1.center.y - a2.center.y);
        });
    }

    /* Attempts to add area to one of the above lists.
     * Prereq: apply snapToWall() beforehand. */
    public static bool addArea(Area area) {
        areas.Add(area);
        return true;
    }

    /* Removes all stored areas */
    public static void removeAll() {
        areas.Clear();
    }

    ////////////
    // PUBLIC //
    ////////////

    public string name = ""; // will be set to the name of the GameObject in the inspector

    public float left = 0;
    public float bottom = 0;
    public float right = 0;
    public float top = 0;
    public float width {
        get { return right - left; }
    }
    public float height {
        get { return top - bottom; }
    }
    public float area {
        get { return width * height; }
    }
    public Vector2 center {
        get { return new Vector2((left + right) / 2, (bottom + top) / 2); }
    }

    ///////////////
    // FUNCTIONS //
    ///////////////

    /* value is in [0, 1).
     * padding is borders to not consider */
    public Vector2 randPoint(float value, float padding = 0) {
        float padW = Mathf.Max(0, width/2 - padding);
        float padH = Mathf.Max(0, height/2 - padding);

        Random.seed = (int)(value * int.MaxValue);
        float value2 = Random.value;

        Vector2 ret = new Vector2(padW * (value * 2 - 1), padH * (value * 2 - 1));
        return ret + center;
    }

    public bool contains(Vector2 point) {
        return left <= point.x && point.x <= right &&
            bottom <= point.y && point.y <= top;
    }

}
