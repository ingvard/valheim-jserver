using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x020000A0 RID: 160
public class InstanceRenderer : MonoBehaviour
{
	// Token: 0x06000AF8 RID: 2808 RVA: 0x0004F230 File Offset: 0x0004D430
	private void Update()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (this.m_instanceCount == 0 || mainCamera == null)
		{
			return;
		}
		if (this.m_frustumCull)
		{
			if (this.m_dirtyBounds)
			{
				this.UpdateBounds();
			}
			if (!Utils.InsideMainCamera(this.m_bounds))
			{
				return;
			}
		}
		if (this.m_useLod)
		{
			float num = this.m_useXZLodDistance ? Utils.DistanceXZ(mainCamera.transform.position, base.transform.position) : Vector3.Distance(mainCamera.transform.position, base.transform.position);
			int num2 = (int)((1f - Utils.LerpStep(this.m_lodMinDistance, this.m_lodMaxDistance, num)) * (float)this.m_instanceCount);
			float maxDelta = Time.deltaTime * (float)this.m_instanceCount;
			this.m_lodCount = Mathf.MoveTowards(this.m_lodCount, (float)num2, maxDelta);
			if (this.m_firstFrame)
			{
				if (num < this.m_lodMinDistance)
				{
					this.m_lodCount = (float)num2;
				}
				this.m_firstFrame = false;
			}
			this.m_lodCount = Mathf.Min(this.m_lodCount, (float)this.m_instanceCount);
			int num3 = (int)this.m_lodCount;
			if (num3 > 0)
			{
				Graphics.DrawMeshInstanced(this.m_mesh, 0, this.m_material, this.m_instances, num3, null, this.m_shadowCasting);
				return;
			}
		}
		else
		{
			Graphics.DrawMeshInstanced(this.m_mesh, 0, this.m_material, this.m_instances, this.m_instanceCount, null, this.m_shadowCasting);
		}
	}

	// Token: 0x06000AF9 RID: 2809 RVA: 0x0004F398 File Offset: 0x0004D598
	private void UpdateBounds()
	{
		this.m_dirtyBounds = false;
		Vector3 vector = new Vector3(9999999f, 9999999f, 9999999f);
		Vector3 vector2 = new Vector3(-9999999f, -9999999f, -9999999f);
		float magnitude = this.m_mesh.bounds.extents.magnitude;
		for (int i = 0; i < this.m_instanceCount; i++)
		{
			Matrix4x4 matrix4x = this.m_instances[i];
			Vector3 a = new Vector3(matrix4x[0, 3], matrix4x[1, 3], matrix4x[2, 3]);
			Vector3 lossyScale = matrix4x.lossyScale;
			float num = Mathf.Max(Mathf.Max(lossyScale.x, lossyScale.y), lossyScale.z);
			Vector3 b = new Vector3(num * magnitude, num * magnitude, num * magnitude);
			vector2 = Vector3.Max(vector2, a + b);
			vector = Vector3.Min(vector, a - b);
		}
		this.m_bounds.position = (vector2 + vector) * 0.5f;
		this.m_bounds.radius = Vector3.Distance(vector2, this.m_bounds.position);
	}

	// Token: 0x06000AFA RID: 2810 RVA: 0x0004F4D8 File Offset: 0x0004D6D8
	public void AddInstance(Vector3 pos, Quaternion rot, float scale)
	{
		Matrix4x4 m = Matrix4x4.TRS(pos, rot, this.m_scale * scale);
		this.AddInstance(m);
	}

	// Token: 0x06000AFB RID: 2811 RVA: 0x0004F500 File Offset: 0x0004D700
	public void AddInstance(Vector3 pos, Quaternion rot)
	{
		Matrix4x4 m = Matrix4x4.TRS(pos, rot, this.m_scale);
		this.AddInstance(m);
	}

	// Token: 0x06000AFC RID: 2812 RVA: 0x0004F522 File Offset: 0x0004D722
	public void AddInstance(Matrix4x4 m)
	{
		if (this.m_instanceCount >= 1023)
		{
			return;
		}
		this.m_instances[this.m_instanceCount] = m;
		this.m_instanceCount++;
		this.m_dirtyBounds = true;
	}

	// Token: 0x06000AFD RID: 2813 RVA: 0x0004F559 File Offset: 0x0004D759
	public void Clear()
	{
		this.m_instanceCount = 0;
		this.m_dirtyBounds = true;
	}

	// Token: 0x06000AFE RID: 2814 RVA: 0x0004F56C File Offset: 0x0004D76C
	public void SetInstance(int index, Vector3 pos, Quaternion rot, float scale)
	{
		Matrix4x4 matrix4x = Matrix4x4.TRS(pos, rot, this.m_scale * scale);
		this.m_instances[index] = matrix4x;
		this.m_dirtyBounds = true;
	}

	// Token: 0x06000AFF RID: 2815 RVA: 0x0004F5A2 File Offset: 0x0004D7A2
	private void Resize(int instances)
	{
		this.m_instanceCount = instances;
		this.m_dirtyBounds = true;
	}

	// Token: 0x06000B00 RID: 2816 RVA: 0x0004F5B4 File Offset: 0x0004D7B4
	public void SetInstances(List<Transform> transforms, bool faceCamera = false)
	{
		this.Resize(transforms.Count);
		for (int i = 0; i < transforms.Count; i++)
		{
			Transform transform = transforms[i];
			this.m_instances[i] = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
		}
		this.m_dirtyBounds = true;
	}

	// Token: 0x06000B01 RID: 2817 RVA: 0x0004F610 File Offset: 0x0004D810
	public void SetInstancesBillboard(List<Vector4> points)
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		Vector3 forward = -mainCamera.transform.forward;
		this.Resize(points.Count);
		for (int i = 0; i < points.Count; i++)
		{
			Vector4 vector = points[i];
			Vector3 pos = new Vector3(vector.x, vector.y, vector.z);
			float w = vector.w;
			Quaternion q = Quaternion.LookRotation(forward);
			this.m_instances[i] = Matrix4x4.TRS(pos, q, w * this.m_scale);
		}
		this.m_dirtyBounds = true;
	}

	// Token: 0x06000B02 RID: 2818 RVA: 0x000027E0 File Offset: 0x000009E0
	private void OnDrawGizmosSelected()
	{
	}

	// Token: 0x04000A5E RID: 2654
	public Mesh m_mesh;

	// Token: 0x04000A5F RID: 2655
	public Material m_material;

	// Token: 0x04000A60 RID: 2656
	public Vector3 m_scale = Vector3.one;

	// Token: 0x04000A61 RID: 2657
	public bool m_frustumCull = true;

	// Token: 0x04000A62 RID: 2658
	private bool m_dirtyBounds = true;

	// Token: 0x04000A63 RID: 2659
	private BoundingSphere m_bounds;

	// Token: 0x04000A64 RID: 2660
	public bool m_useLod;

	// Token: 0x04000A65 RID: 2661
	public bool m_useXZLodDistance = true;

	// Token: 0x04000A66 RID: 2662
	public float m_lodMinDistance = 5f;

	// Token: 0x04000A67 RID: 2663
	public float m_lodMaxDistance = 20f;

	// Token: 0x04000A68 RID: 2664
	public ShadowCastingMode m_shadowCasting;

	// Token: 0x04000A69 RID: 2665
	private float m_lodCount;

	// Token: 0x04000A6A RID: 2666
	private Matrix4x4[] m_instances = new Matrix4x4[1024];

	// Token: 0x04000A6B RID: 2667
	private int m_instanceCount;

	// Token: 0x04000A6C RID: 2668
	private bool m_firstFrame = true;
}
