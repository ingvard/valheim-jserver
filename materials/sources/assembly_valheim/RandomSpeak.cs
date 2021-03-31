using System;
using UnityEngine;

// Token: 0x020000ED RID: 237
public class RandomSpeak : MonoBehaviour
{
	// Token: 0x06000E9B RID: 3739 RVA: 0x00068841 File Offset: 0x00066A41
	private void Start()
	{
		base.InvokeRepeating("Speak", UnityEngine.Random.Range(0f, this.m_interval), this.m_interval);
	}

	// Token: 0x06000E9C RID: 3740 RVA: 0x00068864 File Offset: 0x00066A64
	private void Speak()
	{
		if (UnityEngine.Random.value > this.m_chance)
		{
			return;
		}
		if (this.m_texts.Length == 0)
		{
			return;
		}
		if (Player.m_localPlayer == null || Vector3.Distance(base.transform.position, Player.m_localPlayer.transform.position) > this.m_triggerDistance)
		{
			return;
		}
		this.m_speakEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
		string text = this.m_texts[UnityEngine.Random.Range(0, this.m_texts.Length)];
		Chat.instance.SetNpcText(base.gameObject, this.m_offset, this.m_cullDistance, this.m_ttl, this.m_topic, text, this.m_useLargeDialog);
		if (this.m_onlyOnce)
		{
			base.CancelInvoke("Speak");
		}
	}

	// Token: 0x04000D86 RID: 3462
	public float m_interval = 5f;

	// Token: 0x04000D87 RID: 3463
	public float m_chance = 0.5f;

	// Token: 0x04000D88 RID: 3464
	public float m_triggerDistance = 5f;

	// Token: 0x04000D89 RID: 3465
	public float m_cullDistance = 10f;

	// Token: 0x04000D8A RID: 3466
	public float m_ttl = 10f;

	// Token: 0x04000D8B RID: 3467
	public Vector3 m_offset = new Vector3(0f, 0f, 0f);

	// Token: 0x04000D8C RID: 3468
	public EffectList m_speakEffects = new EffectList();

	// Token: 0x04000D8D RID: 3469
	public bool m_useLargeDialog;

	// Token: 0x04000D8E RID: 3470
	public bool m_onlyOnce;

	// Token: 0x04000D8F RID: 3471
	public string m_topic = "";

	// Token: 0x04000D90 RID: 3472
	public string[] m_texts = new string[0];
}
