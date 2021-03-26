﻿using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000008 RID: 8
public class FootStep : MonoBehaviour
{
	// Token: 0x060000F5 RID: 245 RVA: 0x00007500 File Offset: 0x00005700
	private void Start()
	{
		this.m_animator = base.GetComponentInChildren<Animator>();
		this.m_character = base.GetComponent<Character>();
		this.m_nview = base.GetComponent<ZNetView>();
		if (FootStep.m_footstepID == 0)
		{
			FootStep.m_footstepID = Animator.StringToHash("footstep");
			FootStep.m_forwardSpeedID = Animator.StringToHash("forward_speed");
			FootStep.m_sidewaySpeedID = Animator.StringToHash("sideway_speed");
		}
		this.m_footstep = this.m_animator.GetFloat(FootStep.m_footstepID);
		if (this.m_pieceLayer == 0)
		{
			this.m_pieceLayer = LayerMask.NameToLayer("piece");
		}
		Character character = this.m_character;
		character.m_onLand = (Action<Vector3>)Delegate.Combine(character.m_onLand, new Action<Vector3>(this.OnLand));
		if (this.m_nview.IsValid())
		{
			this.m_nview.Register<int, Vector3>("Step", new Action<long, int, Vector3>(this.RPC_Step));
		}
	}

	// Token: 0x060000F6 RID: 246 RVA: 0x000075E3 File Offset: 0x000057E3
	private void Update()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.UpdateFootstep(Time.deltaTime);
	}

	// Token: 0x060000F7 RID: 247 RVA: 0x0000760C File Offset: 0x0000580C
	private void UpdateFootstep(float dt)
	{
		if (this.m_feet.Length == 0)
		{
			return;
		}
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		if (Vector3.Distance(base.transform.position, mainCamera.transform.position) > this.m_footstepCullDistance)
		{
			return;
		}
		this.m_footstepTimer += dt;
		float @float = this.m_animator.GetFloat(FootStep.m_footstepID);
		if (Mathf.Sign(@float) != Mathf.Sign(this.m_footstep) && Mathf.Max(Mathf.Abs(this.m_animator.GetFloat(FootStep.m_forwardSpeedID)), Mathf.Abs(this.m_animator.GetFloat(FootStep.m_sidewaySpeedID))) > 0.2f && this.m_footstepTimer > 0.2f)
		{
			this.m_footstepTimer = 0f;
			this.OnFoot();
		}
		this.m_footstep = @float;
	}

	// Token: 0x060000F8 RID: 248 RVA: 0x000076E4 File Offset: 0x000058E4
	private Transform FindActiveFoot()
	{
		Transform transform = null;
		float num = 9999f;
		Vector3 forward = base.transform.forward;
		foreach (Transform transform2 in this.m_feet)
		{
			Vector3 rhs = transform2.position - base.transform.position;
			float num2 = Vector3.Dot(forward, rhs);
			if (num2 > num || transform == null)
			{
				transform = transform2;
				num = num2;
			}
		}
		return transform;
	}

	// Token: 0x060000F9 RID: 249 RVA: 0x0000775C File Offset: 0x0000595C
	private Transform FindFoot(string name)
	{
		foreach (Transform transform in this.m_feet)
		{
			if (transform.gameObject.name == name)
			{
				return transform;
			}
		}
		return null;
	}

	// Token: 0x060000FA RID: 250 RVA: 0x00007798 File Offset: 0x00005998
	public void OnFoot()
	{
		Transform transform = this.FindActiveFoot();
		if (transform == null)
		{
			return;
		}
		this.OnFoot(transform);
	}

	// Token: 0x060000FB RID: 251 RVA: 0x000077C0 File Offset: 0x000059C0
	public void OnFoot(string name)
	{
		Transform transform = this.FindFoot(name);
		if (transform == null)
		{
			ZLog.LogWarning("FAiled to find foot:" + name);
			return;
		}
		this.OnFoot(transform);
	}

	// Token: 0x060000FC RID: 252 RVA: 0x000077F8 File Offset: 0x000059F8
	private void OnLand(Vector3 point)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		FootStep.GroundMaterial groundMaterial = this.GetGroundMaterial(this.m_character, point);
		int num = this.FindBestStepEffect(groundMaterial, FootStep.MotionType.Land);
		if (num != -1)
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "Step", new object[]
			{
				num,
				point
			});
		}
	}

	// Token: 0x060000FD RID: 253 RVA: 0x0000785C File Offset: 0x00005A5C
	private void OnFoot(Transform foot)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		Vector3 vector = (foot != null) ? foot.position : base.transform.position;
		FootStep.MotionType motionType = this.GetMotionType(this.m_character);
		FootStep.GroundMaterial groundMaterial = this.GetGroundMaterial(this.m_character, vector);
		int num = this.FindBestStepEffect(groundMaterial, motionType);
		if (num != -1)
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "Step", new object[]
			{
				num,
				vector
			});
		}
	}

	// Token: 0x060000FE RID: 254 RVA: 0x000078EC File Offset: 0x00005AEC
	private static void PurgeOldEffects()
	{
		while (FootStep.m_stepInstances.Count > 30)
		{
			GameObject gameObject = FootStep.m_stepInstances.Dequeue();
			if (gameObject)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
	}

	// Token: 0x060000FF RID: 255 RVA: 0x00007924 File Offset: 0x00005B24
	private void DoEffect(FootStep.StepEffect effect, Vector3 point)
	{
		foreach (GameObject gameObject in effect.m_effectPrefabs)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, point, base.transform.rotation);
			FootStep.m_stepInstances.Enqueue(gameObject2);
			if (gameObject2.GetComponent<ZNetView>() != null)
			{
				ZLog.LogWarning(string.Concat(new string[]
				{
					"Foot step effect ",
					effect.m_name,
					" prefab ",
					gameObject.name,
					" in ",
					this.m_character.gameObject.name,
					" should not contain a ZNetView component"
				}));
			}
		}
		FootStep.PurgeOldEffects();
	}

	// Token: 0x06000100 RID: 256 RVA: 0x000079D8 File Offset: 0x00005BD8
	private void RPC_Step(long sender, int effectIndex, Vector3 point)
	{
		FootStep.StepEffect effect = this.m_effects[effectIndex];
		this.DoEffect(effect, point);
	}

	// Token: 0x06000101 RID: 257 RVA: 0x000079FA File Offset: 0x00005BFA
	private FootStep.MotionType GetMotionType(Character character)
	{
		if (this.m_character.IsSwiming())
		{
			return FootStep.MotionType.Swiming;
		}
		if (this.m_character.IsWallRunning())
		{
			return FootStep.MotionType.Climbing;
		}
		if (this.m_character.IsRunning())
		{
			return FootStep.MotionType.Run;
		}
		if (this.m_character.IsSneaking())
		{
			return FootStep.MotionType.Sneak;
		}
		return FootStep.MotionType.Walk;
	}

	// Token: 0x06000102 RID: 258 RVA: 0x00007A3C File Offset: 0x00005C3C
	private FootStep.GroundMaterial GetGroundMaterial(Character character, Vector3 point)
	{
		if (character.InWater())
		{
			return FootStep.GroundMaterial.Water;
		}
		if (!character.IsOnGround())
		{
			return FootStep.GroundMaterial.None;
		}
		float num = Mathf.Acos(Mathf.Clamp01(character.GetLastGroundNormal().y)) * 57.29578f;
		Collider lastGroundCollider = character.GetLastGroundCollider();
		if (lastGroundCollider)
		{
			Heightmap component = lastGroundCollider.GetComponent<Heightmap>();
			if (component != null)
			{
				Heightmap.Biome biome = component.GetBiome(point);
				if (biome == Heightmap.Biome.Mountain || biome == Heightmap.Biome.DeepNorth)
				{
					if (num < 40f && !component.IsCleared(point))
					{
						return FootStep.GroundMaterial.Snow;
					}
				}
				else if (biome == Heightmap.Biome.Swamp)
				{
					if (num < 40f)
					{
						return FootStep.GroundMaterial.Mud;
					}
				}
				else if ((biome == Heightmap.Biome.Meadows || biome == Heightmap.Biome.BlackForest) && num < 25f)
				{
					return FootStep.GroundMaterial.Grass;
				}
				return FootStep.GroundMaterial.GenericGround;
			}
			if (lastGroundCollider.gameObject.layer == this.m_pieceLayer)
			{
				WearNTear componentInParent = lastGroundCollider.GetComponentInParent<WearNTear>();
				if (componentInParent)
				{
					switch (componentInParent.m_materialType)
					{
					case WearNTear.MaterialType.Wood:
						return FootStep.GroundMaterial.Wood;
					case WearNTear.MaterialType.Stone:
						return FootStep.GroundMaterial.Stone;
					case WearNTear.MaterialType.Iron:
						return FootStep.GroundMaterial.Metal;
					case WearNTear.MaterialType.HardWood:
						return FootStep.GroundMaterial.Wood;
					}
				}
			}
		}
		return FootStep.GroundMaterial.Default;
	}

	// Token: 0x06000103 RID: 259 RVA: 0x00007B3C File Offset: 0x00005D3C
	public void FindJoints()
	{
		ZLog.Log("Finding joints");
		Transform transform = Utils.FindChild(base.transform, "LeftFootFront");
		Transform transform2 = Utils.FindChild(base.transform, "RightFootFront");
		Transform transform3 = Utils.FindChild(base.transform, "LeftFoot");
		if (transform3 == null)
		{
			transform3 = Utils.FindChild(base.transform, "LeftFootBack");
		}
		if (transform3 == null)
		{
			transform3 = Utils.FindChild(base.transform, "l_foot");
		}
		if (transform3 == null)
		{
			transform3 = Utils.FindChild(base.transform, "Foot.l");
		}
		if (transform3 == null)
		{
			transform3 = Utils.FindChild(base.transform, "foot.l");
		}
		Transform transform4 = Utils.FindChild(base.transform, "RightFoot");
		if (transform4 == null)
		{
			transform4 = Utils.FindChild(base.transform, "RightFootBack");
		}
		if (transform4 == null)
		{
			transform4 = Utils.FindChild(base.transform, "r_foot");
		}
		if (transform4 == null)
		{
			transform4 = Utils.FindChild(base.transform, "Foot.r");
		}
		if (transform4 == null)
		{
			transform4 = Utils.FindChild(base.transform, "foot.r");
		}
		List<Transform> list = new List<Transform>();
		if (transform)
		{
			list.Add(transform);
		}
		if (transform2)
		{
			list.Add(transform2);
		}
		if (transform3)
		{
			list.Add(transform3);
		}
		if (transform4)
		{
			list.Add(transform4);
		}
		this.m_feet = list.ToArray();
	}

	// Token: 0x06000104 RID: 260 RVA: 0x00007CBC File Offset: 0x00005EBC
	private int FindBestStepEffect(FootStep.GroundMaterial material, FootStep.MotionType motion)
	{
		FootStep.StepEffect stepEffect = null;
		int result = -1;
		for (int i = 0; i < this.m_effects.Count; i++)
		{
			FootStep.StepEffect stepEffect2 = this.m_effects[i];
			if (((stepEffect2.m_material & material) != FootStep.GroundMaterial.None || (stepEffect == null && (stepEffect2.m_material & FootStep.GroundMaterial.Default) != FootStep.GroundMaterial.None)) && (stepEffect2.m_motionType & motion) != (FootStep.MotionType)0)
			{
				stepEffect = stepEffect2;
				result = i;
			}
		}
		return result;
	}

	// Token: 0x040000D1 RID: 209
	private static Queue<GameObject> m_stepInstances = new Queue<GameObject>();

	// Token: 0x040000D2 RID: 210
	private const int m_maxFootstepInstances = 30;

	// Token: 0x040000D3 RID: 211
	public float m_footstepCullDistance = 20f;

	// Token: 0x040000D4 RID: 212
	public List<FootStep.StepEffect> m_effects = new List<FootStep.StepEffect>();

	// Token: 0x040000D5 RID: 213
	public Transform[] m_feet = new Transform[0];

	// Token: 0x040000D6 RID: 214
	private static int m_footstepID = 0;

	// Token: 0x040000D7 RID: 215
	private static int m_forwardSpeedID = 0;

	// Token: 0x040000D8 RID: 216
	private static int m_sidewaySpeedID = 0;

	// Token: 0x040000D9 RID: 217
	private float m_footstep;

	// Token: 0x040000DA RID: 218
	private float m_footstepTimer;

	// Token: 0x040000DB RID: 219
	private const float m_minFootstepInterval = 0.2f;

	// Token: 0x040000DC RID: 220
	private int m_pieceLayer;

	// Token: 0x040000DD RID: 221
	private Animator m_animator;

	// Token: 0x040000DE RID: 222
	private Character m_character;

	// Token: 0x040000DF RID: 223
	private ZNetView m_nview;

	// Token: 0x0200011F RID: 287
	public enum MotionType
	{
		// Token: 0x04000FC3 RID: 4035
		Walk = 1,
		// Token: 0x04000FC4 RID: 4036
		Run,
		// Token: 0x04000FC5 RID: 4037
		Sneak = 4,
		// Token: 0x04000FC6 RID: 4038
		Climbing = 8,
		// Token: 0x04000FC7 RID: 4039
		Swiming = 16,
		// Token: 0x04000FC8 RID: 4040
		Land = 32
	}

	// Token: 0x02000120 RID: 288
	public enum GroundMaterial
	{
		// Token: 0x04000FCA RID: 4042
		None,
		// Token: 0x04000FCB RID: 4043
		Default,
		// Token: 0x04000FCC RID: 4044
		Water,
		// Token: 0x04000FCD RID: 4045
		Stone = 4,
		// Token: 0x04000FCE RID: 4046
		Wood = 8,
		// Token: 0x04000FCF RID: 4047
		Snow = 16,
		// Token: 0x04000FD0 RID: 4048
		Mud = 32,
		// Token: 0x04000FD1 RID: 4049
		Grass = 64,
		// Token: 0x04000FD2 RID: 4050
		GenericGround = 128,
		// Token: 0x04000FD3 RID: 4051
		Metal = 256
	}

	// Token: 0x02000121 RID: 289
	[Serializable]
	public class StepEffect
	{
		// Token: 0x04000FD4 RID: 4052
		public string m_name = "";

		// Token: 0x04000FD5 RID: 4053
		[BitMask(typeof(FootStep.MotionType))]
		public FootStep.MotionType m_motionType = FootStep.MotionType.Walk;

		// Token: 0x04000FD6 RID: 4054
		[BitMask(typeof(FootStep.GroundMaterial))]
		public FootStep.GroundMaterial m_material = FootStep.GroundMaterial.Default;

		// Token: 0x04000FD7 RID: 4055
		public GameObject[] m_effectPrefabs = new GameObject[0];
	}
}
