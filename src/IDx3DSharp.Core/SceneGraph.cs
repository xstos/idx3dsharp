using System.Collections.Generic;

namespace IDx3DSharp
{
    public class SceneGraph
    {
        public static SceneGraph Instance = new SceneGraph();
        public List<Vertex> vertices = new List<Vertex>();
        public List<Triangle> triangles = new List<Triangle>();
        public List<VertexInfo> vertexInfo = new List<VertexInfo>();
        public SceneGraphId AddVertex(Vertex v)
        {
            vertices.Add(v);
            var id = vertices.Count - 1;
            var ret = (PrimitiveType.Vertex, id);
            vertexInfo.Add(new VertexInfo() {ParentVertexId = ret});
            return ret;
        }
        public SceneGraphId AddTriangle(Triangle v)
        {
            triangles.Add(v);
            return (PrimitiveType.Triangle, triangles.Count - 1);
        }

        public VertexInfo getVertexInfo(SceneGraphId id)
        {
            return vertexInfo[id.Id.Item2];
        }
    }
}