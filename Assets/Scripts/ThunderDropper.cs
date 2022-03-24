using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderDropper : MonoBehaviour {
    public event Action OnHit;

    [SerializeField] float expandDuration = .5f;
    [SerializeField] AnimationCurve expandCurve;
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
        if(expandCurve == null) expandCurve = AnimationCurve.Linear(0,0,1,1);
    }

    public void Call() {
        if(called) {
            foreach(var thunder in thunders) {
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
        while (t < expandDuration) {
            t+=Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, expandCurve.Evaluate(t/expandDuration));
            yield return null;
        }

        yield return new WaitForSeconds(thunderDelay);

        RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, 200, 1 << LayerMask.NameToLayer("Land"));
        if(hits.Length > 0) {
            OnHit?.Invoke();
            //Strike Thunder
            float targetScaleY = (hits[0].point - transform.position).sqrMagnitude / (20*20);
            t = 0;
            while(t < thunderReachDuration) {
                t+=Time.deltaTime;
                foreach(var thunder in thunders) {
                    Vector3 temp = thunder.localScale;
                    temp.x = Mathf.Lerp(1f, 0f, thunderDecayCurve.Evaluate(t/thunderReachDuration));
                    temp.y = Mathf.Lerp(0, targetScaleY, thunderDropCurve.Evaluate(t/thunderReachDuration));
                    temp.z = Mathf.Lerp(1f, 0f, thunderDecayCurve.Evaluate(t/thunderReachDuration));
                    thunder.localScale = temp;
                    yield return null;
                }
            }
            //Set Fire to Strikes
            RaycastHit[] hits2 = Physics.SphereCastAll(hits[0].point, fireSpreadRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
            List<Animator> animList = new List<Animator>();
            foreach(var hit in hits2) {
                if(hit.transform.TryGetComponent<Animator>(out Animator animator)) {
                    animList.Add(animator);
                }
            }
            if(animList.Count > 0) {
                Arsonist.Arson(animList);
                animList.Clear();
            }
            yield return null;

            //Decay Thunder
            foreach(var thunder in thunders) {
                Vector3 temp = thunder.localScale;
                temp.y = 0;
                thunder.localScale = temp;
                yield return null;
            }
        }

        yield return new WaitForSeconds(thunderDelay);

        //Hide Thundercloud
        t = 0;
        while (t < expandDuration) {
            t+=Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, expandCurve.Evaluate(t/expandDuration));
            yield return null;
        }
        Destroy(gameObject);
    }
}
