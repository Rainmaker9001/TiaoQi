using UnityEngine;
using System.Collections;
using C5;

public class Tester : MonoBehaviour {

	int rec(int a) {
		if (a == 0)
						return 1;
		return rec(a - 1);
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

	}
}