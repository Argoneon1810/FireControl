using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSpread : MonoBehaviour {
    [SerializeField] float fireSpreadRadius = 7;
    [SerializeField] float spreadability = .1f;
    [SerializeField] float bunnySpawnRate = .1f;
    [SerializeField] float bunnySpawnRadius = 7;
    [SerializeField] GameObject BunnyPrefab;
    const int MAX_TRIAL = 3;

    [SerializeField] bool debugSpawnBunny;

    void Start() {
        StartCoroutine(OnFireWaiter());
    }

    void Update()
    {
        if(debugSpawnBunny){
            debugSpawnBunny = false;
            SpawnBunny();
        }
    }

    IEnumerator OnFireWaiter() {
        while(true) {
            if(transform.TryGetComponent<Animator>(out Animator animator)) {
                if(animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Base Layer")).fullPathHash == Animator.StringToHash("Base Layer.Tree_OnFire")) {
                    StartCoroutine(DoStochasticSpread());
                    break;
                }
            }
            yield return null;
        }
        yield return null;
    }

    IEnumerator DoStochasticSpread() {
        Animator animator = transform.GetComponent<Animator>();
        while(animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Base Layer")).fullPathHash == Animator.StringToHash("Base Layer.Tree_OnFire")) {
            yield return new WaitForSeconds(Random.Range(1f,3f));
            StochasticSpread();
            SpawnBunny();
        }
        yield return null;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 4, fireSpreadRadius);
    }

    public void Spread() {
        List<Animator> animList = new List<Animator>();
        RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.up * 4, fireSpreadRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
        foreach(var hit in hits) {
            if(hit.transform.TryGetComponent<Animator>(out Animator animator)) {
                if(animator != GetComponent<Animator>() && animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Base Layer")).fullPathHash == Animator.StringToHash("Base Layer.Tree_Unburnt"))
                    animList.Add(animator);
            }
        }
        Arsonist.Arson(animList);
        animList.Clear();
    }

    void StochasticSpread() {
        if(Random.Range(0f, 1f) < spreadability) {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.up * 4, fireSpreadRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
            List<Animator> animList = new List<Animator>();
            int trial = 0;
            while(trial < MAX_TRIAL) {
                ++trial;
                if(hits[Random.Range(0, hits.Length)].transform.TryGetComponent<Animator>(out Animator animator)) {
                    animList.Add(animator);
                    break;
                }
            }
            if(animList.Count > 0) Arsonist.Arson(animList);
            animList.Clear();
        }
    }

    void SpawnBunny() {
        if(Random.Range(0f,1f) < bunnySpawnRate) {
            Vector3 randDir = new Vector3(Random.Range(-bunnySpawnRadius, bunnySpawnRadius), 50, Random.Range(-bunnySpawnRadius, bunnySpawnRadius)).normalized;
            float randDist = Random.Range(4, bunnySpawnRadius);
            Vector3 randPosition = transform.position + randDir * randDist;
            RaycastHit[] hits = Physics.RaycastAll(randPosition, Vector3.down, 50*2f, 1 << LayerMask.NameToLayer("Land"));
            foreach(var hit in hits)
                Instantiate(BunnyPrefab, hit.point, Quaternion.identity);
        }
    }
}
