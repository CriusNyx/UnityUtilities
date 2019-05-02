using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thunk<T>
{
    private bool ready = false;
    private T value;
    public T Value
    {
        get
        {
            if (!ready)
            {
                value = generate();
                ready = true;
            }
            return value;
        }
    }

    private readonly Func<T> generate;

    public Thunk(Func<T> generate)
    {
        this.generate = generate;
    }

    public static implicit operator T (Thunk<T> thunk)
    {
        return thunk.Value;
    }

    public static implicit operator Thunk<T>(Func<T> func)
    {
        return new Thunk<T>(func);
    }
}