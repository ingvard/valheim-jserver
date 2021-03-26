using System;
using UnityEngine;

// Token: 0x0200010D RID: 269
public class Valkyrie : MonoBehaviour
{
	// Token: 0x06000FE7 RID: 4071 RVA: 0x0006FDE4 File Offset: 0x0006DFE4
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_animator = base.GetComponentInChildren<Animator>();
		if (!this.m_nview.IsOwner())
		{
			base.enabled = false;
			return;
		}
		ZLog.Log("Setting up valkyrie ");
		float f = UnityEngine.Random.value * 3.1415927f * 2f;
		Vector3 vector = new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f));
		Vector3 a = Vector3.Cross(vector, Vector3.up);
		Player.m_localPlayer.SetIntro(true);
		this.m_targetPoint = Player.m_localPlayer.transform.position + new Vector3(0f, this.m_dropHeight, 0f);
		Vector3 position = this.m_targetPoint + vector * this.m_startDistance;
		position.y = this.m_startAltitude;
		base.transform.position = position;
		this.m_descentStart = this.m_targetPoint + vector * this.m_startDescentDistance + a * 200f;
		this.m_descentStart.y = this.m_descentAltitude;
		Vector3 a2 = this.m_targetPoint - this.m_descentStart;
		a2.y = 0f;
		a2.Normalize();
		this.m_flyAwayPoint = this.m_targetPoint + a2 * this.m_startDescentDistance;
		this.m_flyAwayPoint.y = this.m_startAltitude;
		this.ShowText();
		this.SyncPlayer(true);
		ZLog.Log(string.Concat(new object[]
		{
			"World pos ",
			base.transform.position,
			"   ",
			ZNet.instance.GetReferencePosition()
		}));
	}

	// Token: 0x06000FE8 RID: 4072 RVA: 0x0006FFAE File Offset: 0x0006E1AE
	private void ShowText()
	{
		TextViewer.instance.ShowText(TextViewer.Style.Intro, this.m_introTopic, this.m_introText, false);
	}

	// Token: 0x06000FE9 RID: 4073 RVA: 0x000027E0 File Offset: 0x000009E0
	private void HideText()
	{
	}

	// Token: 0x06000FEA RID: 4074 RVA: 0x0006FFC8 File Offset: 0x0006E1C8
	private void OnDestroy()
	{
		ZLog.Log("Destroying valkyrie");
	}

	// Token: 0x06000FEB RID: 4075 RVA: 0x0006FFD4 File Offset: 0x0006E1D4
	private void FixedUpdate()
	{
		this.UpdateValkyrie(Time.fixedDeltaTime);
		if (!this.m_droppedPlayer)
		{
			this.SyncPlayer(true);
		}
	}

	// Token: 0x06000FEC RID: 4076 RVA: 0x0006FFF0 File Offset: 0x0006E1F0
	private void LateUpdate()
	{
		if (!this.m_droppedPlayer)
		{
			this.SyncPlayer(false);
		}
	}

	// Token: 0x06000FED RID: 4077 RVA: 0x00070004 File Offset: 0x0006E204
	private void UpdateValkyrie(float dt)
	{
		this.m_timer += dt;
		if (this.m_timer < this.m_startPause)
		{
			return;
		}
		Vector3 vector;
		if (this.m_droppedPlayer)
		{
			vector = this.m_flyAwayPoint;
		}
		else if (this.m_descent)
		{
			vector = this.m_targetPoint;
		}
		else
		{
			vector = this.m_descentStart;
		}
		if (Utils.DistanceXZ(vector, base.transform.position) < 0.5f)
		{
			if (!this.m_descent)
			{
				this.m_descent = true;
				ZLog.Log("Starting descent");
			}
			else if (!this.m_droppedPlayer)
			{
				ZLog.Log("We are here");
				this.DropPlayer();
			}
			else
			{
				this.m_nview.Destroy();
			}
		}
		Vector3 normalized = (vector - base.transform.position).normalized;
		Vector3 vector2 = base.transform.position + normalized * 25f;
		float num;
		if (ZoneSystem.instance.GetGroundHeight(vector2, out num))
		{
			vector2.y = Mathf.Max(vector2.y, num + this.m_dropHeight);
		}
		Vector3 normalized2 = (vector2 - base.transform.position).normalized;
		Quaternion quaternion = Quaternion.LookRotation(normalized2);
		Vector3 to = normalized2;
		to.y = 0f;
		to.Normalize();
		Vector3 forward = base.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		float num2 = Mathf.Clamp(Vector3.SignedAngle(forward, to, Vector3.up), -30f, 30f) / 30f;
		quaternion = Quaternion.Euler(0f, 0f, num2 * 45f) * quaternion;
		float num3 = this.m_droppedPlayer ? (this.m_turnRate * 4f) : this.m_turnRate;
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, num3 * dt);
		Vector3 a = base.transform.forward * this.m_speed;
		Vector3 vector3 = base.transform.position + a * dt;
		float num4;
		if (ZoneSystem.instance.GetGroundHeight(vector3, out num4))
		{
			vector3.y = Mathf.Max(vector3.y, num4 + this.m_dropHeight);
		}
		base.transform.position = vector3;
	}

	// Token: 0x06000FEE RID: 4078 RVA: 0x00070258 File Offset: 0x0006E458
	private void DropPlayer()
	{
		ZLog.Log("We are here");
		this.m_droppedPlayer = true;
		Vector3 forward = base.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		Player.m_localPlayer.transform.rotation = Quaternion.LookRotation(forward);
		Player.m_localPlayer.SetIntro(false);
		this.m_animator.SetBool("dropped", true);
	}

	// Token: 0x06000FEF RID: 4079 RVA: 0x000702C8 File Offset: 0x0006E4C8
	private void SyncPlayer(bool doNetworkSync)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null)
		{
			ZLog.LogWarning("No local player");
			return;
		}
		localPlayer.transform.rotation = this.m_attachPoint.rotation;
		localPlayer.transform.position = this.m_attachPoint.position - localPlayer.transform.TransformVector(this.m_attachOffset);
		localPlayer.GetComponent<Rigidbody>().position = localPlayer.transform.position;
		if (doNetworkSync)
		{
			ZNet.instance.SetReferencePosition(localPlayer.transform.position);
			localPlayer.GetComponent<ZSyncTransform>().SyncNow();
			base.GetComponent<ZSyncTransform>().SyncNow();
		}
	}

	// Token: 0x04000EC1 RID: 3777
	public float m_startPause = 10f;

	// Token: 0x04000EC2 RID: 3778
	public float m_speed = 10f;

	// Token: 0x04000EC3 RID: 3779
	public float m_turnRate = 5f;

	// Token: 0x04000EC4 RID: 3780
	public float m_dropHeight = 10f;

	// Token: 0x04000EC5 RID: 3781
	public float m_startAltitude = 500f;

	// Token: 0x04000EC6 RID: 3782
	public float m_descentAltitude = 100f;

	// Token: 0x04000EC7 RID: 3783
	public float m_startDistance = 500f;

	// Token: 0x04000EC8 RID: 3784
	public float m_startDescentDistance = 200f;

	// Token: 0x04000EC9 RID: 3785
	public Vector3 m_attachOffset = new Vector3(0f, 0f, 1f);

	// Token: 0x04000ECA RID: 3786
	public float m_textDuration = 5f;

	// Token: 0x04000ECB RID: 3787
	public string m_introTopic = "";

	// Token: 0x04000ECC RID: 3788
	[TextArea]
	public string m_introText = "";

	// Token: 0x04000ECD RID: 3789
	public Transform m_attachPoint;

	// Token: 0x04000ECE RID: 3790
	private Vector3 m_targetPoint;

	// Token: 0x04000ECF RID: 3791
	private Vector3 m_descentStart;

	// Token: 0x04000ED0 RID: 3792
	private Vector3 m_flyAwayPoint;

	// Token: 0x04000ED1 RID: 3793
	private bool m_descent;

	// Token: 0x04000ED2 RID: 3794
	private bool m_droppedPlayer;

	// Token: 0x04000ED3 RID: 3795
	private Animator m_animator;

	// Token: 0x04000ED4 RID: 3796
	private ZNetView m_nview;

	// Token: 0x04000ED5 RID: 3797
	private float m_timer;
}
