using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TestUI : MonoBehaviour
{
    [SerializeField] private Image lightningImage;  // 稲妻のImage
    [SerializeField] private float moveDistance = 800f;  // 横に動く距離
    [SerializeField] private float duration = 0.3f;      // 移動時間

    void Start()
    {
        lightningImage.enabled = false;

    }

    public void PlayCutIn()
    {
        lightningImage.enabled = true;
        lightningImage.color = new Color(1, 1, 1, 0); // 最初は透明
        lightningImage.rectTransform.anchoredPosition = new Vector2(-moveDistance / 2, 0);

        // アニメーションシーケンス
        Sequence seq = DOTween.Sequence();

        seq.Append(lightningImage.DOFade(1, 0.05f)) // パッと光る
           .Join(lightningImage.rectTransform.DOAnchorPosX(moveDistance / 2, duration).SetEase(Ease.OutQuad))
           .Append(lightningImage.DOFade(0, 0.1f))  // 消える
           .OnComplete(() => lightningImage.enabled = false);
    }
}
