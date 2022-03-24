using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Extinguisher : MonoBehaviour {
    public static void Extinguish(List<Animator> toExtinguish) {
        foreach(Animator animator in toExtinguish) {
            ExtinguishSingle(animator);
        }
    }
    public static void ExtinguishSingle(Animator toExtinguish) {
        toExtinguish.SetTrigger("Extinguished");
    }
}