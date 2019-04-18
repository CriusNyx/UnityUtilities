using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IReadOnlyWrapper<T>
{
    T Get();
}

public interface IWrapper<T> : IReadOnlyWrapper<T>
{
    T Value { get; set; }
}

public class Wrapper<T> : IWrapper<T>
{
    public T Value { get; set; }

    public Wrapper()
    {
    }

    public Wrapper(T value)
    {
        Value = value;
    }
    
    public T Get()
    {
        return Value;
    }
}