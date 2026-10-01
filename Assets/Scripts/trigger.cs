using UnityEngine;

public class Trigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void OntiggerEnter(Collider other){
    Debug.Log("El player ha entrado en el trigger");
    }
}