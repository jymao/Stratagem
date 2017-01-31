using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoManager : MonoBehaviour {

    public GameObject unitInfo;

    public Sprite assassin;
    public Sprite knight;
    public Sprite mage;
    public Sprite spear;
    public Sprite cannon;
    public Sprite ambulance;

    //private GameObject currUnit;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UnitInfoHide()
    {
        unitInfo.SetActive(false);
    }

    public void DisplayUnitInfo(GameObject unit, string unitName, bool enemy)
    {
        Unit unitScript = unit.GetComponent<Unit>();

        unitInfo.SetActive(true);

        unitInfo.transform.GetChild(0).gameObject.GetComponent<Text>().text = unitName;

        ChangeUnitImage(unitName, enemy);

        unitInfo.transform.GetChild(2).gameObject.GetComponent<Text>().text = "HEALTH: " + unitScript.GetCurrHealth() + "/" + unitScript.health;
        unitInfo.transform.GetChild(3).gameObject.GetComponent<Text>().text = "MOVE: " + unitScript.move;
        unitInfo.transform.GetChild(4).gameObject.GetComponent<Text>().text = "RANGE: " + unitScript.range;
        unitInfo.transform.GetChild(5).gameObject.GetComponent<Text>().text = "ATTACK: " + unitScript.attack;
        unitInfo.transform.GetChild(6).gameObject.GetComponent<Text>().text = "DEFENSE: " + unitScript.defense;
        unitInfo.transform.GetChild(7).gameObject.GetComponent<Text>().text = "MAGIC: " + unitScript.magic;
        unitInfo.transform.GetChild(8).gameObject.GetComponent<Text>().text = "RESIST: " + unitScript.resist;
        unitInfo.transform.GetChild(9).gameObject.GetComponent<Text>().text = "SPEED: " + unitScript.speed;
    }

    private void ChangeUnitImage(string unitName, bool enemy)
    {
        GameObject child = unitInfo.transform.GetChild(1).gameObject;
        Image img = child.GetComponent<Image>();

        switch(unitName)
        {
            case "Assassin":
                img.sprite = assassin;
                break;
            case "Knight":
                img.sprite = knight;
                break;
            case "Mage":
                img.sprite = mage;
                break;
            case "Spearwoman":
                img.sprite = spear;
                break;
            case "Cannoneer":
                img.sprite = cannon;
                break;
            case "Ambulance":
                img.sprite = ambulance;
                break;
            default:
                break;
        }
    }
}
