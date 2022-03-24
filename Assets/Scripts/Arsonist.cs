using System.Collections.Generic;
using UnityEngine;

public class Arsonist : MonoBehaviour {
    public static void Arson(List<Animator> toTorch) {
        foreach(Animator animator in toTorch) {
            ArsonSingle(animator);
        }
    }
    public static void ArsonSingle(Animator toTorch) {
        toTorch.SetTrigger("Set Fire");
    }
}
