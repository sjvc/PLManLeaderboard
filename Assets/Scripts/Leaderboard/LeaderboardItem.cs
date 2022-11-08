using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Baviux;

public class LeaderboardItem : MonoBehaviour, IPrefabPoolItem {
    public LeaderboardItemClickHandler clickHandler;

    private const int SCORE_MULTIPLIER = 3000000;
    
    private TextMeshProUGUI rankText;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI scoreText;
    private ParticleSystem spikes;
    private RectTransform dataContainerRect;
    private CanvasGroup dataContainerCanvasGroup;
    private RectTransform canvasRect;

    public int rank {get; private set;}
    public string title {get; private set;}
    public decimal score {get; private set;}

    private void Awake() {
        rankText = transform.Find("DataContainer/RankText").GetComponent<TextMeshProUGUI>();
        titleText = transform.Find("DataContainer/TitleText").GetComponent<TextMeshProUGUI>();
        scoreText = transform.Find("DataContainer/ScoreText").GetComponent<TextMeshProUGUI>();
        spikes = transform.Find("SparkParticles").GetComponent<ParticleSystem>();
        dataContainerRect = transform.Find("DataContainer").GetComponent<RectTransform>();
        dataContainerCanvasGroup = transform.Find("DataContainer").GetComponent<CanvasGroup>();
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    public void SetRank(int rank) {
        this.rank = rank;
        rankText.text = "#" + rank;
    }

    public void SetTitle(string title) {
        this.title = title;
        titleText.text = title;
    }

    public void SetScore(decimal score) {
        this.score = score; 
        scoreText.text = GetDisplayScore();
    }

    public string GetDisplayScore() {
        float smoothedScore = Mathf.Pow((float)score, 0.4f) * (10f / Mathf.Pow(10f, 0.4f)); // Puntuación entre 0 y 10, pero las puntuaciones bajas crecen más rápido
        return ((int)Mathf.Round(smoothedScore * SCORE_MULTIPLIER)).ToString("N0").Replace(",", ".");
    }

    public void SetOnClickListeners(List<GameObject> onClickListeners) {
        clickHandler.onClickListeners = onClickListeners;
    }

    public bool SetTitleAndScoreWithAnimation(string newTitle, decimal newScore) {
        if (newTitle == title && newScore == score) {
            return false;
        }

        spikes.gameObject.SetActive(true);
        LeanTween.delayedCall(gameObject, 2f, () => {
            spikes.gameObject.SetActive(false);
        });

        dataContainerCanvasGroup.alpha = 0;

        SetTitle(newTitle);
        SetScore(newScore);

        LeanTween.delayedCall(gameObject, 1.5f, () => {
            dataContainerCanvasGroup.alpha = 1;
            dataContainerRect.anchoredPosition = dataContainerRect.anchoredPosition.Set2(x: canvasRect.rect.width);

            LeanTween.value(gameObject, (float value) => {
                dataContainerRect.anchoredPosition = dataContainerRect.anchoredPosition.Set2(x: value);
            }, dataContainerRect.anchoredPosition.x, 0, 1f + Random.Range(0f, 0.2f)).setEaseOutBack();
        });

        return true;
    }

    void IPrefabPoolItem.OnInstantiatePoolItem() {

    }

    void IPrefabPoolItem.OnRetrievePoolItem() {
        
    }

    void IPrefabPoolItem.OnRecyclePoolItem() {
        LeanTween.cancel(gameObject);
        spikes.gameObject.SetActive(false);
        dataContainerCanvasGroup.alpha = 1;
        dataContainerRect.anchoredPosition = dataContainerRect.anchoredPosition.Set2(x: 0);

        SetRank(0);
        SetTitle("");
        SetScore(0);
    }

}
