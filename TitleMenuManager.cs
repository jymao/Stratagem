using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleMenuManager : MonoBehaviour {

    public GameObject levelSelectUI;
    public GameObject title;
    public GameObject startButton;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ButtonColorOnEnter(GameObject button)
    {
        GameObject text = button.transform.GetChild(0).gameObject;

        if(button.GetComponent<Button>().interactable)
            text.GetComponent<Text>().color = new Color(1, 32f / 255f, 0);
    }

    public void ButtonColorOnExit(GameObject button)
    {
        GameObject text = button.transform.GetChild(0).gameObject;

        if (button.GetComponent<Button>().interactable)
            text.GetComponent<Text>().color = new Color(0,0,0);
    }

    public void ToLevelSelect()
    {
        startButton.SetActive(false);
        title.SetActive(false);

        levelSelectUI.SetActive(true);
    }

    public void LoadLevel(int level)
    {
        SceneManager.LoadScene(level);
    }
}
