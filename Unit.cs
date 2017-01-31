using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {

    public int health;
    public int move;
    public int range;
    public int attack;
    public int defense;
    public int magic;
    public int resist;
    public int speed;

    private int currHealth;

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

    public void Heal(int amount)
    {
        ChangeHealth(amount);
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
        }
        else
        {
            if(amount - defense > 0)
            {
                damage = amount - defense;
            }
            ChangeHealth(-damage);
        }
    }
}
