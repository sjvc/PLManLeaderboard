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
    private LeaderBoardData initialData = new LeaderBoardData(); // Datos iniciales, para luego comparar las variaciones de posiciones de cada elemento

    private RectTransform rectTransform;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable() {
        rectTransform.anchoredPosition = rectTransform.anchoredPosition.Set2(y: 0); // Reset scroll
    }

    public bool SetData(LeaderBoardData data, string dataId = null, int maxEntries = 0, bool animate = true) {
        bool leaderboardChanged = false;

        data.Sort();

        int dataCount = maxEntries == 0 ? data.Count : Mathf.Min(data.Count, maxEntries);

        // Si tiene el mismo id y mismo número de elementos, hacemos el cambio elemento a elemento
        if (dataId != null && currentDataId == dataId && dataCount == items.Count) {
            for (int i=0; i<dataCount; i++) {
                leaderboardChanged |= items[i].SetTitleAndScore(data.Get(i).text, data.Get(i).score, animate);

                // Si ha habido un cambio -> calcular cambio de posición respecto a los datos iniciales
                if (leaderboardChanged) {
                    int initialPosition = initialData.IndexOf(data.Get(i).text);
                    items[i].SetRankChange(initialPosition - i);
                }
            }
        }
        // Si los datos no tienen el mismo id y número de elementos, borramos todos y los añadimos de nuevo
        else {
            ClearData();

            for (int i=0; i<dataCount; i++) {
                AddItem(i+1, data.Get(i));
            }

            // Guardamos los datos iniciales
            data.CloneTo(initialData);
        } 

        currentDataId = dataId;

        return leaderboardChanged;
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

    public int IndexOf(string text) {
        for (int i=0; i<data.Count; i++) {
            if (data[i].text == text) {
                return i;
            }
        }

        return -1;
    }

    public void Sort() {
        data.Sort((d1, d2) => {
            if (d1.score == d2.score) {
                return d1.text.CompareTo(d2.text);
            }
            
            return d2.score.CompareTo(d1.score);
        });
    }
    
    public void CloneTo(LeaderBoardData clonedData) {
        clonedData.Clear();

        for (int i=0; i<data.Count; i++) {
            clonedData.Add(Get(i).text, Get(i).score);
        }
    }
}
