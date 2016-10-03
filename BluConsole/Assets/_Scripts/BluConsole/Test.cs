using UnityEngine;
using System.Collections;
using UnityEditor;

public class Test : MonoBehaviour {

	void Awake() {
	}
		
	void Update () {
		if (Input.GetKeyDown(KeyCode.S)) {
			Debug.Log("Normal");
		}
        if (Input.GetKeyDown(KeyCode.D)) {
            Debug.LogWarning("Warning");
        }
        if (Input.GetKeyDown(KeyCode.F)) {
            Debug.LogError("Error");
        }
	}

}
