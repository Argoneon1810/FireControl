using System;
using UnityEngine;
using TMPro;

public class GameStateVisualizer : MonoBehaviour {
    public static GameStateVisualizer Instance;
    [SerializeField] TextMeshProUGUI TotalText, LeftText, PercentText;

    void Awake() {
        Instance = Instance ? Instance : this;
    }
    public void VisualizeScore(float currentAliveCount, float supposedAliveCount) {
        LeftText.text = Convert.ToString(currentAliveCount);
        TotalText.text = Convert.ToString(supposedAliveCount);
        PercentText.text = RoundToFirstDecimal(currentAliveCount/(float)supposedAliveCount) + "%";
    }

    static float RoundToFirstDecimal(float val) {
        return (int)(val * 1000)/10f;
    }
}
