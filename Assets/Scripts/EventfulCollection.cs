using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EventfulCollection<T> : ICollection<T> {
    public event EventHandler OnAdded;
    public event EventHandler OnRemoved;
    public event EventHandler OnClear;

    private List<T> _lst = new List<T>();

    public int Count { get { return _lst.Count; } }

    bool ICollection<T>.IsReadOnly { get { return false; } }

    public T this[int index] {
        get => _lst[index];
        set => _lst[index] = value;
    }

    public void Add(T item) {
        _lst.Add(item);
        OnAdded?.Invoke(this, EventArgs.Empty);
    }

    public void Clear() {
        _lst.Clear();
        OnClear?.Invoke(this, EventArgs.Empty);
    }

    public bool Contains(T item) {
        return _lst.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
        _lst.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item) {
        if(_lst.Remove(item)) {
            OnRemoved?.Invoke(this, EventArgs.Empty);
            return true;
        }
        return false;
    }

    public void Sort(Comparison<T> comparison) {
        _lst.Sort(comparison);
    }

    public IEnumerator<T> GetEnumerator() {
        return _lst.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return _lst.GetEnumerator();
    }
}