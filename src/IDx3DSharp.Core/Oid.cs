namespace IDx3DSharp
{
    public struct Oid
    {
        static int seed = 0;
        public int Id;

        public Oid(int id)
        {
            Id = id;
        }
        public static Oid Next()
        {
            return new Oid(seed++);
        }
        public static implicit operator Oid(int id) => new Oid(id);
    }
}