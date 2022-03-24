using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MeteoBody : MonoBehaviour {
    public event Action OnHit;
    [SerializeField] GameObject[] crashParticles;
    [SerializeField] GameObject[] flyParticles;

    [SerializeField] Vector3 gravityDir;
    [SerializeField] bool debugGravityNormalizeNeeded = false;

    [SerializeField] float DestroyDelay = 3f;

    [SerializeField] float FlameRadius = 7f;

    Rigidbody mRigidbody;

    bool doneStrike = false;

    private void Start() {
        mRigidbody = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision) {
        if(doneStrike) return;

        if(collision.gameObject.layer == LayerMask.NameToLayer("Land")) {
            OnHit?.Invoke();
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, FlameRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
            List<Animator> animList = new List<Animator>();
            foreach(var hit in hits) {
                if(hit.transform.TryGetComponent<Animator>(out Animator animator))
                    animList.Add(animator);
            }
            if(animList.Count > 0) {
                Arsonist.Arson(animList);
                animList.Clear();
            }
            gravityDir = Vector3.down;
            foreach(var crashParticle in crashParticles) {
                crashParticle.transform.SetParent(null, false);
                crashParticle.transform.position = collision.contacts[0].point;
                if(collision.contacts[0].normal != Vector3.right) {
                    crashParticle.transform.rotation = Quaternion.Euler(Quaternion.LookRotation(Vector3.Cross(collision.contacts[0].normal, Vector3.right), collision.contacts[0].normal).eulerAngles + Quaternion.Euler(-90, 0, 0).eulerAngles);
                } else {
                    crashParticle.transform.rotation = Quaternion.Euler(Quaternion.LookRotation(Vector3.up, collision.contacts[0].normal).eulerAngles + Quaternion.Euler(-90, 0, 0).eulerAngles);
                }
                crashParticle.SetActive(true);
            }
            foreach(var flyParticle in flyParticles) {
                flyParticle.SetActive(false);
            }
            StartCoroutine(DestroySelf());
            doneStrike = true;
        }
    }

    public void SetGravity(Vector3 gravityNormal) {
        gravityDir = gravityNormal;
    }

    void Update() {
        if(transform.position.y < -30) Kill();
        if(debugGravityNormalizeNeeded) {
            gravityDir = gravityDir.normalized;
            debugGravityNormalizeNeeded = false;
        }
        mRigidbody.AddForce(gravityDir * 9.8f);
    }

    IEnumerator DestroySelf() {
        yield return new WaitForSeconds(DestroyDelay);
        Kill();
    }

    void Kill() {
        foreach(var crashParticle in crashParticles)
            Destroy(crashParticle.gameObject);
        foreach(var flyParticle in flyParticles)
            Destroy(flyParticle.gameObject);
        for(int i = transform.childCount-1; i >= 0; --i) {
            Destroy(transform.GetChild(i).gameObject);
        }
        Destroy(gameObject);
    }
}
