﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Properties with a key and value */

public class Properties {

    public Properties(string strToParse = "") {
        if (strToParse != "") {
            parse(strToParse);
        }
    }

    public string getString(string key, string defaultValue="") {
        if (!dic.ContainsKey(key))
            return defaultValue;
        return dic[key];
    }
    public float getFloat(string key, float defaultValue = 0) {
        if (!dic.ContainsKey(key))
            return defaultValue;
        return float.Parse(dic[key]);
    }
    public int getInt(string key, int defaultValue = 0) {
        if (!dic.ContainsKey(key))
            return defaultValue;
        return int.Parse(dic[key]);
    }
    public bool getBool(string key, bool defaultValue = false) {
        if (!dic.ContainsKey(key))
            return defaultValue;
        string str = dic[key];
        if (str.ToLower() == "false") return false;
        if (str.ToLower() == "0") return false;
        return true;
    }
    public bool containsKey(string key) {
        return dic.ContainsKey(key);
    }
    public List<string> getKeys() {
        return new List<string>(dic.Keys);
    }
    public void setString(string key, string value) {
        dic[key] = value;
    }
    public void setFloat(string key, float value) {
        dic[key] = "" + value;
    }
    public void setInt(string key, int value) {
        dic[key] = "" + value;
    }
    public void setBool(string key, bool value) {
        dic[key] = (value ? "1" : "0");
    }
    public void remove(string key) {
        dic.Remove(key);
    }

    /* Parses string of a file with properties on it.
     * Format:
     *      Key: value
     * (keys and values are trimmed) */
    public void parse(string str) {
        dic.Clear();
        char[] delims = { '\n' };
        string[] lines = str.Split(delims);
        for (int i = 0; i < lines.Length; i++) {
            string line = lines[i];
            int index = line.IndexOf(":");
            if (index == -1)
                continue;
            string key = line.Substring(0, index).Trim();
            string value = line.Substring(index + 1).Trim();
            dic[key] = value;
        }
    }

    /// <summary>
    /// Returns a string representation of the Properties.  Can be parsed.
    /// </summary>
    public string convertToString() {
        string ret = "";
        foreach (string key in dic.Keys) {
            ret += key + ": " + dic[key] + "\n";
        }
        return ret;
    }

    Dictionary<string, string> dic = new Dictionary<string, string>();

}
