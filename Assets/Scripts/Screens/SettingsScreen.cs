using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SettingsScreen : MonoBehaviour {
    public TMP_InputField maxStudentsInputField;
    public TextMeshProUGUI versionText;

    private void Start() {
        versionText.text = "v" + Application.version;
    }

    private void OnEnable() {
        int maxStudents = Preferences.MaxStudentsInLeaderboard;
        maxStudentsInputField.text = maxStudents > 0 ? maxStudents.ToString() : "";

        if (!Application.isMobilePlatform) {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(maxStudentsInputField.gameObject);
        }
    }

    private void OnDisable() {
        int maxStudents;
        Preferences.MaxStudentsInLeaderboard = int.TryParse(maxStudentsInputField.text, out maxStudents) && maxStudents > 0 ? maxStudents : 0;
    }

}
