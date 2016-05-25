using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Decryptor {
    
    // convention: ID's that are numbered are implemented and functional (but the locations of the decryptor pickups aren't necessarily set)
    public enum ID {
        NONE = 0,
        CHARGE_SHOT = 1, // required
        CHARGE_MAGNET = 2,
        ALTERED_SHOT = 3,
        ROOM_RESTART = 4,
        BLINK_DODGE, // required
        BLINK_SLIDE,
        SAFETY_GRENADE,
        ENERGY_GRENADE, // required
        SPLIT_SHOT,
        NATIVE_VISION,
        WALL_RUN, // required
        INTERMEDIATE_FLASHBACK,
        INTUITION,
        MULTI_GRENADE,
        GLIDE, // required
        ENERGY_GRENADE_GROWTH,
        CONCENTRATION,
        HYPER_SPEED, // required
        PIERCING_SHOT,
        BURST_CHARGE_SHOT,
        RESISTANCE,
        FOCUS_ENERGY,
        HARMFUL_AURA,
        DEFENSE,
        HYPER_CHARGE_SHOT, // required
        BLINK_DUO,
        DEADLY_DPS,
        STRIP_SUIT, // required
        VIRUS_WIPE, // required
        GRENADE_FLASHBACK,
        FLARE_DIVE, // required
        NO_OBJECTIONS,
        

        LAST_ID // not a decryptor, keep this as the last one
    }

    public static bool requiresBooster(ID id) {
        switch (id) {
        case ID.GLIDE:
        case ID.HYPER_SPEED:
        case ID.HYPER_CHARGE_SHOT:
        case ID.FLARE_DIVE:
            return true;
        }
        return false;
    }

    public static List<ID> prerequisiteDecryptors(ID id) {
        switch (id) {
        case ID.CHARGE_MAGNET:
        case ID.PIERCING_SHOT:
        case ID.BURST_CHARGE_SHOT:
        case ID.HYPER_CHARGE_SHOT:
        case ID.HARMFUL_AURA:
            return new List<ID> { ID.CHARGE_SHOT };
        case ID.BLINK_SLIDE:
        case ID.BLINK_DUO:
            return new List<ID> { ID.BLINK_DODGE };
        case ID.SAFETY_GRENADE:
        case ID.ENERGY_GRENADE_GROWTH:
            return new List<ID> { ID.ENERGY_GRENADE };
        case ID.MULTI_GRENADE:
            return new List<ID> { ID.CHARGE_SHOT, ID.ENERGY_GRENADE };
        case ID.NO_OBJECTIONS:
            return new List<ID> { ID.STRIP_SUIT };
        }
        return new List<ID>();
    }



    public static bool initialized {  get { return _initialized; } }
    public static void initialize() {
        if (initialized) return;
        TextAsset ta = (Resources.Load("decryptor_info") as TextAsset);
        Properties prop = new Properties(ta.text);
        initialize(prop);
    }
    public static void initialize(Properties prop) {
        if (initialized) return;

        int count = (int)ID.LAST_ID;
        for (int i = 0; i<count; i++) {
            string key = "";
            key+=i;
            if (!prop.containsKey(key+"name")&&!prop.containsKey(key+"description"))
                continue;
            DecryptorInfo di = new DecryptorInfo();
            di.id = (ID)i;
            di.name = prop.getString(key + "name", "");
            di.code = prop.getString(key + "code", "");
            di.description = prop.getString(key + "description", "");
            info[di.id] = di;
        }
        _initialized = true;

    }

    public static bool canUse(ID decryptor, bool hasBooster, List<ID> collectedDecryptors) {
        if (!initialized) initialize();
        if (!hasBooster) {
            if (requiresBooster(decryptor))
                return false;
        }
        List<ID> prereqs = prerequisiteDecryptors(decryptor);
        foreach (ID prereq in prereqs) {
            if (collectedDecryptors.IndexOf(prereq)==-1)
                return false;
        }
        return true;
    }

    public static string getName(ID decryptor) {
        if (!initialized) initialize();
        if (!info.ContainsKey(decryptor)) {
            return "";
        }
        return info[decryptor].name;
    }

    public static string getCode(ID decryptor) {
        if (!initialized) initialize();
        if (!info.ContainsKey(decryptor)) {
            return "";
        }
        return info[decryptor].code;
    }

    public static string getDescription(ID decryptor) {
        if (!initialized) initialize();
        if (!info.ContainsKey(decryptor)) {
            return "";
        }
        return info[decryptor].description;
    }


    class DecryptorInfo {
        public ID id = ID.NONE;
        public string name = "";
        public string code = "";
        public string description = "";
    }

    static Dictionary<ID, DecryptorInfo> info = new Dictionary<ID, DecryptorInfo>();
    static bool _initialized = false;

    

}
