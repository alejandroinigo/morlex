using System;
using System.Collections;
using UnityEngine;

public class HexGameCamera : MonoBehaviour {

    public HexGrid grid;
	static HexGameCamera instance;
    Transform swivel, stick;
    float stickMinZoom = -500;
    float stickMaxZoom = -300;
    float swivelMinZoom = 90;
    float swivelMaxZoom = 45;
    float rotationSpeed = 1f;
    float zoomDelta = 0f;
    float rotationDelta = 0f;
    float translateDelta = 0f;
    float oscillationDelta = 0f;
    float zoom = 0f;
    float translate = 0f;
    float rotationAngle;
    float oscillationAngle = 0;
    float fullCellSize = 1f;
    float halfCellSize = 0.5f;
    bool zoomEvent = false;
    bool translateEvent = false;
    bool translateLimitEvent = false;
    float translateLimitEventFactor = 0.3f;
    float oscillationOffset = 0.5f;
    float oscillationFrequency = 0.1f;
    
	void Awake () {
		swivel = transform.GetChild(0);
		stick = swivel.GetChild(0);
	}

	void OnEnable () {
		instance = this;
	}

    void Update () {
		if (zoomEvent && zoomDelta != 0f) {
			AdjustZoom(zoomDelta);
            if (zoom >= 1f || zoom <= 0f) {
                zoomEvent = false;
            }
		}
		if (rotationDelta != 0f) {
			AdjustRotation(rotationDelta);
		}
        if (!zoomEvent && translateEvent && translateDelta != 0f) {
			AdjustTranslate(translateDelta);
            if (translate >= 1f || translate <= 0f) {
                translateEvent = false;
            } else {
                if(
                    (translateDelta > 0 && translate >= (1f - translateLimitEventFactor))
                    || (translateDelta < 0 && translate < (translateLimitEventFactor))
                )
                    translateLimitEvent = true;
            }
		}
        if (oscillationDelta != 0f) {
            AdjustOscillation(oscillationDelta);
        }
	}
	
    void InitPosition () {
        Vector3 position = transform.localPosition;
        float centerPositionX = 0f;
        float centerPositionZ = 0f;
        if(grid != null && grid.gameObject.activeInHierarchy == true) {
            float xMapSize = (float)(grid.cellCountX) * (2f * HexMetrics.innerRadius);
            float zMapSize = (float)(grid.cellCountZ) * (1.5f * HexMetrics.outerRadius);
            centerPositionX = xMapSize * 0.5f;
            centerPositionZ = zMapSize * 0.5f;
        }
        position.x = centerPositionX;
        position.y = 0;
        position.z = centerPositionZ;
        transform.localPosition = position;
    }

    void SetOriginPosition () {
        Vector3 position = transform.localPosition;
        position.x = 0f;
        position.z = 0f;
        transform.localPosition = position;
    }

	public static void ResetPositionOrigin () {
		instance.SetOriginPosition();
        instance.zoom = 0f;
        instance.AdjustZoom(0f);
        instance.rotationAngle = 0f;
        instance.AdjustRotation(0f);
	}

	public static void ResetZoomOut () {
		instance.InitPosition();
        instance.zoom = 0f;
        instance.AdjustZoom(0f);
        instance.rotationAngle = 0f;
        instance.AdjustRotation(0f);
	}

    public static void ResetZoomMiddle () {
		instance.InitPosition();
        instance.zoom = 0f;
        instance.AdjustZoom(0.5f);
        instance.rotationAngle = 0f;
        instance.AdjustRotation(0f);
	}

    public static void ResetZoomIn () {
		instance.InitPosition();
        instance.zoom = 0f;
        instance.AdjustZoom(1f);
        instance.rotationAngle = 0f;
        instance.AdjustRotation(0f);
	}

    public static void ZoomIn () {
		instance.zoomDelta = 0.001f;
        instance.zoomEvent = true;
	}

    public static void ZoomOut () {
		instance.zoomDelta = -0.001f;
        instance.zoomEvent = true;
	}

    public static IEnumerator TranslateOrigin (Action<bool> done) {
        instance.translate = 1f;
        instance.translateDelta = -0.005f;
        instance.translateEvent = true;
        yield return new WaitWhile(() => instance.translateEvent);
        done(true);
	}

    public static IEnumerator FallDown(Action<bool> done) {
        instance.translate = 1f;
        instance.translateDelta = -0.003f;
        instance.translateEvent = true;
        instance.translateLimitEvent = false;
        yield return new WaitWhile(() => !instance.translateLimitEvent);
        done(true);
	}

	public static void SetRotation (float rotationAngle) {
        instance.rotationAngle = 0f;
        instance.AdjustRotation(0f);
	}

    public static void StartRotation () {
		instance.rotationDelta = 1f;
	}

    public static void StopRotation () {
		instance.rotationDelta = 0f;
	}

    public static void StartOscillation () {
		instance.oscillationDelta = 1f;
	}

    public static void StopOscillation () {
		instance.oscillationDelta = 0f;
	}
    
    void AdjustZoom (float delta) {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
		stick.localPosition = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
		swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
	}

    void AdjustTranslate(float delta) {
        translate = Mathf.Clamp01(translate + delta);

        float translatePositionY = Mathf.Lerp(stickMinZoom, 0f, translate);
        Vector3 position = transform.localPosition;
        position.y = translatePositionY;
        transform.localPosition = position;
	}

    void AdjustRotation (float delta) {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0f) {
			rotationAngle += 360f;
		}
		else if (rotationAngle >= 360f) {
			rotationAngle -= 360f;
		}
		transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
	}

    void AdjustOscillation(float delta) {
        oscillationAngle += delta * oscillationFrequency * Time.deltaTime;
        if (oscillationAngle < 0f) {
			oscillationAngle += 360f;
		}
		else if (oscillationAngle >= 360f) {
			oscillationAngle -= 360f;
		}
        float oscillationX = Mathf.Sin(2f * Mathf.PI * oscillationAngle);
        Vector3 localEulerAngles = transform.localEulerAngles;
		transform.localRotation = Quaternion.Euler(oscillationX * oscillationOffset, localEulerAngles.y, 0f);
    }

    Vector3 ClampPosition (Vector3 position) {
        float xMax = (float)(grid.cellCountX - halfCellSize) * (2f * HexMetrics.innerRadius);
		position.x = Mathf.Clamp(position.x, 0f, xMax);

        float zMax = (float)(grid.cellCountZ - fullCellSize) * (1.5f * HexMetrics.outerRadius);
		position.z = Mathf.Clamp(position.z, 0f, zMax);

		return position;
	}
}