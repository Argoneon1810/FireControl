using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverSceneManager : MonoBehaviour {
    SceneController sceneController;
    // Start is called before the first frame update
    void Start() {
        sceneController = SceneController.Instance;
    }

    public void OnClick(string destinationSceneName) {
        if(sceneController==null) {
            var gO = new GameObject();
            sceneController = gO.AddComponent<SceneController>();
        }
        sceneController.GoToSceneByName(destinationSceneName);
    }
}
