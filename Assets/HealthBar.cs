using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    public Slider healthBar;
    private void Start()
    {
        
    }
    public void SetMaxHealth(int hp)
    {
        healthBar.maxValue = hp;
        healthBar.value = hp;
    }
    public void SetHealth(int hp)
    {
        healthBar.value = hp;
    }
}
