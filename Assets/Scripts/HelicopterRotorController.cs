using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopterRotorController : MonoBehaviour {
    const int BASE_ROTOR_SPEED = 3000;
    [SerializeField] float rotorSpeedMultiplier = 1;
    [SerializeField] Transform tailRotor, mainRotor;

    void Update() {
        if(mainRotor!=null)
            mainRotor.rotation = Quaternion.Euler(mainRotor.rotation.eulerAngles + Vector3.up * BASE_ROTOR_SPEED * rotorSpeedMultiplier * Time.deltaTime);
        if(tailRotor!=null)
            tailRotor.rotation = Quaternion.Euler(tailRotor.rotation.eulerAngles + Vector3.forward * BASE_ROTOR_SPEED * rotorSpeedMultiplier * Time.deltaTime);
    }
}
