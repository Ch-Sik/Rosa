using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularBuffer<T>
{
    private T[] _buffer;
    public int Count => _count;
    
    private int _count;
    private readonly int _capacity;
    private int _nextIndex;
    
    public CircularBuffer(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException();
        }
        _capacity = capacity;
        _buffer = new T[capacity];
    }

    public bool Add(T item)
    {
        _buffer[_nextIndex] = item;
        _nextIndex = (_nextIndex + 1) % _capacity;
        if (_count < _capacity)
            _count++;
        return true;
    }

    public T GetLastNth(int n)
    {
        if (n <= 0 || n > _count) return default(T);
        int indexInternal = (_nextIndex - n + _capacity) % _capacity;
        return _buffer[indexInternal];
    }

    public void Reset()
    {
        _count = 0;
    }
}
