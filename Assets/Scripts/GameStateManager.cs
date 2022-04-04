using UnityEngine;

public class GameStateManager : MonoBehaviour {
    [SerializeField] TreeMassPlacerOnPCG treeMassPlacerOnPCG;
    [SerializeField] GameStateVisualizer visualizer;
    
    void Update() {
        if(!treeMassPlacerOnPCG.doneSpawn) return;

        int currentAliveCount = treeMassPlacerOnPCG.numOfSpawned - treeMassPlacerOnPCG.numOfBurnt;
        int supposedAliveCount = treeMassPlacerOnPCG.numOfSpawned;

        visualizer.VisualizeScore(currentAliveCount, supposedAliveCount);

        if(currentAliveCount/(float)supposedAliveCount < .5f) GameOver();
    }

    void GameOver() {
        print("GameOver");
    }
}
