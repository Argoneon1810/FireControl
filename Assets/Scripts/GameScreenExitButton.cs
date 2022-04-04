using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScreenExitButton : MonoBehaviour {
    public void Exit() {
        GameObject.FindObjectOfType<SceneController>().GoToSceneByName("HomeScene");
    }
}
