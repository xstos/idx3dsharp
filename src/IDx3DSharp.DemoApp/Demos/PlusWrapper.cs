using System.Collections.Generic;

namespace IDx3DSharp.DemoApp.Demos
{
    public class PlusWrapper<T>
    {
        public List<T> inner;
        public static implicit operator PlusWrapper<T>(List<T> list)
        {
            return new PlusWrapper<T>(){inner=list};
        }
        public static PlusWrapper<T> operator +(PlusWrapper<T> o, T c1)
        {
            o.inner.Add(c1);
            return o;
        } 
    }
}