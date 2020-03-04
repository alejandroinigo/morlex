using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    public int AverageFps { get; private set; }
    public int frameRange = 60;
    int[] fpsBuffer;
    int fpsBufferIndex;

    void Update()
    {
        if (fpsBuffer == null || fpsBuffer.Length != frameRange) {
            InitializeBuffer();
        }
        UpdateBuffer();
        CalculateFps();
    }

    void InitializeBuffer () {
        if (frameRange <= 0) {
            frameRange = 1;
        }
        fpsBuffer = new int[frameRange];
        fpsBufferIndex = 0;
    }

    void UpdateBuffer () {
        fpsBuffer[fpsBufferIndex++] = (int)(1f / Time.unscaledDeltaTime);
        if (fpsBufferIndex >= frameRange) {
            fpsBufferIndex = 0;
        }
    }

    void CalculateFps () {
        int sum = 0;
        for (int i = 0; i < frameRange; i++) {
            sum += fpsBuffer[i];
        }
        AverageFps = sum / frameRange;
    }
}
