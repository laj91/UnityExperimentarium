using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using StarterAssets;
public class PalyerHealth : MonoBehaviour
{
    private StarterAssetsInputs starterAssetsInputs;
    private float health;
    private float lerpTimer;
    public float maxHealth = 100f;
    public float chipSpeed = 2f;
    public Image frontHealtbar;
    public Image backHealtbar;

    public Image overlay;
    public float duration;
    public float fadeSpeed;

    private float durationTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        health = maxHealth;
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 0);
    }

    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
        if (overlay.color.a > 0)
        {
            if (health < 30) return;
            durationTimer += Time.deltaTime;
            if (durationTimer > duration)
            {
                // fade the image
                float tempAlpha = overlay.color.a;
                tempAlpha -= Time.deltaTime * fadeSpeed;
                overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, tempAlpha);
            }
        }
    }


    private void UpdateHealthUI()
    {
        Debug.Log(health);
        float fillF = frontHealtbar.fillAmount;
        float fillB = backHealtbar.fillAmount;
        float hFraction = health / maxHealth;
        if (fillB > hFraction)
        {
            frontHealtbar.fillAmount = hFraction;
            backHealtbar.color = Color.red;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete = percentComplete * percentComplete;
            backHealtbar.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
        }

        if (fillF < hFraction)
        {
            backHealtbar.color = Color.green;
            backHealtbar.fillAmount = hFraction;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / chipSpeed;
            percentComplete = percentComplete * percentComplete;
            frontHealtbar.fillAmount = Mathf.Lerp(fillF, backHealtbar.fillAmount, percentComplete);
        }

    }


    public void TakeDamage(float damage)
    {
        health -= damage;
        lerpTimer = 0f;
        durationTimer = 1f;
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 1);
    }

    public void HealDamage(float healAmount)
    {
        health += healAmount;
        lerpTimer = 0f;
    }

}
