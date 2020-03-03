using UnityEngine;

public static class HexMetrics {

    public const float outerRadius = 10f;
    public const float innerRadius = outerRadius * 0.866025404f;
	public const float solidFactor = 0.98f;
	public const float blendFactor = 1f - solidFactor;
	public const float elevationStep = 4f;
	public const float riseTime = 2.0f;
	public const float bumpFactor = 0.95f;
	public const float bumpStep = 0.90f;
	public const int chunkSizeX = 5, chunkSizeZ = 5;
	public const float waterElevationOffset = -0.5f;
	public const float waterFactor = 0.6f;
	public const float waterBlendFactor = 1f - waterFactor;
	public static Texture2D noiseSource;
	public const float cellPerturbStrength = 4f;
	public const float noiseScale = 0.003f;
	public const int hashGridSize = 256;
	public const float hashGridScale = 0.25f;
    public const float floatingDetailsHeight = 5f;

	static HexHash[] hashGrid;

	static Vector3[] corners = {
		new Vector3(0f, 0f, outerRadius),
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(0f, 0f, -outerRadius),
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
                new Vector3(0f, 0f, outerRadius)
	};

	static float[][] featureThresholds = {
		new float[] {0.0f, 0.0f, 0.4f},
		new float[] {0.0f, 0.4f, 0.6f},
		new float[] {0.4f, 0.6f, 0.8f}
	};
	
	public static Vector3 GetFirstCorner (HexDirection direction) {
		return corners[(int)direction];
	}

	public static Vector3 GetSecondCorner (HexDirection direction) {
		return corners[(int)direction + 1];
	}

	public static Vector3 GetFirstOuterCorner (HexDirection direction) {
		return corners[(int)direction] * solidFactor;
	}

	public static Vector3 GetSecondOuterCorner (HexDirection direction) {
		return corners[(int)direction + 1] * solidFactor;
	}

	public static Vector3 GetFirstBumpOuterCorner (HexDirection direction) {
		Vector3 firstBumpOuterCorner = corners[(int)direction] * solidFactor;
		firstBumpOuterCorner.y -= HexMetrics.bumpStep;
		return firstBumpOuterCorner;
	}

	public static Vector3 GetSecondBumpOuterCorner (HexDirection direction) {
		Vector3 secondBumpOuterCorner = corners[(int)direction + 1] * solidFactor;
		secondBumpOuterCorner.y -= HexMetrics.bumpStep;
		return secondBumpOuterCorner;
	}

	public static Vector3 GetFirstInnerCorner (HexDirection direction) {
		return corners[(int)direction] * solidFactor * bumpFactor;
	}

	public static Vector3 GetSecondInnerCorner (HexDirection direction) {
		return corners[(int)direction + 1] * solidFactor * bumpFactor;
	}

	public static Vector3 GetFirstWaterCorner (HexDirection direction) {
		return corners[(int)direction] * waterFactor;
	}

	public static Vector3 GetSecondWaterCorner (HexDirection direction) {
		return corners[(int)direction + 1] * waterFactor;
	}

	public static Vector3 GetBridge (HexDirection direction) {
		return (corners[(int)direction] + corners[(int)direction + 1]) *
			blendFactor;
	}

	public static Vector3 GetWaterBridge (HexDirection direction) {
		return (corners[(int)direction] + corners[(int)direction + 1]) *
			waterBlendFactor;
	}

	public static Vector4 SampleNoise (Vector3 position) {
		return noiseSource.GetPixelBilinear(
			position.x * noiseScale,
			position.z * noiseScale
		);
	}

	public static Vector3 Perturb (Vector3 position) {
		Vector4 sample = SampleNoise(position);
		position.x += (sample.x * 2f - 1f) * cellPerturbStrength;
		position.z += (sample.z * 2f - 1f) * cellPerturbStrength;
		return position;
	}

	public static void InitializeHashGrid (int seed) {
		Random.State currentState = Random.state;
		hashGrid = new HexHash[hashGridSize * hashGridSize];
		Random.InitState(seed);
		for (int i = 0; i < hashGrid.Length; i++) {
			hashGrid[i] = HexHash.Create();
		}
		Random.state = currentState;
	}

	public static HexHash SampleHashGrid (Vector3 position) {
		int x = (int)(position.x * hashGridScale) % hashGridSize;
		if (x < 0) {
			x += hashGridSize;
		}
		int z = (int)(position.z * hashGridScale) % hashGridSize;
		if (z < 0) {
			z += hashGridSize;
		}
		return hashGrid[x + z * hashGridSize];
	}

	public static float[] GetFeatureThresholds (int level) {
		return featureThresholds[level];
	}
}
