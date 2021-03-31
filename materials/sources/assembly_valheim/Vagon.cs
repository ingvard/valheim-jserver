using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200010C RID: 268
public class Vagon : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000FD1 RID: 4049 RVA: 0x0006F6E0 File Offset: 0x0006D8E0
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			base.enabled = false;
			return;
		}
		Vagon.m_instances.Add(this);
		Heightmap.ForceGenerateAll();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_bodies = base.GetComponentsInChildren<Rigidbody>();
		this.m_lineRenderer = base.GetComponent<LineRenderer>();
		Rigidbody[] bodies = this.m_bodies;
		for (int i = 0; i < bodies.Length; i++)
		{
			bodies[i].maxDepenetrationVelocity = 2f;
		}
		this.m_nview.Register("RequestOwn", new Action<long>(this.RPC_RequestOwn));
		this.m_nview.Register("RequestDenied", new Action<long>(this.RPC_RequestDenied));
		base.InvokeRepeating("UpdateMass", 0f, 5f);
		base.InvokeRepeating("UpdateLoadVisualization", 0f, 3f);
	}

	// Token: 0x06000FD2 RID: 4050 RVA: 0x0006F7C6 File Offset: 0x0006D9C6
	private void OnDestroy()
	{
		Vagon.m_instances.Remove(this);
	}

	// Token: 0x06000FD3 RID: 4051 RVA: 0x0006F7D4 File Offset: 0x0006D9D4
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000FD4 RID: 4052 RVA: 0x0006F7DC File Offset: 0x0006D9DC
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] Use");
	}

	// Token: 0x06000FD5 RID: 4053 RVA: 0x0006F7F8 File Offset: 0x0006D9F8
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		this.m_useRequester = character;
		if (!this.m_nview.IsOwner())
		{
			this.m_nview.InvokeRPC("RequestOwn", Array.Empty<object>());
		}
		return false;
	}

	// Token: 0x06000FD6 RID: 4054 RVA: 0x0006F82C File Offset: 0x0006DA2C
	public void RPC_RequestOwn(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.InUse())
		{
			ZLog.Log("Requested use, but is already in use");
			this.m_nview.InvokeRPC(sender, "RequestDenied", Array.Empty<object>());
			return;
		}
		this.m_nview.GetZDO().SetOwner(sender);
	}

	// Token: 0x06000FD7 RID: 4055 RVA: 0x0006F881 File Offset: 0x0006DA81
	private void RPC_RequestDenied(long sender)
	{
		ZLog.Log("Got request denied");
		if (this.m_useRequester)
		{
			this.m_useRequester.Message(MessageHud.MessageType.Center, this.m_name + " is in use by someone else", 0, null);
			this.m_useRequester = null;
		}
	}

	// Token: 0x06000FD8 RID: 4056 RVA: 0x0006F8C0 File Offset: 0x0006DAC0
	private void FixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.UpdateAudio(Time.fixedDeltaTime);
		if (this.m_nview.IsOwner())
		{
			if (this.m_useRequester)
			{
				if (this.IsAttached())
				{
					this.Detach();
				}
				else if (this.CanAttach(this.m_useRequester.gameObject))
				{
					this.AttachTo(this.m_useRequester.gameObject);
				}
				else
				{
					this.m_useRequester.Message(MessageHud.MessageType.Center, "Not in the right position", 0, null);
				}
				this.m_useRequester = null;
			}
			if (this.IsAttached() && !this.CanAttach(this.m_attachJoin.connectedBody.gameObject))
			{
				this.Detach();
				return;
			}
		}
		else if (this.IsAttached())
		{
			this.Detach();
		}
	}

	// Token: 0x06000FD9 RID: 4057 RVA: 0x0006F988 File Offset: 0x0006DB88
	private void LateUpdate()
	{
		if (this.IsAttached())
		{
			this.m_lineRenderer.enabled = true;
			this.m_lineRenderer.SetPosition(0, this.m_lineAttachPoints0.position);
			this.m_lineRenderer.SetPosition(1, this.m_attachJoin.connectedBody.transform.position + this.m_lineAttachOffset);
			this.m_lineRenderer.SetPosition(2, this.m_lineAttachPoints1.position);
			return;
		}
		this.m_lineRenderer.enabled = false;
	}

	// Token: 0x06000FDA RID: 4058 RVA: 0x0006FA10 File Offset: 0x0006DC10
	public bool IsAttached(Character character)
	{
		return this.m_attachJoin && this.m_attachJoin.connectedBody.gameObject == character.gameObject;
	}

	// Token: 0x06000FDB RID: 4059 RVA: 0x0006FA3F File Offset: 0x0006DC3F
	public bool InUse()
	{
		return (this.m_container && this.m_container.IsInUse()) || this.IsAttached();
	}

	// Token: 0x06000FDC RID: 4060 RVA: 0x0006FA63 File Offset: 0x0006DC63
	private bool IsAttached()
	{
		return this.m_attachJoin != null;
	}

	// Token: 0x06000FDD RID: 4061 RVA: 0x0006FA74 File Offset: 0x0006DC74
	private bool CanAttach(GameObject go)
	{
		if (base.transform.up.y < 0.1f)
		{
			return false;
		}
		Humanoid component = go.GetComponent<Humanoid>();
		return (!component || (!component.InDodge() && !component.IsTeleporting())) && Vector3.Distance(go.transform.position + this.m_attachOffset, this.m_attachPoint.position) < this.m_detachDistance;
	}

	// Token: 0x06000FDE RID: 4062 RVA: 0x0006FAEC File Offset: 0x0006DCEC
	private void AttachTo(GameObject go)
	{
		Vagon.DetachAll();
		this.m_attachJoin = base.gameObject.AddComponent<ConfigurableJoint>();
		this.m_attachJoin.autoConfigureConnectedAnchor = false;
		this.m_attachJoin.anchor = this.m_attachPoint.localPosition;
		this.m_attachJoin.connectedAnchor = this.m_attachOffset;
		this.m_attachJoin.breakForce = this.m_breakForce;
		this.m_attachJoin.xMotion = ConfigurableJointMotion.Limited;
		this.m_attachJoin.yMotion = ConfigurableJointMotion.Limited;
		this.m_attachJoin.zMotion = ConfigurableJointMotion.Limited;
		SoftJointLimit linearLimit = default(SoftJointLimit);
		linearLimit.limit = 0.001f;
		this.m_attachJoin.linearLimit = linearLimit;
		SoftJointLimitSpring linearLimitSpring = default(SoftJointLimitSpring);
		linearLimitSpring.spring = this.m_spring;
		linearLimitSpring.damper = this.m_springDamping;
		this.m_attachJoin.linearLimitSpring = linearLimitSpring;
		this.m_attachJoin.zMotion = ConfigurableJointMotion.Locked;
		this.m_attachJoin.connectedBody = go.GetComponent<Rigidbody>();
	}

	// Token: 0x06000FDF RID: 4063 RVA: 0x0006FBE4 File Offset: 0x0006DDE4
	private static void DetachAll()
	{
		foreach (Vagon vagon in Vagon.m_instances)
		{
			vagon.Detach();
		}
	}

	// Token: 0x06000FE0 RID: 4064 RVA: 0x0006FC34 File Offset: 0x0006DE34
	private void Detach()
	{
		if (this.m_attachJoin)
		{
			UnityEngine.Object.Destroy(this.m_attachJoin);
			this.m_attachJoin = null;
			this.m_body.WakeUp();
			this.m_body.AddForce(0f, 1f, 0f);
		}
	}

	// Token: 0x06000FE1 RID: 4065 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000FE2 RID: 4066 RVA: 0x0006FC88 File Offset: 0x0006DE88
	private void UpdateMass()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_container == null)
		{
			return;
		}
		float totalWeight = this.m_container.GetInventory().GetTotalWeight();
		float mass = this.m_baseMass + totalWeight * this.m_itemWeightMassFactor;
		this.SetMass(mass);
	}

	// Token: 0x06000FE3 RID: 4067 RVA: 0x0006FCDC File Offset: 0x0006DEDC
	private void SetMass(float mass)
	{
		float mass2 = mass / (float)this.m_bodies.Length;
		Rigidbody[] bodies = this.m_bodies;
		for (int i = 0; i < bodies.Length; i++)
		{
			bodies[i].mass = mass2;
		}
	}

	// Token: 0x06000FE4 RID: 4068 RVA: 0x0006FD14 File Offset: 0x0006DF14
	private void UpdateLoadVisualization()
	{
		if (this.m_container == null)
		{
			return;
		}
		float num = this.m_container.GetInventory().SlotsUsedPercentage();
		foreach (Vagon.LoadData loadData in this.m_loadVis)
		{
			loadData.m_gameobject.SetActive(num >= loadData.m_minPercentage);
		}
	}

	// Token: 0x06000FE5 RID: 4069 RVA: 0x0006FD98 File Offset: 0x0006DF98
	private void UpdateAudio(float dt)
	{
		float num = 0f;
		foreach (Rigidbody rigidbody in this.m_wheels)
		{
			num += rigidbody.angularVelocity.magnitude;
		}
		num /= (float)this.m_wheels.Length;
		float target = Mathf.Lerp(this.m_minPitch, this.m_maxPitch, Mathf.Clamp01(num / this.m_maxPitchVel));
		float target2 = this.m_maxVol * Mathf.Clamp01(num / this.m_maxVolVel);
		foreach (AudioSource audioSource in this.m_wheelLoops)
		{
			audioSource.volume = Mathf.MoveTowards(audioSource.volume, target2, this.m_audioChangeSpeed * dt);
			audioSource.pitch = Mathf.MoveTowards(audioSource.pitch, target, this.m_audioChangeSpeed * dt);
		}
	}

	// Token: 0x04000EAA RID: 3754
	private static List<Vagon> m_instances = new List<Vagon>();

	// Token: 0x04000EAB RID: 3755
	public Transform m_attachPoint;

	// Token: 0x04000EAC RID: 3756
	public string m_name = "Wagon";

	// Token: 0x04000EAD RID: 3757
	public float m_detachDistance = 2f;

	// Token: 0x04000EAE RID: 3758
	public Vector3 m_attachOffset = new Vector3(0f, 0.8f, 0f);

	// Token: 0x04000EAF RID: 3759
	public Container m_container;

	// Token: 0x04000EB0 RID: 3760
	public Transform m_lineAttachPoints0;

	// Token: 0x04000EB1 RID: 3761
	public Transform m_lineAttachPoints1;

	// Token: 0x04000EB2 RID: 3762
	public Vector3 m_lineAttachOffset = new Vector3(0f, 1f, 0f);

	// Token: 0x04000EB3 RID: 3763
	public float m_breakForce = 10000f;

	// Token: 0x04000EB4 RID: 3764
	public float m_spring = 5000f;

	// Token: 0x04000EB5 RID: 3765
	public float m_springDamping = 1000f;

	// Token: 0x04000EB6 RID: 3766
	public float m_baseMass = 20f;

	// Token: 0x04000EB7 RID: 3767
	public float m_itemWeightMassFactor = 1f;

	// Token: 0x04000EB8 RID: 3768
	public AudioSource[] m_wheelLoops;

	// Token: 0x04000EB9 RID: 3769
	public float m_minPitch = 1f;

	// Token: 0x04000EBA RID: 3770
	public float m_maxPitch = 1.5f;

	// Token: 0x04000EBB RID: 3771
	public float m_maxPitchVel = 10f;

	// Token: 0x04000EBC RID: 3772
	public float m_maxVol = 1f;

	// Token: 0x04000EBD RID: 3773
	public float m_maxVolVel = 10f;

	// Token: 0x04000EBE RID: 3774
	public float m_audioChangeSpeed = 2f;

	// Token: 0x04000EBF RID: 3775
	public Rigidbody[] m_wheels = new Rigidbody[0];

	// Token: 0x04000EC0 RID: 3776
	public List<Vagon.LoadData> m_loadVis = new List<Vagon.LoadData>();

	// Token: 0x04000EC1 RID: 3777
	private ZNetView m_nview;

	// Token: 0x04000EC2 RID: 3778
	private ConfigurableJoint m_attachJoin;

	// Token: 0x04000EC3 RID: 3779
	private Rigidbody m_body;

	// Token: 0x04000EC4 RID: 3780
	private LineRenderer m_lineRenderer;

	// Token: 0x04000EC5 RID: 3781
	private Rigidbody[] m_bodies;

	// Token: 0x04000EC6 RID: 3782
	private Humanoid m_useRequester;

	// Token: 0x020001B1 RID: 433
	[Serializable]
	public class LoadData
	{
		// Token: 0x04001324 RID: 4900
		public GameObject m_gameobject;

		// Token: 0x04001325 RID: 4901
		public float m_minPercentage;
	}
}
