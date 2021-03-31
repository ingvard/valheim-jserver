using System;
using System.Collections;
using UnityEngine;

// Token: 0x020000BC RID: 188
public class BossStone : MonoBehaviour
{
	// Token: 0x06000C81 RID: 3201 RVA: 0x00059734 File Offset: 0x00057934
	private void Start()
	{
		if (this.m_mesh.material.HasProperty("_EmissionColor"))
		{
			this.m_mesh.materials[this.m_emissiveMaterialIndex].SetColor("_EmissionColor", Color.black);
		}
		if (this.m_activeEffect)
		{
			this.m_activeEffect.SetActive(false);
		}
		this.SetActivated(this.m_itemStand.HaveAttachment(), false);
		base.InvokeRepeating("UpdateVisual", 1f, 1f);
	}

	// Token: 0x06000C82 RID: 3202 RVA: 0x000597B9 File Offset: 0x000579B9
	private void UpdateVisual()
	{
		this.SetActivated(this.m_itemStand.HaveAttachment(), true);
	}

	// Token: 0x06000C83 RID: 3203 RVA: 0x000597D0 File Offset: 0x000579D0
	private void SetActivated(bool active, bool triggerEffect)
	{
		if (active == this.m_active)
		{
			return;
		}
		this.m_active = active;
		if (triggerEffect && active)
		{
			base.Invoke("DelayedAttachEffects_Step1", 1f);
			base.Invoke("DelayedAttachEffects_Step2", 5f);
			base.Invoke("DelayedAttachEffects_Step3", 11f);
			return;
		}
		if (this.m_activeEffect)
		{
			this.m_activeEffect.SetActive(active);
		}
		base.StopCoroutine("FadeEmission");
		base.StartCoroutine("FadeEmission");
	}

	// Token: 0x06000C84 RID: 3204 RVA: 0x00059854 File Offset: 0x00057A54
	private void DelayedAttachEffects_Step1()
	{
		this.m_activateStep1.Create(this.m_itemStand.transform.position, base.transform.rotation, null, 1f);
	}

	// Token: 0x06000C85 RID: 3205 RVA: 0x00059883 File Offset: 0x00057A83
	private void DelayedAttachEffects_Step2()
	{
		this.m_activateStep2.Create(base.transform.position, base.transform.rotation, null, 1f);
	}

	// Token: 0x06000C86 RID: 3206 RVA: 0x000598B0 File Offset: 0x00057AB0
	private void DelayedAttachEffects_Step3()
	{
		if (this.m_activeEffect)
		{
			this.m_activeEffect.SetActive(true);
		}
		this.m_activateStep3.Create(base.transform.position, base.transform.rotation, null, 1f);
		base.StopCoroutine("FadeEmission");
		base.StartCoroutine("FadeEmission");
		Player.MessageAllInRange(base.transform.position, 20f, MessageHud.MessageType.Center, this.m_completedMessage, null);
	}

	// Token: 0x06000C87 RID: 3207 RVA: 0x00059932 File Offset: 0x00057B32
	private IEnumerator FadeEmission()
	{
		if (this.m_mesh && this.m_mesh.materials[this.m_emissiveMaterialIndex].HasProperty("_EmissionColor"))
		{
			Color startColor = this.m_mesh.materials[this.m_emissiveMaterialIndex].GetColor("_EmissionColor");
			Color targetColor = this.m_active ? this.m_activeEmissiveColor : Color.black;
			for (float t = 0f; t < 1f; t += Time.deltaTime)
			{
				Color value = Color.Lerp(startColor, targetColor, t / 1f);
				this.m_mesh.materials[this.m_emissiveMaterialIndex].SetColor("_EmissionColor", value);
				yield return null;
			}
			startColor = default(Color);
			targetColor = default(Color);
		}
		ZLog.Log("Done fading color");
		yield break;
	}

	// Token: 0x06000C88 RID: 3208 RVA: 0x00059941 File Offset: 0x00057B41
	public bool IsActivated()
	{
		return this.m_active;
	}

	// Token: 0x04000B6B RID: 2923
	public ItemStand m_itemStand;

	// Token: 0x04000B6C RID: 2924
	public GameObject m_activeEffect;

	// Token: 0x04000B6D RID: 2925
	public EffectList m_activateStep1 = new EffectList();

	// Token: 0x04000B6E RID: 2926
	public EffectList m_activateStep2 = new EffectList();

	// Token: 0x04000B6F RID: 2927
	public EffectList m_activateStep3 = new EffectList();

	// Token: 0x04000B70 RID: 2928
	public string m_completedMessage = "";

	// Token: 0x04000B71 RID: 2929
	public MeshRenderer m_mesh;

	// Token: 0x04000B72 RID: 2930
	public int m_emissiveMaterialIndex;

	// Token: 0x04000B73 RID: 2931
	public Color m_activeEmissiveColor = Color.white;

	// Token: 0x04000B74 RID: 2932
	private bool m_active;

	// Token: 0x04000B75 RID: 2933
	private ZNetView m_nview;
}
