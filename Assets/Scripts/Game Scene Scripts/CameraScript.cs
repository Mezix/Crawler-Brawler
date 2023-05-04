using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CameraScript : MonoBehaviour
{
    public Transform objToTrack;
    private bool isMovingToPos;

    public float zoomAmount;
    public float desiredZoom;
    public float minZoom;
    public float maxZoom;

    private void Awake()
    {
        REF.cam = this;
        isMovingToPos = false;
    }
    private void Start()
    {
        zoomAmount = 0.75f;
        maxZoom = 1f;
        desiredZoom = minZoom = 7.5f;
        SetZoom(minZoom);
    }

    void Update()
    {
        if (objToTrack) MoveCameraToLocalPos();
    }

    //  Cam Movement

    public void SetObjectToTrack(Transform obj)
    {
        objToTrack = obj;
        transform.SetParent(objToTrack);
        isMovingToPos = true;
    }
    private void MoveCameraToLocalPos()
    {
        HM.RotateTransformToAngle(transform, Vector3.zero);
        if (isMovingToPos)
        {
            Vector3 newPos = Vector2.Lerp(transform.localPosition, Vector3.zero, 0.1f);
            newPos.z = -10;
            if (Vector3.Distance(Vector3.zero, newPos) < 1f)
            {
                newPos = Vector3.zero;
                isMovingToPos = false;
            }
            transform.localPosition = newPos;
        }
    }
    private void StopTracking()
    {
        objToTrack = null;
        isMovingToPos = false;
        transform.parent = null;
    }

    //  Zoom

    private void HandleZoomInput()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            if (Input.mouseScrollDelta.y > 0)
            {
                ZoomOut();
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                ZoomIn();
            }
        }
    }
    private void ZoomIn()
    {
        if (desiredZoom + zoomAmount > minZoom) desiredZoom = minZoom;
        else desiredZoom += zoomAmount;
        SetZoom(desiredZoom);
    }
    private void ZoomOut()
    {
        if (desiredZoom - zoomAmount < maxZoom) desiredZoom = maxZoom;
        else desiredZoom -= zoomAmount;
        SetZoom(desiredZoom);
    }
    public void SetZoom(float zoomLevel)
    {
        Camera.main.orthographicSize = zoomLevel;
    }
    public void SetDesiredZoom(float zoom)
    {
        if (desiredZoom > minZoom) desiredZoom = minZoom;
        else if (desiredZoom < maxZoom) desiredZoom = maxZoom;
        else desiredZoom = zoom;
    }

    //  Camera Shake

    public void StartShake(float duration, float magnitude)
    {
        StartCoroutine(Shake(duration, magnitude));
    }
    private IEnumerator Shake(float duration, float magnitude)
    {
        Camera cam = GetComponentInChildren<Camera>();
        Vector3 originalPos = cam.transform.localPosition;
        float elapsed = 0.0f;
        while(elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            cam.transform.localPosition = new Vector3(x, y, 0);
            elapsed += Time.deltaTime;

            yield return new WaitForFixedUpdate();
        }
        cam.transform.localPosition = originalPos;
    }
}
