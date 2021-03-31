using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000044 RID: 68
public class Smoke : MonoBehaviour
{
	// Token: 0x0600047F RID: 1151 RVA: 0x00024510 File Offset: 0x00022710
	private void Awake()
	{
		this.m_body = base.GetComponent<Rigidbody>();
		Smoke.m_smoke.Add(this);
		this.m_added = true;
		this.m_mr = base.GetComponent<MeshRenderer>();
		this.m_vel += Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * this.m_randomVel;
	}

	// Token: 0x06000480 RID: 1152 RVA: 0x00024588 File Offset: 0x00022788
	private void OnDestroy()
	{
		if (this.m_added)
		{
			Smoke.m_smoke.Remove(this);
			this.m_added = false;
		}
	}

	// Token: 0x06000481 RID: 1153 RVA: 0x000245A5 File Offset: 0x000227A5
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

	// Token: 0x06000482 RID: 1154 RVA: 0x000245DB File Offset: 0x000227DB
	public static int GetTotalSmoke()
	{
		return Smoke.m_smoke.Count;
	}

	// Token: 0x06000483 RID: 1155 RVA: 0x000245E7 File Offset: 0x000227E7
	public static void FadeOldest()
	{
		if (Smoke.m_smoke.Count == 0)
		{
			return;
		}
		Smoke.m_smoke[0].StartFadeOut();
	}

	// Token: 0x06000484 RID: 1156 RVA: 0x00024608 File Offset: 0x00022808
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

	// Token: 0x06000485 RID: 1157 RVA: 0x0002469C File Offset: 0x0002289C
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

	// Token: 0x04000493 RID: 1171
	public Vector3 m_vel = Vector3.up;

	// Token: 0x04000494 RID: 1172
	public float m_randomVel = 0.1f;

	// Token: 0x04000495 RID: 1173
	public float m_force = 0.1f;

	// Token: 0x04000496 RID: 1174
	public float m_ttl = 10f;

	// Token: 0x04000497 RID: 1175
	public float m_fadetime = 3f;

	// Token: 0x04000498 RID: 1176
	private Rigidbody m_body;

	// Token: 0x04000499 RID: 1177
	private float m_time;

	// Token: 0x0400049A RID: 1178
	private float m_fadeTimer = -1f;

	// Token: 0x0400049B RID: 1179
	private bool m_added;

	// Token: 0x0400049C RID: 1180
	private MeshRenderer m_mr;

	// Token: 0x0400049D RID: 1181
	private static List<Smoke> m_smoke = new List<Smoke>();
}
