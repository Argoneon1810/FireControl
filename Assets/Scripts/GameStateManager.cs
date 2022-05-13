using UnityEngine;
using System.Collections;

public class GameStateManager : MonoBehaviour {
    [SerializeField] TreeMassPlacerOnPCG treeMassPlacerOnPCG;
    [SerializeField] GameStateVisualizer visualizer;

    bool canCheckWinCondition = false;

    void Start() {
        StartCoroutine(WaitForFirstFewTreesCatchFire());
    }

    IEnumerator WaitForFirstFewTreesCatchFire() {
        while (treeMassPlacerOnPCG.numOfBurning == 0) {
            yield return null;
        }
        canCheckWinCondition = true;
    }
    
    void Update() {
        if(!treeMassPlacerOnPCG.doneSpawn) return;

        int currentAliveCount = treeMassPlacerOnPCG.numOfSpawned - treeMassPlacerOnPCG.numOfBurnt;
        int supposedAliveCount = treeMassPlacerOnPCG.numOfSpawned;
        int currentOnFireCount = treeMassPlacerOnPCG.numOfBurning - treeMassPlacerOnPCG.numOfExtinguished - treeMassPlacerOnPCG.numOfBurnt;

        visualizer.VisualizeScore(currentAliveCount, supposedAliveCount);

        if(currentAliveCount/(float)supposedAliveCount < .3f) GameOver();
        if(canCheckWinCondition && currentOnFireCount <= 0) Win();
    }

    void GameOver() {
        if(SceneController.Instance == null) SceneController.Instance = new GameObject().AddComponent<SceneController>();
        SceneController.Instance.GoToSceneByName("GameOverScene");
    }

    void Win() {
        if(SceneController.Instance == null) SceneController.Instance = new GameObject().AddComponent<SceneController>();
        SceneController.Instance.GoToSceneByName("WinScene");
    }
}
