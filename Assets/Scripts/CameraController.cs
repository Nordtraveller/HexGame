using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    Transform rotation, zoom;

    public HexagonGrid grid;

    float zoomValue = 1f;
    float rotationAngle;

    public float minZoom, maxZoom;
    public float minZoomRotation, maxZoomRotation;
    public float moveSpeedMinZoom, moveSpeedMaxZoom;
    public float rotationSpeed;

    private void Awake()
    {
        rotation = transform.GetChild(0);
        zoom = rotation.GetChild(0);
    }

    private void Update()
    {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        float rotationDelta = Input.GetAxis("Rotation");
        if (rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f)
        {
            AdjustPosition(xDelta, zDelta);
        }
    }

    void AdjustZoom(float zoomDelta)
    {
        zoomValue = Mathf.Clamp01(zoomValue + zoomDelta);

        float distance = Mathf.Lerp(minZoom, maxZoom, zoomValue);
        zoom.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(minZoomRotation, maxZoomRotation, zoomValue);
        rotation.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    void AdjustRotation(float rotationDelta)
    {
        rotationAngle += rotationDelta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0f)
        {
            rotationAngle += 360f;
        }
        else if (rotationAngle >= 360f)
        {
            rotationAngle -= 360f;
        }
        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }

    void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoomValue) * damping * Time.deltaTime;
        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = ClampPosition(position);
    }

    Vector3 ClampPosition(Vector3 position)
    {
        float xMax = (grid.meshCountX * HexagonMetrics.meshPartSizeX - 0.5f) * (2f * HexagonMetrics.innerRadius);
        position.x = Mathf.Clamp(position.x, 0f, xMax);

        float zMax = (grid.meshCountZ * HexagonMetrics.meshPartSizeZ - 1) * (1.5f * HexagonMetrics.outerRadius);
        position.z = Mathf.Clamp(position.z, 0f, zMax);

        return position;
    }
}
