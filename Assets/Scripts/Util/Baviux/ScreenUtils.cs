using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Baviux {

public static class ScreenUtils {
	private static string pathFromPersistentDataPathToRoot = null;

	// Obtiene el alto de la pantalla en unidades (depende del tamaño de la cámara ortográfica)
	public static float ScreenHeightUnits(Camera camera){
		return camera.orthographicSize * 2f;
	}

	// Obtiene el ancho de la pantalla en unidades (depende del tamaño de la cámara ortográfica)
	public static float ScreenWidthUnits(Camera camera){
		return ScreenHeightUnits(camera) * camera.aspect;
	}

	// Convierte DP a píxeles de pantalla
	public static int ConvertDpToPx(float dp) {
		#if UNITY_IOS && !UNITY_EDITOR
			return (int)Mathf.Round(dp * IosDeviceDisplay.scaleFactor); // En iOS no siempre es Screen.dpi / 160.0f el factor de escala
		#else
			return (int)Mathf.Round(dp * Screen.dpi / 160.0f);
		#endif
	}

	// Convierte píxeles de pantalla a DP
	public static float ConvertPxToDp(int px) {
		#if UNITY_IOS && !UNITY_EDITOR
			return (int)Mathf.Round(px / IosDeviceDisplay.scaleFactor); // En iOS no siempre es Screen.dpi / 160.0f el factor de escala
		#else
			return (int)Mathf.Round(px * (160.0f / Screen.dpi));
		#endif	
	}

	// Convierte unidades del juego a píxeles en pantalla (depende del tamaño de la cámara ortográfica)
	public static int ConvertUnitsToPx(float units, Camera camera) {
		return (int)Mathf.Round(units * (ScreenManager.instance.NativeHeightPx / (float)ScreenUtils.ScreenHeightUnits(camera)));
	}

	// Convierte píxeles de pantalla a unidades del juego (depende del tamaño de la cámara ortográfica)
	public static float ConvertPxToUnits(int px, Camera camera) {
		return px * (ScreenUtils.ScreenHeightUnits(camera) / (float)ScreenManager.instance.NativeHeightPx);
	}

	public static void TakeScreenshot(string path, string fileName){
		// On mobile platforms the filename is appended to the persistent data path (Application.persistentDataPath)
		#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
			if (Application.persistentDataPath.Equals(path) || string.IsNullOrEmpty(path)){
				path = null;
			} else {
				path = string.Format("{0}{1}{2}", GetPathFromPersistentDataToRoot(), path.Length > 0 && path[0] == Path.DirectorySeparatorChar ? "" : Path.DirectorySeparatorChar.ToString(), path);
			}
		#endif

		ScreenCapture.CaptureScreenshot(string.IsNullOrEmpty(path) ? fileName : Path.Combine(path, fileName));
	}

	private static string GetPathFromPersistentDataToRoot(){
		if (string.IsNullOrEmpty(pathFromPersistentDataPathToRoot)){
			pathFromPersistentDataPathToRoot = "";
			DirectoryInfo folder = new DirectoryInfo(Application.persistentDataPath);
			while(folder.Parent != null){
				pathFromPersistentDataPathToRoot = Path.Combine(pathFromPersistentDataPathToRoot, "..");
				folder = folder.Parent;
			}
		}

		return pathFromPersistentDataPathToRoot;
	}
}

}