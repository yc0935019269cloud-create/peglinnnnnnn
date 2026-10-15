using DG.Tweening;

namespace Battle.Attacks
{
	public class CauldrunBubbalShotTween : DoFireShotTweenOnEnable
	{
		protected override void StartTween()
		{
			Sequence sequence = DOTween.Sequence();
			sequence.Append(base.gameObject.transform.DOMoveX(peglinNormalPos.x + 12f, 1.333f).From(peglinGameObject.transform.position.x).SetEase(Ease.Linear));
			sequence.Insert(0f, base.gameObject.transform.DOMoveY(peglinArmPos.y + 4.6f, 0.666f).From(peglinGameObject.transform.position.y + 1.24f).SetEase(Ease.OutSine));
			sequence.Insert(0f, base.gameObject.transform.DOMoveY(peglinArmPos.y, 0.666f).SetEase(Ease.InSine).SetDelay(0.666f));
			sequence.Play();
		}
	}
}
