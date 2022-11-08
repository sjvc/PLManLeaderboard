/**
	IMPORTANT: Set this script to be executed before the default time (Script Execution Order)
	so resolution, camera size and safe area is set an the beginning
**/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Baviux {

public class ScreenManager : MonoBehaviour {
	[Serializable]
	public class CameraSetEvent : UnityEvent<ScreenManager, Camera>{};

	public bool resizeOnMobile = true;
	[ConditionalHide("resizeOnMobile", true)]
	public int targetWidthPx = 0;
	[ConditionalHide("resizeOnMobile", true)]
	public int targetHeightPx = 1080;
	
	public CameraSetEvent OnSetCameraSize; // Set here the camera size in units here, so then we can calculate the unsafe areas in units

	[HideInInspector]
	public static ScreenManager instance;
	
	// Resolución nativa en pixels del dispositivo (con la que se inicia el juego)
	public int NativeWidthPx {get { return Mathf.Approximately(nativeWidthPx, 0) ? CurrentWidthPx : nativeWidthPx; } }
	public int NativeHeightPx {get { return Mathf.Approximately(nativeHeightPx, 0) ? CurrentHeightPx : nativeHeightPx; } }

	// Resolución atual en pixels (si tras llamar a SetResolution Screen.width y Screen.height no cambian, es porque no se ha podido establecer la resolución)
	public int CurrentWidthPx {get { return Screen.width; } }
	public int CurrentHeightPx {get { return Screen.height; } }

	// Tamaños en unidades de las partes superior e inferior fuera de la safe area
	public float UnsafeAreaTopUnits {get {RefreshSafeArea(); return unsafeAreaTopUnits;}}
	public float UnsafeAreaBottomUnits {get {RefreshSafeArea(); return unsafeAreaBottomUnits;}}

	private int nativeWidthPx = 0;
	private int nativeHeightPx = 0;

	private float unsafeAreaTopUnits = 0;
	private float unsafeAreaBottomUnits = 0;

	private bool desktopFullScreen = false;

	private Rect lastSafeArea;

	private Camera mainCamera;
	
	void Awake(){
		instance = this;

		nativeWidthPx = Screen.width;
		nativeHeightPx = Screen.height;

		// Set here the camera size in units now, so then we can calculate the unsafe areas in units (relative to the total units [this is the double of camera size])
		if (OnSetCameraSize != null) {
			OnSetCameraSize.Invoke(this, GetMainCamera());
		}

		RefreshSafeArea();

		if (Application.isMobilePlatform && resizeOnMobile) {
			SetMobileOptimalResolution();

			// Dejo la resolución como estaba al salir (si no lo hago, en el editor, al reiniciar el juego, a veces todavía tiene la resolución cambiada)
			#if UNITY_EDITOR
			EditorApplication.playModeStateChanged += ((PlayModeStateChange state) => {
				if (state == PlayModeStateChange.ExitingPlayMode) {
					SetScreenResolution(NativeWidthPx, NativeHeightPx, true);
				}
			});
			#endif
		}
	}

	private void Start() {
		SetDesktopResolution();
	}
	
	private void Update() {
		if (!Application.isMobilePlatform) {
			if ((Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.RightCommand)) && Input.GetKeyDown(KeyCode.F)) {
				desktopFullScreen = !desktopFullScreen;
				SetDesktopResolution();
			}
		}
	}

	private void SetDesktopResolution() {
		if (!Application.isMobilePlatform) {
			if (desktopFullScreen) {
				Screen.fullScreenMode = Application.platform == RuntimePlatform.OSXPlayer ? FullScreenMode.MaximizedWindow : FullScreenMode.ExclusiveFullScreen;
				OnEnterFullScreen();
			} else {
				Screen.fullScreenMode = FullScreenMode.Windowed;
				OnExitFullScreen();
			}
		}
	}

	private void OnEnterFullScreen() {
		SetScreenResolution(Screen.mainWindowDisplayInfo.width, Screen.mainWindowDisplayInfo.height, true);
	}

	private void OnExitFullScreen() {
		SetScreenResolution(Screen.mainWindowDisplayInfo.width/2, Screen.mainWindowDisplayInfo.height/2, false);
	}

	private Camera GetMainCamera() {
		if (mainCamera == null) {
			mainCamera = Camera.main;
		}

		return mainCamera;
	}

	// https://connect.unity.com/p/updating-your-gui-for-the-iphone-x-and-other-notched-devices
	private void RefreshSafeArea() {
	    Rect safeArea = Screen.safeArea; // 0,0 point is at bottom-left

		if (lastSafeArea == safeArea) {
			return;
		}

		lastSafeArea = safeArea;

		float unitsPerPixel = (ScreenUtils.ScreenHeightUnits(GetMainCamera()) / (float)CurrentHeightPx);
		unsafeAreaTopUnits = (CurrentHeightPx - safeArea.yMax) * unitsPerPixel;
		unsafeAreaBottomUnits = safeArea.y * unitsPerPixel;
	}

	private void SetMobileOptimalResolution() {
		if (targetHeightPx != 0 && targetWidthPx != 0) {
			throw new ArgumentException("Please set targetHeightPx or targetWidthPx, but not both. Leave at least one of them as 0");
		}

		if (targetHeightPx != 0) {
			int optimalHeightPx = GetMobileOptimalDimensionPx(targetHeightPx, NativeHeightPx);
			if (optimalHeightPx != CurrentHeightPx) {
				SetScreenResolution(Mathf.RoundToInt(optimalHeightPx * Camera.main.aspect), optimalHeightPx, true);
			}
		} else if (targetWidthPx != 0) {
			int optimalWidthPx = GetMobileOptimalDimensionPx(targetWidthPx, NativeWidthPx);
			if (optimalWidthPx != CurrentWidthPx) {
				SetScreenResolution(optimalWidthPx, Mathf.RoundToInt(optimalWidthPx / Camera.main.aspect), true);
			}
		}
	}

	private int GetMobileOptimalDimensionPx(int targetDimensionPx, int nativeDimesionPx) {
		if (targetDimensionPx <= 0 || nativeDimesionPx <= targetDimensionPx){
			return nativeDimesionPx;
		}

		float factor = (float)targetDimensionPx / (float)nativeDimesionPx;
		factor = Mathf.Round(factor / 0.25f) * 0.25f; // Get nearest multiple of 0.25

		return (int)(nativeDimesionPx * factor);
	}

	private void SetScreenResolution(int width, int height, bool fullScreen) {
		Screen.SetResolution(width, height, fullScreen); // Resolution won't be changed until next frame
	}

	// void OnGUI() {
    //     GUI.Label(
    //        new Rect(
    //            5,                   // x, left offset
    //            Screen.height - 150, // y, bottom offset
    //            300f,                // width
    //            150f                 // height
    //        ),      
    //        Screen.width + " " + Screen.height,             // the display text
    //        GUI.skin.textArea        // use a multi-line text area
    //     );
    // }

}

}
