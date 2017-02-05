using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthNumber : MonoBehaviour {

    private Text number;

    void Awake()
    {
        number = gameObject.GetComponent<Text>();
    }

	// Use this for initialization
	void Start () {
        StartCoroutine(Decay());
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    public void SetText(string s)
    {
        number.text = s;
    }

    private IEnumerator Decay()
    {
        Color fade = new Color(1, 1, 1, 1);

        for (int i = 0; i < 50; i++)
        {
            gameObject.transform.position = gameObject.transform.position + new Vector3(0, 0.02f, 0);
            fade.a = fade.a - 0.02f;
            number.color = fade;

            yield return new WaitForSeconds(0.02f);
        }

        Destroy(gameObject);
    }
}
