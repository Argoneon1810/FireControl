using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneConteroller : MonoBehaviour {
    public static SceneConteroller Instance;

    void Start() {
        Instance = Instance ? Instance : this;
        DontDestroyOnLoad(this);
    }

    public void GoToSceneByName(string name) {
        SceneManager.LoadScene(name, LoadSceneMode.Single);
    }
}
