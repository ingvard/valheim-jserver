using System;
using System.Collections;
using UnityEngine;

// Token: 0x020000BC RID: 188
public class BossStone : MonoBehaviour
{
	// Token: 0x06000C80 RID: 3200 RVA: 0x000595AC File Offset: 0x000577AC
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

	// Token: 0x06000C81 RID: 3201 RVA: 0x00059631 File Offset: 0x00057831
	private void UpdateVisual()
	{
		this.SetActivated(this.m_itemStand.HaveAttachment(), true);
	}

	// Token: 0x06000C82 RID: 3202 RVA: 0x00059648 File Offset: 0x00057848
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

	// Token: 0x06000C83 RID: 3203 RVA: 0x000596CC File Offset: 0x000578CC
	private void DelayedAttachEffects_Step1()
	{
		this.m_activateStep1.Create(this.m_itemStand.transform.position, base.transform.rotation, null, 1f);
	}

	// Token: 0x06000C84 RID: 3204 RVA: 0x000596FB File Offset: 0x000578FB
	private void DelayedAttachEffects_Step2()
	{
		this.m_activateStep2.Create(base.transform.position, base.transform.rotation, null, 1f);
	}

	// Token: 0x06000C85 RID: 3205 RVA: 0x00059728 File Offset: 0x00057928
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

	// Token: 0x06000C86 RID: 3206 RVA: 0x000597AA File Offset: 0x000579AA
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

	// Token: 0x06000C87 RID: 3207 RVA: 0x000597B9 File Offset: 0x000579B9
	public bool IsActivated()
	{
		return this.m_active;
	}

	// Token: 0x04000B65 RID: 2917
	public ItemStand m_itemStand;

	// Token: 0x04000B66 RID: 2918
	public GameObject m_activeEffect;

	// Token: 0x04000B67 RID: 2919
	public EffectList m_activateStep1 = new EffectList();

	// Token: 0x04000B68 RID: 2920
	public EffectList m_activateStep2 = new EffectList();

	// Token: 0x04000B69 RID: 2921
	public EffectList m_activateStep3 = new EffectList();

	// Token: 0x04000B6A RID: 2922
	public string m_completedMessage = "";

	// Token: 0x04000B6B RID: 2923
	public MeshRenderer m_mesh;

	// Token: 0x04000B6C RID: 2924
	public int m_emissiveMaterialIndex;

	// Token: 0x04000B6D RID: 2925
	public Color m_activeEmissiveColor = Color.white;

	// Token: 0x04000B6E RID: 2926
	private bool m_active;

	// Token: 0x04000B6F RID: 2927
	private ZNetView m_nview;
}
