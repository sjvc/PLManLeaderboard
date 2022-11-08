using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLight : MonoBehaviour {
    private SpriteRenderer spriteRenderer;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Flash(float duration) {
        spriteRenderer.color = spriteRenderer.color.Set(a: 1);
        LeanTween.value(gameObject, (float value) => {
            spriteRenderer.color = spriteRenderer.color.Set(a: value);
        }, 1f, 0f, duration).setEase(LeanTweenType.easeOutSine);
    }
}
