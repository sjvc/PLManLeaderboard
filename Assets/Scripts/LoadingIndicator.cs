using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingIndicator : MonoBehaviour {
    private Image[] images;
    private int currentHighlightedIndex = 0;
    private WaitForSeconds waitInterval = new WaitForSeconds(0.05f);
    private Coroutine animationCoroutine = null;

    private void Awake() {
        images = GetComponentsInChildren<Image>();
    }

    private void OnEnable() {
        animationCoroutine = StartCoroutine(AnimationCoroutine());
    }

    private void OnDisable() {
        StopCoroutine(animationCoroutine);
    }
    
    private IEnumerator AnimationCoroutine() {
        while(true) {
            for(int i=0; i<images.Length; i++) {
                images[i].transform.localScale = (currentHighlightedIndex == i ? 1.3f : 1.0f) * Vector3.one;
                images[i].color = images[i].color.Set(a: currentHighlightedIndex == i ? 1.0f : 0.4f);
            }

            if (++currentHighlightedIndex >= images.Length) {
                currentHighlightedIndex = 0;
            }

            yield return waitInterval;
        }
    }
}
