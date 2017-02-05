using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleMenuManager : MonoBehaviour {

    public GameObject levelSelectUI;
    public GameObject title;
    public GameObject startButton;

    public Sprite level1_image;
    public Sprite level2_image;

    private int level = 1;
    private int maxLevel = 2;

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

    public void LoadLevel()
    {
        SceneManager.LoadScene(level);
    }

    public void NextLevel()
    {
        level++;
        //enable prev button
        levelSelectUI.transform.GetChild(1).gameObject.SetActive(true);

        if(level == maxLevel)
        {
            //disable next button
            ButtonColorOnExit(levelSelectUI.transform.GetChild(2).gameObject);
            levelSelectUI.transform.GetChild(2).gameObject.SetActive(false);
        }

        if(level == 2)
        {
            levelSelectUI.transform.GetChild(4).gameObject.GetComponent<SpriteRenderer>().sprite = level2_image;
        }
    }

    public void PrevLevel()
    {
        level--;
        //enable next button
        levelSelectUI.transform.GetChild(2).gameObject.SetActive(true);

        if (level == 1)
        {
            //disable prev button
            ButtonColorOnExit(levelSelectUI.transform.GetChild(1).gameObject);
            levelSelectUI.transform.GetChild(1).gameObject.SetActive(false);
            levelSelectUI.transform.GetChild(4).gameObject.GetComponent<SpriteRenderer>().sprite = level1_image;
        }

    }
}
