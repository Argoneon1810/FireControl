using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraShaker : MonoBehaviour {
    [SerializeField] AnimationCurve shakeReductionCurve;
    [SerializeField] float shakeLength;
    [SerializeField] float shakeAmplitude;
    
    public delegate void Callback();

    IEnumerator ShakeCoroutine;
    CinemachineBasicMultiChannelPerlin noise;

    void Start() {
        noise = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public Callback GetShake() {
        return Shake;
    }

    void Shake() {
        if(ShakeCoroutine != null) StopCoroutine(ShakeCoroutine);
        ShakeCoroutine = DoShake();
        StartCoroutine(ShakeCoroutine);
    }

    IEnumerator DoShake() {
        float t = 0;
        while(t < shakeLength) {
            t+=Time.deltaTime;
            noise.m_AmplitudeGain = shakeReductionCurve.Evaluate(t/shakeLength) * shakeAmplitude;
            yield return null;
        }
    }
}
