using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Baviux;

public class Leaderboard : MonoBehaviour {
    public GameObject leaderboardItemPrefab;
    public List<GameObject> onItemClickListeners;

    private List<LeaderboardItem> items = new List<LeaderboardItem>();
    private string currentDataId = null;

    private RectTransform rectTransform;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable() {
        rectTransform.anchoredPosition = rectTransform.anchoredPosition.Set2(y: 0); // Reset scroll
    }

    public bool SetData(LeaderBoardData data, string dataId = null, int maxEntries = 0, bool animate = true) {
        bool animationPerformed = false;

        data.Sort();

        int dataCount = maxEntries == 0 ? data.Count : Mathf.Min(data.Count, maxEntries);

        // Si no tiene el mismo id o no tiene el mismo número de elementos, borramos todos y los añadimos de nuevo
        if (!animate || (dataId == null || currentDataId != dataId || dataCount != items.Count)) {
            ClearData();

            for (int i=0; i<dataCount; i++) {
                AddItem(i+1, data.Get(i));
            }
        } 
        // Si tiene el mismo id y mismo número de elementos, hacemos el cambio de forma animada
        else {
            for (int i=0; i<dataCount; i++) {
                animationPerformed |= items[i].SetTitleAndScoreWithAnimation(data.Get(i).text, data.Get(i).score);
            }
        }

        currentDataId = dataId;

        return animationPerformed;
    }

    private void AddItem(int rank, LeaderBoardData.LeaderBoardItemData data) {
        LeaderboardItem item = PrefabPool.instance.Retrieve(leaderboardItemPrefab, Vector3.zero, parent: transform, instantiateInWorldSpace: false).GetComponent<LeaderboardItem>();
        item.SetRank(rank);
        item.SetTitle(data.text);
        item.SetScore(data.score);
        item.SetOnClickListeners(onItemClickListeners);

        items.Add(item); 
    }

    public void ClearData() {
        for(int i=0; i<items.Count; i++) {
            PrefabPool.instance.Recycle(items[i].gameObject);
        }
        items.Clear();
    }
}

public class LeaderBoardData {
    public struct LeaderBoardItemData {
        public string text;
        public decimal score;

        public LeaderBoardItemData(string text, decimal score) {
            this.text = text;
            this.score = score;
        }
    }

    private List<LeaderBoardItemData> data = new List<LeaderBoardItemData>();

    public void Clear() {
        data.Clear();
    }

    public void Add(string text, decimal score) {
        data.Add(new LeaderBoardItemData(text, score));
    }

    public int Count {
        get {
            return data.Count;
        }
    }

    public LeaderBoardItemData Get(int i) {
        if (i<0 || i>=Count) {
            return new LeaderBoardItemData();
        }

        return data[i];
    }

    public void Sort() {
        data.Sort((d1, d2) => {return d2.score.CompareTo(d1.score);});
    }
}
