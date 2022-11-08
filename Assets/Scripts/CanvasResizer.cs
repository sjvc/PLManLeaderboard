using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Baviux;

public class CanvasResizer : MonoBehaviour {
	private CanvasScaler canvasScaler;
	private RectTransform screenSpace;
	
	void Awake(){
		canvasScaler = GetComponent<CanvasScaler>();

		SetScalerReferenceResolution();
	}

	private void SetScalerReferenceResolution() {
		// Estableciendo la resolución actual como referenceResolution, conseguimos una escala canvasScaler.referencePixelsPerUnit = 1ud.
		// Así conseguimos que la escala (px a unidades) de las imágenes de la UI sea la misma que la de los sprites del juego.
		Camera mainCamera = Camera.main;
		canvasScaler.referenceResolution = new Vector2(ScreenUtils.ScreenWidthUnits(mainCamera) * canvasScaler.referencePixelsPerUnit, ScreenUtils.ScreenHeightUnits(mainCamera) * canvasScaler.referencePixelsPerUnit);
	}
}
