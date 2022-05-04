using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FireSpread : MonoBehaviour {
    private const int MAX_TRIAL = 3;

    [SerializeField] private float spreadSpherecastRadius = 7;
    [SerializeField] private float _spreadTimeGoal = 5;
    [SerializeField] private float stochasticSpreadRate = .2f;
    [SerializeField] private float bunnySpawnRate = .05f;
    [SerializeField] private float bunnySpawnRadius = 20;
    [SerializeField] private GameObject BunnyPrefab;

    private bool _bIsOnFire, _bIsBurnt;
    private Animator mAnimator;

    public bool bIsOnFire { get => _bIsOnFire; }
    public bool bIsBurnt { get => _bIsBurnt; }
    public float spreadTimeGoal { get => _spreadTimeGoal; set => _spreadTimeGoal = value; }

    float continuousFlameTime;

    float randSpawnInterval = float.MinValue;

    public event Action OnDoneBurning;
    public event Action OnCatchFire;
    public event Action OnExtinguished;

    void Start() {
        mAnimator = GetComponent<Animator>();
    }

    void Update() {
        if(_bIsOnFire) {
            continuousFlameTime += Time.deltaTime;
            if(continuousFlameTime > _spreadTimeGoal) {
                MarkDoneBurning();
                Spread();
            } else {
                if(randSpawnInterval < 0) {
                    randSpawnInterval = UnityEngine.Random.Range(1f,3f);
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
        foreach(RaycastHit hit in hits) {
            if(hit.transform.TryGetComponent<FireSpread>(out FireSpread spreadable)) {
                if(spreadable != this && !spreadable.bIsOnFire && !spreadable.bIsBurnt)
                    spreadable.MarkTorched();
            }
        }
    }

    void StochasticSpread() {
        if(UnityEngine.Random.Range(0f, 1f) < stochasticSpreadRate) {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position + Vector3.up * 4, spreadSpherecastRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
            int trial = 0;
            while(trial < MAX_TRIAL) {
                ++trial;
                if(hits[UnityEngine.Random.Range(0, hits.Length)].transform.TryGetComponent<FireSpread>(out FireSpread spreadable)) {
                    spreadable.MarkTorched();
                    break;
                }
            }
        }
    }

    void SpawnBunny() {
        if(UnityEngine.Random.Range(0f,1f) < bunnySpawnRate) {
            Vector3 randDir = new Vector3(UnityEngine.Random.Range(-bunnySpawnRadius, bunnySpawnRadius), 50, UnityEngine.Random.Range(-bunnySpawnRadius, bunnySpawnRadius)).normalized;
            float randDist = UnityEngine.Random.Range(4, bunnySpawnRadius);
            Vector3 randPosition = transform.position + randDir * randDist;
            RaycastHit[] hits = Physics.RaycastAll(randPosition, Vector3.down, 50*2f, 1 << LayerMask.NameToLayer("Land"));
            foreach(RaycastHit hit in hits)
                Instantiate(BunnyPrefab, hit.point, Quaternion.identity);
        }
    }

    public void MarkTorched() {
        _bIsOnFire = true;
        if(mAnimator.GetCurrentAnimatorStateInfo(mAnimator.GetLayerIndex("Base Layer")).fullPathHash == Animator.StringToHash("Base Layer.Tree_Unburnt")) {
            mAnimator.SetTrigger("Set Fire");
            OnCatchFire?.Invoke();
        }
    }

    public void MarkExtinguished() {
        _bIsOnFire = false;
        if(mAnimator.GetCurrentAnimatorStateInfo(mAnimator.GetLayerIndex("Base Layer")).fullPathHash == Animator.StringToHash("Base Layer.Tree_OnFire")) {
            mAnimator.SetTrigger("Extinguished");
            mAnimator.SetBool("Set Fire", false);   //unneccesary, but just in case
            OnExtinguished?.Invoke();
        }
        continuousFlameTime = 0;
    }

    public void MarkDoneBurning() {
        if(_bIsBurnt) return;
        
        _bIsOnFire = false;
        _bIsBurnt = true;
        OnDoneBurning?.Invoke();
        mAnimator.SetTrigger("Done Burning");
    }
}
