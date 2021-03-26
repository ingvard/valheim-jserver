using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000044 RID: 68
public class Smoke : MonoBehaviour
{
	// Token: 0x0600047E RID: 1150 RVA: 0x0002445C File Offset: 0x0002265C
	private void Awake()
	{
		this.m_body = base.GetComponent<Rigidbody>();
		Smoke.m_smoke.Add(this);
		this.m_added = true;
		this.m_mr = base.GetComponent<MeshRenderer>();
		this.m_vel += Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * this.m_randomVel;
	}

	// Token: 0x0600047F RID: 1151 RVA: 0x000244D4 File Offset: 0x000226D4
	private void OnDestroy()
	{
		if (this.m_added)
		{
			Smoke.m_smoke.Remove(this);
			this.m_added = false;
		}
	}

	// Token: 0x06000480 RID: 1152 RVA: 0x000244F1 File Offset: 0x000226F1
	public void StartFadeOut()
	{
		if (this.m_fadeTimer >= 0f)
		{
			return;
		}
		if (this.m_added)
		{
			Smoke.m_smoke.Remove(this);
			this.m_added = false;
		}
		this.m_fadeTimer = 0f;
	}

	// Token: 0x06000481 RID: 1153 RVA: 0x00024527 File Offset: 0x00022727
	public static int GetTotalSmoke()
	{
		return Smoke.m_smoke.Count;
	}

	// Token: 0x06000482 RID: 1154 RVA: 0x00024533 File Offset: 0x00022733
	public static void FadeOldest()
	{
		if (Smoke.m_smoke.Count == 0)
		{
			return;
		}
		Smoke.m_smoke[0].StartFadeOut();
	}

	// Token: 0x06000483 RID: 1155 RVA: 0x00024554 File Offset: 0x00022754
	public static void FadeMostDistant()
	{
		if (Smoke.m_smoke.Count == 0)
		{
			return;
		}
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		Vector3 position = mainCamera.transform.position;
		int num = -1;
		float num2 = 0f;
		for (int i = 0; i < Smoke.m_smoke.Count; i++)
		{
			float num3 = Vector3.Distance(Smoke.m_smoke[i].transform.position, position);
			if (num3 > num2)
			{
				num = i;
				num2 = num3;
			}
		}
		if (num != -1)
		{
			Smoke.m_smoke[num].StartFadeOut();
		}
	}

	// Token: 0x06000484 RID: 1156 RVA: 0x000245E8 File Offset: 0x000227E8
	private void Update()
	{
		this.m_time += Time.deltaTime;
		if (this.m_time > this.m_ttl && this.m_fadeTimer < 0f)
		{
			this.StartFadeOut();
		}
		float num = 1f - Mathf.Clamp01(this.m_time / this.m_ttl);
		this.m_body.mass = num * num;
		Vector3 velocity = this.m_body.velocity;
		Vector3 vel = this.m_vel;
		vel.y *= num;
		Vector3 a = vel - velocity;
		this.m_body.AddForce(a * this.m_force * Time.deltaTime, ForceMode.VelocityChange);
		if (this.m_fadeTimer >= 0f)
		{
			this.m_fadeTimer += Time.deltaTime;
			float a2 = 1f - Mathf.Clamp01(this.m_fadeTimer / this.m_fadetime);
			Color color = this.m_mr.material.color;
			color.a = a2;
			this.m_mr.material.color = color;
			if (this.m_fadeTimer >= this.m_fadetime)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	// Token: 0x0400048F RID: 1167
	public Vector3 m_vel = Vector3.up;

	// Token: 0x04000490 RID: 1168
	public float m_randomVel = 0.1f;

	// Token: 0x04000491 RID: 1169
	public float m_force = 0.1f;

	// Token: 0x04000492 RID: 1170
	public float m_ttl = 10f;

	// Token: 0x04000493 RID: 1171
	public float m_fadetime = 3f;

	// Token: 0x04000494 RID: 1172
	private Rigidbody m_body;

	// Token: 0x04000495 RID: 1173
	private float m_time;

	// Token: 0x04000496 RID: 1174
	private float m_fadeTimer = -1f;

	// Token: 0x04000497 RID: 1175
	private bool m_added;

	// Token: 0x04000498 RID: 1176
	private MeshRenderer m_mr;

	// Token: 0x04000499 RID: 1177
	private static List<Smoke> m_smoke = new List<Smoke>();
}
