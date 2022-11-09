using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class LoginScreen : MonoBehaviour {
    public PLManWebScrapper plman;

    public TMP_InputField userInputField;
    public TMP_InputField passwordInputfield;
    public Button loginButton;
    public TextMeshProUGUI errorText;

    public event System.Action OnLoggedIn;

    private void Start() {
        errorText.gameObject.SetActive(false);
        
        string lastLoggedUser = Preferences.LastLoggedUser;
        if (lastLoggedUser != null && lastLoggedUser.Length > 0) {
            userInputField.text = lastLoggedUser;
            if (!Application.isMobilePlatform) EventSystem.current.SetSelectedGameObject(passwordInputfield.gameObject);
        } else {
            if (!Application.isMobilePlatform) EventSystem.current.SetSelectedGameObject(userInputField.gameObject);
        }
    }

    private void OnEnable() {
        plman.OnRequestFinished += OnPLManWebRequestFinished;
    }

    private void OnDisable() {
        plman.OnRequestFinished -= OnPLManWebRequestFinished;
    }

    private void Update() {

        if (Input.GetKeyDown(KeyCode.Tab)) {
            Selectable next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
            if (next != null) {         
                EventSystem.current.SetSelectedGameObject(next.gameObject);
            }
        }

        if (Input.GetKeyDown(KeyCode.Return) && EventSystem.current.currentSelectedGameObject != loginButton.gameObject) {
            loginButton.onClick.Invoke();
        }
        
    }

    public void OnLoginClick() {
        if (userInputField.text.Trim().Length == 0 || passwordInputfield.text.Trim().Length == 0) {
            errorText.gameObject.SetActive(true);
            errorText.text = "All fields are required";
            return;
        }
        
        SetControlsEnabled(false);

        errorText.gameObject.SetActive(false);

        plman.GetDataFromServer(userInputField.text, passwordInputfield.text);
    }

    private void OnPLManWebRequestFinished(System.Exception e) {
        SetControlsEnabled(true);

        if (e != null) {
            Debug.LogError(e);
            errorText.gameObject.SetActive(true);
            errorText.text = e.Message;
            return;
        }

        Preferences.LastLoggedUser = userInputField.text;

        if (OnLoggedIn != null) {
            OnLoggedIn();
        }
    }

    private void SetControlsEnabled(bool value) {
        userInputField.enabled = value;
        passwordInputfield.enabled = value;
        loginButton.enabled = value;
    }

}
