using UnityEngine;

namespace Curve.Scripts.Examples
{
    [RequireComponent(typeof(PathCreator))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class RoadCreator : MonoBehaviour
    {
        [Range(.05f, 1.5f)]
        public float spacing = 1f;
        public float roadWidth = 1;

        public bool autoUpdate;

        public void UpdateRoad()
        {
            var path = GetComponent<PathCreator>().path;
            var points = path.CalculateEvenlySpacedPoints(spacing);

            var meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = CreateRoadMesh(points, path.IsClosed);
        }
        
        private Mesh CreateRoadMesh(Vector2[] points, bool isClosed)
        {
            var verts = new Vector3[points.Length * 2];

            var uvs = new Vector2[verts.Length];
            
            var numOfTris = 2 * (points.Length - 1) + ((isClosed) ? 2 : 0);
            var tris = new int[numOfTris * 3];
            
            
            var vertexIndex = 0;
            var triIndex = 0;

            for (var i = 0; i < points.Length; i++)
            {
                var forward = Vector2.zero;

                if (i < points.Length - 1 || isClosed)
                {
                    forward += points[(i + 1) % points.Length] - points[i];
                }

                if (i > 0 || isClosed)
                {
                    forward += points[i] - points[(i - 1 + points.Length) % points.Length];
                }
                forward.Normalize();

                var left = new Vector2(-forward.y, forward.x);

                verts[vertexIndex] = points[i] + left * roadWidth * .5f;
                verts[vertexIndex + 1] = points[i] -left * roadWidth * .5f;

                var completionPercent = i / (float)(points.Length - 1);
                uvs[vertexIndex] = new Vector2(0, completionPercent);
                uvs[vertexIndex + 1] = new Vector2(1, completionPercent);
                

                if (i < points.Length - 1 || isClosed)
                {
                    tris[triIndex] = vertexIndex;
                    tris[triIndex + 1] = (vertexIndex + 2) % verts.Length;
                    tris[triIndex + 2] = vertexIndex + 1;

                    tris[triIndex + 3] = vertexIndex + 1;
                    tris[triIndex + 4] = (vertexIndex + 2) % verts.Length;
                    tris[triIndex + 5] = (vertexIndex + 3) % verts.Length;
                }
                
                vertexIndex += 2;
                triIndex += 6;
            }

            var mesh = new Mesh();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.uv = uvs;

            return mesh;
        }
    }
}