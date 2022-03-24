using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HelicopterExtinguisherVis : MonoBehaviour {
    [SerializeField] HelicopterExtinguisher heliExtinguisher;
    [SerializeField] RectTransform maskedMeter;
    [SerializeField] TMPro.TextMeshProUGUI textMeter;
    [SerializeField] Color warningColor;

    // Update is called once per frame
    void Update() {
        maskedMeter.sizeDelta = new Vector2(maskedMeter.sizeDelta.x, heliExtinguisher.waterAmount);
        textMeter.text = heliExtinguisher.waterAmount.ToString();
        if(heliExtinguisher.waterAmount <= 20)
            textMeter.color = warningColor;
        else
            textMeter.color = Color.white;
    }
}
