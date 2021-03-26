using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200003F RID: 63
public class LineConnect : MonoBehaviour
{
	// Token: 0x0600043E RID: 1086 RVA: 0x000226F4 File Offset: 0x000208F4
	private void Awake()
	{
		this.m_lineRenderer = base.GetComponent<LineRenderer>();
		this.m_nview = base.GetComponentInParent<ZNetView>();
		this.m_linePeerID = ZDO.GetHashZDOID(this.m_netViewPrefix + "line_peer");
	}

	// Token: 0x0600043F RID: 1087 RVA: 0x0002272C File Offset: 0x0002092C
	private void LateUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			this.m_lineRenderer.enabled = false;
			return;
		}
		ZDOID zdoid = this.m_nview.GetZDO().GetZDOID(this.m_linePeerID);
		GameObject gameObject = ZNetScene.instance.FindInstance(zdoid);
		if (gameObject && !string.IsNullOrEmpty(this.m_childObject))
		{
			Transform transform = Utils.FindChild(gameObject.transform, this.m_childObject);
			if (transform)
			{
				gameObject = transform.gameObject;
			}
		}
		if (gameObject != null)
		{
			Vector3 endpoint = gameObject.transform.position;
			if (this.m_centerOfCharacter)
			{
				Character component = gameObject.GetComponent<Character>();
				if (component)
				{
					endpoint = component.GetCenterPoint();
				}
			}
			this.SetEndpoint(endpoint);
			this.m_lineRenderer.enabled = true;
			return;
		}
		if (this.m_hideIfNoConnection)
		{
			this.m_lineRenderer.enabled = false;
			return;
		}
		this.m_lineRenderer.enabled = true;
		this.SetEndpoint(base.transform.position + this.m_noConnectionWorldOffset);
	}

	// Token: 0x06000440 RID: 1088 RVA: 0x00022834 File Offset: 0x00020A34
	private void SetEndpoint(Vector3 pos)
	{
		Vector3 vector = base.transform.InverseTransformPoint(pos);
		Vector3 a = base.transform.InverseTransformDirection(Vector3.down);
		if (this.m_dynamicSlack)
		{
			Vector3 position = this.m_lineRenderer.GetPosition(0);
			Vector3 b = vector;
			float d = Vector3.Distance(position, b) / 2f;
			for (int i = 1; i < this.m_lineRenderer.positionCount; i++)
			{
				float num = (float)i / (float)(this.m_lineRenderer.positionCount - 1);
				float num2 = Mathf.Abs(0.5f - num) * 2f;
				num2 *= num2;
				num2 = 1f - num2;
				Vector3 vector2 = Vector3.Lerp(position, b, num);
				vector2 += a * d * this.m_slack * num2;
				this.m_lineRenderer.SetPosition(i, vector2);
			}
		}
		else
		{
			this.m_lineRenderer.SetPosition(1, vector);
		}
		if (this.m_dynamicThickness)
		{
			float v = Vector3.Distance(base.transform.position, pos);
			float num3 = Utils.LerpStep(this.m_minDistance, this.m_maxDistance, v);
			num3 = Mathf.Pow(num3, this.m_thicknessPower);
			this.m_lineRenderer.widthMultiplier = Mathf.Lerp(this.m_maxThickness, this.m_minThickness, num3);
		}
	}

	// Token: 0x06000441 RID: 1089 RVA: 0x00022988 File Offset: 0x00020B88
	public void SetPeer(ZNetView other)
	{
		if (other)
		{
			this.SetPeer(other.GetZDO().m_uid);
			return;
		}
		this.SetPeer(ZDOID.None);
	}

	// Token: 0x06000442 RID: 1090 RVA: 0x000229AF File Offset: 0x00020BAF
	public void SetPeer(ZDOID zdoid)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.GetZDO().Set(this.m_linePeerID, zdoid);
	}

	// Token: 0x0400045E RID: 1118
	public bool m_centerOfCharacter;

	// Token: 0x0400045F RID: 1119
	public string m_childObject = "";

	// Token: 0x04000460 RID: 1120
	public bool m_hideIfNoConnection = true;

	// Token: 0x04000461 RID: 1121
	public Vector3 m_noConnectionWorldOffset = new Vector3(0f, -1f, 0f);

	// Token: 0x04000462 RID: 1122
	[Header("Dynamic slack")]
	public bool m_dynamicSlack;

	// Token: 0x04000463 RID: 1123
	public float m_slack = 0.5f;

	// Token: 0x04000464 RID: 1124
	[Header("Thickness")]
	public bool m_dynamicThickness = true;

	// Token: 0x04000465 RID: 1125
	public float m_minDistance = 6f;

	// Token: 0x04000466 RID: 1126
	public float m_maxDistance = 30f;

	// Token: 0x04000467 RID: 1127
	public float m_minThickness = 0.2f;

	// Token: 0x04000468 RID: 1128
	public float m_maxThickness = 0.8f;

	// Token: 0x04000469 RID: 1129
	public float m_thicknessPower = 0.2f;

	// Token: 0x0400046A RID: 1130
	public string m_netViewPrefix = "";

	// Token: 0x0400046B RID: 1131
	private LineRenderer m_lineRenderer;

	// Token: 0x0400046C RID: 1132
	private ZNetView m_nview;

	// Token: 0x0400046D RID: 1133
	private KeyValuePair<int, int> m_linePeerID;
}
