using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FireSpread : MonoBehaviour {
    private const int MAX_TRIAL = 3;

    [SerializeField] private float spreadSpherecastRadius = 7;
    [SerializeField] private float spreadTimeGoal = 5;
    [SerializeField] private float stochasticSpreadRate = .2f;
    [SerializeField] private float bunnySpawnRate = .05f;
    [SerializeField] private float bunnySpawnRadius = 20;
    [SerializeField] private GameObject BunnyPrefab;

    private bool _bIsOnFire, _bIsBurnt;
    private Animator mAnimator;

    public bool bIsOnFire {get=>_bIsOnFire;}
    public bool bIsBurnt {get=>_bIsBurnt;}

    float continuousFlameTime;

    float randSpawnInterval = float.MinValue;

    void Start() {
        mAnimator = GetComponent<Animator>();
    }

    void Update() {
        if(_bIsOnFire) {
            continuousFlameTime += Time.deltaTime;
            if(continuousFlameTime > spreadTimeGoal) {
                MarkDoneBurning();
                Spread();
            } else {
                if(randSpawnInterval < 0) {
                    randSpawnInterval = Random.Range(1f,3f);
                    SpawnBunny();
                    StochasticSpread();
                }
                randSpawnInterval -= Time.deltaTime;
            }
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 4, spreadSpherecastRadius);
    }

    void Spread() {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.up * 4, spreadSpherecastRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
        foreach(var hit in hits) {
            if(hit.transform.TryGetComponent<FireSpread>(out FireSpread spreadable)) {
                if(spreadable != this && !spreadable.bIsOnFire && !spreadable.bIsBurnt)
                    spreadable.MarkTorched();
            }
        }
    }

    void StochasticSpread() {
        if(Random.Range(0f, 1f) < stochasticSpreadRate) {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.up * 4, spreadSpherecastRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
            int trial = 0;
            while(trial < MAX_TRIAL) {
                ++trial;
                if(hits[Random.Range(0, hits.Length)].transform.TryGetComponent<FireSpread>(out FireSpread spreadable)) {
                    spreadable.MarkTorched();
                    break;
                }
            }
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

    public void MarkTorched() {
        _bIsOnFire = true;
        mAnimator.SetTrigger("Set Fire");
    }

    public void MarkExtinguished() {
        _bIsOnFire = false;
        if(mAnimator.GetCurrentAnimatorStateInfo(mAnimator.GetLayerIndex("Base Layer")).fullPathHash == Animator.StringToHash("Base Layer.Tree_OnFire"))
            mAnimator.SetTrigger("Extinguished");
        continuousFlameTime = 0;
    }

    void MarkDoneBurning() {
        _bIsOnFire = false;
        _bIsOnFire = true;
        mAnimator.SetTrigger("Done Burning");
    }
}
