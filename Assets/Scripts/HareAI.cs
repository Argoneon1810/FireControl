using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
public class HareAI : MonoBehaviour {
    [SerializeField] float movementSpeed;
    [SerializeField] float searchRadius;
    [SerializeField] float rotationDuration;
    [SerializeField] float deathDistance;
    [SerializeField] ParticleSystem fireParticle;

    Transform target;
    float turnSmoothVelocity;

    float life;

    private void Start() {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, searchRadius, Vector3.forward, 0, 1 << LayerMask.NameToLayer("Flameable"));
        float sqrDistance = float.MinValue;
        foreach(RaycastHit hit in hits) {
            float tempDist = (hit.transform.position - transform.position).sqrMagnitude;
            if(tempDist > sqrDistance) {
                sqrDistance = tempDist;
                target = hit.transform;
            }
        }
        if(fireParticle == null) 
            fireParticle = GetComponentInChildren<ParticleSystem>();
        life = Random.Range(3f, 5f);
    }

    void Update() {
        if(life <= 0) Death();
        if(target==null) return;
        
        life -= Time.deltaTime;

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * movementSpeed * Time.deltaTime;

        float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, rotationDuration);
        transform.rotation = Quaternion.Euler(0, angle, 0);

        dir.y = 0;
        Vector3 facingVector = transform.forward;
        facingVector.y = 0;
        float facingAngle = Mathf.Acos(Vector3.Dot(facingVector.normalized, dir.normalized)) * Mathf.Rad2Deg;
        if(facingAngle > 15)
            GetComponent<Animator>().SetFloat("Blend", 0);
        else if(facingAngle > 5)
            GetComponent<Animator>().SetFloat("Blend", .25f);
        else if (facingAngle < -15)
            GetComponent<Animator>().SetFloat("Blend", 1);
        else if (facingAngle < -5)
            GetComponent<Animator>().SetFloat("Blend", .75f);
        else 
            GetComponent<Animator>().SetFloat("Blend", 0.5f);
    }

    void OnDrawGizmos() {
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }

    void OnCollisionEnter(Collision collision) {
        Animator animator = collision.gameObject.GetComponent<Animator>();
        if(collision.gameObject.layer == LayerMask.NameToLayer("Flameable") && animator.GetCurrentAnimatorStateInfo(animator.GetLayerIndex("Base Layer")).fullPathHash == Animator.StringToHash("Base Layer.Tree_Unburnt")) {
            Death();
            collision.gameObject.GetComponent<FireSpread>().MarkTorched();
        }
    }

    void Death() {
        target = null;
        GetComponent<Animator>().SetTrigger("Death");
        GetComponent<Rigidbody>().isKinematic = true;
        ParticleSystem.EmissionModule emission = fireParticle.emission;
        emission.enabled = false;
        StartCoroutine(SelfKillCoroutine());
    }

    IEnumerator SelfKillCoroutine() {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
