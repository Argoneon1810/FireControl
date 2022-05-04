using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverSceneManager : MonoBehaviour {
    public void OnClick(string destinationSceneName) {
        if(SceneController.Instance==null) {
            var gO = new GameObject();
            SceneController.Instance = gO.AddComponent<SceneController>();
        }
        SceneController.Instance.GoToSceneByName(destinationSceneName);
    }
}
