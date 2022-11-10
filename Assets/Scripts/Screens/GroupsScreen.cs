using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupsScreen : MonoBehaviour, ILeaderboardItemClickListener {
    public PLManWebScrapper plman;
    public Leaderboard leaderboard;

    private LeaderBoardData data = new LeaderBoardData();

    public event System.Action<string> OnGroupSelected;
    public event System.Action OnDataRefreshAnimationPerformed;

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
        data.Clear();

        for (int i=0; i<plman.groups.Count; i++) {
            data.Add(plman.groups[i].name, plman.groups[i].averageScore);
        }

        bool leaderboardChanged = leaderboard.SetData(data, "groups", animate: animate);

        if (leaderboardChanged && animate && OnDataRefreshAnimationPerformed != null) {
            OnDataRefreshAnimationPerformed();
        }
    }

    void ILeaderboardItemClickListener.OnItemClick(string text) {
        if (OnGroupSelected != null) {
            OnGroupSelected(text);
        }
    }

}
