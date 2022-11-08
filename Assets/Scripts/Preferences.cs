using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Preferences {
    private const string KEY_LAST_USER = "last_user";
    private const string KEY_MAX_STUDENTS = "max_students";

    public static string LastLoggedUser {
        get {
            return PlayerPrefs.GetString(KEY_LAST_USER, null);
        }

        set {
            PlayerPrefs.SetString(KEY_LAST_USER, value);
        }
    }

    public static int MaxStudentsInLeaderboard {
        get {
            return PlayerPrefs.GetInt(KEY_MAX_STUDENTS, 0);
        }

        set {
            PlayerPrefs.SetInt(KEY_MAX_STUDENTS, value);
        }
    }

}
