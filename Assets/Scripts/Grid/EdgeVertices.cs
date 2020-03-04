using UnityEngine;

public struct EdgeVertices {

    public Vector3 v1, v2;

    public EdgeVertices (Vector3 corner1, Vector3 corner2) {
        v1 = corner1;
        v2 = corner2;
    }
}