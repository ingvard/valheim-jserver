using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000031 RID: 49
public class AnimationEffect : MonoBehaviour
{
	// Token: 0x060003FA RID: 1018 RVA: 0x00020B4A File Offset: 0x0001ED4A
	private void Start()
	{
		this.m_animator = base.GetComponent<Animator>();
	}

	// Token: 0x060003FB RID: 1019 RVA: 0x00020B58 File Offset: 0x0001ED58
	public void Effect(AnimationEvent e)
	{
		string stringParameter = e.stringParameter;
		GameObject original = e.objectReferenceParameter as GameObject;
		Transform transform = null;
		if (stringParameter.Length > 0)
		{
			transform = Utils.FindChild(base.transform, stringParameter);
		}
		if (transform == null)
		{
			transform = (this.m_effectRoot ? this.m_effectRoot : base.transform);
		}
		UnityEngine.Object.Instantiate<GameObject>(original, transform.position, transform.rotation);
	}

	// Token: 0x060003FC RID: 1020 RVA: 0x00020BC8 File Offset: 0x0001EDC8
	public void Attach(AnimationEvent e)
	{
		string stringParameter = e.stringParameter;
		GameObject original = e.objectReferenceParameter as GameObject;
		Transform transform = Utils.FindChild(base.transform, stringParameter);
		if (transform == null)
		{
			ZLog.LogWarning("Failed to find attach joint " + stringParameter);
			return;
		}
		this.ClearAttachment(transform);
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original, transform.position, transform.rotation);
		gameObject.transform.SetParent(transform, true);
		if (this.m_attachments == null)
		{
			this.m_attachments = new List<GameObject>();
		}
		this.m_attachments.Add(gameObject);
		this.m_attachStateHash = e.animatorStateInfo.fullPathHash;
		base.CancelInvoke("UpdateAttachments");
		base.InvokeRepeating("UpdateAttachments", 0.1f, 0.1f);
	}

	// Token: 0x060003FD RID: 1021 RVA: 0x00020C8C File Offset: 0x0001EE8C
	private void ClearAttachment(Transform parent)
	{
		if (this.m_attachments == null)
		{
			return;
		}
		foreach (GameObject gameObject in this.m_attachments)
		{
			if (gameObject && gameObject.transform.parent == parent)
			{
				this.m_attachments.Remove(gameObject);
				UnityEngine.Object.Destroy(gameObject);
				break;
			}
		}
	}

	// Token: 0x060003FE RID: 1022 RVA: 0x00020D14 File Offset: 0x0001EF14
	public void RemoveAttachments()
	{
		if (this.m_attachments == null)
		{
			return;
		}
		foreach (GameObject obj in this.m_attachments)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_attachments.Clear();
	}

	// Token: 0x060003FF RID: 1023 RVA: 0x00020D78 File Offset: 0x0001EF78
	private void UpdateAttachments()
	{
		if (this.m_attachments != null && this.m_attachments.Count > 0)
		{
			if (this.m_attachStateHash != this.m_animator.GetCurrentAnimatorStateInfo(0).fullPathHash && this.m_attachStateHash != this.m_animator.GetNextAnimatorStateInfo(0).fullPathHash)
			{
				this.RemoveAttachments();
				return;
			}
		}
		else
		{
			base.CancelInvoke("UpdateAttachments");
		}
	}

	// Token: 0x040003F3 RID: 1011
	public Transform m_effectRoot;

	// Token: 0x040003F4 RID: 1012
	private Animator m_animator;

	// Token: 0x040003F5 RID: 1013
	private List<GameObject> m_attachments;

	// Token: 0x040003F6 RID: 1014
	private int m_attachStateHash;
}
