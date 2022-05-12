using System.Collections;
using UnityEngine;

public class LoadingScreen : MonoBehaviour {
    [SerializeField] GameObject loadingCover;
    [SerializeField] RectTransform loadingBar;
    [SerializeField] TMPro.TextMeshProUGUI loadingTextfield;
    [SerializeField] TreeMassPlacerOnPCG treeMassPlacerOnPCG;
    [SerializeField] string loadingText;

    bool doneLoading;
    float t = 0;

    private void Start() {
        treeMassPlacerOnPCG.PostParentingEvent += FinishLoading;
        if(loadingText.Equals("")) loadingText = loadingTextfield.text;
        StartCoroutine(WatchTreeSpawnProgress());
    }
    
    IEnumerator WatchTreeSpawnProgress() {
        float leftOffset;
        while(!treeMassPlacerOnPCG.doneSpawn) {
            leftOffset = -Screen.currentResolution.width * treeMassPlacerOnPCG.spawnProgress;
            loadingBar.offsetMax = new Vector2(leftOffset, loadingBar.offsetMax.y);
            yield return null;
        }
    }
    
    void FinishLoading() {
        Destroy(loadingCover);
    }

    void Update() {
        if(!doneLoading) {
            t+=Time.deltaTime;
            switch(Mathf.FloorToInt(t%3)) {
                case 0:
                    loadingTextfield.text = loadingText + ".";
                    break;
                case 1:
                    loadingTextfield.text = loadingText + "..";
                    break;
                case 2:
                    loadingTextfield.text = loadingText + "...";
                    break;
            }
        }
    }
}
