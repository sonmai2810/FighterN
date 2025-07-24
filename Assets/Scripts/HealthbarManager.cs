using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthbarManager : MonoBehaviour
{
    public Image healthBarGreen; // UI cho máu hiện tại
    public Image healthBarRed;   // UI cho máu đã mất
    public float maxHealth = 50f;
    public float health = 50f;

    private Coroutine greenBarCoroutine;
    private Coroutine redBarCoroutine;

    public void TakeDamage(float damage)
    {
        float oldHealth = health;
        health = Mathf.Clamp(health - damage, 0f, maxHealth);
        float targetFill = health / maxHealth;

        // Animate green bar (immediate decrease)
        if (greenBarCoroutine != null) StopCoroutine(greenBarCoroutine);
        greenBarCoroutine = StartCoroutine(AnimateHealthBar(healthBarGreen, oldHealth / maxHealth, targetFill, 0.3f));

        // Animate red bar (delayed decrease for effect)
        if (redBarCoroutine != null) StopCoroutine(redBarCoroutine);
        redBarCoroutine = StartCoroutine(AnimateHealthBar(healthBarRed, healthBarRed.fillAmount, targetFill, 0.3f, 0.4f));
    }

    private IEnumerator AnimateHealthBar(Image bar, float from, float to, float duration, float delay = 0f)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            bar.fillAmount = Mathf.Lerp(from, to, t);
            yield return null;
        }
        bar.fillAmount = to;
    }
}
