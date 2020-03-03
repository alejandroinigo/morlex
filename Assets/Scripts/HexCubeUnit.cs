using System;
using System.Collections;
using UnityEngine;

public class HexCubeUnit : MonoBehaviour
{
    static HexCubeUnit instance;
    float rotationSpeed;
    float rotationSpeedDelta = 10f;
    float maxRotationSpeed = 250f;
    float rotationDelta = 0f;
    float rotationAngle = 0f;
    bool stopRotationEvent = true;
    bool rotating = false;

	void OnEnable () {
		instance = this;
	}

    void Update()
    {
        if (rotationDelta != 0f) {
            rotating = true;
			AdjustSpeed(rotationSpeedDelta);
            AdjustRotation(rotationDelta);
		} else {
            rotating = false;
        }
    }

    public IEnumerator ShowChilds() {
        foreach (Transform childCube in transform)
        {
            yield return new WaitForSeconds(1);
            childCube.gameObject.SetActive(true);
        }        
    }
    public IEnumerator StartRotation (Action<bool> done) {
        instance.stopRotationEvent = false;
		instance.rotationDelta = 1f;
        instance.rotationSpeed = 0f;
        yield return new WaitWhile(() => rotationSpeed < maxRotationSpeed);
        done(true);
	}

    public IEnumerator StopRotation (Action<bool> done) {
        instance.stopRotationEvent = true;
        yield return new WaitWhile(() => instance.rotating);
        done(true);
	}

    void AdjustRotation (float delta) {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if (stopRotationEvent && (rotationDelta < 0f || rotationAngle >= 360f)) {
            rotationDelta = 0f;
        }
        if (rotationAngle < 0f) {
			rotationAngle += 360f;
		}
		else if (rotationAngle >= 360f) {
			rotationAngle -= 360f;
		}
		transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
	}

    void AdjustSpeed (float delta) {
        if (rotationSpeed < maxRotationSpeed) {
            rotationSpeed += delta * Time.deltaTime;
		}
	}
}
