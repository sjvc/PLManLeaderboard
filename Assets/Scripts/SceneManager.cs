using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Baviux;

public class SceneManager : MonoBehaviour  {
    private const float TRANSITION_DURATION = 0.3f;

    public float autoRefreshInterval = 30; // Seconds

    public SpriteRenderer backgroundSprite;
    public CanvasGroup canvasGroup;
    public PLManWebScrapper plman;

    public CameraController cameraController;
    public FlashLight flashLight;

    public Button backButton;
    public Button settingsButton;

    public GameObject loginScreen;
    public GameObject groupsScreen;
    public GameObject studentsScreen;
    public GameObject settingsScreen;

    private GameObject currentScreen;
    private GameObject previousScreen;

    private LoginScreen loginScreenController;
    private GroupsScreen groupsScreenController;
    private StudentsScreen studentsScreenController;

    private int alphaCanvasTweenId;

    private WaitForSeconds autoRefreshWaitInterval;
    private Coroutine autoRefreshCoroutine;

    private Camera mainCamera;

    private SpriteRenderer flashLightSpriteRenderer;

    private void Awake() {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
		Application.targetFrameRate = 60;

        loginScreenController = loginScreen.GetComponent<LoginScreen>();
        groupsScreenController = groupsScreen.GetComponent<GroupsScreen>();
        studentsScreenController = studentsScreen.GetComponent<StudentsScreen>();

        mainCamera = Camera.main;

        flashLightSpriteRenderer = flashLight.GetComponent<SpriteRenderer>();
    }

    private void Start() {
        SetCurrentScreen(null); // Deactivate all the screens

        // Intentar obtener los datos sin usuario ni contraseña (si me he logueado recientemente, la sesión sigue abierta)
        plman.GetDataFromServer(null, null);
    }

    private void OnEnable() {
        plman.OnRequestFinished += OnPlmanWebRequestFinished;

        loginScreenController.OnLoggedIn += OnLoggedIn;
        groupsScreenController.OnGroupSelected += OnGroupSelected;
        groupsScreenController.OnDataRefreshAnimationPerformed += ShakeAndFlash;
        studentsScreenController.OnDataRefreshAnimationPerformed += ShakeAndFlash;
    }

    private void OnDisable() {
        plman.OnRequestFinished -= OnPlmanWebRequestFinished;

        loginScreenController.OnLoggedIn -= OnLoggedIn;
        groupsScreenController.OnGroupSelected -= OnGroupSelected;
        groupsScreenController.OnDataRefreshAnimationPerformed -= ShakeAndFlash;
        studentsScreenController.OnDataRefreshAnimationPerformed -= ShakeAndFlash;
    }

    private void Update() {
        if (currentScreen == null) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && backButton.gameObject.activeSelf) {
            OnBackButtonClick();
        }
        else if (Input.GetKeyDown(KeyCode.F1)) {
            OnSettingsButtonClick();
        }
        else if (Input.GetKeyDown(KeyCode.F5) && currentScreen != loginScreen) {
            RefreshData();
        }

        AdjustBackgroundSize();
    }

    public void OnSettingsButtonClick() {
        SetCurrentScreen(settingsScreen);
    }

    public void OnBackButtonClick() {
        SetCurrentScreen(currentScreen == studentsScreen ? groupsScreen : previousScreen);
    }

    private void AdjustBackgroundSize() {
        float screenWidthUnits = ScreenUtils.ScreenWidthUnits(mainCamera);
        backgroundSprite.transform.localScale = Vector3.one * (screenWidthUnits <= backgroundSprite.size.x ? 1 : screenWidthUnits / backgroundSprite.size.x);
    }

    private void OnPlmanWebRequestFinished(System.Exception e) {
        if (e != null) {
            if (autoRefreshCoroutine != null) {
                StopCoroutine(autoRefreshCoroutine);
            }
            SetCurrentScreen(loginScreen);
        } else if (currentScreen == null) {
            OnLoggedIn();
        }
    }
    
    private void SetCurrentScreen(GameObject screen) {
        if (screen != null && screen == currentScreen) {
            return;
        }

        LeanTween.cancel(alphaCanvasTweenId);

        alphaCanvasTweenId = LeanTween.alphaCanvas(canvasGroup, 0f, currentScreen != null ? TRANSITION_DURATION * 0.5f : 0f).setEaseOutCirc().setOnComplete(() => {
            // Todas a false, de forma que se ejecuta el OnDisable de la que pasa a estar inactiva
            loginScreen.SetActive(false);
            groupsScreen.SetActive(false);
            studentsScreen.SetActive(false);
            settingsScreen.SetActive(false);

            backButton.gameObject.SetActive(false);
            settingsButton.gameObject.SetActive(false);

            // La pantalla activa ejecuta ahora su OnEnable
            if (screen != null) {
                backButton.gameObject.SetActive(screen == studentsScreen || screen == settingsScreen);
                settingsButton.gameObject.SetActive( !backButton.gameObject.activeSelf );

                screen.SetActive(true);
                alphaCanvasTweenId = LeanTween.alphaCanvas(canvasGroup, 1f, TRANSITION_DURATION * 0.5f).setEaseInCirc().id;
            }
            
            previousScreen = currentScreen;
            currentScreen = screen;
        }).id;
    }

    private void OnLoggedIn() {
        SetCurrentScreen(groupsScreen);

        // Una vez hecho el login, se hacen actualizaciones de datos de forma automática cada cierto tiempo
        if (autoRefreshCoroutine == null) {
            autoRefreshCoroutine = StartCoroutine(AutoRefreshCoroutine());
        }
    }

    private IEnumerator AutoRefreshCoroutine() {
        autoRefreshWaitInterval = new WaitForSeconds(autoRefreshInterval);

        while (true) {
            yield return autoRefreshWaitInterval;
            RefreshData();
        }
    }

    private void RefreshData() {
        plman.GetDataFromServer(null, null);
    }

    private void OnGroupSelected(string group) {
        studentsScreenController.group = group;
        SetCurrentScreen(studentsScreen);
    }

    private void ShakeAndFlash() {
        cameraController.Shake(0.02f, 0.7f);
        
        float screenWidthUnits = ScreenUtils.ScreenWidthUnits(mainCamera);
        flashLight.transform.localScale = 2f * flashLight.transform.localScale.Set3(x: screenWidthUnits <= flashLightSpriteRenderer.size.x ? 1 : screenWidthUnits / flashLightSpriteRenderer.size.x);
        flashLight.Flash(0.8f);
    }

}
