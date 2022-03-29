using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderCloud : MonoBehaviour {
    public event Action OnHit;

    [SerializeField] float showHideDuration = .5f;
    [SerializeField] AnimationCurve showHideCurve;
    [SerializeField] Transform[] thunders;
    [SerializeField] float thunderDelay = 1;
    [SerializeField] AnimationCurve thunderDropCurve;
    [SerializeField] AnimationCurve thunderDecayCurve;
    [SerializeField] float thunderReachDuration = .5f;
    [SerializeField] float fireSpreadRadius = 7f;

    [SerializeField] bool DebugCall;
    bool called;

    IEnumerator LastCoroutine;

    void Update() {
        if(DebugCall) {
            DebugCall = false;
            Call();
        }
    }

    void Start() {
        if(showHideCurve == null) showHideCurve = AnimationCurve.Linear(0,0,1,1);
    }

    public void Call() {
        if(called) {
            foreach(Transform thunder in thunders) {
                Vector3 scale = thunder.localScale;
                scale.x = 1;
                scale.y = 0;
                scale.z = 1;
                thunder.localScale = scale;
            }
            called = false;
        }
        called = true;
        if(LastCoroutine!=null) StopCoroutine(LastCoroutine);
        LastCoroutine = OnCall();
        StartCoroutine(LastCoroutine);
    }

    IEnumerator OnCall() {
        //Show Thundercloud
        float t = 0;
        while (t < showHideDuration) {
            t+=Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, showHideCurve.Evaluate(t/showHideDuration));
            yield return null;
        }

        yield return new WaitForSeconds(thunderDelay);

        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, 200, 1 << LayerMask.NameToLayer("Land"));
        if(hits.Length > 0) {
            OnHit?.Invoke();
            StartCoroutine(DropThunder(hits[0].point));
            StartCoroutine(SetFireToStrike(hits[0].point));
        }

        yield return new WaitForSeconds(thunderDelay);

        //Hide Thundercloud
        t = 0;
        while (t < showHideDuration) {
            t+=Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, showHideCurve.Evaluate(t/showHideDuration));
            yield return null;
        }
        Destroy(gameObject);
    }

    IEnumerator DropThunder(Vector3 point) {
        //Drop Thunder
        float targetScaleY = (point - transform.position).sqrMagnitude / (20*20);
        float t = 0;
        while(t < thunderReachDuration) {
            t+=Time.deltaTime;
            foreach(Transform thunder in thunders) {
                Vector3 temp = thunder.localScale;
                temp.x = Mathf.Lerp(1f, 0f, thunderDecayCurve.Evaluate(t/thunderReachDuration));
                temp.y = Mathf.Lerp(0, targetScaleY, thunderDropCurve.Evaluate(t/thunderReachDuration));
                temp.z = Mathf.Lerp(1f, 0f, thunderDecayCurve.Evaluate(t/thunderReachDuration));
                thunder.localScale = temp;
                yield return null;
            }
        }
        //Decay Thunder
        foreach(var thunder in thunders) {
            Vector3 temp = thunder.localScale;
            temp.y = 0;
            thunder.localScale = temp;
            yield return null;
        }
    }

    IEnumerator SetFireToStrike(Vector3 point) {
        //Set Fire to Strikes
        RaycastHit[] hits = Physics.SphereCastAll(point, fireSpreadRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
        List<FireSpread> spreadables = new List<FireSpread>();
        foreach(RaycastHit hit in hits) {
            if(hit.transform.TryGetComponent<FireSpread>(out FireSpread spreadable)) {
                spreadable.MarkTorched();
            }
        }
        yield return null;
    }
}
