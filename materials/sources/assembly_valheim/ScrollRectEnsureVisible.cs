using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200005C RID: 92
[RequireComponent(typeof(ScrollRect))]
public class ScrollRectEnsureVisible : MonoBehaviour
{
	// Token: 0x060005F7 RID: 1527 RVA: 0x00032EE1 File Offset: 0x000310E1
	private void Awake()
	{
		if (!this.mInitialized)
		{
			this.Initialize();
		}
	}

	// Token: 0x060005F8 RID: 1528 RVA: 0x00032EF4 File Offset: 0x000310F4
	private void Initialize()
	{
		this.mScrollRect = base.GetComponent<ScrollRect>();
		this.mScrollTransform = (this.mScrollRect.transform as RectTransform);
		this.mContent = this.mScrollRect.content;
		this.Reset();
		this.mInitialized = true;
	}

	// Token: 0x060005F9 RID: 1529 RVA: 0x00032F44 File Offset: 0x00031144
	public void CenterOnItem(RectTransform target)
	{
		if (!this.mInitialized)
		{
			this.Initialize();
		}
		Vector3 worldPointInWidget = this.GetWorldPointInWidget(this.mScrollTransform, this.GetWidgetWorldPoint(target));
		Vector3 vector = this.GetWorldPointInWidget(this.mScrollTransform, this.GetWidgetWorldPoint(this.maskTransform)) - worldPointInWidget;
		vector.z = 0f;
		if (!this.mScrollRect.horizontal)
		{
			vector.x = 0f;
		}
		if (!this.mScrollRect.vertical)
		{
			vector.y = 0f;
		}
		Vector2 b = new Vector2(vector.x / (this.mContent.rect.size.x - this.mScrollTransform.rect.size.x), vector.y / (this.mContent.rect.size.y - this.mScrollTransform.rect.size.y));
		Vector2 vector2 = this.mScrollRect.normalizedPosition - b;
		if (this.mScrollRect.movementType != ScrollRect.MovementType.Unrestricted)
		{
			vector2.x = Mathf.Clamp01(vector2.x);
			vector2.y = Mathf.Clamp01(vector2.y);
		}
		this.mScrollRect.normalizedPosition = vector2;
	}

	// Token: 0x060005FA RID: 1530 RVA: 0x0003309C File Offset: 0x0003129C
	private void Reset()
	{
		if (this.maskTransform == null)
		{
			Mask componentInChildren = base.GetComponentInChildren<Mask>(true);
			if (componentInChildren)
			{
				this.maskTransform = componentInChildren.rectTransform;
			}
			if (this.maskTransform == null)
			{
				RectMask2D componentInChildren2 = base.GetComponentInChildren<RectMask2D>(true);
				if (componentInChildren2)
				{
					this.maskTransform = componentInChildren2.rectTransform;
				}
			}
		}
	}

	// Token: 0x060005FB RID: 1531 RVA: 0x00033100 File Offset: 0x00031300
	private Vector3 GetWidgetWorldPoint(RectTransform target)
	{
		Vector3 b = new Vector3((0.5f - target.pivot.x) * target.rect.size.x, (0.5f - target.pivot.y) * target.rect.size.y, 0f);
		Vector3 position = target.localPosition + b;
		return target.parent.TransformPoint(position);
	}

	// Token: 0x060005FC RID: 1532 RVA: 0x0003317C File Offset: 0x0003137C
	private Vector3 GetWorldPointInWidget(RectTransform target, Vector3 worldPoint)
	{
		return target.InverseTransformPoint(worldPoint);
	}

	// Token: 0x040006A0 RID: 1696
	private RectTransform maskTransform;

	// Token: 0x040006A1 RID: 1697
	private ScrollRect mScrollRect;

	// Token: 0x040006A2 RID: 1698
	private RectTransform mScrollTransform;

	// Token: 0x040006A3 RID: 1699
	private RectTransform mContent;

	// Token: 0x040006A4 RID: 1700
	private bool mInitialized;
}
