using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000034 RID: 52
public class CircleProjector : MonoBehaviour
{
	// Token: 0x06000414 RID: 1044 RVA: 0x00021160 File Offset: 0x0001F360
	private void Start()
	{
		this.CreateSegments();
	}

	// Token: 0x06000415 RID: 1045 RVA: 0x00021168 File Offset: 0x0001F368
	private void Update()
	{
		this.CreateSegments();
		float num = 6.2831855f / (float)this.m_segments.Count;
		for (int i = 0; i < this.m_segments.Count; i++)
		{
			float f = (float)i * num + Time.time * 0.1f;
			Vector3 vector = base.transform.position + new Vector3(Mathf.Sin(f) * this.m_radius, 0f, Mathf.Cos(f) * this.m_radius);
			GameObject gameObject = this.m_segments[i];
			RaycastHit raycastHit;
			if (Physics.Raycast(vector + Vector3.up * 500f, Vector3.down, out raycastHit, 1000f, this.m_mask.value))
			{
				vector.y = raycastHit.point.y;
			}
			gameObject.transform.position = vector;
		}
		for (int j = 0; j < this.m_segments.Count; j++)
		{
			GameObject gameObject2 = this.m_segments[j];
			GameObject gameObject3 = (j == 0) ? this.m_segments[this.m_segments.Count - 1] : this.m_segments[j - 1];
			Vector3 normalized = (((j == this.m_segments.Count - 1) ? this.m_segments[0] : this.m_segments[j + 1]).transform.position - gameObject3.transform.position).normalized;
			gameObject2.transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
		}
	}

	// Token: 0x06000416 RID: 1046 RVA: 0x00021314 File Offset: 0x0001F514
	private void CreateSegments()
	{
		if (this.m_segments.Count == this.m_nrOfSegments)
		{
			return;
		}
		foreach (GameObject obj in this.m_segments)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_segments.Clear();
		for (int i = 0; i < this.m_nrOfSegments; i++)
		{
			GameObject item = UnityEngine.Object.Instantiate<GameObject>(this.m_prefab, base.transform.position, Quaternion.identity, base.transform);
			this.m_segments.Add(item);
		}
	}

	// Token: 0x04000406 RID: 1030
	public float m_radius = 5f;

	// Token: 0x04000407 RID: 1031
	public int m_nrOfSegments = 20;

	// Token: 0x04000408 RID: 1032
	public GameObject m_prefab;

	// Token: 0x04000409 RID: 1033
	public LayerMask m_mask;

	// Token: 0x0400040A RID: 1034
	private List<GameObject> m_segments = new List<GameObject>();
}
