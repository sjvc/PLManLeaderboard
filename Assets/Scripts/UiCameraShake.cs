using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiCameraShake : MonoBehaviour {
    public float distanceRatio = 1f;

    private Transform cameraTransform;
    private Vector3 initialPosition;

    private void Awake() {
        cameraTransform = Camera.main.transform;
        initialPosition = transform.position;
    }

    private void Update() {
        transform.position = (initialPosition - cameraTransform.position * distanceRatio).Set3(z: initialPosition.z);
    }
}
