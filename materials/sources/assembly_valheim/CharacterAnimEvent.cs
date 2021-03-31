using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000003 RID: 3
public class CharacterAnimEvent : MonoBehaviour
{
	// Token: 0x060000C5 RID: 197 RVA: 0x00005EE4 File Offset: 0x000040E4
	private void Awake()
	{
		this.m_character = base.GetComponentInParent<Character>();
		this.m_nview = this.m_character.GetComponent<ZNetView>();
		this.m_animator = base.GetComponent<Animator>();
		this.m_monsterAI = this.m_character.GetComponent<MonsterAI>();
		this.m_visEquipment = this.m_character.GetComponent<VisEquipment>();
		this.m_footStep = this.m_character.GetComponent<FootStep>();
		this.m_head = this.m_animator.GetBoneTransform(HumanBodyBones.Head);
		this.m_chainID = Animator.StringToHash("chain");
		this.m_headLookDir = this.m_character.transform.forward;
		if (CharacterAnimEvent.m_ikGroundMask == 0)
		{
			CharacterAnimEvent.m_ikGroundMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"terrain",
				"vehicle"
			});
		}
	}

	// Token: 0x060000C6 RID: 198 RVA: 0x00005FCD File Offset: 0x000041CD
	private void OnAnimatorMove()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.m_character.AddRootMotion(this.m_animator.deltaPosition);
	}

	// Token: 0x060000C7 RID: 199 RVA: 0x00006000 File Offset: 0x00004200
	private void FixedUpdate()
	{
		if (this.m_character == null)
		{
			return;
		}
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (!this.m_character.InAttack() && !this.m_character.InMinorAction() && !this.m_character.InEmote() && this.m_character.CanMove())
		{
			this.m_animator.speed = 1f;
		}
		if (this.m_pauseTimer > 0f)
		{
			this.m_pauseTimer -= Time.fixedDeltaTime;
			if (this.m_pauseTimer <= 0f)
			{
				this.m_animator.speed = this.m_pauseSpeed;
			}
		}
	}

	// Token: 0x060000C8 RID: 200 RVA: 0x000060AB File Offset: 0x000042AB
	public bool CanChain()
	{
		return this.m_chain;
	}

	// Token: 0x060000C9 RID: 201 RVA: 0x000060B4 File Offset: 0x000042B4
	public void FreezeFrame(float delay)
	{
		if (delay <= 0f)
		{
			return;
		}
		if (this.m_pauseTimer > 0f)
		{
			this.m_pauseTimer = delay;
			return;
		}
		this.m_pauseTimer = delay;
		this.m_pauseSpeed = this.m_animator.speed;
		this.m_animator.speed = 0.0001f;
		if (this.m_pauseSpeed <= 0.01f)
		{
			this.m_pauseSpeed = 1f;
		}
	}

	// Token: 0x060000CA RID: 202 RVA: 0x0000611F File Offset: 0x0000431F
	public void Speed(float speedScale)
	{
		this.m_animator.speed = speedScale;
	}

	// Token: 0x060000CB RID: 203 RVA: 0x0000612D File Offset: 0x0000432D
	public void Chain()
	{
		this.m_chain = true;
	}

	// Token: 0x060000CC RID: 204 RVA: 0x00006136 File Offset: 0x00004336
	public void ResetChain()
	{
		this.m_chain = false;
	}

	// Token: 0x060000CD RID: 205 RVA: 0x00006140 File Offset: 0x00004340
	public void FootStep(AnimationEvent e)
	{
		if ((double)e.animatorClipInfo.weight < 0.33)
		{
			return;
		}
		if (this.m_footStep)
		{
			if (e.stringParameter.Length > 0)
			{
				this.m_footStep.OnFoot(e.stringParameter);
				return;
			}
			this.m_footStep.OnFoot();
		}
	}

	// Token: 0x060000CE RID: 206 RVA: 0x000061A0 File Offset: 0x000043A0
	public void Hit()
	{
		this.m_character.OnAttackTrigger();
	}

	// Token: 0x060000CF RID: 207 RVA: 0x000061A0 File Offset: 0x000043A0
	public void OnAttackTrigger()
	{
		this.m_character.OnAttackTrigger();
	}

	// Token: 0x060000D0 RID: 208 RVA: 0x000061AD File Offset: 0x000043AD
	public void Stop(AnimationEvent e)
	{
		this.m_character.OnStopMoving();
	}

	// Token: 0x060000D1 RID: 209 RVA: 0x000061BC File Offset: 0x000043BC
	public void DodgeMortal()
	{
		Player player = this.m_character as Player;
		if (player)
		{
			player.OnDodgeMortal();
		}
	}

	// Token: 0x060000D2 RID: 210 RVA: 0x000061E3 File Offset: 0x000043E3
	public void TrailOn()
	{
		if (this.m_visEquipment)
		{
			this.m_visEquipment.SetWeaponTrails(true);
		}
		this.m_character.OnWeaponTrailStart();
	}

	// Token: 0x060000D3 RID: 211 RVA: 0x00006209 File Offset: 0x00004409
	public void TrailOff()
	{
		if (this.m_visEquipment)
		{
			this.m_visEquipment.SetWeaponTrails(false);
		}
	}

	// Token: 0x060000D4 RID: 212 RVA: 0x00006224 File Offset: 0x00004424
	public void GPower()
	{
		Player player = this.m_character as Player;
		if (player)
		{
			player.ActivateGuardianPower();
		}
	}

	// Token: 0x060000D5 RID: 213 RVA: 0x0000624C File Offset: 0x0000444C
	private void OnAnimatorIK(int layerIndex)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.UpdateLookat();
		this.UpdateFootIK();
	}

	// Token: 0x060000D6 RID: 214 RVA: 0x00006268 File Offset: 0x00004468
	private void LateUpdate()
	{
		this.UpdateHeadRotation(Time.deltaTime);
		if (this.m_femaleHack)
		{
			Character character = this.m_character;
			float num = (this.m_visEquipment.GetModelIndex() == 1) ? this.m_femaleOffset : this.m_maleOffset;
			Vector3 localPosition = this.m_leftShoulder.localPosition;
			localPosition.x = -num;
			this.m_leftShoulder.localPosition = localPosition;
			Vector3 localPosition2 = this.m_rightShoulder.localPosition;
			localPosition2.x = num;
			this.m_rightShoulder.localPosition = localPosition2;
		}
	}

	// Token: 0x060000D7 RID: 215 RVA: 0x000062F0 File Offset: 0x000044F0
	private void UpdateLookat()
	{
		if (this.m_headRotation && this.m_head)
		{
			float target = this.m_lookWeight;
			if (this.m_headLookDir != Vector3.zero)
			{
				this.m_animator.SetLookAtPosition(this.m_head.position + this.m_headLookDir * 10f);
			}
			if (this.m_character.InAttack() || (!this.m_character.IsPlayer() && !this.m_character.CanMove()))
			{
				target = 0f;
			}
			this.m_lookAtWeight = Mathf.MoveTowards(this.m_lookAtWeight, target, Time.deltaTime);
			float bodyWeight = this.m_character.IsAttached() ? 0f : this.m_bodyLookWeight;
			this.m_animator.SetLookAtWeight(this.m_lookAtWeight, bodyWeight, this.m_headLookWeight, this.m_eyeLookWeight, this.m_lookClamp);
		}
	}

	// Token: 0x060000D8 RID: 216 RVA: 0x000063E0 File Offset: 0x000045E0
	private void UpdateFootIK()
	{
		if (!this.m_footIK)
		{
			return;
		}
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		if (Vector3.Distance(base.transform.position, mainCamera.transform.position) > 64f)
		{
			return;
		}
		if ((this.m_character.IsFlying() && !this.m_character.IsOnGround()) || (this.m_character.IsSwiming() && !this.m_character.IsOnGround()))
		{
			for (int i = 0; i < this.m_feets.Length; i++)
			{
				CharacterAnimEvent.Foot foot = this.m_feets[i];
				this.m_animator.SetIKPositionWeight(foot.m_ikHandle, 0f);
				this.m_animator.SetIKRotationWeight(foot.m_ikHandle, 0f);
			}
			return;
		}
		float deltaTime = Time.deltaTime;
		for (int j = 0; j < this.m_feets.Length; j++)
		{
			CharacterAnimEvent.Foot foot2 = this.m_feets[j];
			Vector3 position = foot2.m_transform.position;
			AvatarIKGoal ikHandle = foot2.m_ikHandle;
			float num = this.m_useFeetValues ? foot2.m_footDownMax : this.m_footDownMax;
			float d = this.m_useFeetValues ? foot2.m_footOffset : this.m_footOffset;
			float num2 = this.m_useFeetValues ? foot2.m_footStepHeight : this.m_footStepHeight;
			float num3 = this.m_useFeetValues ? foot2.m_stabalizeDistance : this.m_stabalizeDistance;
			Vector3 vector = base.transform.InverseTransformPoint(position - base.transform.up * d);
			float target = 1f - Mathf.Clamp01(vector.y / num);
			foot2.m_ikWeight = Mathf.MoveTowards(foot2.m_ikWeight, target, deltaTime * 10f);
			this.m_animator.SetIKPositionWeight(ikHandle, foot2.m_ikWeight);
			this.m_animator.SetIKRotationWeight(ikHandle, foot2.m_ikWeight * 0.5f);
			if (foot2.m_ikWeight > 0f)
			{
				RaycastHit raycastHit;
				if (Physics.Raycast(position + Vector3.up * num2, Vector3.down, out raycastHit, num2 * 4f, CharacterAnimEvent.m_ikGroundMask))
				{
					Vector3 vector2 = raycastHit.point + Vector3.up * d;
					Vector3 plantNormal = raycastHit.normal;
					if (num3 > 0f)
					{
						if (foot2.m_ikWeight >= 1f)
						{
							if (!foot2.m_isPlanted)
							{
								foot2.m_plantPosition = vector2;
								foot2.m_plantNormal = plantNormal;
								foot2.m_isPlanted = true;
							}
							else if (Vector3.Distance(foot2.m_plantPosition, vector2) > num3)
							{
								foot2.m_isPlanted = false;
							}
							else
							{
								vector2 = foot2.m_plantPosition;
								plantNormal = foot2.m_plantNormal;
							}
						}
						else
						{
							foot2.m_isPlanted = false;
						}
					}
					this.m_animator.SetIKPosition(ikHandle, vector2);
					Quaternion goalRotation = Quaternion.LookRotation(Vector3.Cross(this.m_animator.GetIKRotation(ikHandle) * Vector3.right, raycastHit.normal), raycastHit.normal);
					this.m_animator.SetIKRotation(ikHandle, goalRotation);
				}
				else
				{
					foot2.m_ikWeight = Mathf.MoveTowards(foot2.m_ikWeight, 0f, deltaTime * 4f);
					this.m_animator.SetIKPositionWeight(ikHandle, foot2.m_ikWeight);
					this.m_animator.SetIKRotationWeight(ikHandle, foot2.m_ikWeight * 0.5f);
				}
			}
		}
	}

	// Token: 0x060000D9 RID: 217 RVA: 0x00006750 File Offset: 0x00004950
	private void UpdateHeadRotation(float dt)
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_headRotation && this.m_head)
		{
			Vector3 lookFromPos = this.GetLookFromPos();
			Vector3 vector = Vector3.zero;
			if (this.m_nview.IsOwner())
			{
				if (this.m_monsterAI != null)
				{
					Character targetCreature = this.m_monsterAI.GetTargetCreature();
					if (targetCreature != null)
					{
						vector = targetCreature.GetEyePoint();
					}
				}
				else
				{
					vector = lookFromPos + this.m_character.GetLookDir() * 100f;
				}
				if (this.m_lookAt != null)
				{
					vector = this.m_lookAt.position;
				}
				this.m_sendTimer += Time.deltaTime;
				if (this.m_sendTimer > 0.2f)
				{
					this.m_sendTimer = 0f;
					this.m_nview.GetZDO().Set("LookTarget", vector);
				}
			}
			else
			{
				vector = this.m_nview.GetZDO().GetVec3("LookTarget", Vector3.zero);
			}
			if (vector != Vector3.zero)
			{
				Vector3 b = Vector3.Normalize(vector - lookFromPos);
				this.m_headLookDir = Vector3.Lerp(this.m_headLookDir, b, 0.1f);
				return;
			}
			this.m_headLookDir = this.m_character.transform.forward;
		}
	}

	// Token: 0x060000DA RID: 218 RVA: 0x000068B8 File Offset: 0x00004AB8
	private Vector3 GetLookFromPos()
	{
		if (this.m_eyes != null && this.m_eyes.Length != 0)
		{
			Vector3 a = Vector3.zero;
			foreach (Transform transform in this.m_eyes)
			{
				a += transform.position;
			}
			return a / (float)this.m_eyes.Length;
		}
		return this.m_head.position;
	}

	// Token: 0x060000DB RID: 219 RVA: 0x00006920 File Offset: 0x00004B20
	public void FindJoints()
	{
		ZLog.Log("Finding joints");
		List<Transform> list = new List<Transform>();
		Transform transform = Utils.FindChild(base.transform, "LeftEye");
		Transform transform2 = Utils.FindChild(base.transform, "RightEye");
		if (transform)
		{
			list.Add(transform);
		}
		if (transform2)
		{
			list.Add(transform2);
		}
		this.m_eyes = list.ToArray();
		Transform transform3 = Utils.FindChild(base.transform, "LeftFootFront");
		Transform transform4 = Utils.FindChild(base.transform, "RightFootFront");
		Transform transform5 = Utils.FindChild(base.transform, "LeftFoot");
		if (transform5 == null)
		{
			transform5 = Utils.FindChild(base.transform, "LeftFootBack");
		}
		if (transform5 == null)
		{
			transform5 = Utils.FindChild(base.transform, "l_foot");
		}
		if (transform5 == null)
		{
			transform5 = Utils.FindChild(base.transform, "Foot.l");
		}
		if (transform5 == null)
		{
			transform5 = Utils.FindChild(base.transform, "foot.l");
		}
		Transform transform6 = Utils.FindChild(base.transform, "RightFoot");
		if (transform6 == null)
		{
			transform6 = Utils.FindChild(base.transform, "RightFootBack");
		}
		if (transform6 == null)
		{
			transform6 = Utils.FindChild(base.transform, "r_foot");
		}
		if (transform6 == null)
		{
			transform6 = Utils.FindChild(base.transform, "Foot.r");
		}
		if (transform6 == null)
		{
			transform6 = Utils.FindChild(base.transform, "foot.r");
		}
		List<CharacterAnimEvent.Foot> list2 = new List<CharacterAnimEvent.Foot>();
		if (transform3)
		{
			list2.Add(new CharacterAnimEvent.Foot(transform3, AvatarIKGoal.LeftHand));
		}
		if (transform4)
		{
			list2.Add(new CharacterAnimEvent.Foot(transform4, AvatarIKGoal.RightHand));
		}
		if (transform5)
		{
			list2.Add(new CharacterAnimEvent.Foot(transform5, AvatarIKGoal.LeftFoot));
		}
		if (transform6)
		{
			list2.Add(new CharacterAnimEvent.Foot(transform6, AvatarIKGoal.RightFoot));
		}
		this.m_feets = list2.ToArray();
	}

	// Token: 0x0400008D RID: 141
	[Header("Foot IK")]
	public bool m_footIK;

	// Token: 0x0400008E RID: 142
	public float m_footDownMax = 0.4f;

	// Token: 0x0400008F RID: 143
	public float m_footOffset = 0.1f;

	// Token: 0x04000090 RID: 144
	public float m_footStepHeight = 1f;

	// Token: 0x04000091 RID: 145
	public float m_stabalizeDistance;

	// Token: 0x04000092 RID: 146
	public bool m_useFeetValues;

	// Token: 0x04000093 RID: 147
	public CharacterAnimEvent.Foot[] m_feets = new CharacterAnimEvent.Foot[0];

	// Token: 0x04000094 RID: 148
	[Header("Head/eye rotation")]
	public bool m_headRotation = true;

	// Token: 0x04000095 RID: 149
	public Transform[] m_eyes;

	// Token: 0x04000096 RID: 150
	public float m_lookWeight = 0.5f;

	// Token: 0x04000097 RID: 151
	public float m_bodyLookWeight = 0.1f;

	// Token: 0x04000098 RID: 152
	public float m_headLookWeight = 1f;

	// Token: 0x04000099 RID: 153
	public float m_eyeLookWeight;

	// Token: 0x0400009A RID: 154
	public float m_lookClamp = 0.5f;

	// Token: 0x0400009B RID: 155
	private const float m_headRotationSmoothness = 0.1f;

	// Token: 0x0400009C RID: 156
	public Transform m_lookAt;

	// Token: 0x0400009D RID: 157
	[Header("Player Female hack")]
	public bool m_femaleHack;

	// Token: 0x0400009E RID: 158
	public Transform m_leftShoulder;

	// Token: 0x0400009F RID: 159
	public Transform m_rightShoulder;

	// Token: 0x040000A0 RID: 160
	public float m_femaleOffset = 0.0004f;

	// Token: 0x040000A1 RID: 161
	public float m_maleOffset = 0.0007651657f;

	// Token: 0x040000A2 RID: 162
	private Character m_character;

	// Token: 0x040000A3 RID: 163
	private Animator m_animator;

	// Token: 0x040000A4 RID: 164
	private ZNetView m_nview;

	// Token: 0x040000A5 RID: 165
	private MonsterAI m_monsterAI;

	// Token: 0x040000A6 RID: 166
	private VisEquipment m_visEquipment;

	// Token: 0x040000A7 RID: 167
	private FootStep m_footStep;

	// Token: 0x040000A8 RID: 168
	private int m_chainID;

	// Token: 0x040000A9 RID: 169
	private float m_pauseTimer;

	// Token: 0x040000AA RID: 170
	private float m_pauseSpeed = 1f;

	// Token: 0x040000AB RID: 171
	private float m_sendTimer;

	// Token: 0x040000AC RID: 172
	private Vector3 m_headLookDir;

	// Token: 0x040000AD RID: 173
	private float m_lookAtWeight;

	// Token: 0x040000AE RID: 174
	private Transform m_head;

	// Token: 0x040000AF RID: 175
	private bool m_chain;

	// Token: 0x040000B0 RID: 176
	private static int m_ikGroundMask;

	// Token: 0x0200011D RID: 285
	[Serializable]
	public class Foot
	{
		// Token: 0x060010B3 RID: 4275 RVA: 0x00076AE8 File Offset: 0x00074CE8
		public Foot(Transform t, AvatarIKGoal handle)
		{
			this.m_transform = t;
			this.m_ikHandle = handle;
			this.m_ikWeight = 0f;
		}

		// Token: 0x04000FB8 RID: 4024
		public Transform m_transform;

		// Token: 0x04000FB9 RID: 4025
		public AvatarIKGoal m_ikHandle;

		// Token: 0x04000FBA RID: 4026
		public float m_footDownMax = 0.4f;

		// Token: 0x04000FBB RID: 4027
		public float m_footOffset = 0.1f;

		// Token: 0x04000FBC RID: 4028
		public float m_footStepHeight = 1f;

		// Token: 0x04000FBD RID: 4029
		public float m_stabalizeDistance;

		// Token: 0x04000FBE RID: 4030
		[NonSerialized]
		public float m_ikWeight;

		// Token: 0x04000FBF RID: 4031
		[NonSerialized]
		public Vector3 m_plantPosition = Vector3.zero;

		// Token: 0x04000FC0 RID: 4032
		[NonSerialized]
		public Vector3 m_plantNormal = Vector3.up;

		// Token: 0x04000FC1 RID: 4033
		[NonSerialized]
		public bool m_isPlanted;
	}
}
