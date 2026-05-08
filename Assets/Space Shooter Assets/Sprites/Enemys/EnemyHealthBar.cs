using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Image fillImage;

    public void UpdateBar(float fraction)
    {
        if (fillImage != null)
            fillImage.fillAmount = Mathf.Clamp01(fraction);
    }
}
