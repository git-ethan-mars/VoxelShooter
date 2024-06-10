using System;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool<T> where T : Component 
{
    private readonly GameObject _container;
    private readonly Func<T> _createFunction;

    private readonly Stack<T> _stack;

    public GameObjectPool(GameObject container, int initPoolSize, Func<T> createFunction)
    {
        _container = container;
        _createFunction = createFunction;
    }

    public T Get()
    {
        if (_stack.TryPop(out var component))
        {
            return component;
        }

        return _createFunction();
    }

    public T Release()
    {
        return null;
    }
}