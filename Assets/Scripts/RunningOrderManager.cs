using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunningOrderManager : MonoBehaviour {
    public static RunningOrderManager Instance;

    public delegate void Callback();

    [SerializeField] int numberOfCallbacks = 5;     //number of callbacks added from other scripts
    int callbackAdded = 0;

    bool ran = false;

    List<KeyValuePair<int, Callback>> runnableList;

    private void Awake() {
        runnableList = new List<KeyValuePair<int, Callback>>();
        Instance = this;
    }

    public void AddCallbackToCollection(int order, Callback callback) {
        runnableList.Add(new KeyValuePair<int, Callback>(order, callback));
        ++callbackAdded;
    }

    private void Update() {
        if(!ran && callbackAdded == numberOfCallbacks) {
            Execute();
            ran = true;
        }
    }

    void Execute() {
        runnableList.Sort(delegate(KeyValuePair<int, Callback> a, KeyValuePair<int, Callback> b) {
            if(a.Key > b.Key) return 1;
            else if (a.Key < b.Key) return -1;
            else return 0;
        });
        foreach(KeyValuePair<int, Callback> runnable in runnableList) {
            runnable.Value.Invoke();
        }
    }
}
