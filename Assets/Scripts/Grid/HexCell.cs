using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class HexCell : MonoBehaviour
{
    public HexCoordinates coordinates;
    public RectTransform uiRect;
    public HexGridChunk chunk;
    public HexCell PathFrom { get; set; }
    public int SearchHeuristic { get; set; }
    public HexCell NextWithSamePriority { get; set; }
    public int SearchPhase { get; set; }
    public int Index { get; set; }

    [SerializeField]
    HexCell[] neighbors = new HexCell[0];
    int elevation = int.MinValue;
    int terrainTypeIndex;
    int waterLevel;
    int firstDetailLevel, secondDetailLevel, thirdDetailLevel;
    int specialIndex;
    int distance;
    int visibility;

    public void FixedUpdate() {
        if (Height != elevation * HexMetrics.elevationStep) {
            RaiseHeight();
        }
    }

    public bool IsVisible {
        get {
            return true;
        }
    }

    public void SetLabel (string text) {
        UnityEngine.UI.Text label = uiRect.GetComponent<Text>();
        label.text = text;
    }

    public int Distance {
        get {
            return distance;
        }
        set {
            distance = value;
        }
    }

    public int SearchPriority {
        get {
            return distance + SearchHeuristic;
        }
    }

    public int Elevation {
        get {
            return elevation;
        }
        set {
            if (elevation == value) {
                return;
            }
            elevation = value;
            RefreshPosition();
        }
    }

    void RaiseHeight() {
        Vector3 position = transform.localPosition;
        float riseDistance = HexMetrics.elevationStep * Time.deltaTime / HexMetrics.riseTime;
        float finalElevation = elevation * HexMetrics.elevationStep;
        if (position.y > finalElevation) {
            position.y = Mathf.Max(Height - riseDistance, finalElevation);
        } else {
            position.y = Mathf.Min(Height + riseDistance, finalElevation);
        }
        transform.localPosition = position;
        Refresh();
    }
    
    public float Height {
        get {
            Vector3 position = transform.localPosition;
            return position.y;
        }
    }

    public int ViewElevation {
        get {
            return elevation >= waterLevel ? elevation : waterLevel;
        }
    }

    void RefreshPosition () {
            Vector3 position = transform.localPosition;
            position.y = Height;
            transform.localPosition = position;

            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = -Height;
            uiRect.localPosition = uiPosition;
    }

    public int TerrainTypeIndex {
        get {
            return terrainTypeIndex;
        }
        set {
            if (terrainTypeIndex != value) {
                terrainTypeIndex = value;
            }
        }
    }
    
    public int WaterLevel {
        get {
            return waterLevel;
        }
        set {
            if (waterLevel == value) {
                return;
            }
            waterLevel = value;
            Refresh();
        }
    }

    public bool IsUnderwater {
        get {
            return WaterSurfaceY > Height;
        }
    }

    public float WaterSurfaceY {
        get {
            return
                (waterLevel *
                HexMetrics.elevationStep) + HexMetrics.waterElevationOffset;
        }
    }
    
    public int FirstDetailLevel {
        get {
            return firstDetailLevel;
        }
        set {
            if (firstDetailLevel != value) {
                firstDetailLevel = value;
                RefreshSelfOnly();
            }
        }
    }

    public int SecondDetailLevel {
        get {
            return secondDetailLevel;
        }
        set {
            if (secondDetailLevel != value) {
                secondDetailLevel = value;
                RefreshSelfOnly();
            }
        }
    }

    public int ThirdDetailLevel {
        get {
            return thirdDetailLevel;
        }
        set {
            if (thirdDetailLevel != value) {
                thirdDetailLevel = value;
                RefreshSelfOnly();
            }
        }
    }

    public int SpecialIndex {
        get {
            return specialIndex;
        }
        set {
            if (specialIndex != value) {
                specialIndex = value;
                RefreshSelfOnly();
            }
        }
    }

    public bool IsSpecial {
        get {
            return specialIndex > 0;
        }
    }

    public HexCell GetNeighbor (HexDirection direction) {
        return neighbors[(int)direction];
    }

    public void SetNeighbor (HexDirection direction, HexCell cell) {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    public Vector3 Position {
        get {
            return transform.localPosition;
        }
    }

    void Refresh () {
        if (chunk) {
            chunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++) {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk) {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }

    void RefreshSelfOnly () {
        chunk.Refresh();
    }

    public void Save (BinaryWriter writer) {
        writer.Write((byte)terrainTypeIndex);
        writer.Write((byte)(elevation + 127));
        writer.Write((byte)waterLevel);
        writer.Write((byte)firstDetailLevel);
        writer.Write((byte)secondDetailLevel);
        writer.Write((byte)thirdDetailLevel);
        writer.Write((byte)specialIndex);
    }

    public void Load (BinaryReader reader, int header) {
        terrainTypeIndex = reader.ReadByte();
        elevation = reader.ReadByte();
        if (header >= 4) {
            elevation -= 127;
        }
        waterLevel = reader.ReadByte();
        firstDetailLevel = reader.ReadByte();
        secondDetailLevel = reader.ReadByte();
        thirdDetailLevel = reader.ReadByte();
        specialIndex = reader.ReadByte();

        RefreshPosition();
    }
}
