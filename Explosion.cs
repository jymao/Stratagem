using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(Decay());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private IEnumerator Decay()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
