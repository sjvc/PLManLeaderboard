using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudentsScreen : MonoBehaviour {
    public PLManWebScrapper plman;
    public Leaderboard leaderboard;

    private LeaderBoardData data = new LeaderBoardData();

    public event System.Action OnDataRefreshAnimationPerformed;

    [System.NonSerialized]
    public string group;

    private void OnEnable() {
        RefreshLeaderboardData(animate: false);
        
        plman.OnRequestFinished += OnPLManWebRequestFinished;
    }

    private void OnDisable() {
        plman.OnRequestFinished -= OnPLManWebRequestFinished;
    }

    private void OnPLManWebRequestFinished(System.Exception e) {
        if (e == null) {
            RefreshLeaderboardData();
        }
    }

    private void RefreshLeaderboardData(bool animate = true) {
        if (group == null) {
            return;
        }

        data.Clear();

        List<Student> students = plman.groups.Find(g => g.name == group).students;
        for (int i=0; i<students.Count; i++) {
            data.Add(students[i].name, students[i].score);
        }

        bool leaderboardChanged = leaderboard.SetData(data, group, Preferences.MaxStudentsInLeaderboard, animate);

        if (leaderboardChanged && animate && OnDataRefreshAnimationPerformed != null) {
            OnDataRefreshAnimationPerformed();
        }
    }
}
