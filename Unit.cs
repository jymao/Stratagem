using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour {

    public int health;
    public int move;
    public int range;
    public int attack;
    public int defense;
    public int magic;
    public int resist;
    public int speed;

    public GameObject healthNumber;

    private int currHealth;

    private bool hasMoved = false;
    private bool hasActed = false;

	// Use this for initialization
	void Start () {
        currHealth = health;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public int GetCurrHealth()
    {
        return currHealth;
    }

    public void SetMoved(bool b)
    {
        hasMoved = b;
    }

    public bool GetMoved()
    {
        return hasMoved;
    }

    public void SetActed(bool b)
    {
        hasActed = b;
    }

    public bool GetActed()
    {
        return hasActed;
    }

    private void ChangeHealth(int amount)
    {
        currHealth += amount;
        if(currHealth > health)
        {
            currHealth = health;
        }
        if(currHealth < 0)
        {
            currHealth = 0;
        }
    }

    public IEnumerator Heal(int amount)
    {
        StartCoroutine(BattleResult(amount, false));
        ChangeHealth(amount);

        Color healAura = new Color(1, 1, 1, 1);

        for (int i = 0; i < 10; i++)
        {
            healAura.r = healAura.r - 0.1f;
            healAura.b = healAura.b - 0.1f;
            gameObject.GetComponent<SpriteRenderer>().color = healAura;
            yield return new WaitForSeconds(0.1f);
        }

        for (int i = 0; i < 10; i++)
        {
            healAura.r = healAura.r + 0.1f;
            healAura.b = healAura.b + 0.1f;
            gameObject.GetComponent<SpriteRenderer>().color = healAura;
            yield return new WaitForSeconds(0.1f);
        }

    }

    private IEnumerator BattleResult(int amount, bool isDamage)
    {
        yield return new WaitForSeconds(1f);

        GameObject canvas = GameObject.Find("Canvas");
        GameObject numberObject = (GameObject)Instantiate(healthNumber, gameObject.transform.position, Quaternion.identity);

        numberObject.transform.SetParent(canvas.transform);

        string modifier = "-";
        if (!isDamage)
        {
            modifier = "+";
        }
        numberObject.GetComponent<HealthNumber>().SetText(modifier + amount.ToString());
    }

    public void Damage(int amount, bool magical)
    {
        int damage = 0;
        if(magical)
        {
            if(amount - resist > 0)
            {
                damage = amount - resist;
            }
            ChangeHealth(-damage);
            StartCoroutine(BattleResult(damage, true));
        }
        else
        {
            if(amount - defense > 0)
            {
                damage = amount - defense;
            }
            ChangeHealth(-damage);
            StartCoroutine(BattleResult(damage, true));
        }
    }

    public IEnumerator Death()
    {
        yield return new WaitForSeconds(1f);

        Color fade = new Color(1, 1, 1, 1);

        for(int i = 0; i < 10; i++)
        {
            fade.a = fade.a - 0.1f;
            gameObject.GetComponent<SpriteRenderer>().color = fade;
            yield return new WaitForSeconds(0.15f);
        }

        Destroy(gameObject);
    }
}
