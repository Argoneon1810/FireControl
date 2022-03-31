using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeSceneManager : MonoBehaviour {
    [SerializeField] List<Animator> ToIgnite;

    // Start is called before the first frame update
    void Start() {
        if(ToIgnite.Count == 0) {
            GameObject[] flameables = GameObject.FindGameObjectsWithTag("Flameable");
            foreach(GameObject gO in flameables) {
                if(gO.TryGetComponent<Animator>(out Animator animator))
                    ToIgnite.Add(animator);
            }
        }
        foreach(Animator animator in ToIgnite) {
            animator.SetTrigger("Set Fire");
        }
    }
}
