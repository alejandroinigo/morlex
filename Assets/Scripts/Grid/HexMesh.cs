using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

    public bool useCollider, useCellData, useUVCoordinates;

    Mesh hexMesh;
    MeshCollider meshCollider;
    [NonSerialized] List<Vector3> vertices, cellIndices;
	[NonSerialized] List<Color> cellWeights;
	[NonSerialized] List<int> triangles;
    [NonSerialized] List<Vector2> uvs;

    void Awake () {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        if (useCollider) {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        hexMesh.name = "Hex Mesh";
    }

    public void AddTriangle (Vector3 v1, Vector3 v2, Vector3 v3) {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    public void AddTriangleCellData (
		Vector3 indices, Color weights1, Color weights2, Color weights3
	) {
		cellIndices.Add(indices);
		cellIndices.Add(indices);
		cellIndices.Add(indices);
		cellWeights.Add(weights1);
		cellWeights.Add(weights2);
		cellWeights.Add(weights3);
	}
		
	public void AddTriangleCellData (Vector3 indices, Color weights) {
		AddTriangleCellData(indices, weights, weights, weights);
	}
    
	public void AddTriangleUV (Vector2 uv1, Vector2 uv2, Vector3 uv3) {
		uvs.Add(uv1);
		uvs.Add(uv2);
		uvs.Add(uv3);
	}
	
	public void AddQuadUV (Vector2 uv1, Vector2 uv2, Vector3 uv3, Vector3 uv4) {
		uvs.Add(uv1);
		uvs.Add(uv2);
		uvs.Add(uv3);
		uvs.Add(uv4);
	}

    public void AddQuad (Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		vertices.Add(v4);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 3);
	}

    public void AddQuadUV (float uMin, float uMax, float vMin, float vMax) {
		uvs.Add(new Vector2(uMin, vMin));
		uvs.Add(new Vector2(uMax, vMin));
		uvs.Add(new Vector2(uMin, vMax));
		uvs.Add(new Vector2(uMax, vMax));
	}

    public void AddQuadCellData (
		Vector3 indices,
		Color weights1, Color weights2, Color weights3, Color weights4
	) {
		cellIndices.Add(indices);
		cellIndices.Add(indices);
		cellIndices.Add(indices);
		cellIndices.Add(indices);
		cellWeights.Add(weights1);
		cellWeights.Add(weights2);
		cellWeights.Add(weights3);
		cellWeights.Add(weights4);
	}

	public void AddQuadCellData (
		Vector3 indices, Color weights1, Color weights2
	) {
		AddQuadCellData(indices, weights1, weights1, weights2, weights2);
	}

    public void AddQuadCellData (Vector3 indices, Color weights) {
		AddQuadCellData(indices, weights, weights, weights, weights);
	}

    public void Clear () {
		hexMesh.Clear();
		vertices = ListPool<Vector3>.Get();
        if (useCellData) {
			cellWeights = ListPool<Color>.Get();
			cellIndices = ListPool<Vector3>.Get();
		}
        if (useUVCoordinates) {
			uvs = ListPool<Vector2>.Get();
		}
		triangles = ListPool<int>.Get();
	}

	public void Apply () {
		hexMesh.SetVertices(vertices);
		ListPool<Vector3>.Add(vertices);
        if (useCellData) {
			hexMesh.SetColors(cellWeights);
			ListPool<Color>.Add(cellWeights);
			hexMesh.SetUVs(2, cellIndices);
			ListPool<Vector3>.Add(cellIndices);
		}
        if (useUVCoordinates) {
			hexMesh.SetUVs(0, uvs);
			ListPool<Vector2>.Add(uvs);
		}
		hexMesh.SetTriangles(triangles, 0);
		ListPool<int>.Add(triangles);
		hexMesh.RecalculateNormals();
        if (useCollider) {
		    meshCollider.sharedMesh = hexMesh;
        }
	}
}