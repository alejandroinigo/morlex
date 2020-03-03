using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour {
    
	public HexMesh terrain, water, waterShore;
	public HexFeatureManager features;

	HexCell[] cells;
	Canvas gridCanvas;
    static Color terrainColorBase = new Color(0f, 0f, 0f);
    static Color terrainColorBump = new Color(1f, 0f, 1f);
    static Color weights1 = terrainColorBase;
	static Color weights2 = terrainColorBase;
	static Color weights3 = terrainColorBase;

	void Awake () {
		gridCanvas = GetComponentInChildren<Canvas>();
		cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
	}
	
	public void Triangulate () {
        terrain.Clear();
		water.Clear();
		waterShore.Clear();
		features.Clear();
        for (int i = 0; i < cells.Length; i++) {
            Triangulate(cells[i]);
        }
        terrain.Apply();
		water.Apply();
		waterShore.Apply();
		features.Apply();
    }
    
    void Triangulate (HexCell cell) {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) {
			Triangulate(d, cell);
		}
		if (!cell.IsUnderwater) {
			features.AddFeature(cell, cell.Position);
			if (cell.IsSpecial) {
				features.AddSpecialFeature(cell, cell.Position);
			}
		}
    }
    
    void Triangulate (HexDirection direction, HexCell cell) {
        Vector3 center = cell.Position;
        Vector3 v1 = center + HexMetrics.GetFirstInnerCorner(direction);
		Vector3 v2 = center + HexMetrics.GetSecondInnerCorner(direction);
        Vector3 v1OuterBump = center + HexMetrics.GetFirstBumpOuterCorner(direction);
		Vector3 v2OuterBump = center + HexMetrics.GetSecondBumpOuterCorner(direction);
		
		Vector3 indices;
		indices.x = indices.y = indices.z = cell.Index;

        terrain.AddTriangle(center, v1, v2);
        terrain.AddTriangleCellData(indices, weights1);
        
        terrain.AddQuad(v1, v2, v1OuterBump, v2OuterBump);
        if (cell.IsUnderwater) {
            terrain.AddQuadCellData(indices, weights1, weights1);
        } else {
            terrain.AddQuadCellData(indices, terrainColorBump, terrainColorBump);
        }

        if (direction <= HexDirection.SE) {
            TriangulateConnection(direction, cell, v1OuterBump, v2OuterBump);
        }

		if (cell.IsUnderwater) {
			TriangulateWater(direction, cell, center);
		}

		if (!cell.IsUnderwater) {
			features.AddFeature(cell, (center + v1 + v2) * (1f / 3f));
		}
    }

	void TriangulateWater (HexDirection direction, HexCell cell, Vector3 center) {
		center.y = cell.WaterSurfaceY;

		HexCell neighbor = cell.GetNeighbor(direction);
		if (neighbor != null && !neighbor.IsUnderwater) {
			TriangulateWaterShore(direction, cell, neighbor, center);
		}
		else {
			TriangulateOpenWater(direction, cell, neighbor, center);
		}
	}

	void TriangulateOpenWater (
		HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center
	) {
		Vector3 c1 = center + HexMetrics.GetFirstWaterCorner(direction);
		Vector3 c2 = center + HexMetrics.GetSecondWaterCorner(direction);

		water.AddTriangle(center, c1, c2);

		if (direction <= HexDirection.SE && neighbor != null) {
			Vector3 bridge = HexMetrics.GetWaterBridge(direction);
			Vector3 e1 = c1 + bridge;
			Vector3 e2 = c2 + bridge;

			water.AddQuad(c1, c2, e1, e2);

			if (direction <= HexDirection.E) {
				HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
				if (nextNeighbor == null || !nextNeighbor.IsUnderwater) {
					return;
				}
				water.AddTriangle(
					c2, e2, c2 + HexMetrics.GetWaterBridge(direction.Next())
				);
			}
		}
	}

	void TriangulateWaterShore (
		HexDirection direction, HexCell cell, HexCell neighbor, Vector3 center
	) {
		EdgeVertices e1 = new EdgeVertices(
			center + HexMetrics.GetFirstWaterCorner(direction),
			center + HexMetrics.GetSecondWaterCorner(direction)
		);
		water.AddTriangle(center, e1.v1, e1.v2);

		Vector3 center2 = neighbor.Position;
		center2.y = center.y;
		EdgeVertices e2 = new EdgeVertices(
			center2 + HexMetrics.GetSecondOuterCorner(direction.Opposite()),
			center2 + HexMetrics.GetFirstOuterCorner(direction.Opposite())
		);

		waterShore.AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
		waterShore.AddQuadUV(0f, 0f, 0f, 1f);

		HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (nextNeighbor != null) {
			Vector3 v3 = nextNeighbor.Position + (nextNeighbor.IsUnderwater ?
				HexMetrics.GetFirstWaterCorner(direction.Previous()) :
				HexMetrics.GetFirstOuterCorner(direction.Previous()));
			v3.y = center.y;
			waterShore.AddTriangle(
				e1.v2, e2.v2, v3
			);
			waterShore.AddTriangleUV(
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(0f, nextNeighbor.IsUnderwater ? 0f : 1f)
			);
		}
	}

    void TriangulateConnection (
		HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2
	) {

        HexCell neighbor = cell.GetNeighbor(direction);
		if (neighbor == null) {
			return;
		}
        
        // Edge Bridges
        Vector3 bridge = HexMetrics.GetBridge(direction);
		Vector3 v3 = v1 + bridge;
		Vector3 v4 = v2 + bridge;
        v3.y = v4.y = neighbor.Height - HexMetrics.bumpStep;
		
		float cellTerrainType = cell.Index;
		float neighborTerrainType = neighbor.Index;
		
		Vector3 indices;
		indices.x = indices.z = cellTerrainType;
		indices.y = neighborTerrainType;

        terrain.AddQuad(v1, v2, v3, v4);
        terrain.AddQuadCellData(indices, weights1, weights2);

        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
		if (direction <= HexDirection.E && nextNeighbor != null) {
			Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
			v5.y = nextNeighbor.Height - HexMetrics.bumpStep;
			TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
		}
    }

	void TriangulateCorner(
		Vector3 bottom, HexCell bottomCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell)
	{
			Vector3 indices;
			indices.x = bottomCell.Index;
			indices.y = leftCell.Index;
			indices.z = rightCell.Index;
			
			terrain.AddTriangle(bottom, left, right);
            terrain.AddTriangleCellData(indices, weights1, weights2, weights3);
	}
    public void AddCell (int index, HexCell cell) {
		cells[index] = cell;
        cell.chunk = this;
		cell.transform.SetParent(transform, false);
		cell.uiRect.SetParent(gridCanvas.transform, false);
	}

    public void Refresh () {
		enabled = true;
	}

	void LateUpdate () {
		Triangulate();
        enabled = false;
	}

	public void ShowUI (bool visible) {
		gridCanvas.gameObject.SetActive(visible);
	}
}