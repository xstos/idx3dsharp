namespace IDx3DSharp
{
    public enum PrimitiveType
    {
        Triangle,
        Vertex
    }
    public struct SceneGraphId
    {
        public (PrimitiveType, int) Id;

        public SceneGraphId((PrimitiveType,int) id)
        {
            Id = id;
        }
        public static implicit operator SceneGraphId((PrimitiveType,int) id) => new SceneGraphId(id);
    }
}