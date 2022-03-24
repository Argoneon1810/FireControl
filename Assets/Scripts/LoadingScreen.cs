using System;
using UnityEngine;

public class LoadingScreen : MonoBehaviour {
    [SerializeField] GameObject loadingCover;
    [SerializeField] RectTransform loadingBar;
    [SerializeField] TMPro.TextMeshProUGUI loadingTextfield;
    [SerializeField] TreeMassPlacerOnPCG treeMassPlacerOnPCG;
    [SerializeField] string loadingText;

    bool doneLoading;
    float t = 0;
    int count = 0;

    private void Start() {
        treeMassPlacerOnPCG.PostParentingEvent += FinishLoading;
        if(loadingText.Equals("")) loadingText = loadingTextfield.text;
        // treeMassPlacerOnPCG.AddOnAddedEvent(TreeSpawned);
        treeMassPlacerOnPCG.OnTrialSpawn += TreeSpawned;
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
    
    void FinishLoading() {
        loadingCover.SetActive(false);
        doneLoading = true;
    }


    void TreeSpawned(object sender, EventArgs e) {
        TreeSpawned();
    }
    
    void TreeSpawned() {
        ++count;
        float leftOffset;
        if(!treeMassPlacerOnPCG.IsDone()) {
            leftOffset = - Screen.width * (count / (float)treeMassPlacerOnPCG.numberOfTree);
        }
        else
            leftOffset = 0;
        
        loadingBar.offsetMax = new Vector2(leftOffset, loadingBar.offsetMax.y);
    }
}
