using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x0200000E RID: 14
public class Player : Humanoid
{
	// Token: 0x06000159 RID: 345 RVA: 0x0000A868 File Offset: 0x00008A68
	protected override void Awake()
	{
		base.Awake();
		Player.m_players.Add(this);
		this.m_skills = base.GetComponent<Skills>();
		this.SetupAwake();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_placeRayMask = LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"Default_small",
			"piece",
			"piece_nonsolid",
			"terrain",
			"vehicle"
		});
		this.m_placeWaterRayMask = LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"Default_small",
			"piece",
			"piece_nonsolid",
			"terrain",
			"Water",
			"vehicle"
		});
		this.m_removeRayMask = LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"Default_small",
			"piece",
			"piece_nonsolid",
			"terrain",
			"vehicle"
		});
		this.m_interactMask = LayerMask.GetMask(new string[]
		{
			"item",
			"piece",
			"piece_nonsolid",
			"Default",
			"static_solid",
			"Default_small",
			"character",
			"character_net",
			"terrain",
			"vehicle"
		});
		this.m_autoPickupMask = LayerMask.GetMask(new string[]
		{
			"item"
		});
		Inventory inventory = this.m_inventory;
		inventory.m_onChanged = (Action)Delegate.Combine(inventory.m_onChanged, new Action(this.OnInventoryChanged));
		if (Player.m_attackMask == 0)
		{
			Player.m_attackMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"piece_nonsolid",
				"terrain",
				"character",
				"character_net",
				"character_ghost",
				"hitbox",
				"character_noenv",
				"vehicle"
			});
		}
		if (Player.crouching == 0)
		{
			Player.crouching = ZSyncAnimation.GetHash("crouching");
		}
		this.m_nview.Register("OnDeath", new Action<long>(this.RPC_OnDeath));
		if (this.m_nview.IsOwner())
		{
			this.m_nview.Register<int, string, int>("Message", new Action<long, int, string, int>(this.RPC_Message));
			this.m_nview.Register<bool, bool>("OnTargeted", new Action<long, bool, bool>(this.RPC_OnTargeted));
			this.m_nview.Register<float>("UseStamina", new Action<long, float>(this.RPC_UseStamina));
			if (MusicMan.instance)
			{
				MusicMan.instance.TriggerMusic("Wakeup");
			}
			this.UpdateKnownRecipesList();
			this.UpdateAvailablePiecesList();
			this.SetupPlacementGhost();
		}
	}

	// Token: 0x0600015A RID: 346 RVA: 0x0000AB6A File Offset: 0x00008D6A
	public void SetLocalPlayer()
	{
		if (Player.m_localPlayer == this)
		{
			return;
		}
		Player.m_localPlayer = this;
		ZNet.instance.SetReferencePosition(base.transform.position);
		EnvMan.instance.SetForceEnvironment("");
	}

	// Token: 0x0600015B RID: 347 RVA: 0x0000ABA4 File Offset: 0x00008DA4
	public void SetPlayerID(long playerID, string name)
	{
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		if (this.GetPlayerID() != 0L)
		{
			return;
		}
		this.m_nview.GetZDO().Set("playerID", playerID);
		this.m_nview.GetZDO().Set("playerName", name);
	}

	// Token: 0x0600015C RID: 348 RVA: 0x0000ABF4 File Offset: 0x00008DF4
	public long GetPlayerID()
	{
		if (this.m_nview.IsValid())
		{
			return this.m_nview.GetZDO().GetLong("playerID", 0L);
		}
		return 0L;
	}

	// Token: 0x0600015D RID: 349 RVA: 0x0000AC1D File Offset: 0x00008E1D
	public string GetPlayerName()
	{
		if (this.m_nview.IsValid())
		{
			return this.m_nview.GetZDO().GetString("playerName", "...");
		}
		return "";
	}

	// Token: 0x0600015E RID: 350 RVA: 0x0000AC4C File Offset: 0x00008E4C
	public override string GetHoverText()
	{
		return "";
	}

	// Token: 0x0600015F RID: 351 RVA: 0x0000AC53 File Offset: 0x00008E53
	public override string GetHoverName()
	{
		return this.GetPlayerName();
	}

	// Token: 0x06000160 RID: 352 RVA: 0x0000AC5B File Offset: 0x00008E5B
	protected override void Start()
	{
		base.Start();
		this.m_nview.GetZDO();
	}

	// Token: 0x06000161 RID: 353 RVA: 0x0000AC70 File Offset: 0x00008E70
	public override void OnDestroy()
	{
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo != null && ZNet.instance != null)
		{
			ZLog.LogWarning(string.Concat(new object[]
			{
				"Player destroyed sec:",
				zdo.GetSector(),
				"  pos:",
				base.transform.position,
				"  zdopos:",
				zdo.GetPosition(),
				"  ref ",
				ZNet.instance.GetReferencePosition()
			}));
		}
		if (this.m_placementGhost)
		{
			UnityEngine.Object.Destroy(this.m_placementGhost);
			this.m_placementGhost = null;
		}
		base.OnDestroy();
		Player.m_players.Remove(this);
		if (Player.m_localPlayer == this)
		{
			ZLog.LogWarning("Local player destroyed");
			Player.m_localPlayer = null;
		}
	}

	// Token: 0x06000162 RID: 354 RVA: 0x0000AD58 File Offset: 0x00008F58
	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		float fixedDeltaTime = Time.fixedDeltaTime;
		this.UpdateAwake(fixedDeltaTime);
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.UpdateTargeted(fixedDeltaTime);
		if (this.m_nview.IsOwner())
		{
			if (Player.m_localPlayer != this)
			{
				ZLog.Log("Destroying old local player");
				ZNetScene.instance.Destroy(base.gameObject);
				return;
			}
			if (this.IsDead())
			{
				return;
			}
			this.UpdateEquipQueue(fixedDeltaTime);
			this.PlayerAttackInput(fixedDeltaTime);
			this.UpdateAttach();
			this.UpdateShipControl(fixedDeltaTime);
			this.UpdateCrouch(fixedDeltaTime);
			this.UpdateDodge(fixedDeltaTime);
			this.UpdateCover(fixedDeltaTime);
			this.UpdateStations(fixedDeltaTime);
			this.UpdateGuardianPower(fixedDeltaTime);
			this.UpdateBaseValue(fixedDeltaTime);
			this.UpdateStats(fixedDeltaTime);
			this.UpdateTeleport(fixedDeltaTime);
			this.AutoPickup(fixedDeltaTime);
			this.EdgeOfWorldKill(fixedDeltaTime);
			this.UpdateBiome(fixedDeltaTime);
			this.UpdateStealth(fixedDeltaTime);
			if (GameCamera.instance && Vector3.Distance(GameCamera.instance.transform.position, base.transform.position) < 2f)
			{
				base.SetVisible(false);
			}
			AudioMan.instance.SetIndoor(this.InShelter());
		}
	}

	// Token: 0x06000163 RID: 355 RVA: 0x0000AE88 File Offset: 0x00009088
	private void Update()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		bool flag = this.TakeInput();
		this.UpdateHover();
		if (flag)
		{
			if (Player.m_debugMode && global::Console.instance.IsCheatsEnabled())
			{
				if (Input.GetKeyDown(KeyCode.Z))
				{
					this.m_debugFly = !this.m_debugFly;
					this.m_nview.GetZDO().Set("DebugFly", this.m_debugFly);
					this.Message(MessageHud.MessageType.TopLeft, "Debug fly:" + this.m_debugFly.ToString(), 0, null);
				}
				if (Input.GetKeyDown(KeyCode.B))
				{
					this.m_noPlacementCost = !this.m_noPlacementCost;
					this.Message(MessageHud.MessageType.TopLeft, "No placement cost:" + this.m_noPlacementCost.ToString(), 0, null);
					this.UpdateAvailablePiecesList();
				}
				if (Input.GetKeyDown(KeyCode.K))
				{
					int num = 0;
					foreach (Character character in Character.GetAllCharacters())
					{
						if (!character.IsPlayer())
						{
							HitData hitData = new HitData();
							hitData.m_damage.m_damage = 99999f;
							character.Damage(hitData);
							num++;
						}
					}
					this.Message(MessageHud.MessageType.TopLeft, "Killing all the monsters:" + num, 0, null);
				}
			}
			if (ZInput.GetButtonDown("Use") || ZInput.GetButtonDown("JoyUse"))
			{
				if (this.m_hovering)
				{
					this.Interact(this.m_hovering, false);
				}
				else if (this.m_shipControl)
				{
					this.StopShipControl();
				}
			}
			else if ((ZInput.GetButton("Use") || ZInput.GetButton("JoyUse")) && this.m_hovering)
			{
				this.Interact(this.m_hovering, true);
			}
			if (ZInput.GetButtonDown("Hide") || ZInput.GetButtonDown("JoyHide"))
			{
				if (base.GetRightItem() != null || base.GetLeftItem() != null)
				{
					if (!this.InAttack())
					{
						base.HideHandItems();
					}
				}
				else if (!base.IsSwiming() || base.IsOnGround())
				{
					base.ShowHandItems();
				}
			}
			if (ZInput.GetButtonDown("ToggleWalk"))
			{
				base.SetWalk(!base.GetWalk());
				if (base.GetWalk())
				{
					this.Message(MessageHud.MessageType.TopLeft, "$msg_walk 1", 0, null);
				}
				else
				{
					this.Message(MessageHud.MessageType.TopLeft, "$msg_walk 0", 0, null);
				}
			}
			if (ZInput.GetButtonDown("Sit") || (!this.InPlaceMode() && ZInput.GetButtonDown("JoySit")))
			{
				if (this.InEmote() && base.IsSitting())
				{
					this.StopEmote();
				}
				else
				{
					this.StartEmote("sit", false);
				}
			}
			if (ZInput.GetButtonDown("GPower") || ZInput.GetButtonDown("JoyGPower"))
			{
				this.StartGuardianPower();
			}
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				this.UseHotbarItem(1);
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				this.UseHotbarItem(2);
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				this.UseHotbarItem(3);
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				this.UseHotbarItem(4);
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				this.UseHotbarItem(5);
			}
			if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				this.UseHotbarItem(6);
			}
			if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				this.UseHotbarItem(7);
			}
			if (Input.GetKeyDown(KeyCode.Alpha8))
			{
				this.UseHotbarItem(8);
			}
		}
		this.UpdatePlacement(flag, Time.deltaTime);
	}

	// Token: 0x06000164 RID: 356 RVA: 0x0000B1F4 File Offset: 0x000093F4
	private void UpdatePlacement(bool takeInput, float dt)
	{
		this.UpdateWearNTearHover();
		if (!this.InPlaceMode())
		{
			if (this.m_placementGhost)
			{
				this.m_placementGhost.SetActive(false);
			}
			return;
		}
		if (!takeInput)
		{
			return;
		}
		this.UpdateBuildGuiInput();
		if (Hud.IsPieceSelectionVisible())
		{
			return;
		}
		ItemDrop.ItemData rightItem = base.GetRightItem();
		if ((ZInput.GetButton("Remove") || ZInput.GetButton("JoyRemove")) && rightItem.m_shared.m_buildPieces.m_canRemovePieces && Time.time - this.m_lastToolUseTime > this.m_toolUseDelay)
		{
			if (this.HaveStamina(rightItem.m_shared.m_attack.m_attackStamina))
			{
				if (this.RemovePiece())
				{
					this.m_lastToolUseTime = Time.time;
					base.AddNoise(50f);
					this.UseStamina(rightItem.m_shared.m_attack.m_attackStamina);
					if (rightItem.m_shared.m_useDurability)
					{
						rightItem.m_durability -= rightItem.m_shared.m_useDurabilityDrain;
					}
				}
			}
			else
			{
				Hud.instance.StaminaBarNoStaminaFlash();
			}
		}
		if (ZInput.GetButtonDown("Attack") || ZInput.GetButtonDown("JoyPlace"))
		{
			Piece selectedPiece = this.m_buildPieces.GetSelectedPiece();
			if (selectedPiece != null && Time.time - this.m_lastToolUseTime > this.m_toolUseDelay)
			{
				if (this.HaveStamina(rightItem.m_shared.m_attack.m_attackStamina))
				{
					if (selectedPiece.m_repairPiece)
					{
						this.Repair(rightItem, selectedPiece);
					}
					else if (this.m_placementGhost != null)
					{
						if (this.m_noPlacementCost || this.HaveRequirements(selectedPiece, Player.RequirementMode.CanBuild))
						{
							if (this.PlacePiece(selectedPiece))
							{
								this.m_lastToolUseTime = Time.time;
								this.ConsumeResources(selectedPiece.m_resources, 0);
								this.UseStamina(rightItem.m_shared.m_attack.m_attackStamina);
								if (rightItem.m_shared.m_useDurability)
								{
									rightItem.m_durability -= rightItem.m_shared.m_useDurabilityDrain;
								}
							}
						}
						else
						{
							this.Message(MessageHud.MessageType.Center, "$msg_missingrequirement", 0, null);
						}
					}
				}
				else
				{
					Hud.instance.StaminaBarNoStaminaFlash();
				}
			}
		}
		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			this.m_placeRotation--;
		}
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			this.m_placeRotation++;
		}
		float joyRightStickX = ZInput.GetJoyRightStickX();
		if (ZInput.GetButton("JoyRotate") && Mathf.Abs(joyRightStickX) > 0.5f)
		{
			if (this.m_rotatePieceTimer == 0f)
			{
				if (joyRightStickX < 0f)
				{
					this.m_placeRotation++;
				}
				else
				{
					this.m_placeRotation--;
				}
			}
			else if (this.m_rotatePieceTimer > 0.25f)
			{
				if (joyRightStickX < 0f)
				{
					this.m_placeRotation++;
				}
				else
				{
					this.m_placeRotation--;
				}
				this.m_rotatePieceTimer = 0.17f;
			}
			this.m_rotatePieceTimer += dt;
			return;
		}
		this.m_rotatePieceTimer = 0f;
	}

	// Token: 0x06000165 RID: 357 RVA: 0x0000B50C File Offset: 0x0000970C
	private void UpdateBuildGuiInput()
	{
		if (Hud.instance.IsQuickPieceSelectEnabled())
		{
			if (!Hud.IsPieceSelectionVisible() && ZInput.GetButtonDown("BuildMenu"))
			{
				Hud.instance.TogglePieceSelection();
			}
		}
		else if (ZInput.GetButtonDown("BuildMenu"))
		{
			Hud.instance.TogglePieceSelection();
		}
		if (ZInput.GetButtonDown("JoyUse"))
		{
			Hud.instance.TogglePieceSelection();
		}
		if (Hud.IsPieceSelectionVisible())
		{
			if (Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyButtonB"))
			{
				Hud.HidePieceSelection();
			}
			if (ZInput.GetButtonDown("JoyTabLeft") || ZInput.GetButtonDown("BuildPrev") || Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				this.m_buildPieces.PrevCategory();
				this.UpdateAvailablePiecesList();
			}
			if (ZInput.GetButtonDown("JoyTabRight") || ZInput.GetButtonDown("BuildNext") || Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				this.m_buildPieces.NextCategory();
				this.UpdateAvailablePiecesList();
			}
			if (ZInput.GetButtonDown("JoyLStickLeft"))
			{
				this.m_buildPieces.LeftPiece();
				this.SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickRight"))
			{
				this.m_buildPieces.RightPiece();
				this.SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickUp"))
			{
				this.m_buildPieces.UpPiece();
				this.SetupPlacementGhost();
			}
			if (ZInput.GetButtonDown("JoyLStickDown"))
			{
				this.m_buildPieces.DownPiece();
				this.SetupPlacementGhost();
			}
		}
	}

	// Token: 0x06000166 RID: 358 RVA: 0x0000B67C File Offset: 0x0000987C
	public void SetSelectedPiece(Vector2Int p)
	{
		if (this.m_buildPieces && this.m_buildPieces.GetSelectedIndex() != p)
		{
			this.m_buildPieces.SetSelected(p);
			this.SetupPlacementGhost();
		}
	}

	// Token: 0x06000167 RID: 359 RVA: 0x0000B6B0 File Offset: 0x000098B0
	public Piece GetPiece(Vector2Int p)
	{
		if (this.m_buildPieces)
		{
			return this.m_buildPieces.GetPiece(p);
		}
		return null;
	}

	// Token: 0x06000168 RID: 360 RVA: 0x0000B6CD File Offset: 0x000098CD
	public bool IsPieceAvailable(Piece piece)
	{
		return this.m_buildPieces && this.m_buildPieces.IsPieceAvailable(piece);
	}

	// Token: 0x06000169 RID: 361 RVA: 0x0000B6EA File Offset: 0x000098EA
	public Piece GetSelectedPiece()
	{
		if (this.m_buildPieces)
		{
			return this.m_buildPieces.GetSelectedPiece();
		}
		return null;
	}

	// Token: 0x0600016A RID: 362 RVA: 0x0000B706 File Offset: 0x00009906
	private void LateUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.UpdateEmote();
		if (this.m_nview.IsOwner())
		{
			ZNet.instance.SetReferencePosition(base.transform.position);
			this.UpdatePlacementGhost(false);
		}
	}

	// Token: 0x0600016B RID: 363 RVA: 0x0000B748 File Offset: 0x00009948
	private void SetupAwake()
	{
		if (this.m_nview.GetZDO() == null)
		{
			this.m_animator.SetBool("wakeup", false);
			return;
		}
		bool @bool = this.m_nview.GetZDO().GetBool("wakeup", true);
		this.m_animator.SetBool("wakeup", @bool);
		if (@bool)
		{
			this.m_wakeupTimer = 0f;
		}
	}

	// Token: 0x0600016C RID: 364 RVA: 0x0000B7AC File Offset: 0x000099AC
	private void UpdateAwake(float dt)
	{
		if (this.m_wakeupTimer >= 0f)
		{
			this.m_wakeupTimer += dt;
			if (this.m_wakeupTimer > 1f)
			{
				this.m_wakeupTimer = -1f;
				this.m_animator.SetBool("wakeup", false);
				if (this.m_nview.IsOwner())
				{
					this.m_nview.GetZDO().Set("wakeup", false);
				}
			}
		}
	}

	// Token: 0x0600016D RID: 365 RVA: 0x0000B820 File Offset: 0x00009A20
	private void EdgeOfWorldKill(float dt)
	{
		if (this.IsDead())
		{
			return;
		}
		float magnitude = base.transform.position.magnitude;
		float num = 10420f;
		if (magnitude > num && (base.IsSwiming() || base.transform.position.y < ZoneSystem.instance.m_waterLevel))
		{
			Vector3 a = Vector3.Normalize(base.transform.position);
			float d = Utils.LerpStep(num, 10500f, magnitude) * 10f;
			this.m_body.MovePosition(this.m_body.position + a * d * dt);
		}
		if (magnitude > num && base.transform.position.y < ZoneSystem.instance.m_waterLevel - 40f)
		{
			HitData hitData = new HitData();
			hitData.m_damage.m_damage = 99999f;
			base.Damage(hitData);
		}
	}

	// Token: 0x0600016E RID: 366 RVA: 0x0000B90C File Offset: 0x00009B0C
	private void AutoPickup(float dt)
	{
		if (this.IsTeleporting())
		{
			return;
		}
		Vector3 vector = base.transform.position + Vector3.up;
		foreach (Collider collider in Physics.OverlapSphere(vector, this.m_autoPickupRange, this.m_autoPickupMask))
		{
			if (collider.attachedRigidbody)
			{
				ItemDrop component = collider.attachedRigidbody.GetComponent<ItemDrop>();
				if (!(component == null) && component.m_autoPickup && !this.HaveUniqueKey(component.m_itemData.m_shared.m_name) && component.GetComponent<ZNetView>().IsValid())
				{
					if (!component.CanPickup())
					{
						component.RequestOwn();
					}
					else if (this.m_inventory.CanAddItem(component.m_itemData, -1) && component.m_itemData.GetWeight() + this.m_inventory.GetTotalWeight() <= this.GetMaxCarryWeight())
					{
						float num = Vector3.Distance(component.transform.position, vector);
						if (num <= this.m_autoPickupRange)
						{
							if (num < 0.3f)
							{
								base.Pickup(component.gameObject);
							}
							else
							{
								Vector3 a = Vector3.Normalize(vector - component.transform.position);
								float d = 15f;
								component.transform.position = component.transform.position + a * d * dt;
							}
						}
					}
				}
			}
		}
	}

	// Token: 0x0600016F RID: 367 RVA: 0x0000BA9C File Offset: 0x00009C9C
	private void PlayerAttackInput(float dt)
	{
		if (this.InPlaceMode())
		{
			return;
		}
		ItemDrop.ItemData currentWeapon = base.GetCurrentWeapon();
		if (currentWeapon != null && currentWeapon.m_shared.m_holdDurationMin > 0f)
		{
			if (this.m_blocking || this.InMinorAction())
			{
				this.m_attackDrawTime = -1f;
				if (!string.IsNullOrEmpty(currentWeapon.m_shared.m_holdAnimationState))
				{
					this.m_zanim.SetBool(currentWeapon.m_shared.m_holdAnimationState, false);
				}
				return;
			}
			bool flag = currentWeapon.m_shared.m_holdStaminaDrain <= 0f || this.HaveStamina(0f);
			if (this.m_attackDrawTime < 0f)
			{
				if (!this.m_attackDraw)
				{
					this.m_attackDrawTime = 0f;
					return;
				}
			}
			else
			{
				if (this.m_attackDraw && flag && this.m_attackDrawTime >= 0f)
				{
					if (this.m_attackDrawTime == 0f)
					{
						if (!currentWeapon.m_shared.m_attack.StartDraw(this, currentWeapon))
						{
							this.m_attackDrawTime = -1f;
							return;
						}
						currentWeapon.m_shared.m_holdStartEffect.Create(base.transform.position, Quaternion.identity, base.transform, 1f);
					}
					this.m_attackDrawTime += Time.fixedDeltaTime;
					if (!string.IsNullOrEmpty(currentWeapon.m_shared.m_holdAnimationState))
					{
						this.m_zanim.SetBool(currentWeapon.m_shared.m_holdAnimationState, true);
					}
					this.UseStamina(currentWeapon.m_shared.m_holdStaminaDrain * dt);
					return;
				}
				if (this.m_attackDrawTime > 0f)
				{
					if (flag)
					{
						this.StartAttack(null, false);
					}
					if (!string.IsNullOrEmpty(currentWeapon.m_shared.m_holdAnimationState))
					{
						this.m_zanim.SetBool(currentWeapon.m_shared.m_holdAnimationState, false);
					}
					this.m_attackDrawTime = 0f;
					return;
				}
			}
		}
		else
		{
			if (this.m_attack)
			{
				this.m_queuedAttackTimer = 0.5f;
				this.m_queuedSecondAttackTimer = 0f;
			}
			if (this.m_secondaryAttack)
			{
				this.m_queuedSecondAttackTimer = 0.5f;
				this.m_queuedAttackTimer = 0f;
			}
			this.m_queuedAttackTimer -= Time.fixedDeltaTime;
			this.m_queuedSecondAttackTimer -= Time.fixedDeltaTime;
			if (this.m_queuedAttackTimer > 0f && this.StartAttack(null, false))
			{
				this.m_queuedAttackTimer = 0f;
			}
			if (this.m_queuedSecondAttackTimer > 0f && this.StartAttack(null, true))
			{
				this.m_queuedSecondAttackTimer = 0f;
			}
		}
	}

	// Token: 0x06000170 RID: 368 RVA: 0x0000BD18 File Offset: 0x00009F18
	protected override bool HaveQueuedChain()
	{
		return this.m_queuedAttackTimer > 0f && base.GetCurrentWeapon() != null && this.m_currentAttack != null && this.m_currentAttack.CanStartChainAttack();
	}

	// Token: 0x06000171 RID: 369 RVA: 0x0000BD44 File Offset: 0x00009F44
	private void UpdateBaseValue(float dt)
	{
		this.m_baseValueUpdatetimer += dt;
		if (this.m_baseValueUpdatetimer > 2f)
		{
			this.m_baseValueUpdatetimer = 0f;
			this.m_baseValue = EffectArea.GetBaseValue(base.transform.position, 20f);
			this.m_nview.GetZDO().Set("baseValue", this.m_baseValue);
			this.m_comfortLevel = SE_Rested.CalculateComfortLevel(this);
		}
	}

	// Token: 0x06000172 RID: 370 RVA: 0x0000BDB9 File Offset: 0x00009FB9
	public int GetComfortLevel()
	{
		return this.m_comfortLevel;
	}

	// Token: 0x06000173 RID: 371 RVA: 0x0000BDC1 File Offset: 0x00009FC1
	public int GetBaseValue()
	{
		if (!this.m_nview.IsValid())
		{
			return 0;
		}
		if (this.m_nview.IsOwner())
		{
			return this.m_baseValue;
		}
		return this.m_nview.GetZDO().GetInt("baseValue", 0);
	}

	// Token: 0x06000174 RID: 372 RVA: 0x0000BDFC File Offset: 0x00009FFC
	public bool IsSafeInHome()
	{
		return this.m_safeInHome;
	}

	// Token: 0x06000175 RID: 373 RVA: 0x0000BE04 File Offset: 0x0000A004
	private void UpdateBiome(float dt)
	{
		if (this.InIntro())
		{
			return;
		}
		this.m_biomeTimer += dt;
		if (this.m_biomeTimer > 1f)
		{
			this.m_biomeTimer = 0f;
			Heightmap.Biome biome = Heightmap.FindBiome(base.transform.position);
			if (this.m_currentBiome != biome)
			{
				this.m_currentBiome = biome;
				this.AddKnownBiome(biome);
			}
		}
	}

	// Token: 0x06000176 RID: 374 RVA: 0x0000BE68 File Offset: 0x0000A068
	public Heightmap.Biome GetCurrentBiome()
	{
		return this.m_currentBiome;
	}

	// Token: 0x06000177 RID: 375 RVA: 0x0000BE70 File Offset: 0x0000A070
	public override void RaiseSkill(Skills.SkillType skill, float value = 1f)
	{
		float num = 1f;
		this.m_seman.ModifyRaiseSkill(skill, ref num);
		value *= num;
		this.m_skills.RaiseSkill(skill, value);
	}

	// Token: 0x06000178 RID: 376 RVA: 0x0000BEA4 File Offset: 0x0000A0A4
	private void UpdateStats(float dt)
	{
		if (this.InIntro() || this.IsTeleporting())
		{
			return;
		}
		this.m_timeSinceDeath += dt;
		this.UpdateMovementModifier();
		this.UpdateFood(dt, false);
		bool flag = this.IsEncumbered();
		float maxStamina = this.GetMaxStamina();
		float num = 1f;
		if (this.IsBlocking())
		{
			num *= 0.8f;
		}
		if ((base.IsSwiming() && !base.IsOnGround()) || this.InAttack() || this.InDodge() || this.m_wallRunning || flag)
		{
			num = 0f;
		}
		float num2 = (this.m_staminaRegen + (1f - this.m_stamina / maxStamina) * this.m_staminaRegen * this.m_staminaRegenTimeMultiplier) * num;
		float num3 = 1f;
		this.m_seman.ModifyStaminaRegen(ref num3);
		num2 *= num3;
		this.m_staminaRegenTimer -= dt;
		if (this.m_stamina < maxStamina && this.m_staminaRegenTimer <= 0f)
		{
			this.m_stamina = Mathf.Min(maxStamina, this.m_stamina + num2 * dt);
		}
		this.m_nview.GetZDO().Set("stamina", this.m_stamina);
		if (flag)
		{
			if (this.m_moveDir.magnitude > 0.1f)
			{
				this.UseStamina(this.m_encumberedStaminaDrain * dt);
			}
			this.m_seman.AddStatusEffect("Encumbered", false);
			this.ShowTutorial("encumbered", false);
		}
		else
		{
			this.m_seman.RemoveStatusEffect("Encumbered", false);
		}
		if (!this.HardDeath())
		{
			this.m_seman.AddStatusEffect("SoftDeath", false);
		}
		else
		{
			this.m_seman.RemoveStatusEffect("SoftDeath", false);
		}
		this.UpdateEnvStatusEffects(dt);
	}

	// Token: 0x06000179 RID: 377 RVA: 0x0000C054 File Offset: 0x0000A254
	private void UpdateEnvStatusEffects(float dt)
	{
		this.m_nearFireTimer += dt;
		HitData.DamageModifiers damageModifiers = base.GetDamageModifiers();
		bool flag = this.m_nearFireTimer < 0.25f;
		bool flag2 = this.m_seman.HaveStatusEffect("Burning");
		bool flag3 = this.InShelter();
		HitData.DamageModifier modifier = damageModifiers.GetModifier(HitData.DamageType.Frost);
		bool flag4 = EnvMan.instance.IsFreezing();
		bool flag5 = EnvMan.instance.IsCold();
		bool flag6 = EnvMan.instance.IsWet();
		bool flag7 = this.IsSensed();
		bool flag8 = this.m_seman.HaveStatusEffect("Wet");
		bool flag9 = base.IsSitting();
		bool flag10 = flag4 && !flag && !flag3;
		bool flag11 = (flag5 && !flag) || (flag4 && flag && !flag3) || (flag4 && !flag && flag3);
		if (modifier == HitData.DamageModifier.Resistant || modifier == HitData.DamageModifier.VeryResistant)
		{
			flag10 = false;
			flag11 = false;
		}
		if (flag6 && !this.m_underRoof)
		{
			this.m_seman.AddStatusEffect("Wet", true);
		}
		if (flag3)
		{
			this.m_seman.AddStatusEffect("Shelter", false);
		}
		else
		{
			this.m_seman.RemoveStatusEffect("Shelter", false);
		}
		if (flag)
		{
			this.m_seman.AddStatusEffect("CampFire", false);
		}
		else
		{
			this.m_seman.RemoveStatusEffect("CampFire", false);
		}
		bool flag12 = !flag7 && (flag9 || flag3) && (!flag11 & !flag10) && !flag8 && !flag2 && flag;
		if (flag12)
		{
			this.m_seman.AddStatusEffect("Resting", false);
		}
		else
		{
			this.m_seman.RemoveStatusEffect("Resting", false);
		}
		this.m_safeInHome = (flag12 && flag3);
		if (flag10)
		{
			if (!this.m_seman.RemoveStatusEffect("Cold", true))
			{
				this.m_seman.AddStatusEffect("Freezing", false);
				return;
			}
		}
		else if (flag11)
		{
			if (!this.m_seman.RemoveStatusEffect("Freezing", true) && this.m_seman.AddStatusEffect("Cold", false))
			{
				this.ShowTutorial("cold", false);
				return;
			}
		}
		else
		{
			this.m_seman.RemoveStatusEffect("Cold", false);
			this.m_seman.RemoveStatusEffect("Freezing", false);
		}
	}

	// Token: 0x0600017A RID: 378 RVA: 0x0000C280 File Offset: 0x0000A480
	private bool CanEat(ItemDrop.ItemData item, bool showMessages)
	{
		foreach (Player.Food food in this.m_foods)
		{
			if (food.m_item.m_shared.m_name == item.m_shared.m_name)
			{
				if (food.CanEatAgain())
				{
					return true;
				}
				this.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_nomore", new string[]
				{
					item.m_shared.m_name
				}), 0, null);
				return false;
			}
		}
		using (List<Player.Food>.Enumerator enumerator = this.m_foods.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.CanEatAgain())
				{
					return true;
				}
			}
		}
		if (this.m_foods.Count >= 3)
		{
			this.Message(MessageHud.MessageType.Center, "$msg_isfull", 0, null);
			return false;
		}
		return true;
	}

	// Token: 0x0600017B RID: 379 RVA: 0x0000C394 File Offset: 0x0000A594
	private Player.Food GetMostDepletedFood()
	{
		Player.Food food = null;
		foreach (Player.Food food2 in this.m_foods)
		{
			if (food2.CanEatAgain() && (food == null || food2.m_health < food.m_health))
			{
				food = food2;
			}
		}
		return food;
	}

	// Token: 0x0600017C RID: 380 RVA: 0x0000C400 File Offset: 0x0000A600
	public void ClearFood()
	{
		this.m_foods.Clear();
	}

	// Token: 0x0600017D RID: 381 RVA: 0x0000C410 File Offset: 0x0000A610
	private bool EatFood(ItemDrop.ItemData item)
	{
		if (!this.CanEat(item, false))
		{
			return false;
		}
		foreach (Player.Food food in this.m_foods)
		{
			if (food.m_item.m_shared.m_name == item.m_shared.m_name)
			{
				if (food.CanEatAgain())
				{
					food.m_health = item.m_shared.m_food;
					food.m_stamina = item.m_shared.m_foodStamina;
					this.UpdateFood(0f, true);
					return true;
				}
				return false;
			}
		}
		if (this.m_foods.Count < 3)
		{
			Player.Food food2 = new Player.Food();
			food2.m_name = item.m_dropPrefab.name;
			food2.m_item = item;
			food2.m_health = item.m_shared.m_food;
			food2.m_stamina = item.m_shared.m_foodStamina;
			this.m_foods.Add(food2);
			this.UpdateFood(0f, true);
			return true;
		}
		Player.Food mostDepletedFood = this.GetMostDepletedFood();
		if (mostDepletedFood != null)
		{
			mostDepletedFood.m_name = item.m_dropPrefab.name;
			mostDepletedFood.m_item = item;
			mostDepletedFood.m_health = item.m_shared.m_food;
			mostDepletedFood.m_stamina = item.m_shared.m_foodStamina;
			return true;
		}
		return false;
	}

	// Token: 0x0600017E RID: 382 RVA: 0x0000C588 File Offset: 0x0000A788
	private void UpdateFood(float dt, bool forceUpdate)
	{
		this.m_foodUpdateTimer += dt;
		if (this.m_foodUpdateTimer >= 1f || forceUpdate)
		{
			this.m_foodUpdateTimer = 0f;
			foreach (Player.Food food in this.m_foods)
			{
				food.m_health -= food.m_item.m_shared.m_food / food.m_item.m_shared.m_foodBurnTime;
				food.m_stamina -= food.m_item.m_shared.m_foodStamina / food.m_item.m_shared.m_foodBurnTime;
				if (food.m_health < 0f)
				{
					food.m_health = 0f;
				}
				if (food.m_stamina < 0f)
				{
					food.m_stamina = 0f;
				}
				if (food.m_health <= 0f)
				{
					this.Message(MessageHud.MessageType.Center, "$msg_food_done", 0, null);
					this.m_foods.Remove(food);
					break;
				}
			}
			float health;
			float stamina;
			this.GetTotalFoodValue(out health, out stamina);
			this.SetMaxHealth(health, true);
			this.SetMaxStamina(stamina, true);
		}
		if (!forceUpdate)
		{
			this.m_foodRegenTimer += dt;
			if (this.m_foodRegenTimer >= 10f)
			{
				this.m_foodRegenTimer = 0f;
				float num = 0f;
				foreach (Player.Food food2 in this.m_foods)
				{
					num += food2.m_item.m_shared.m_foodRegen;
				}
				if (num > 0f)
				{
					float num2 = 1f;
					this.m_seman.ModifyHealthRegen(ref num2);
					num *= num2;
					base.Heal(num, true);
				}
			}
		}
	}

	// Token: 0x0600017F RID: 383 RVA: 0x0000C790 File Offset: 0x0000A990
	private void GetTotalFoodValue(out float hp, out float stamina)
	{
		hp = 25f;
		stamina = 75f;
		foreach (Player.Food food in this.m_foods)
		{
			hp += food.m_health;
			stamina += food.m_stamina;
		}
	}

	// Token: 0x06000180 RID: 384 RVA: 0x0000C800 File Offset: 0x0000AA00
	public float GetBaseFoodHP()
	{
		return 25f;
	}

	// Token: 0x06000181 RID: 385 RVA: 0x0000C807 File Offset: 0x0000AA07
	public List<Player.Food> GetFoods()
	{
		return this.m_foods;
	}

	// Token: 0x06000182 RID: 386 RVA: 0x0000C810 File Offset: 0x0000AA10
	public void OnSpawned()
	{
		this.m_spawnEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
		if (this.m_firstSpawn)
		{
			if (this.m_valkyrie != null)
			{
				UnityEngine.Object.Instantiate<GameObject>(this.m_valkyrie, base.transform.position, Quaternion.identity);
			}
			this.m_firstSpawn = false;
		}
	}

	// Token: 0x06000183 RID: 387 RVA: 0x0000C878 File Offset: 0x0000AA78
	protected override bool CheckRun(Vector3 moveDir, float dt)
	{
		if (!base.CheckRun(moveDir, dt))
		{
			return false;
		}
		bool flag = this.HaveStamina(0f);
		float skillFactor = this.m_skills.GetSkillFactor(Skills.SkillType.Run);
		float num = Mathf.Lerp(1f, 0.5f, skillFactor);
		float num2 = this.m_runStaminaDrain * num;
		this.m_seman.ModifyRunStaminaDrain(num2, ref num2);
		this.UseStamina(dt * num2);
		if (this.HaveStamina(0f))
		{
			this.m_runSkillImproveTimer += dt;
			if (this.m_runSkillImproveTimer > 1f)
			{
				this.m_runSkillImproveTimer = 0f;
				this.RaiseSkill(Skills.SkillType.Run, 1f);
			}
			this.AbortEquipQueue();
			return true;
		}
		if (flag)
		{
			Hud.instance.StaminaBarNoStaminaFlash();
		}
		return false;
	}

	// Token: 0x06000184 RID: 388 RVA: 0x0000C934 File Offset: 0x0000AB34
	private void UpdateMovementModifier()
	{
		this.m_equipmentMovementModifier = 0f;
		if (this.m_rightItem != null)
		{
			this.m_equipmentMovementModifier += this.m_rightItem.m_shared.m_movementModifier;
		}
		if (this.m_leftItem != null)
		{
			this.m_equipmentMovementModifier += this.m_leftItem.m_shared.m_movementModifier;
		}
		if (this.m_chestItem != null)
		{
			this.m_equipmentMovementModifier += this.m_chestItem.m_shared.m_movementModifier;
		}
		if (this.m_legItem != null)
		{
			this.m_equipmentMovementModifier += this.m_legItem.m_shared.m_movementModifier;
		}
		if (this.m_helmetItem != null)
		{
			this.m_equipmentMovementModifier += this.m_helmetItem.m_shared.m_movementModifier;
		}
		if (this.m_shoulderItem != null)
		{
			this.m_equipmentMovementModifier += this.m_shoulderItem.m_shared.m_movementModifier;
		}
		if (this.m_utilityItem != null)
		{
			this.m_equipmentMovementModifier += this.m_utilityItem.m_shared.m_movementModifier;
		}
	}

	// Token: 0x06000185 RID: 389 RVA: 0x0000CA4F File Offset: 0x0000AC4F
	public void OnSkillLevelup(Skills.SkillType skill, float level)
	{
		this.m_skillLevelupEffects.Create(this.m_head.position, this.m_head.rotation, this.m_head, 1f);
	}

	// Token: 0x06000186 RID: 390 RVA: 0x0000CA80 File Offset: 0x0000AC80
	protected override void OnJump()
	{
		this.AbortEquipQueue();
		float num = this.m_jumpStaminaUsage - this.m_jumpStaminaUsage * this.m_equipmentMovementModifier;
		this.m_seman.ModifyJumpStaminaUsage(num, ref num);
		this.UseStamina(num);
	}

	// Token: 0x06000187 RID: 391 RVA: 0x0000CAC0 File Offset: 0x0000ACC0
	protected override void OnSwiming(Vector3 targetVel, float dt)
	{
		base.OnSwiming(targetVel, dt);
		if (targetVel.magnitude > 0.1f)
		{
			float skillFactor = this.m_skills.GetSkillFactor(Skills.SkillType.Swim);
			float num = Mathf.Lerp(this.m_swimStaminaDrainMinSkill, this.m_swimStaminaDrainMaxSkill, skillFactor);
			this.UseStamina(dt * num);
			this.m_swimSkillImproveTimer += dt;
			if (this.m_swimSkillImproveTimer > 1f)
			{
				this.m_swimSkillImproveTimer = 0f;
				this.RaiseSkill(Skills.SkillType.Swim, 1f);
			}
		}
		if (!this.HaveStamina(0f))
		{
			this.m_drownDamageTimer += dt;
			if (this.m_drownDamageTimer > 1f)
			{
				this.m_drownDamageTimer = 0f;
				float damage = Mathf.Ceil(base.GetMaxHealth() / 20f);
				HitData hitData = new HitData();
				hitData.m_damage.m_damage = damage;
				hitData.m_point = base.GetCenterPoint();
				hitData.m_dir = Vector3.down;
				hitData.m_pushForce = 10f;
				base.Damage(hitData);
				Vector3 position = base.transform.position;
				position.y = this.m_waterLevel;
				this.m_drownEffects.Create(position, base.transform.rotation, null, 1f);
			}
		}
	}

	// Token: 0x06000188 RID: 392 RVA: 0x0000CC00 File Offset: 0x0000AE00
	protected override bool TakeInput()
	{
		bool result = (!Chat.instance || !Chat.instance.HasFocus()) && !global::Console.IsVisible() && !TextInput.IsVisible() && (!StoreGui.IsVisible() && !InventoryGui.IsVisible() && !Menu.IsVisible() && (!TextViewer.instance || !TextViewer.instance.IsVisible()) && !Minimap.IsOpen()) && !GameCamera.InFreeFly();
		if (this.IsDead() || this.InCutscene() || this.IsTeleporting())
		{
			result = false;
		}
		return result;
	}

	// Token: 0x06000189 RID: 393 RVA: 0x0000CC94 File Offset: 0x0000AE94
	public void UseHotbarItem(int index)
	{
		ItemDrop.ItemData itemAt = this.m_inventory.GetItemAt(index - 1, 0);
		if (itemAt != null)
		{
			base.UseItem(null, itemAt, false);
		}
	}

	// Token: 0x0600018A RID: 394 RVA: 0x0000CCC0 File Offset: 0x0000AEC0
	public bool RequiredCraftingStation(Recipe recipe, int qualityLevel, bool checkLevel)
	{
		CraftingStation requiredStation = recipe.GetRequiredStation(qualityLevel);
		if (requiredStation != null)
		{
			if (this.m_currentStation == null)
			{
				return false;
			}
			if (requiredStation.m_name != this.m_currentStation.m_name)
			{
				return false;
			}
			if (checkLevel)
			{
				int requiredStationLevel = recipe.GetRequiredStationLevel(qualityLevel);
				if (this.m_currentStation.GetLevel() < requiredStationLevel)
				{
					return false;
				}
			}
		}
		else if (this.m_currentStation != null && !this.m_currentStation.m_showBasicRecipies)
		{
			return false;
		}
		return true;
	}

	// Token: 0x0600018B RID: 395 RVA: 0x0000CD44 File Offset: 0x0000AF44
	public bool HaveRequirements(Recipe recipe, bool discover, int qualityLevel)
	{
		if (discover)
		{
			if (recipe.m_craftingStation && !this.KnowStationLevel(recipe.m_craftingStation.m_name, recipe.m_minStationLevel))
			{
				return false;
			}
		}
		else if (!this.RequiredCraftingStation(recipe, qualityLevel, true))
		{
			return false;
		}
		return (recipe.m_item.m_itemData.m_shared.m_dlc.Length <= 0 || DLCMan.instance.IsDLCInstalled(recipe.m_item.m_itemData.m_shared.m_dlc)) && this.HaveRequirements(recipe.m_resources, discover, qualityLevel);
	}

	// Token: 0x0600018C RID: 396 RVA: 0x0000CDDC File Offset: 0x0000AFDC
	private bool HaveRequirements(Piece.Requirement[] resources, bool discover, int qualityLevel)
	{
		foreach (Piece.Requirement requirement in resources)
		{
			if (requirement.m_resItem)
			{
				if (discover)
				{
					if (requirement.m_amount > 0 && !this.m_knownMaterial.Contains(requirement.m_resItem.m_itemData.m_shared.m_name))
					{
						return false;
					}
				}
				else
				{
					int amount = requirement.GetAmount(qualityLevel);
					if (this.m_inventory.CountItems(requirement.m_resItem.m_itemData.m_shared.m_name) < amount)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	// Token: 0x0600018D RID: 397 RVA: 0x0000CE68 File Offset: 0x0000B068
	public bool HaveRequirements(Piece piece, Player.RequirementMode mode)
	{
		if (piece.m_craftingStation)
		{
			if (mode == Player.RequirementMode.IsKnown || mode == Player.RequirementMode.CanAlmostBuild)
			{
				if (!this.m_knownStations.ContainsKey(piece.m_craftingStation.m_name))
				{
					return false;
				}
			}
			else if (!CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, base.transform.position))
			{
				return false;
			}
		}
		if (piece.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(piece.m_dlc))
		{
			return false;
		}
		foreach (Piece.Requirement requirement in piece.m_resources)
		{
			if (requirement.m_resItem && requirement.m_amount > 0)
			{
				if (mode == Player.RequirementMode.IsKnown)
				{
					if (!this.m_knownMaterial.Contains(requirement.m_resItem.m_itemData.m_shared.m_name))
					{
						return false;
					}
				}
				else if (mode == Player.RequirementMode.CanAlmostBuild)
				{
					if (!this.m_inventory.HaveItem(requirement.m_resItem.m_itemData.m_shared.m_name))
					{
						return false;
					}
				}
				else if (mode == Player.RequirementMode.CanBuild && this.m_inventory.CountItems(requirement.m_resItem.m_itemData.m_shared.m_name) < requirement.m_amount)
				{
					return false;
				}
			}
		}
		return true;
	}

	// Token: 0x0600018E RID: 398 RVA: 0x0000CFA0 File Offset: 0x0000B1A0
	public void SetCraftingStation(CraftingStation station)
	{
		if (this.m_currentStation == station)
		{
			return;
		}
		if (station)
		{
			this.AddKnownStation(station);
			station.PokeInUse();
		}
		this.m_currentStation = station;
		base.HideHandItems();
		int value = this.m_currentStation ? this.m_currentStation.m_useAnimation : 0;
		this.m_zanim.SetInt("crafting", value);
	}

	// Token: 0x0600018F RID: 399 RVA: 0x0000D00B File Offset: 0x0000B20B
	public CraftingStation GetCurrentCraftingStation()
	{
		return this.m_currentStation;
	}

	// Token: 0x06000190 RID: 400 RVA: 0x0000D014 File Offset: 0x0000B214
	public void ConsumeResources(Piece.Requirement[] requirements, int qualityLevel)
	{
		foreach (Piece.Requirement requirement in requirements)
		{
			if (requirement.m_resItem)
			{
				int amount = requirement.GetAmount(qualityLevel);
				if (amount > 0)
				{
					this.m_inventory.RemoveItem(requirement.m_resItem.m_itemData.m_shared.m_name, amount);
				}
			}
		}
	}

	// Token: 0x06000191 RID: 401 RVA: 0x0000D070 File Offset: 0x0000B270
	private void UpdateHover()
	{
		if (this.InPlaceMode() || this.IsDead() || this.m_shipControl != null)
		{
			this.m_hovering = null;
			this.m_hoveringCreature = null;
			return;
		}
		this.FindHoverObject(out this.m_hovering, out this.m_hoveringCreature);
	}

	// Token: 0x06000192 RID: 402 RVA: 0x0000D0BC File Offset: 0x0000B2BC
	private bool CheckCanRemovePiece(Piece piece)
	{
		if (!this.m_noPlacementCost && piece.m_craftingStation != null && !CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, base.transform.position))
		{
			this.Message(MessageHud.MessageType.Center, "$msg_missingstation", 0, null);
			return false;
		}
		return true;
	}

	// Token: 0x06000193 RID: 403 RVA: 0x0000D114 File Offset: 0x0000B314
	private bool RemovePiece()
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, out raycastHit, 50f, this.m_removeRayMask) && Vector3.Distance(raycastHit.point, this.m_eye.position) < this.m_maxPlaceDistance)
		{
			Piece piece = raycastHit.collider.GetComponentInParent<Piece>();
			if (piece == null && raycastHit.collider.GetComponent<Heightmap>())
			{
				piece = TerrainModifier.FindClosestModifierPieceInRange(raycastHit.point, 2.5f);
			}
			if (piece)
			{
				if (!piece.m_canBeRemoved)
				{
					return false;
				}
				if (Location.IsInsideNoBuildLocation(piece.transform.position))
				{
					this.Message(MessageHud.MessageType.Center, "$msg_nobuildzone", 0, null);
					return false;
				}
				if (!PrivateArea.CheckAccess(piece.transform.position, 0f, true, false))
				{
					this.Message(MessageHud.MessageType.Center, "$msg_privatezone", 0, null);
					return false;
				}
				if (!this.CheckCanRemovePiece(piece))
				{
					return false;
				}
				ZNetView component = piece.GetComponent<ZNetView>();
				if (component == null)
				{
					return false;
				}
				if (!piece.CanBeRemoved())
				{
					this.Message(MessageHud.MessageType.Center, "$msg_cantremovenow", 0, null);
					return false;
				}
				WearNTear component2 = piece.GetComponent<WearNTear>();
				if (component2)
				{
					component2.Remove();
				}
				else
				{
					ZLog.Log("Removing non WNT object with hammer " + piece.name);
					component.ClaimOwnership();
					piece.DropResources();
					piece.m_placeEffect.Create(piece.transform.position, piece.transform.rotation, piece.gameObject.transform, 1f);
					this.m_removeEffects.Create(piece.transform.position, Quaternion.identity, null, 1f);
					ZNetScene.instance.Destroy(piece.gameObject);
				}
				ItemDrop.ItemData rightItem = base.GetRightItem();
				if (rightItem != null)
				{
					this.FaceLookDirection();
					this.m_zanim.SetTrigger(rightItem.m_shared.m_attack.m_attackAnimation);
				}
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000194 RID: 404 RVA: 0x0000D318 File Offset: 0x0000B518
	public void FaceLookDirection()
	{
		base.transform.rotation = base.GetLookYaw();
	}

	// Token: 0x06000195 RID: 405 RVA: 0x0000D32C File Offset: 0x0000B52C
	private bool PlacePiece(Piece piece)
	{
		this.UpdatePlacementGhost(true);
		Vector3 position = this.m_placementGhost.transform.position;
		Quaternion rotation = this.m_placementGhost.transform.rotation;
		GameObject gameObject = piece.gameObject;
		switch (this.m_placementStatus)
		{
		case Player.PlacementStatus.Invalid:
			this.Message(MessageHud.MessageType.Center, "$msg_invalidplacement", 0, null);
			return false;
		case Player.PlacementStatus.BlockedbyPlayer:
			this.Message(MessageHud.MessageType.Center, "$msg_blocked", 0, null);
			return false;
		case Player.PlacementStatus.NoBuildZone:
			this.Message(MessageHud.MessageType.Center, "$msg_nobuildzone", 0, null);
			return false;
		case Player.PlacementStatus.PrivateZone:
			this.Message(MessageHud.MessageType.Center, "$msg_privatezone", 0, null);
			return false;
		case Player.PlacementStatus.MoreSpace:
			this.Message(MessageHud.MessageType.Center, "$msg_needspace", 0, null);
			return false;
		case Player.PlacementStatus.NoTeleportArea:
			this.Message(MessageHud.MessageType.Center, "$msg_noteleportarea", 0, null);
			return false;
		case Player.PlacementStatus.ExtensionMissingStation:
			this.Message(MessageHud.MessageType.Center, "$msg_extensionmissingstation", 0, null);
			return false;
		case Player.PlacementStatus.WrongBiome:
			this.Message(MessageHud.MessageType.Center, "$msg_wrongbiome", 0, null);
			return false;
		case Player.PlacementStatus.NeedCultivated:
			this.Message(MessageHud.MessageType.Center, "$msg_needcultivated", 0, null);
			return false;
		case Player.PlacementStatus.NotInDungeon:
			this.Message(MessageHud.MessageType.Center, "$msg_notindungeon", 0, null);
			return false;
		default:
		{
			TerrainModifier.SetTriggerOnPlaced(true);
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, position, rotation);
			TerrainModifier.SetTriggerOnPlaced(false);
			CraftingStation componentInChildren = gameObject2.GetComponentInChildren<CraftingStation>();
			if (componentInChildren)
			{
				this.AddKnownStation(componentInChildren);
			}
			Piece component = gameObject2.GetComponent<Piece>();
			if (component)
			{
				component.SetCreator(this.GetPlayerID());
			}
			PrivateArea component2 = gameObject2.GetComponent<PrivateArea>();
			if (component2)
			{
				component2.Setup(Game.instance.GetPlayerProfile().GetName());
			}
			WearNTear component3 = gameObject2.GetComponent<WearNTear>();
			if (component3)
			{
				component3.OnPlaced();
			}
			ItemDrop.ItemData rightItem = base.GetRightItem();
			if (rightItem != null)
			{
				this.FaceLookDirection();
				this.m_zanim.SetTrigger(rightItem.m_shared.m_attack.m_attackAnimation);
			}
			piece.m_placeEffect.Create(position, rotation, gameObject2.transform, 1f);
			base.AddNoise(50f);
			Game.instance.GetPlayerProfile().m_playerStats.m_builds++;
			ZLog.Log("Placed " + gameObject.name);
			Gogan.LogEvent("Game", "PlacedPiece", gameObject.name, 0L);
			return true;
		}
		}
	}

	// Token: 0x06000196 RID: 406 RVA: 0x000027E2 File Offset: 0x000009E2
	public override bool IsPlayer()
	{
		return true;
	}

	// Token: 0x06000197 RID: 407 RVA: 0x0000D570 File Offset: 0x0000B770
	public void GetBuildSelection(out Piece go, out Vector2Int id, out int total, out Piece.PieceCategory category, out bool useCategory)
	{
		category = this.m_buildPieces.m_selectedCategory;
		useCategory = this.m_buildPieces.m_useCategories;
		if (this.m_buildPieces.GetAvailablePiecesInSelectedCategory() == 0)
		{
			go = null;
			id = Vector2Int.zero;
			total = 0;
			return;
		}
		GameObject selectedPrefab = this.m_buildPieces.GetSelectedPrefab();
		go = (selectedPrefab ? selectedPrefab.GetComponent<Piece>() : null);
		id = this.m_buildPieces.GetSelectedIndex();
		total = this.m_buildPieces.GetAvailablePiecesInSelectedCategory();
	}

	// Token: 0x06000198 RID: 408 RVA: 0x0000D5F5 File Offset: 0x0000B7F5
	public List<Piece> GetBuildPieces()
	{
		if (this.m_buildPieces)
		{
			return this.m_buildPieces.GetPiecesInSelectedCategory();
		}
		return null;
	}

	// Token: 0x06000199 RID: 409 RVA: 0x0000D611 File Offset: 0x0000B811
	public int GetAvailableBuildPiecesInCategory(Piece.PieceCategory cat)
	{
		if (this.m_buildPieces)
		{
			return this.m_buildPieces.GetAvailablePiecesInCategory(cat);
		}
		return 0;
	}

	// Token: 0x0600019A RID: 410 RVA: 0x0000D62E File Offset: 0x0000B82E
	private void RPC_OnDeath(long sender)
	{
		this.m_visual.SetActive(false);
	}

	// Token: 0x0600019B RID: 411 RVA: 0x0000D63C File Offset: 0x0000B83C
	private void CreateDeathEffects()
	{
		GameObject[] array = this.m_deathEffects.Create(base.transform.position, base.transform.rotation, base.transform, 1f);
		for (int i = 0; i < array.Length; i++)
		{
			Ragdoll component = array[i].GetComponent<Ragdoll>();
			if (component)
			{
				Vector3 velocity = this.m_body.velocity;
				if (this.m_pushForce.magnitude * 0.5f > velocity.magnitude)
				{
					velocity = this.m_pushForce * 0.5f;
				}
				component.Setup(velocity, 0f, 0f, 0f, null);
				this.OnRagdollCreated(component);
				this.m_ragdoll = component;
			}
		}
	}

	// Token: 0x0600019C RID: 412 RVA: 0x0000D6F4 File Offset: 0x0000B8F4
	public void UnequipDeathDropItems()
	{
		if (this.m_rightItem != null)
		{
			base.UnequipItem(this.m_rightItem, false);
		}
		if (this.m_leftItem != null)
		{
			base.UnequipItem(this.m_leftItem, false);
		}
		if (this.m_ammoItem != null)
		{
			base.UnequipItem(this.m_ammoItem, false);
		}
		if (this.m_utilityItem != null)
		{
			base.UnequipItem(this.m_utilityItem, false);
		}
	}

	// Token: 0x0600019D RID: 413 RVA: 0x0000D758 File Offset: 0x0000B958
	private void CreateTombStone()
	{
		if (this.m_inventory.NrOfItems() == 0)
		{
			return;
		}
		base.UnequipAllItems();
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_tombstone, base.GetCenterPoint(), base.transform.rotation);
		gameObject.GetComponent<Container>().GetInventory().MoveInventoryToGrave(this.m_inventory);
		TombStone component = gameObject.GetComponent<TombStone>();
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		component.Setup(playerProfile.GetName(), playerProfile.GetPlayerID());
	}

	// Token: 0x0600019E RID: 414 RVA: 0x0000D7CC File Offset: 0x0000B9CC
	private bool HardDeath()
	{
		return this.m_timeSinceDeath > this.m_hardDeathCooldown;
	}

	// Token: 0x0600019F RID: 415 RVA: 0x0000D7DC File Offset: 0x0000B9DC
	protected override void OnDeath()
	{
		bool flag = this.HardDeath();
		this.m_nview.GetZDO().Set("dead", true);
		this.m_nview.InvokeRPC(ZNetView.Everybody, "OnDeath", Array.Empty<object>());
		Game.instance.GetPlayerProfile().m_playerStats.m_deaths++;
		Game.instance.GetPlayerProfile().SetDeathPoint(base.transform.position);
		this.CreateDeathEffects();
		this.CreateTombStone();
		this.m_foods.Clear();
		if (flag)
		{
			this.m_skills.OnDeath();
		}
		Game.instance.RequestRespawn(10f);
		this.m_timeSinceDeath = 0f;
		if (!flag)
		{
			this.Message(MessageHud.MessageType.TopLeft, "$msg_softdeath", 0, null);
		}
		this.Message(MessageHud.MessageType.Center, "$msg_youdied", 0, null);
		this.ShowTutorial("death", false);
		string eventLabel = "biome:" + this.GetCurrentBiome().ToString();
		Gogan.LogEvent("Game", "Death", eventLabel, 0L);
	}

	// Token: 0x060001A0 RID: 416 RVA: 0x0000D8EF File Offset: 0x0000BAEF
	public void OnRespawn()
	{
		this.m_nview.GetZDO().Set("dead", false);
		base.SetHealth(base.GetMaxHealth());
	}

	// Token: 0x060001A1 RID: 417 RVA: 0x0000D914 File Offset: 0x0000BB14
	private void SetupPlacementGhost()
	{
		if (this.m_placementGhost)
		{
			UnityEngine.Object.Destroy(this.m_placementGhost);
			this.m_placementGhost = null;
		}
		if (this.m_buildPieces == null)
		{
			return;
		}
		GameObject selectedPrefab = this.m_buildPieces.GetSelectedPrefab();
		if (selectedPrefab == null)
		{
			return;
		}
		if (selectedPrefab.GetComponent<Piece>().m_repairPiece)
		{
			return;
		}
		bool enabled = false;
		TerrainModifier componentInChildren = selectedPrefab.GetComponentInChildren<TerrainModifier>();
		if (componentInChildren)
		{
			enabled = componentInChildren.enabled;
			componentInChildren.enabled = false;
		}
		ZNetView.m_forceDisableInit = true;
		this.m_placementGhost = UnityEngine.Object.Instantiate<GameObject>(selectedPrefab);
		ZNetView.m_forceDisableInit = false;
		this.m_placementGhost.name = selectedPrefab.name;
		if (componentInChildren)
		{
			componentInChildren.enabled = enabled;
		}
		Joint[] componentsInChildren = this.m_placementGhost.GetComponentsInChildren<Joint>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i]);
		}
		Rigidbody[] componentsInChildren2 = this.m_placementGhost.GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren2[i]);
		}
		foreach (Collider collider in this.m_placementGhost.GetComponentsInChildren<Collider>())
		{
			if ((1 << collider.gameObject.layer & this.m_placeRayMask) == 0)
			{
				ZLog.Log("Disabling " + collider.gameObject.name + "  " + LayerMask.LayerToName(collider.gameObject.layer));
				collider.enabled = false;
			}
		}
		Transform[] componentsInChildren4 = this.m_placementGhost.GetComponentsInChildren<Transform>();
		int layer = LayerMask.NameToLayer("ghost");
		Transform[] array = componentsInChildren4;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.layer = layer;
		}
		TerrainModifier[] componentsInChildren5 = this.m_placementGhost.GetComponentsInChildren<TerrainModifier>();
		for (int i = 0; i < componentsInChildren5.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren5[i]);
		}
		GuidePoint[] componentsInChildren6 = this.m_placementGhost.GetComponentsInChildren<GuidePoint>();
		for (int i = 0; i < componentsInChildren6.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren6[i]);
		}
		Light[] componentsInChildren7 = this.m_placementGhost.GetComponentsInChildren<Light>();
		for (int i = 0; i < componentsInChildren7.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren7[i]);
		}
		AudioSource[] componentsInChildren8 = this.m_placementGhost.GetComponentsInChildren<AudioSource>();
		for (int i = 0; i < componentsInChildren8.Length; i++)
		{
			componentsInChildren8[i].enabled = false;
		}
		ZSFX[] componentsInChildren9 = this.m_placementGhost.GetComponentsInChildren<ZSFX>();
		for (int i = 0; i < componentsInChildren9.Length; i++)
		{
			componentsInChildren9[i].enabled = false;
		}
		Windmill componentInChildren2 = this.m_placementGhost.GetComponentInChildren<Windmill>();
		if (componentInChildren2)
		{
			componentInChildren2.enabled = false;
		}
		ParticleSystem[] componentsInChildren10 = this.m_placementGhost.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren10.Length; i++)
		{
			componentsInChildren10[i].gameObject.SetActive(false);
		}
		Transform transform = this.m_placementGhost.transform.Find("_GhostOnly");
		if (transform)
		{
			transform.gameObject.SetActive(true);
		}
		this.m_placementGhost.transform.position = base.transform.position;
		this.m_placementGhost.transform.localScale = selectedPrefab.transform.localScale;
		foreach (MeshRenderer meshRenderer in this.m_placementGhost.GetComponentsInChildren<MeshRenderer>())
		{
			if (!(meshRenderer.sharedMaterial == null))
			{
				Material[] sharedMaterials = meshRenderer.sharedMaterials;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					Material material = new Material(sharedMaterials[j]);
					material.SetFloat("_RippleDistance", 0f);
					material.SetFloat("_ValueNoise", 0f);
					sharedMaterials[j] = material;
				}
				meshRenderer.sharedMaterials = sharedMaterials;
				meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			}
		}
	}

	// Token: 0x060001A2 RID: 418 RVA: 0x0000DCF3 File Offset: 0x0000BEF3
	private void SetPlacementGhostValid(bool valid)
	{
		this.m_placementGhost.GetComponent<Piece>().SetInvalidPlacementHeightlight(!valid);
	}

	// Token: 0x060001A3 RID: 419 RVA: 0x0000DD09 File Offset: 0x0000BF09
	protected override void SetPlaceMode(PieceTable buildPieces)
	{
		base.SetPlaceMode(buildPieces);
		this.m_buildPieces = buildPieces;
		this.UpdateAvailablePiecesList();
	}

	// Token: 0x060001A4 RID: 420 RVA: 0x0000DD1F File Offset: 0x0000BF1F
	public void SetBuildCategory(int index)
	{
		if (this.m_buildPieces != null)
		{
			this.m_buildPieces.SetCategory(index);
			this.UpdateAvailablePiecesList();
		}
	}

	// Token: 0x060001A5 RID: 421 RVA: 0x0000DD41 File Offset: 0x0000BF41
	public override bool InPlaceMode()
	{
		return this.m_buildPieces != null;
	}

	// Token: 0x060001A6 RID: 422 RVA: 0x0000DD50 File Offset: 0x0000BF50
	private void Repair(ItemDrop.ItemData toolItem, Piece repairPiece)
	{
		if (!this.InPlaceMode())
		{
			return;
		}
		Piece hoveringPiece = this.GetHoveringPiece();
		if (hoveringPiece)
		{
			if (!this.CheckCanRemovePiece(hoveringPiece))
			{
				return;
			}
			if (!PrivateArea.CheckAccess(hoveringPiece.transform.position, 0f, true, false))
			{
				return;
			}
			bool flag = false;
			WearNTear component = hoveringPiece.GetComponent<WearNTear>();
			if (component && component.Repair())
			{
				flag = true;
			}
			if (flag)
			{
				this.FaceLookDirection();
				this.m_zanim.SetTrigger(toolItem.m_shared.m_attack.m_attackAnimation);
				hoveringPiece.m_placeEffect.Create(hoveringPiece.transform.position, hoveringPiece.transform.rotation, null, 1f);
				this.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_repaired", new string[]
				{
					hoveringPiece.m_name
				}), 0, null);
				this.UseStamina(toolItem.m_shared.m_attack.m_attackStamina);
				if (toolItem.m_shared.m_useDurability)
				{
					toolItem.m_durability -= toolItem.m_shared.m_useDurabilityDrain;
					return;
				}
			}
			else
			{
				this.Message(MessageHud.MessageType.TopLeft, hoveringPiece.m_name + " $msg_doesnotneedrepair", 0, null);
			}
		}
	}

	// Token: 0x060001A7 RID: 423 RVA: 0x0000DE84 File Offset: 0x0000C084
	private void UpdateWearNTearHover()
	{
		if (!this.InPlaceMode())
		{
			this.m_hoveringPiece = null;
			return;
		}
		this.m_hoveringPiece = null;
		RaycastHit raycastHit;
		if (Physics.Raycast(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, out raycastHit, 50f, this.m_removeRayMask) && Vector3.Distance(this.m_eye.position, raycastHit.point) < this.m_maxPlaceDistance)
		{
			Piece componentInParent = raycastHit.collider.GetComponentInParent<Piece>();
			this.m_hoveringPiece = componentInParent;
			if (componentInParent)
			{
				WearNTear component = componentInParent.GetComponent<WearNTear>();
				if (component)
				{
					component.Highlight();
				}
			}
		}
	}

	// Token: 0x060001A8 RID: 424 RVA: 0x0000DF2A File Offset: 0x0000C12A
	public Piece GetHoveringPiece()
	{
		if (this.InPlaceMode())
		{
			return this.m_hoveringPiece;
		}
		return null;
	}

	// Token: 0x060001A9 RID: 425 RVA: 0x0000DF3C File Offset: 0x0000C13C
	private void UpdatePlacementGhost(bool flashGuardStone)
	{
		if (this.m_placementGhost == null)
		{
			if (this.m_placementMarkerInstance)
			{
				this.m_placementMarkerInstance.SetActive(false);
			}
			return;
		}
		bool flag = ZInput.GetButton("AltPlace") || ZInput.GetButton("JoyAltPlace");
		Piece component = this.m_placementGhost.GetComponent<Piece>();
		bool water = component.m_waterPiece || component.m_noInWater;
		Vector3 vector;
		Vector3 up;
		Piece piece;
		Heightmap heightmap;
		Collider x;
		if (this.PieceRayTest(out vector, out up, out piece, out heightmap, out x, water))
		{
			this.m_placementStatus = Player.PlacementStatus.Valid;
			if (this.m_placementMarkerInstance == null)
			{
				this.m_placementMarkerInstance = UnityEngine.Object.Instantiate<GameObject>(this.m_placeMarker, vector, Quaternion.identity);
			}
			this.m_placementMarkerInstance.SetActive(true);
			this.m_placementMarkerInstance.transform.position = vector;
			this.m_placementMarkerInstance.transform.rotation = Quaternion.LookRotation(up);
			if (component.m_groundOnly || component.m_groundPiece || component.m_cultivatedGroundOnly)
			{
				this.m_placementMarkerInstance.SetActive(false);
			}
			WearNTear wearNTear = (piece != null) ? piece.GetComponent<WearNTear>() : null;
			StationExtension component2 = component.GetComponent<StationExtension>();
			if (component2 != null)
			{
				CraftingStation craftingStation = component2.FindClosestStationInRange(vector);
				if (craftingStation)
				{
					component2.StartConnectionEffect(craftingStation);
				}
				else
				{
					component2.StopConnectionEffect();
					this.m_placementStatus = Player.PlacementStatus.ExtensionMissingStation;
				}
				if (component2.OtherExtensionInRange(component.m_spaceRequirement))
				{
					this.m_placementStatus = Player.PlacementStatus.MoreSpace;
				}
			}
			if (wearNTear && !wearNTear.m_supports)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_waterPiece && x == null && !flag)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_noInWater && x != null)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_groundPiece && heightmap == null)
			{
				this.m_placementGhost.SetActive(false);
				this.m_placementStatus = Player.PlacementStatus.Invalid;
				return;
			}
			if (component.m_groundOnly && heightmap == null)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_cultivatedGroundOnly && (heightmap == null || !heightmap.IsCultivated(vector)))
			{
				this.m_placementStatus = Player.PlacementStatus.NeedCultivated;
			}
			if (component.m_notOnWood && piece && wearNTear && (wearNTear.m_materialType == WearNTear.MaterialType.Wood || wearNTear.m_materialType == WearNTear.MaterialType.HardWood))
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_notOnTiltingSurface && up.y < 0.8f)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_inCeilingOnly && up.y > -0.5f)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_notOnFloor && up.y > 0.1f)
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
			if (component.m_onlyInTeleportArea && !EffectArea.IsPointInsideArea(vector, EffectArea.Type.Teleport, 0f))
			{
				this.m_placementStatus = Player.PlacementStatus.NoTeleportArea;
			}
			if (!component.m_allowedInDungeons && base.InInterior())
			{
				this.m_placementStatus = Player.PlacementStatus.NotInDungeon;
			}
			if (heightmap)
			{
				up = Vector3.up;
			}
			this.m_placementGhost.SetActive(true);
			Quaternion rotation = Quaternion.Euler(0f, 22.5f * (float)this.m_placeRotation, 0f);
			if (((component.m_groundPiece || component.m_clipGround) && heightmap) || component.m_clipEverything)
			{
				if (this.m_buildPieces.GetSelectedPrefab().GetComponent<TerrainModifier>() && component.m_allowAltGroundPlacement && component.m_groundPiece && !ZInput.GetButton("AltPlace") && !ZInput.GetButton("JoyAltPlace"))
				{
					float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
					vector.y = groundHeight;
				}
				this.m_placementGhost.transform.position = vector;
				this.m_placementGhost.transform.rotation = rotation;
			}
			else
			{
				Collider[] componentsInChildren = this.m_placementGhost.GetComponentsInChildren<Collider>();
				if (componentsInChildren.Length != 0)
				{
					this.m_placementGhost.transform.position = vector + up * 50f;
					this.m_placementGhost.transform.rotation = rotation;
					Vector3 b = Vector3.zero;
					float num = 999999f;
					foreach (Collider collider in componentsInChildren)
					{
						if (!collider.isTrigger && collider.enabled)
						{
							MeshCollider meshCollider = collider as MeshCollider;
							if (!(meshCollider != null) || meshCollider.convex)
							{
								Vector3 vector2 = collider.ClosestPoint(vector);
								float num2 = Vector3.Distance(vector2, vector);
								if (num2 < num)
								{
									b = vector2;
									num = num2;
								}
							}
						}
					}
					Vector3 b2 = this.m_placementGhost.transform.position - b;
					if (component.m_waterPiece)
					{
						b2.y = 3f;
					}
					this.m_placementGhost.transform.position = vector + b2;
					this.m_placementGhost.transform.rotation = rotation;
				}
			}
			if (!flag)
			{
				this.m_tempPieces.Clear();
				Transform transform;
				Transform transform2;
				if (this.FindClosestSnapPoints(this.m_placementGhost.transform, 0.5f, out transform, out transform2, this.m_tempPieces))
				{
					Vector3 position = transform2.parent.position;
					Vector3 vector3 = transform2.position - (transform.position - this.m_placementGhost.transform.position);
					if (!this.IsOverlapingOtherPiece(vector3, this.m_placementGhost.name, this.m_tempPieces))
					{
						this.m_placementGhost.transform.position = vector3;
					}
				}
			}
			if (Location.IsInsideNoBuildLocation(this.m_placementGhost.transform.position))
			{
				this.m_placementStatus = Player.PlacementStatus.NoBuildZone;
			}
			PrivateArea component3 = component.GetComponent<PrivateArea>();
			float radius = component3 ? component3.m_radius : 0f;
			bool wardCheck = component3 != null;
			if (!PrivateArea.CheckAccess(this.m_placementGhost.transform.position, radius, flashGuardStone, wardCheck))
			{
				this.m_placementStatus = Player.PlacementStatus.PrivateZone;
			}
			if (this.CheckPlacementGhostVSPlayers())
			{
				this.m_placementStatus = Player.PlacementStatus.BlockedbyPlayer;
			}
			if (component.m_onlyInBiome != Heightmap.Biome.None && (Heightmap.FindBiome(this.m_placementGhost.transform.position) & component.m_onlyInBiome) == Heightmap.Biome.None)
			{
				this.m_placementStatus = Player.PlacementStatus.WrongBiome;
			}
			if (component.m_noClipping && this.TestGhostClipping(this.m_placementGhost, 0.2f))
			{
				this.m_placementStatus = Player.PlacementStatus.Invalid;
			}
		}
		else
		{
			if (this.m_placementMarkerInstance)
			{
				this.m_placementMarkerInstance.SetActive(false);
			}
			this.m_placementGhost.SetActive(false);
			this.m_placementStatus = Player.PlacementStatus.Invalid;
		}
		this.SetPlacementGhostValid(this.m_placementStatus == Player.PlacementStatus.Valid);
	}

	// Token: 0x060001AA RID: 426 RVA: 0x0000E5C0 File Offset: 0x0000C7C0
	private bool IsOverlapingOtherPiece(Vector3 p, string pieceName, List<Piece> pieces)
	{
		foreach (Piece piece in this.m_tempPieces)
		{
			if (Vector3.Distance(p, piece.transform.position) < 0.05f && piece.gameObject.name.StartsWith(pieceName))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060001AB RID: 427 RVA: 0x0000E640 File Offset: 0x0000C840
	private bool FindClosestSnapPoints(Transform ghost, float maxSnapDistance, out Transform a, out Transform b, List<Piece> pieces)
	{
		this.m_tempSnapPoints1.Clear();
		ghost.GetComponent<Piece>().GetSnapPoints(this.m_tempSnapPoints1);
		this.m_tempSnapPoints2.Clear();
		this.m_tempPieces.Clear();
		Piece.GetSnapPoints(ghost.transform.position, 10f, this.m_tempSnapPoints2, this.m_tempPieces);
		float num = 9999999f;
		a = null;
		b = null;
		foreach (Transform transform in this.m_tempSnapPoints1)
		{
			Transform transform2;
			float num2;
			if (this.FindClosestSnappoint(transform.position, this.m_tempSnapPoints2, maxSnapDistance, out transform2, out num2) && num2 < num)
			{
				num = num2;
				a = transform;
				b = transform2;
			}
		}
		return a != null;
	}

	// Token: 0x060001AC RID: 428 RVA: 0x0000E71C File Offset: 0x0000C91C
	private bool FindClosestSnappoint(Vector3 p, List<Transform> snapPoints, float maxDistance, out Transform closest, out float distance)
	{
		closest = null;
		distance = 999999f;
		foreach (Transform transform in snapPoints)
		{
			float num = Vector3.Distance(transform.position, p);
			if (num <= maxDistance && num < distance)
			{
				closest = transform;
				distance = num;
			}
		}
		return closest != null;
	}

	// Token: 0x060001AD RID: 429 RVA: 0x0000E798 File Offset: 0x0000C998
	private bool TestGhostClipping(GameObject ghost, float maxPenetration)
	{
		Collider[] componentsInChildren = ghost.GetComponentsInChildren<Collider>();
		Collider[] array = Physics.OverlapSphere(ghost.transform.position, 10f, this.m_placeRayMask);
		foreach (Collider collider in componentsInChildren)
		{
			foreach (Collider collider2 in array)
			{
				Vector3 vector;
				float num;
				if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, collider2, collider2.transform.position, collider2.transform.rotation, out vector, out num) && num > maxPenetration)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060001AE RID: 430 RVA: 0x0000E83C File Offset: 0x0000CA3C
	private bool CheckPlacementGhostVSPlayers()
	{
		if (this.m_placementGhost == null)
		{
			return false;
		}
		List<Character> list = new List<Character>();
		Character.GetCharactersInRange(base.transform.position, 30f, list);
		foreach (Collider collider in this.m_placementGhost.GetComponentsInChildren<Collider>())
		{
			if (!collider.isTrigger && collider.enabled)
			{
				MeshCollider meshCollider = collider as MeshCollider;
				if (!(meshCollider != null) || meshCollider.convex)
				{
					foreach (Character character in list)
					{
						CapsuleCollider collider2 = character.GetCollider();
						Vector3 vector;
						float num;
						if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, collider2, collider2.transform.position, collider2.transform.rotation, out vector, out num))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	// Token: 0x060001AF RID: 431 RVA: 0x0000E950 File Offset: 0x0000CB50
	private bool PieceRayTest(out Vector3 point, out Vector3 normal, out Piece piece, out Heightmap heightmap, out Collider waterSurface, bool water)
	{
		int layerMask = this.m_placeRayMask;
		if (water)
		{
			layerMask = this.m_placeWaterRayMask;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, out raycastHit, 50f, layerMask) && raycastHit.collider && !raycastHit.collider.attachedRigidbody && Vector3.Distance(this.m_eye.position, raycastHit.point) < this.m_maxPlaceDistance)
		{
			point = raycastHit.point;
			normal = raycastHit.normal;
			piece = raycastHit.collider.GetComponentInParent<Piece>();
			heightmap = raycastHit.collider.GetComponent<Heightmap>();
			if (raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
			{
				waterSurface = raycastHit.collider;
			}
			else
			{
				waterSurface = null;
			}
			return true;
		}
		point = Vector3.zero;
		normal = Vector3.zero;
		piece = null;
		heightmap = null;
		waterSurface = null;
		return false;
	}

	// Token: 0x060001B0 RID: 432 RVA: 0x0000EA70 File Offset: 0x0000CC70
	private void FindHoverObject(out GameObject hover, out Character hoverCreature)
	{
		hover = null;
		hoverCreature = null;
		RaycastHit[] array = Physics.RaycastAll(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, 50f, this.m_interactMask);
		Array.Sort<RaycastHit>(array, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));
		RaycastHit[] array2 = array;
		int i = 0;
		while (i < array2.Length)
		{
			RaycastHit raycastHit = array2[i];
			if (!raycastHit.collider.attachedRigidbody || !(raycastHit.collider.attachedRigidbody.gameObject == base.gameObject))
			{
				if (hoverCreature == null)
				{
					Character character = raycastHit.collider.attachedRigidbody ? raycastHit.collider.attachedRigidbody.GetComponent<Character>() : raycastHit.collider.GetComponent<Character>();
					if (character != null)
					{
						hoverCreature = character;
					}
				}
				if (Vector3.Distance(this.m_eye.position, raycastHit.point) >= this.m_maxInteractDistance)
				{
					break;
				}
				if (raycastHit.collider.GetComponent<Hoverable>() != null)
				{
					hover = raycastHit.collider.gameObject;
					return;
				}
				if (raycastHit.collider.attachedRigidbody)
				{
					hover = raycastHit.collider.attachedRigidbody.gameObject;
					return;
				}
				hover = raycastHit.collider.gameObject;
				return;
			}
			else
			{
				i++;
			}
		}
	}

	// Token: 0x060001B1 RID: 433 RVA: 0x0000EBE4 File Offset: 0x0000CDE4
	private void Interact(GameObject go, bool hold)
	{
		if (this.InAttack() || this.InDodge())
		{
			return;
		}
		if (hold && Time.time - this.m_lastHoverInteractTime < 0.2f)
		{
			return;
		}
		Interactable componentInParent = go.GetComponentInParent<Interactable>();
		if (componentInParent != null)
		{
			this.m_lastHoverInteractTime = Time.time;
			if (componentInParent.Interact(this, hold))
			{
				Vector3 forward = go.transform.position - base.transform.position;
				forward.y = 0f;
				forward.Normalize();
				base.transform.rotation = Quaternion.LookRotation(forward);
				this.m_zanim.SetTrigger("interact");
			}
		}
	}

	// Token: 0x060001B2 RID: 434 RVA: 0x0000EC88 File Offset: 0x0000CE88
	private void UpdateStations(float dt)
	{
		this.m_stationDiscoverTimer += dt;
		if (this.m_stationDiscoverTimer > 1f)
		{
			this.m_stationDiscoverTimer = 0f;
			CraftingStation.UpdateKnownStationsInRange(this);
		}
		if (this.m_currentStation != null)
		{
			if (!this.m_currentStation.InUseDistance(this))
			{
				InventoryGui.instance.Hide();
				this.SetCraftingStation(null);
				return;
			}
			if (!InventoryGui.IsVisible())
			{
				this.SetCraftingStation(null);
				return;
			}
			this.m_currentStation.PokeInUse();
			if (this.m_currentStation && !this.AlwaysRotateCamera())
			{
				Vector3 normalized = (this.m_currentStation.transform.position - base.transform.position).normalized;
				normalized.y = 0f;
				normalized.Normalize();
				Quaternion to = Quaternion.LookRotation(normalized);
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, this.m_turnSpeed * dt);
			}
		}
	}

	// Token: 0x060001B3 RID: 435 RVA: 0x0000ED88 File Offset: 0x0000CF88
	private void UpdateCover(float dt)
	{
		this.m_updateCoverTimer += dt;
		if (this.m_updateCoverTimer > 1f)
		{
			this.m_updateCoverTimer = 0f;
			Cover.GetCoverForPoint(base.GetCenterPoint(), out this.m_coverPercentage, out this.m_underRoof);
		}
	}

	// Token: 0x060001B4 RID: 436 RVA: 0x0000EDC7 File Offset: 0x0000CFC7
	public Character GetHoverCreature()
	{
		return this.m_hoveringCreature;
	}

	// Token: 0x060001B5 RID: 437 RVA: 0x0000EDCF File Offset: 0x0000CFCF
	public override GameObject GetHoverObject()
	{
		return this.m_hovering;
	}

	// Token: 0x060001B6 RID: 438 RVA: 0x0000EDD7 File Offset: 0x0000CFD7
	public override void OnNearFire(Vector3 point)
	{
		this.m_nearFireTimer = 0f;
	}

	// Token: 0x060001B7 RID: 439 RVA: 0x0000EDE4 File Offset: 0x0000CFE4
	public bool InShelter()
	{
		return this.m_coverPercentage >= 0.8f && this.m_underRoof;
	}

	// Token: 0x060001B8 RID: 440 RVA: 0x0000EDFB File Offset: 0x0000CFFB
	public float GetStamina()
	{
		return this.m_stamina;
	}

	// Token: 0x060001B9 RID: 441 RVA: 0x0000EE03 File Offset: 0x0000D003
	public override float GetMaxStamina()
	{
		return this.m_maxStamina;
	}

	// Token: 0x060001BA RID: 442 RVA: 0x0000EE0B File Offset: 0x0000D00B
	public override float GetStaminaPercentage()
	{
		return this.m_stamina / this.m_maxStamina;
	}

	// Token: 0x060001BB RID: 443 RVA: 0x0000EE1A File Offset: 0x0000D01A
	public void SetGodMode(bool godMode)
	{
		this.m_godMode = godMode;
	}

	// Token: 0x060001BC RID: 444 RVA: 0x0000EE23 File Offset: 0x0000D023
	public override bool InGodMode()
	{
		return this.m_godMode;
	}

	// Token: 0x060001BD RID: 445 RVA: 0x0000EE2B File Offset: 0x0000D02B
	public void SetGhostMode(bool ghostmode)
	{
		this.m_ghostMode = ghostmode;
	}

	// Token: 0x060001BE RID: 446 RVA: 0x0000EE34 File Offset: 0x0000D034
	public override bool InGhostMode()
	{
		return this.m_ghostMode;
	}

	// Token: 0x060001BF RID: 447 RVA: 0x0000EE3C File Offset: 0x0000D03C
	public override bool IsDebugFlying()
	{
		if (this.m_nview.IsOwner())
		{
			return this.m_debugFly;
		}
		return this.m_nview.GetZDO().GetBool("DebugFly", false);
	}

	// Token: 0x060001C0 RID: 448 RVA: 0x0000EE68 File Offset: 0x0000D068
	public override void AddStamina(float v)
	{
		this.m_stamina += v;
		if (this.m_stamina > this.m_maxStamina)
		{
			this.m_stamina = this.m_maxStamina;
		}
	}

	// Token: 0x060001C1 RID: 449 RVA: 0x0000EE94 File Offset: 0x0000D094
	public override void UseStamina(float v)
	{
		if (v == 0f)
		{
			return;
		}
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			this.RPC_UseStamina(0L, v);
			return;
		}
		this.m_nview.InvokeRPC("UseStamina", new object[]
		{
			v
		});
	}

	// Token: 0x060001C2 RID: 450 RVA: 0x0000EEEE File Offset: 0x0000D0EE
	private void RPC_UseStamina(long sender, float v)
	{
		if (v == 0f)
		{
			return;
		}
		this.m_stamina -= v;
		if (this.m_stamina < 0f)
		{
			this.m_stamina = 0f;
		}
		this.m_staminaRegenTimer = this.m_staminaRegenDelay;
	}

	// Token: 0x060001C3 RID: 451 RVA: 0x0000EF2C File Offset: 0x0000D12C
	public override bool HaveStamina(float amount = 0f)
	{
		if (this.m_nview.IsValid() && !this.m_nview.IsOwner())
		{
			return this.m_nview.GetZDO().GetFloat("stamina", this.m_maxStamina) > amount;
		}
		return this.m_stamina > amount;
	}

	// Token: 0x060001C4 RID: 452 RVA: 0x0000EF7C File Offset: 0x0000D17C
	public void Save(ZPackage pkg)
	{
		pkg.Write(24);
		pkg.Write(base.GetMaxHealth());
		pkg.Write(base.GetHealth());
		pkg.Write(this.GetMaxStamina());
		pkg.Write(this.m_firstSpawn);
		pkg.Write(this.m_timeSinceDeath);
		pkg.Write(this.m_guardianPower);
		pkg.Write(this.m_guardianPowerCooldown);
		this.m_inventory.Save(pkg);
		pkg.Write(this.m_knownRecipes.Count);
		foreach (string data in this.m_knownRecipes)
		{
			pkg.Write(data);
		}
		pkg.Write(this.m_knownStations.Count);
		foreach (KeyValuePair<string, int> keyValuePair in this.m_knownStations)
		{
			pkg.Write(keyValuePair.Key);
			pkg.Write(keyValuePair.Value);
		}
		pkg.Write(this.m_knownMaterial.Count);
		foreach (string data2 in this.m_knownMaterial)
		{
			pkg.Write(data2);
		}
		pkg.Write(this.m_shownTutorials.Count);
		foreach (string data3 in this.m_shownTutorials)
		{
			pkg.Write(data3);
		}
		pkg.Write(this.m_uniques.Count);
		foreach (string data4 in this.m_uniques)
		{
			pkg.Write(data4);
		}
		pkg.Write(this.m_trophies.Count);
		foreach (string data5 in this.m_trophies)
		{
			pkg.Write(data5);
		}
		pkg.Write(this.m_knownBiome.Count);
		foreach (Heightmap.Biome data6 in this.m_knownBiome)
		{
			pkg.Write((int)data6);
		}
		pkg.Write(this.m_knownTexts.Count);
		foreach (KeyValuePair<string, string> keyValuePair2 in this.m_knownTexts)
		{
			pkg.Write(keyValuePair2.Key);
			pkg.Write(keyValuePair2.Value);
		}
		pkg.Write(this.m_beardItem);
		pkg.Write(this.m_hairItem);
		pkg.Write(this.m_skinColor);
		pkg.Write(this.m_hairColor);
		pkg.Write(this.m_modelIndex);
		pkg.Write(this.m_foods.Count);
		foreach (Player.Food food in this.m_foods)
		{
			pkg.Write(food.m_name);
			pkg.Write(food.m_health);
			pkg.Write(food.m_stamina);
		}
		this.m_skills.Save(pkg);
	}

	// Token: 0x060001C5 RID: 453 RVA: 0x0000F380 File Offset: 0x0000D580
	public void Load(ZPackage pkg)
	{
		this.m_isLoading = true;
		base.UnequipAllItems();
		int num = pkg.ReadInt();
		if (num >= 7)
		{
			this.SetMaxHealth(pkg.ReadSingle(), false);
		}
		float num2 = pkg.ReadSingle();
		float maxHealth = base.GetMaxHealth();
		if (num2 <= 0f || num2 > maxHealth || float.IsNaN(num2))
		{
			num2 = maxHealth;
		}
		base.SetHealth(num2);
		if (num >= 10)
		{
			float stamina = pkg.ReadSingle();
			this.SetMaxStamina(stamina, false);
			this.m_stamina = stamina;
		}
		if (num >= 8)
		{
			this.m_firstSpawn = pkg.ReadBool();
		}
		if (num >= 20)
		{
			this.m_timeSinceDeath = pkg.ReadSingle();
		}
		if (num >= 23)
		{
			string guardianPower = pkg.ReadString();
			this.SetGuardianPower(guardianPower);
		}
		if (num >= 24)
		{
			this.m_guardianPowerCooldown = pkg.ReadSingle();
		}
		if (num == 2)
		{
			pkg.ReadZDOID();
		}
		this.m_inventory.Load(pkg);
		int num3 = pkg.ReadInt();
		for (int i = 0; i < num3; i++)
		{
			string item = pkg.ReadString();
			this.m_knownRecipes.Add(item);
		}
		if (num < 15)
		{
			int num4 = pkg.ReadInt();
			for (int j = 0; j < num4; j++)
			{
				pkg.ReadString();
			}
		}
		else
		{
			int num5 = pkg.ReadInt();
			for (int k = 0; k < num5; k++)
			{
				string key = pkg.ReadString();
				int value = pkg.ReadInt();
				this.m_knownStations.Add(key, value);
			}
		}
		int num6 = pkg.ReadInt();
		for (int l = 0; l < num6; l++)
		{
			string item2 = pkg.ReadString();
			this.m_knownMaterial.Add(item2);
		}
		if (num < 19 || num >= 21)
		{
			int num7 = pkg.ReadInt();
			for (int m = 0; m < num7; m++)
			{
				string item3 = pkg.ReadString();
				this.m_shownTutorials.Add(item3);
			}
		}
		if (num >= 6)
		{
			int num8 = pkg.ReadInt();
			for (int n = 0; n < num8; n++)
			{
				string item4 = pkg.ReadString();
				this.m_uniques.Add(item4);
			}
		}
		if (num >= 9)
		{
			int num9 = pkg.ReadInt();
			for (int num10 = 0; num10 < num9; num10++)
			{
				string item5 = pkg.ReadString();
				this.m_trophies.Add(item5);
			}
		}
		if (num >= 18)
		{
			int num11 = pkg.ReadInt();
			for (int num12 = 0; num12 < num11; num12++)
			{
				Heightmap.Biome item6 = (Heightmap.Biome)pkg.ReadInt();
				this.m_knownBiome.Add(item6);
			}
		}
		if (num >= 22)
		{
			int num13 = pkg.ReadInt();
			for (int num14 = 0; num14 < num13; num14++)
			{
				string key2 = pkg.ReadString();
				string value2 = pkg.ReadString();
				this.m_knownTexts.Add(key2, value2);
			}
		}
		if (num >= 4)
		{
			string beard = pkg.ReadString();
			string hair = pkg.ReadString();
			base.SetBeard(beard);
			base.SetHair(hair);
		}
		if (num >= 5)
		{
			Vector3 skinColor = pkg.ReadVector3();
			Vector3 hairColor = pkg.ReadVector3();
			this.SetSkinColor(skinColor);
			this.SetHairColor(hairColor);
		}
		if (num >= 11)
		{
			int playerModel = pkg.ReadInt();
			this.SetPlayerModel(playerModel);
		}
		if (num >= 12)
		{
			this.m_foods.Clear();
			int num15 = pkg.ReadInt();
			for (int num16 = 0; num16 < num15; num16++)
			{
				if (num >= 14)
				{
					Player.Food food = new Player.Food();
					food.m_name = pkg.ReadString();
					food.m_health = pkg.ReadSingle();
					if (num >= 16)
					{
						food.m_stamina = pkg.ReadSingle();
					}
					GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(food.m_name);
					if (itemPrefab == null)
					{
						ZLog.LogWarning("FAiled to find food item " + food.m_name);
					}
					else
					{
						food.m_item = itemPrefab.GetComponent<ItemDrop>().m_itemData;
						this.m_foods.Add(food);
					}
				}
				else
				{
					pkg.ReadString();
					pkg.ReadSingle();
					pkg.ReadSingle();
					pkg.ReadSingle();
					pkg.ReadSingle();
					pkg.ReadSingle();
					pkg.ReadSingle();
					if (num >= 13)
					{
						pkg.ReadSingle();
					}
				}
			}
		}
		if (num >= 17)
		{
			this.m_skills.Load(pkg);
		}
		this.m_isLoading = false;
		this.UpdateAvailablePiecesList();
		this.EquipIventoryItems();
	}

	// Token: 0x060001C6 RID: 454 RVA: 0x0000F7A8 File Offset: 0x0000D9A8
	private void EquipIventoryItems()
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory.GetEquipedtems())
		{
			if (!base.EquipItem(itemData, false))
			{
				itemData.m_equiped = false;
			}
		}
	}

	// Token: 0x060001C7 RID: 455 RVA: 0x0000F80C File Offset: 0x0000DA0C
	public override bool CanMove()
	{
		return !this.m_teleporting && !this.InCutscene() && (!this.IsEncumbered() || this.HaveStamina(0f)) && base.CanMove();
	}

	// Token: 0x060001C8 RID: 456 RVA: 0x0000F83F File Offset: 0x0000DA3F
	public override bool IsEncumbered()
	{
		return this.m_inventory.GetTotalWeight() > this.GetMaxCarryWeight();
	}

	// Token: 0x060001C9 RID: 457 RVA: 0x0000F854 File Offset: 0x0000DA54
	public float GetMaxCarryWeight()
	{
		float maxCarryWeight = this.m_maxCarryWeight;
		this.m_seman.ModifyMaxCarryWeight(maxCarryWeight, ref maxCarryWeight);
		return maxCarryWeight;
	}

	// Token: 0x060001CA RID: 458 RVA: 0x0000F877 File Offset: 0x0000DA77
	public override bool HaveUniqueKey(string name)
	{
		return this.m_uniques.Contains(name);
	}

	// Token: 0x060001CB RID: 459 RVA: 0x0000F885 File Offset: 0x0000DA85
	public override void AddUniqueKey(string name)
	{
		if (!this.m_uniques.Contains(name))
		{
			this.m_uniques.Add(name);
		}
	}

	// Token: 0x060001CC RID: 460 RVA: 0x0000F8A2 File Offset: 0x0000DAA2
	public bool IsBiomeKnown(Heightmap.Biome biome)
	{
		return this.m_knownBiome.Contains(biome);
	}

	// Token: 0x060001CD RID: 461 RVA: 0x0000F8B0 File Offset: 0x0000DAB0
	public void AddKnownBiome(Heightmap.Biome biome)
	{
		if (!this.m_knownBiome.Contains(biome))
		{
			this.m_knownBiome.Add(biome);
			if (biome != Heightmap.Biome.Meadows && biome != Heightmap.Biome.None)
			{
				string text = "$biome_" + biome.ToString().ToLower();
				MessageHud.instance.ShowBiomeFoundMsg(text, true);
			}
			if (biome == Heightmap.Biome.BlackForest && !ZoneSystem.instance.GetGlobalKey("defeated_eikthyr"))
			{
				this.ShowTutorial("blackforest", false);
			}
			Gogan.LogEvent("Game", "BiomeFound", biome.ToString(), 0L);
		}
	}

	// Token: 0x060001CE RID: 462 RVA: 0x0000F947 File Offset: 0x0000DB47
	public bool IsRecipeKnown(string name)
	{
		return this.m_knownRecipes.Contains(name);
	}

	// Token: 0x060001CF RID: 463 RVA: 0x0000F958 File Offset: 0x0000DB58
	public void AddKnownRecipe(Recipe recipe)
	{
		if (!this.m_knownRecipes.Contains(recipe.m_item.m_itemData.m_shared.m_name))
		{
			this.m_knownRecipes.Add(recipe.m_item.m_itemData.m_shared.m_name);
			MessageHud.instance.QueueUnlockMsg(recipe.m_item.m_itemData.GetIcon(), "$msg_newrecipe", recipe.m_item.m_itemData.m_shared.m_name);
			Gogan.LogEvent("Game", "RecipeFound", recipe.m_item.m_itemData.m_shared.m_name, 0L);
		}
	}

	// Token: 0x060001D0 RID: 464 RVA: 0x0000FA04 File Offset: 0x0000DC04
	public void AddKnownPiece(Piece piece)
	{
		if (!this.m_knownRecipes.Contains(piece.m_name))
		{
			this.m_knownRecipes.Add(piece.m_name);
			MessageHud.instance.QueueUnlockMsg(piece.m_icon, "$msg_newpiece", piece.m_name);
			Gogan.LogEvent("Game", "PieceFound", piece.m_name, 0L);
		}
	}

	// Token: 0x060001D1 RID: 465 RVA: 0x0000FA68 File Offset: 0x0000DC68
	public void AddKnownStation(CraftingStation station)
	{
		int level = station.GetLevel();
		int num;
		if (this.m_knownStations.TryGetValue(station.m_name, out num))
		{
			if (num < level)
			{
				this.m_knownStations[station.m_name] = level;
				MessageHud.instance.QueueUnlockMsg(station.m_icon, "$msg_newstation_level", station.m_name + " $msg_level " + level);
				this.UpdateKnownRecipesList();
			}
			return;
		}
		this.m_knownStations.Add(station.m_name, level);
		MessageHud.instance.QueueUnlockMsg(station.m_icon, "$msg_newstation", station.m_name);
		Gogan.LogEvent("Game", "StationFound", station.m_name, 0L);
		this.UpdateKnownRecipesList();
	}

	// Token: 0x060001D2 RID: 466 RVA: 0x0000FB24 File Offset: 0x0000DD24
	private bool KnowStationLevel(string name, int level)
	{
		int num;
		return this.m_knownStations.TryGetValue(name, out num) && num >= level;
	}

	// Token: 0x060001D3 RID: 467 RVA: 0x0000FB4C File Offset: 0x0000DD4C
	public void AddKnownText(string label, string text)
	{
		if (label.Length == 0)
		{
			ZLog.LogWarning("Text " + text + " Is missing label");
			return;
		}
		if (!this.m_knownTexts.ContainsKey(label))
		{
			this.m_knownTexts.Add(label, text);
			this.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_newtext", new string[]
			{
				label
			}), 0, this.m_textIcon);
		}
	}

	// Token: 0x060001D4 RID: 468 RVA: 0x0000FBB9 File Offset: 0x0000DDB9
	public List<KeyValuePair<string, string>> GetKnownTexts()
	{
		return this.m_knownTexts.ToList<KeyValuePair<string, string>>();
	}

	// Token: 0x060001D5 RID: 469 RVA: 0x0000FBC8 File Offset: 0x0000DDC8
	public void AddKnownItem(ItemDrop.ItemData item)
	{
		if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophie)
		{
			this.AddTrophie(item);
		}
		if (!this.m_knownMaterial.Contains(item.m_shared.m_name))
		{
			this.m_knownMaterial.Add(item.m_shared.m_name);
			if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Material)
			{
				MessageHud.instance.QueueUnlockMsg(item.GetIcon(), "$msg_newmaterial", item.m_shared.m_name);
			}
			else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophie)
			{
				MessageHud.instance.QueueUnlockMsg(item.GetIcon(), "$msg_newtrophy", item.m_shared.m_name);
			}
			else
			{
				MessageHud.instance.QueueUnlockMsg(item.GetIcon(), "$msg_newitem", item.m_shared.m_name);
			}
			Gogan.LogEvent("Game", "ItemFound", item.m_shared.m_name, 0L);
			this.UpdateKnownRecipesList();
		}
	}

	// Token: 0x060001D6 RID: 470 RVA: 0x0000FCC0 File Offset: 0x0000DEC0
	private void AddTrophie(ItemDrop.ItemData item)
	{
		if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Trophie)
		{
			return;
		}
		if (!this.m_trophies.Contains(item.m_dropPrefab.name))
		{
			this.m_trophies.Add(item.m_dropPrefab.name);
		}
	}

	// Token: 0x060001D7 RID: 471 RVA: 0x0000FD0C File Offset: 0x0000DF0C
	public List<string> GetTrophies()
	{
		List<string> list = new List<string>();
		list.AddRange(this.m_trophies);
		return list;
	}

	// Token: 0x060001D8 RID: 472 RVA: 0x0000FD20 File Offset: 0x0000DF20
	private void UpdateKnownRecipesList()
	{
		if (Game.instance == null)
		{
			return;
		}
		foreach (Recipe recipe in ObjectDB.instance.m_recipes)
		{
			if (recipe.m_enabled && !this.m_knownRecipes.Contains(recipe.m_item.m_itemData.m_shared.m_name) && this.HaveRequirements(recipe, true, 0))
			{
				this.AddKnownRecipe(recipe);
			}
		}
		this.m_tempOwnedPieceTables.Clear();
		this.m_inventory.GetAllPieceTables(this.m_tempOwnedPieceTables);
		bool flag = false;
		foreach (PieceTable pieceTable in this.m_tempOwnedPieceTables)
		{
			foreach (GameObject gameObject in pieceTable.m_pieces)
			{
				Piece component = gameObject.GetComponent<Piece>();
				if (component.m_enabled && !this.m_knownRecipes.Contains(component.m_name) && this.HaveRequirements(component, Player.RequirementMode.IsKnown))
				{
					this.AddKnownPiece(component);
					flag = true;
				}
			}
		}
		if (flag)
		{
			this.UpdateAvailablePiecesList();
		}
	}

	// Token: 0x060001D9 RID: 473 RVA: 0x0000FE90 File Offset: 0x0000E090
	private void UpdateAvailablePiecesList()
	{
		if (this.m_buildPieces != null)
		{
			this.m_buildPieces.UpdateAvailable(this.m_knownRecipes, this, this.m_hideUnavailable, this.m_noPlacementCost);
		}
		this.SetupPlacementGhost();
	}

	// Token: 0x060001DA RID: 474 RVA: 0x0000FEC4 File Offset: 0x0000E0C4
	public override void Message(MessageHud.MessageType type, string msg, int amount = 0, Sprite icon = null)
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			if (MessageHud.instance)
			{
				MessageHud.instance.ShowMessage(type, msg, amount, icon);
				return;
			}
		}
		else
		{
			this.m_nview.InvokeRPC("Message", new object[]
			{
				(int)type,
				msg,
				amount
			});
		}
	}

	// Token: 0x060001DB RID: 475 RVA: 0x0000FF42 File Offset: 0x0000E142
	private void RPC_Message(long sender, int type, string msg, int amount)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (MessageHud.instance)
		{
			MessageHud.instance.ShowMessage((MessageHud.MessageType)type, msg, amount, null);
		}
	}

	// Token: 0x060001DC RID: 476 RVA: 0x0000FF70 File Offset: 0x0000E170
	public static Player GetPlayer(long playerID)
	{
		foreach (Player player in Player.m_players)
		{
			if (player.GetPlayerID() == playerID)
			{
				return player;
			}
		}
		return null;
	}

	// Token: 0x060001DD RID: 477 RVA: 0x0000FFCC File Offset: 0x0000E1CC
	public static Player GetClosestPlayer(Vector3 point, float maxRange)
	{
		Player result = null;
		float num = 999999f;
		foreach (Player player in Player.m_players)
		{
			float num2 = Vector3.Distance(player.transform.position, point);
			if (num2 < num && num2 < maxRange)
			{
				num = num2;
				result = player;
			}
		}
		return result;
	}

	// Token: 0x060001DE RID: 478 RVA: 0x00010044 File Offset: 0x0000E244
	public static bool IsPlayerInRange(Vector3 point, float range, long playerID)
	{
		foreach (Player player in Player.m_players)
		{
			if (player.GetPlayerID() == playerID)
			{
				return Utils.DistanceXZ(player.transform.position, point) < range;
			}
		}
		return false;
	}

	// Token: 0x060001DF RID: 479 RVA: 0x000100B4 File Offset: 0x0000E2B4
	public static void MessageAllInRange(Vector3 point, float range, MessageHud.MessageType type, string msg, Sprite icon = null)
	{
		foreach (Player player in Player.m_players)
		{
			if (Vector3.Distance(player.transform.position, point) < range)
			{
				player.Message(type, msg, 0, icon);
			}
		}
	}

	// Token: 0x060001E0 RID: 480 RVA: 0x00010120 File Offset: 0x0000E320
	public static int GetPlayersInRangeXZ(Vector3 point, float range)
	{
		int num = 0;
		using (List<Player>.Enumerator enumerator = Player.m_players.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (Utils.DistanceXZ(enumerator.Current.transform.position, point) < range)
				{
					num++;
				}
			}
		}
		return num;
	}

	// Token: 0x060001E1 RID: 481 RVA: 0x00010184 File Offset: 0x0000E384
	public static void GetPlayersInRange(Vector3 point, float range, List<Player> players)
	{
		foreach (Player player in Player.m_players)
		{
			if (Vector3.Distance(player.transform.position, point) < range)
			{
				players.Add(player);
			}
		}
	}

	// Token: 0x060001E2 RID: 482 RVA: 0x000101EC File Offset: 0x0000E3EC
	public static bool IsPlayerInRange(Vector3 point, float range)
	{
		using (List<Player>.Enumerator enumerator = Player.m_players.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (Vector3.Distance(enumerator.Current.transform.position, point) < range)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060001E3 RID: 483 RVA: 0x00010250 File Offset: 0x0000E450
	public static bool IsPlayerInRange(Vector3 point, float range, float minNoise)
	{
		foreach (Player player in Player.m_players)
		{
			if (Vector3.Distance(player.transform.position, point) < range)
			{
				float noiseRange = player.GetNoiseRange();
				if (range <= noiseRange && noiseRange >= minNoise)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060001E4 RID: 484 RVA: 0x000102C8 File Offset: 0x0000E4C8
	public static Player GetPlayerNoiseRange(Vector3 point, float noiseRangeScale = 1f)
	{
		foreach (Player player in Player.m_players)
		{
			float num = Vector3.Distance(player.transform.position, point);
			float noiseRange = player.GetNoiseRange();
			if (num < noiseRange * noiseRangeScale)
			{
				return player;
			}
		}
		return null;
	}

	// Token: 0x060001E5 RID: 485 RVA: 0x0001033C File Offset: 0x0000E53C
	public static List<Player> GetAllPlayers()
	{
		return Player.m_players;
	}

	// Token: 0x060001E6 RID: 486 RVA: 0x00010343 File Offset: 0x0000E543
	public static Player GetRandomPlayer()
	{
		if (Player.m_players.Count == 0)
		{
			return null;
		}
		return Player.m_players[UnityEngine.Random.Range(0, Player.m_players.Count)];
	}

	// Token: 0x060001E7 RID: 487 RVA: 0x00010370 File Offset: 0x0000E570
	public void GetAvailableRecipes(ref List<Recipe> available)
	{
		available.Clear();
		foreach (Recipe recipe in ObjectDB.instance.m_recipes)
		{
			if (recipe.m_enabled && (recipe.m_item.m_itemData.m_shared.m_dlc.Length <= 0 || DLCMan.instance.IsDLCInstalled(recipe.m_item.m_itemData.m_shared.m_dlc)) && (this.m_knownRecipes.Contains(recipe.m_item.m_itemData.m_shared.m_name) || this.m_noPlacementCost) && (this.RequiredCraftingStation(recipe, 1, false) || this.m_noPlacementCost))
			{
				available.Add(recipe);
			}
		}
	}

	// Token: 0x060001E8 RID: 488 RVA: 0x0001045C File Offset: 0x0000E65C
	private void OnInventoryChanged()
	{
		if (this.m_isLoading)
		{
			return;
		}
		foreach (ItemDrop.ItemData itemData in this.m_inventory.GetAllItems())
		{
			this.AddKnownItem(itemData);
			if (itemData.m_shared.m_name == "$item_hammer")
			{
				this.ShowTutorial("hammer", false);
			}
			else if (itemData.m_shared.m_name == "$item_hoe")
			{
				this.ShowTutorial("hoe", false);
			}
			else if (itemData.m_shared.m_name == "$item_pickaxe_antler")
			{
				this.ShowTutorial("pickaxe", false);
			}
			if (itemData.m_shared.m_name == "$item_trophy_eikthyr")
			{
				this.ShowTutorial("boss_trophy", false);
			}
			if (itemData.m_shared.m_name == "$item_wishbone")
			{
				this.ShowTutorial("wishbone", false);
			}
			else if (itemData.m_shared.m_name == "$item_copperore" || itemData.m_shared.m_name == "$item_tinore")
			{
				this.ShowTutorial("ore", false);
			}
			else if (itemData.m_shared.m_food > 0f)
			{
				this.ShowTutorial("food", false);
			}
		}
		this.UpdateKnownRecipesList();
		this.UpdateAvailablePiecesList();
	}

	// Token: 0x060001E9 RID: 489 RVA: 0x000105EC File Offset: 0x0000E7EC
	public bool InDebugFlyMode()
	{
		return this.m_debugFly;
	}

	// Token: 0x060001EA RID: 490 RVA: 0x000105F4 File Offset: 0x0000E7F4
	public void ShowTutorial(string name, bool force = false)
	{
		if (this.HaveSeenTutorial(name))
		{
			return;
		}
		Tutorial.instance.ShowText(name, force);
	}

	// Token: 0x060001EB RID: 491 RVA: 0x0001060C File Offset: 0x0000E80C
	public void SetSeenTutorial(string name)
	{
		if (name.Length == 0)
		{
			return;
		}
		if (this.m_shownTutorials.Contains(name))
		{
			return;
		}
		this.m_shownTutorials.Add(name);
	}

	// Token: 0x060001EC RID: 492 RVA: 0x00010633 File Offset: 0x0000E833
	public bool HaveSeenTutorial(string name)
	{
		return name.Length != 0 && this.m_shownTutorials.Contains(name);
	}

	// Token: 0x060001ED RID: 493 RVA: 0x0001064B File Offset: 0x0000E84B
	public static bool IsSeenTutorialsCleared()
	{
		return !Player.m_localPlayer || Player.m_localPlayer.m_shownTutorials.Count == 0;
	}

	// Token: 0x060001EE RID: 494 RVA: 0x0001066D File Offset: 0x0000E86D
	public static void ResetSeenTutorials()
	{
		if (Player.m_localPlayer)
		{
			Player.m_localPlayer.m_shownTutorials.Clear();
		}
	}

	// Token: 0x060001EF RID: 495 RVA: 0x0001068C File Offset: 0x0000E88C
	public void SetMouseLook(Vector2 mouseLook)
	{
		this.m_lookYaw *= Quaternion.Euler(0f, mouseLook.x, 0f);
		this.m_lookPitch = Mathf.Clamp(this.m_lookPitch - mouseLook.y, -89f, 89f);
		this.UpdateEyeRotation();
		this.m_lookDir = this.m_eye.forward;
	}

	// Token: 0x060001F0 RID: 496 RVA: 0x000106F8 File Offset: 0x0000E8F8
	protected override void UpdateEyeRotation()
	{
		this.m_eye.rotation = this.m_lookYaw * Quaternion.Euler(this.m_lookPitch, 0f, 0f);
	}

	// Token: 0x060001F1 RID: 497 RVA: 0x00010725 File Offset: 0x0000E925
	public Ragdoll GetRagdoll()
	{
		return this.m_ragdoll;
	}

	// Token: 0x060001F2 RID: 498 RVA: 0x0001072D File Offset: 0x0000E92D
	public void OnDodgeMortal()
	{
		this.m_dodgeInvincible = false;
	}

	// Token: 0x060001F3 RID: 499 RVA: 0x00010738 File Offset: 0x0000E938
	private void UpdateDodge(float dt)
	{
		this.m_queuedDodgeTimer -= dt;
		if (this.m_queuedDodgeTimer > 0f && base.IsOnGround() && !this.IsDead() && !this.InAttack() && !this.IsEncumbered() && !this.InDodge())
		{
			float num = this.m_dodgeStaminaUsage - this.m_dodgeStaminaUsage * this.m_equipmentMovementModifier;
			if (this.HaveStamina(num))
			{
				this.AbortEquipQueue();
				this.m_queuedDodgeTimer = 0f;
				this.m_dodgeInvincible = true;
				base.transform.rotation = Quaternion.LookRotation(this.m_queuedDodgeDir);
				this.m_body.rotation = base.transform.rotation;
				this.m_zanim.SetTrigger("dodge");
				base.AddNoise(5f);
				this.UseStamina(num);
				this.m_dodgeEffects.Create(base.transform.position, Quaternion.identity, base.transform, 1f);
			}
			else
			{
				Hud.instance.StaminaBarNoStaminaFlash();
			}
		}
		AnimatorStateInfo currentAnimatorStateInfo = this.m_animator.GetCurrentAnimatorStateInfo(0);
		AnimatorStateInfo nextAnimatorStateInfo = this.m_animator.GetNextAnimatorStateInfo(0);
		bool flag = this.m_animator.IsInTransition(0);
		bool flag2 = this.m_animator.GetBool("dodge") || (currentAnimatorStateInfo.tagHash == Player.m_animatorTagDodge && !flag) || (flag && nextAnimatorStateInfo.tagHash == Player.m_animatorTagDodge);
		bool value = flag2 && this.m_dodgeInvincible;
		this.m_nview.GetZDO().Set("dodgeinv", value);
		this.m_inDodge = flag2;
	}

	// Token: 0x060001F4 RID: 500 RVA: 0x000108E5 File Offset: 0x0000EAE5
	public override bool IsDodgeInvincible()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetBool("dodgeinv", false);
	}

	// Token: 0x060001F5 RID: 501 RVA: 0x0001090C File Offset: 0x0000EB0C
	public override bool InDodge()
	{
		return this.m_nview.IsValid() && this.m_nview.IsOwner() && this.m_inDodge;
	}

	// Token: 0x060001F6 RID: 502 RVA: 0x00010930 File Offset: 0x0000EB30
	public override bool IsDead()
	{
		ZDO zdo = this.m_nview.GetZDO();
		return zdo != null && zdo.GetBool("dead", false);
	}

	// Token: 0x060001F7 RID: 503 RVA: 0x0001095A File Offset: 0x0000EB5A
	protected void Dodge(Vector3 dodgeDir)
	{
		this.m_queuedDodgeTimer = 0.5f;
		this.m_queuedDodgeDir = dodgeDir;
	}

	// Token: 0x060001F8 RID: 504 RVA: 0x00010970 File Offset: 0x0000EB70
	public override bool AlwaysRotateCamera()
	{
		if ((base.GetCurrentWeapon() != null && this.m_currentAttack != null && this.m_lastCombatTimer < 1f && this.m_currentAttack.m_attackType != Attack.AttackType.None && ZInput.IsMouseActive()) || this.IsHoldingAttack() || this.m_blocking)
		{
			return true;
		}
		if (this.InPlaceMode())
		{
			Vector3 from = base.GetLookYaw() * Vector3.forward;
			Vector3 forward = base.transform.forward;
			if (Vector3.Angle(from, forward) > 90f)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060001F9 RID: 505 RVA: 0x000109F8 File Offset: 0x0000EBF8
	public override bool TeleportTo(Vector3 pos, Quaternion rot, bool distantTeleport)
	{
		if (this.IsTeleporting())
		{
			return false;
		}
		if (this.m_teleportCooldown < 2f)
		{
			return false;
		}
		this.m_teleporting = true;
		this.m_distantTeleport = distantTeleport;
		this.m_teleportTimer = 0f;
		this.m_teleportCooldown = 0f;
		this.m_teleportFromPos = base.transform.position;
		this.m_teleportFromRot = base.transform.rotation;
		this.m_teleportTargetPos = pos;
		this.m_teleportTargetRot = rot;
		return true;
	}

	// Token: 0x060001FA RID: 506 RVA: 0x00010A74 File Offset: 0x0000EC74
	private void UpdateTeleport(float dt)
	{
		if (!this.m_teleporting)
		{
			this.m_teleportCooldown += dt;
			return;
		}
		this.m_teleportCooldown = 0f;
		this.m_teleportTimer += dt;
		if (this.m_teleportTimer > 2f)
		{
			Vector3 lookDir = this.m_teleportTargetRot * Vector3.forward;
			base.transform.position = this.m_teleportTargetPos;
			base.transform.rotation = this.m_teleportTargetRot;
			this.m_body.velocity = Vector3.zero;
			this.m_maxAirAltitude = base.transform.position.y;
			base.SetLookDir(lookDir);
			if ((this.m_teleportTimer > 8f || !this.m_distantTeleport) && ZNetScene.instance.IsAreaReady(this.m_teleportTargetPos))
			{
				float num = 0f;
				if (ZoneSystem.instance.FindFloor(this.m_teleportTargetPos, out num))
				{
					this.m_teleportTimer = 0f;
					this.m_teleporting = false;
					base.ResetCloth();
					return;
				}
				if (this.m_teleportTimer > 15f || !this.m_distantTeleport)
				{
					if (this.m_distantTeleport)
					{
						Vector3 position = base.transform.position;
						position.y = ZoneSystem.instance.GetSolidHeight(this.m_teleportTargetPos) + 0.5f;
						base.transform.position = position;
					}
					else
					{
						base.transform.rotation = this.m_teleportFromRot;
						base.transform.position = this.m_teleportFromPos;
						this.m_maxAirAltitude = base.transform.position.y;
						this.Message(MessageHud.MessageType.Center, "$msg_portal_blocked", 0, null);
					}
					this.m_teleportTimer = 0f;
					this.m_teleporting = false;
					base.ResetCloth();
				}
			}
		}
	}

	// Token: 0x060001FB RID: 507 RVA: 0x00010C36 File Offset: 0x0000EE36
	public override bool IsTeleporting()
	{
		return this.m_teleporting;
	}

	// Token: 0x060001FC RID: 508 RVA: 0x00010C3E File Offset: 0x0000EE3E
	public bool ShowTeleportAnimation()
	{
		return this.m_teleporting && this.m_distantTeleport;
	}

	// Token: 0x060001FD RID: 509 RVA: 0x00010C50 File Offset: 0x0000EE50
	public void SetPlayerModel(int index)
	{
		if (this.m_modelIndex == index)
		{
			return;
		}
		this.m_modelIndex = index;
		this.m_visEquipment.SetModel(index);
	}

	// Token: 0x060001FE RID: 510 RVA: 0x00010C6F File Offset: 0x0000EE6F
	public int GetPlayerModel()
	{
		return this.m_modelIndex;
	}

	// Token: 0x060001FF RID: 511 RVA: 0x00010C77 File Offset: 0x0000EE77
	public void SetSkinColor(Vector3 color)
	{
		if (color == this.m_skinColor)
		{
			return;
		}
		this.m_skinColor = color;
		this.m_visEquipment.SetSkinColor(this.m_skinColor);
	}

	// Token: 0x06000200 RID: 512 RVA: 0x00010CA0 File Offset: 0x0000EEA0
	public void SetHairColor(Vector3 color)
	{
		if (this.m_hairColor == color)
		{
			return;
		}
		this.m_hairColor = color;
		this.m_visEquipment.SetHairColor(this.m_hairColor);
	}

	// Token: 0x06000201 RID: 513 RVA: 0x00010CC9 File Offset: 0x0000EEC9
	protected override void SetupVisEquipment(VisEquipment visEq, bool isRagdoll)
	{
		base.SetupVisEquipment(visEq, isRagdoll);
		visEq.SetModel(this.m_modelIndex);
		visEq.SetSkinColor(this.m_skinColor);
		visEq.SetHairColor(this.m_hairColor);
	}

	// Token: 0x06000202 RID: 514 RVA: 0x00010CF8 File Offset: 0x0000EEF8
	public override bool CanConsumeItem(ItemDrop.ItemData item)
	{
		if (item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Consumable)
		{
			return false;
		}
		if (item.m_shared.m_food > 0f && !this.CanEat(item, true))
		{
			return false;
		}
		if (item.m_shared.m_consumeStatusEffect)
		{
			StatusEffect consumeStatusEffect = item.m_shared.m_consumeStatusEffect;
			if (this.m_seman.HaveStatusEffect(item.m_shared.m_consumeStatusEffect.name) || this.m_seman.HaveStatusEffectCategory(consumeStatusEffect.m_category))
			{
				this.Message(MessageHud.MessageType.Center, "$msg_cantconsume", 0, null);
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000203 RID: 515 RVA: 0x00010D94 File Offset: 0x0000EF94
	public override bool ConsumeItem(Inventory inventory, ItemDrop.ItemData item)
	{
		if (!this.CanConsumeItem(item))
		{
			return false;
		}
		if (item.m_shared.m_consumeStatusEffect)
		{
			StatusEffect consumeStatusEffect = item.m_shared.m_consumeStatusEffect;
			this.m_seman.AddStatusEffect(item.m_shared.m_consumeStatusEffect, true);
		}
		if (item.m_shared.m_food > 0f)
		{
			this.EatFood(item);
		}
		inventory.RemoveOneItem(item);
		return true;
	}

	// Token: 0x06000204 RID: 516 RVA: 0x00010E05 File Offset: 0x0000F005
	public void SetIntro(bool intro)
	{
		if (this.m_intro == intro)
		{
			return;
		}
		this.m_intro = intro;
		this.m_zanim.SetBool("intro", intro);
	}

	// Token: 0x06000205 RID: 517 RVA: 0x00010E29 File Offset: 0x0000F029
	public override bool InIntro()
	{
		return this.m_intro;
	}

	// Token: 0x06000206 RID: 518 RVA: 0x00010E34 File Offset: 0x0000F034
	public override bool InCutscene()
	{
		return this.m_animator.GetCurrentAnimatorStateInfo(0).tagHash == Player.m_animatorTagCutscene || this.InIntro() || this.m_sleeping || base.InCutscene();
	}

	// Token: 0x06000207 RID: 519 RVA: 0x00010E78 File Offset: 0x0000F078
	public void SetMaxStamina(float stamina, bool flashBar)
	{
		if (flashBar && Hud.instance != null && stamina > this.m_maxStamina)
		{
			Hud.instance.StaminaBarUppgradeFlash();
		}
		this.m_maxStamina = stamina;
		this.m_stamina = Mathf.Clamp(this.m_stamina, 0f, this.m_maxStamina);
	}

	// Token: 0x06000208 RID: 520 RVA: 0x00010ECB File Offset: 0x0000F0CB
	public void SetMaxHealth(float health, bool flashBar)
	{
		if (flashBar && Hud.instance != null && health > base.GetMaxHealth())
		{
			Hud.instance.FlashHealthBar();
		}
		base.SetMaxHealth(health);
	}

	// Token: 0x06000209 RID: 521 RVA: 0x00010EF7 File Offset: 0x0000F0F7
	public override bool IsPVPEnabled()
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (this.m_nview.IsOwner())
		{
			return this.m_pvp;
		}
		return this.m_nview.GetZDO().GetBool("pvp", false);
	}

	// Token: 0x0600020A RID: 522 RVA: 0x00010F34 File Offset: 0x0000F134
	public void SetPVP(bool enabled)
	{
		if (this.m_pvp == enabled)
		{
			return;
		}
		this.m_pvp = enabled;
		this.m_nview.GetZDO().Set("pvp", this.m_pvp);
		if (this.m_pvp)
		{
			this.Message(MessageHud.MessageType.Center, "$msg_pvpon", 0, null);
			return;
		}
		this.Message(MessageHud.MessageType.Center, "$msg_pvpoff", 0, null);
	}

	// Token: 0x0600020B RID: 523 RVA: 0x00010F92 File Offset: 0x0000F192
	public bool CanSwitchPVP()
	{
		return this.m_lastCombatTimer > 10f;
	}

	// Token: 0x0600020C RID: 524 RVA: 0x00010FA1 File Offset: 0x0000F1A1
	public bool NoCostCheat()
	{
		return this.m_noPlacementCost;
	}

	// Token: 0x0600020D RID: 525 RVA: 0x00010FAC File Offset: 0x0000F1AC
	public void StartEmote(string emote, bool oneshot = true)
	{
		if (!this.CanMove() || this.InAttack() || this.IsHoldingAttack())
		{
			return;
		}
		this.SetCrouch(false);
		int @int = this.m_nview.GetZDO().GetInt("emoteID", 0);
		this.m_nview.GetZDO().Set("emoteID", @int + 1);
		this.m_nview.GetZDO().Set("emote", emote);
		this.m_nview.GetZDO().Set("emote_oneshot", oneshot);
	}

	// Token: 0x0600020E RID: 526 RVA: 0x00011034 File Offset: 0x0000F234
	protected override void StopEmote()
	{
		if (this.m_nview.GetZDO().GetString("emote", "") != "")
		{
			int @int = this.m_nview.GetZDO().GetInt("emoteID", 0);
			this.m_nview.GetZDO().Set("emoteID", @int + 1);
			this.m_nview.GetZDO().Set("emote", "");
		}
	}

	// Token: 0x0600020F RID: 527 RVA: 0x000110B0 File Offset: 0x0000F2B0
	private void UpdateEmote()
	{
		if (this.m_nview.IsOwner() && this.InEmote() && this.m_moveDir != Vector3.zero)
		{
			this.StopEmote();
		}
		int @int = this.m_nview.GetZDO().GetInt("emoteID", 0);
		if (@int != this.m_emoteID)
		{
			this.m_emoteID = @int;
			if (!string.IsNullOrEmpty(this.m_emoteState))
			{
				this.m_animator.SetBool("emote_" + this.m_emoteState, false);
			}
			this.m_emoteState = "";
			this.m_animator.SetTrigger("emote_stop");
			string @string = this.m_nview.GetZDO().GetString("emote", "");
			if (!string.IsNullOrEmpty(@string))
			{
				bool @bool = this.m_nview.GetZDO().GetBool("emote_oneshot", false);
				this.m_animator.ResetTrigger("emote_stop");
				if (@bool)
				{
					this.m_animator.SetTrigger("emote_" + @string);
					return;
				}
				this.m_emoteState = @string;
				this.m_animator.SetBool("emote_" + @string, true);
			}
		}
	}

	// Token: 0x06000210 RID: 528 RVA: 0x000111D8 File Offset: 0x0000F3D8
	public override bool InEmote()
	{
		return !string.IsNullOrEmpty(this.m_emoteState) || this.m_animator.GetCurrentAnimatorStateInfo(0).tagHash == Player.m_animatorTagEmote;
	}

	// Token: 0x06000211 RID: 529 RVA: 0x00011210 File Offset: 0x0000F410
	public override bool IsCrouching()
	{
		return this.m_animator.GetCurrentAnimatorStateInfo(0).tagHash == Player.m_animatorTagCrouch;
	}

	// Token: 0x06000212 RID: 530 RVA: 0x00011238 File Offset: 0x0000F438
	private void UpdateCrouch(float dt)
	{
		if (this.m_crouchToggled)
		{
			if (!this.HaveStamina(0f) || base.IsSwiming() || this.InBed() || this.InPlaceMode() || this.m_run || this.IsBlocking() || base.IsFlying())
			{
				this.SetCrouch(false);
			}
			bool flag = this.InAttack() || this.IsHoldingAttack();
			this.m_zanim.SetBool(Player.crouching, this.m_crouchToggled && !flag);
			return;
		}
		this.m_zanim.SetBool(Player.crouching, false);
	}

	// Token: 0x06000213 RID: 531 RVA: 0x000112D4 File Offset: 0x0000F4D4
	protected override void SetCrouch(bool crouch)
	{
		if (this.m_crouchToggled == crouch)
		{
			return;
		}
		this.m_crouchToggled = crouch;
	}

	// Token: 0x06000214 RID: 532 RVA: 0x000112E7 File Offset: 0x0000F4E7
	public void SetGuardianPower(string name)
	{
		this.m_guardianPower = name;
		this.m_guardianSE = ObjectDB.instance.GetStatusEffect(this.m_guardianPower);
	}

	// Token: 0x06000215 RID: 533 RVA: 0x00011306 File Offset: 0x0000F506
	public string GetGuardianPowerName()
	{
		return this.m_guardianPower;
	}

	// Token: 0x06000216 RID: 534 RVA: 0x0001130E File Offset: 0x0000F50E
	public void GetGuardianPowerHUD(out StatusEffect se, out float cooldown)
	{
		se = this.m_guardianSE;
		cooldown = this.m_guardianPowerCooldown;
	}

	// Token: 0x06000217 RID: 535 RVA: 0x00011320 File Offset: 0x0000F520
	public bool StartGuardianPower()
	{
		if (this.m_guardianSE == null)
		{
			return false;
		}
		if ((this.InAttack() && !this.HaveQueuedChain()) || this.InDodge() || !this.CanMove() || base.IsKnockedBack() || base.IsStaggering() || this.InMinorAction())
		{
			return false;
		}
		if (this.m_guardianPowerCooldown > 0f)
		{
			this.Message(MessageHud.MessageType.Center, "$hud_powernotready", 0, null);
			return false;
		}
		this.m_zanim.SetTrigger("gpower");
		return true;
	}

	// Token: 0x06000218 RID: 536 RVA: 0x000113A8 File Offset: 0x0000F5A8
	public bool ActivateGuardianPower()
	{
		if (this.m_guardianPowerCooldown > 0f)
		{
			return false;
		}
		if (this.m_guardianSE == null)
		{
			return false;
		}
		List<Player> list = new List<Player>();
		Player.GetPlayersInRange(base.transform.position, 10f, list);
		foreach (Player player in list)
		{
			player.GetSEMan().AddStatusEffect(this.m_guardianSE.name, true);
		}
		this.m_guardianPowerCooldown = this.m_guardianSE.m_cooldown;
		return false;
	}

	// Token: 0x06000219 RID: 537 RVA: 0x00011454 File Offset: 0x0000F654
	private void UpdateGuardianPower(float dt)
	{
		this.m_guardianPowerCooldown -= dt;
		if (this.m_guardianPowerCooldown < 0f)
		{
			this.m_guardianPowerCooldown = 0f;
		}
	}

	// Token: 0x0600021A RID: 538 RVA: 0x0001147C File Offset: 0x0000F67C
	public override void AttachStart(Transform attachPoint, bool hideWeapons, bool isBed, string attachAnimation, Vector3 detachOffset)
	{
		if (this.m_attached)
		{
			return;
		}
		this.m_attached = true;
		this.m_attachPoint = attachPoint;
		this.m_detachOffset = detachOffset;
		this.m_attachAnimation = attachAnimation;
		this.m_zanim.SetBool(attachAnimation, true);
		this.m_nview.GetZDO().Set("inBed", isBed);
		if (hideWeapons)
		{
			base.HideHandItems();
		}
		base.ResetCloth();
	}

	// Token: 0x0600021B RID: 539 RVA: 0x000114E4 File Offset: 0x0000F6E4
	private void UpdateAttach()
	{
		if (this.m_attached)
		{
			if (this.m_attachPoint != null)
			{
				base.transform.position = this.m_attachPoint.position;
				base.transform.rotation = this.m_attachPoint.rotation;
				Rigidbody componentInParent = this.m_attachPoint.GetComponentInParent<Rigidbody>();
				this.m_body.useGravity = false;
				this.m_body.velocity = (componentInParent ? componentInParent.GetPointVelocity(base.transform.position) : Vector3.zero);
				this.m_body.angularVelocity = Vector3.zero;
				this.m_maxAirAltitude = base.transform.position.y;
				return;
			}
			this.AttachStop();
		}
	}

	// Token: 0x0600021C RID: 540 RVA: 0x000115A9 File Offset: 0x0000F7A9
	public override bool IsAttached()
	{
		return this.m_attached;
	}

	// Token: 0x0600021D RID: 541 RVA: 0x000115B1 File Offset: 0x0000F7B1
	public override bool InBed()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetBool("inBed", false);
	}

	// Token: 0x0600021E RID: 542 RVA: 0x000115D8 File Offset: 0x0000F7D8
	public override void AttachStop()
	{
		if (this.m_sleeping)
		{
			return;
		}
		if (this.m_attached)
		{
			if (this.m_attachPoint != null)
			{
				base.transform.position = this.m_attachPoint.TransformPoint(this.m_detachOffset);
			}
			this.m_body.useGravity = true;
			this.m_attached = false;
			this.m_attachPoint = null;
			this.m_zanim.SetBool(this.m_attachAnimation, false);
			this.m_nview.GetZDO().Set("inBed", false);
			base.ResetCloth();
		}
	}

	// Token: 0x0600021F RID: 543 RVA: 0x00011668 File Offset: 0x0000F868
	public void StartShipControl(ShipControlls shipControl)
	{
		this.m_shipControl = shipControl;
		ZLog.Log("ship controlls set " + shipControl.GetShip().gameObject.name);
	}

	// Token: 0x06000220 RID: 544 RVA: 0x00011690 File Offset: 0x0000F890
	public void StopShipControl()
	{
		if (this.m_shipControl != null)
		{
			if (this.m_shipControl)
			{
				this.m_shipControl.OnUseStop(this);
			}
			ZLog.Log("Stop ship controlls");
			this.m_shipControl = null;
		}
	}

	// Token: 0x06000221 RID: 545 RVA: 0x000116CA File Offset: 0x0000F8CA
	private void SetShipControl(ref Vector3 moveDir)
	{
		this.m_shipControl.GetShip().ApplyMovementControlls(moveDir);
		moveDir = Vector3.zero;
	}

	// Token: 0x06000222 RID: 546 RVA: 0x000116ED File Offset: 0x0000F8ED
	public Ship GetControlledShip()
	{
		if (this.m_shipControl)
		{
			return this.m_shipControl.GetShip();
		}
		return null;
	}

	// Token: 0x06000223 RID: 547 RVA: 0x00011709 File Offset: 0x0000F909
	public ShipControlls GetShipControl()
	{
		return this.m_shipControl;
	}

	// Token: 0x06000224 RID: 548 RVA: 0x00011714 File Offset: 0x0000F914
	private void UpdateShipControl(float dt)
	{
		if (!this.m_shipControl)
		{
			return;
		}
		Vector3 forward = this.m_shipControl.GetShip().transform.forward;
		forward.y = 0f;
		forward.Normalize();
		Quaternion to = Quaternion.LookRotation(forward);
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, 100f * dt);
		if (Vector3.Distance(this.m_shipControl.transform.position, base.transform.position) > this.m_maxInteractDistance)
		{
			this.StopShipControl();
		}
	}

	// Token: 0x06000225 RID: 549 RVA: 0x000117B0 File Offset: 0x0000F9B0
	public bool IsSleeping()
	{
		return this.m_sleeping;
	}

	// Token: 0x06000226 RID: 550 RVA: 0x000117B8 File Offset: 0x0000F9B8
	public void SetSleeping(bool sleep)
	{
		if (this.m_sleeping == sleep)
		{
			return;
		}
		this.m_sleeping = sleep;
		if (!sleep)
		{
			this.Message(MessageHud.MessageType.Center, "$msg_goodmorning", 0, null);
			this.m_seman.AddStatusEffect("Rested", true);
		}
	}

	// Token: 0x06000227 RID: 551 RVA: 0x000117F0 File Offset: 0x0000F9F0
	public void SetControls(Vector3 movedir, bool attack, bool attackHold, bool secondaryAttack, bool block, bool blockHold, bool jump, bool crouch, bool run, bool autoRun)
	{
		if ((movedir != Vector3.zero || attack || secondaryAttack || block || blockHold || jump || crouch) && this.GetControlledShip() == null)
		{
			this.StopEmote();
			this.AttachStop();
		}
		if (this.m_shipControl)
		{
			this.SetShipControl(ref movedir);
			if (jump)
			{
				this.StopShipControl();
			}
		}
		if (run)
		{
			this.m_walk = false;
		}
		if (!this.m_autoRun)
		{
			Vector3 lookDir = this.m_lookDir;
			lookDir.y = 0f;
			lookDir.Normalize();
			this.m_moveDir = movedir.z * lookDir + movedir.x * Vector3.Cross(Vector3.up, lookDir);
		}
		if (!this.m_autoRun && autoRun && !this.InPlaceMode())
		{
			this.m_autoRun = true;
			this.SetCrouch(false);
			this.m_moveDir = this.m_lookDir;
			this.m_moveDir.y = 0f;
			this.m_moveDir.Normalize();
		}
		else if (this.m_autoRun)
		{
			if (attack || jump || crouch || movedir != Vector3.zero || this.InPlaceMode() || attackHold)
			{
				this.m_autoRun = false;
			}
			else if (autoRun || blockHold)
			{
				this.m_moveDir = this.m_lookDir;
				this.m_moveDir.y = 0f;
				this.m_moveDir.Normalize();
				blockHold = false;
				block = false;
			}
		}
		this.m_attack = attack;
		this.m_attackDraw = attackHold;
		this.m_secondaryAttack = secondaryAttack;
		this.m_blocking = blockHold;
		this.m_run = run;
		if (crouch)
		{
			this.SetCrouch(!this.m_crouchToggled);
		}
		if (jump)
		{
			if (this.m_blocking)
			{
				Vector3 dodgeDir = this.m_moveDir;
				if (dodgeDir.magnitude < 0.1f)
				{
					dodgeDir = -this.m_lookDir;
					dodgeDir.y = 0f;
					dodgeDir.Normalize();
				}
				this.Dodge(dodgeDir);
				return;
			}
			if (this.IsCrouching() || this.m_crouchToggled)
			{
				Vector3 dodgeDir2 = this.m_moveDir;
				if (dodgeDir2.magnitude < 0.1f)
				{
					dodgeDir2 = this.m_lookDir;
					dodgeDir2.y = 0f;
					dodgeDir2.Normalize();
				}
				this.Dodge(dodgeDir2);
				return;
			}
			base.Jump();
		}
	}

	// Token: 0x06000228 RID: 552 RVA: 0x00011A34 File Offset: 0x0000FC34
	private void UpdateTargeted(float dt)
	{
		this.m_timeSinceTargeted += dt;
		this.m_timeSinceSensed += dt;
	}

	// Token: 0x06000229 RID: 553 RVA: 0x00011A54 File Offset: 0x0000FC54
	public override void OnTargeted(bool sensed, bool alerted)
	{
		if (sensed)
		{
			if (this.m_timeSinceSensed > 0.5f)
			{
				this.m_timeSinceSensed = 0f;
				this.m_nview.InvokeRPC("OnTargeted", new object[]
				{
					sensed,
					alerted
				});
				return;
			}
		}
		else if (this.m_timeSinceTargeted > 0.5f)
		{
			this.m_timeSinceTargeted = 0f;
			this.m_nview.InvokeRPC("OnTargeted", new object[]
			{
				sensed,
				alerted
			});
		}
	}

	// Token: 0x0600022A RID: 554 RVA: 0x00011AE5 File Offset: 0x0000FCE5
	private void RPC_OnTargeted(long sender, bool sensed, bool alerted)
	{
		this.m_timeSinceTargeted = 0f;
		if (sensed)
		{
			this.m_timeSinceSensed = 0f;
		}
		if (alerted)
		{
			MusicMan.instance.ResetCombatTimer();
		}
	}

	// Token: 0x0600022B RID: 555 RVA: 0x00011B0D File Offset: 0x0000FD0D
	protected override void OnDamaged(HitData hit)
	{
		base.OnDamaged(hit);
		Hud.instance.DamageFlash();
	}

	// Token: 0x0600022C RID: 556 RVA: 0x00011B20 File Offset: 0x0000FD20
	public bool IsTargeted()
	{
		return this.m_timeSinceTargeted < 1f;
	}

	// Token: 0x0600022D RID: 557 RVA: 0x00011B2F File Offset: 0x0000FD2F
	public bool IsSensed()
	{
		return this.m_timeSinceSensed < 1f;
	}

	// Token: 0x0600022E RID: 558 RVA: 0x00011B40 File Offset: 0x0000FD40
	protected override void ApplyArmorDamageMods(ref HitData.DamageModifiers mods)
	{
		if (this.m_chestItem != null)
		{
			mods.Apply(this.m_chestItem.m_shared.m_damageModifiers);
		}
		if (this.m_legItem != null)
		{
			mods.Apply(this.m_legItem.m_shared.m_damageModifiers);
		}
		if (this.m_helmetItem != null)
		{
			mods.Apply(this.m_helmetItem.m_shared.m_damageModifiers);
		}
		if (this.m_shoulderItem != null)
		{
			mods.Apply(this.m_shoulderItem.m_shared.m_damageModifiers);
		}
	}

	// Token: 0x0600022F RID: 559 RVA: 0x00011BC8 File Offset: 0x0000FDC8
	public override float GetBodyArmor()
	{
		float num = 0f;
		if (this.m_chestItem != null)
		{
			num += this.m_chestItem.GetArmor();
		}
		if (this.m_legItem != null)
		{
			num += this.m_legItem.GetArmor();
		}
		if (this.m_helmetItem != null)
		{
			num += this.m_helmetItem.GetArmor();
		}
		if (this.m_shoulderItem != null)
		{
			num += this.m_shoulderItem.GetArmor();
		}
		return num;
	}

	// Token: 0x06000230 RID: 560 RVA: 0x00011C34 File Offset: 0x0000FE34
	protected override void OnSneaking(float dt)
	{
		float t = Mathf.Pow(this.m_skills.GetSkillFactor(Skills.SkillType.Sneak), 0.5f);
		float num = Mathf.Lerp(1f, 0.25f, t);
		this.UseStamina(dt * this.m_sneakStaminaDrain * num);
		if (!this.HaveStamina(0f))
		{
			Hud.instance.StaminaBarNoStaminaFlash();
		}
		this.m_sneakSkillImproveTimer += dt;
		if (this.m_sneakSkillImproveTimer > 1f)
		{
			this.m_sneakSkillImproveTimer = 0f;
			if (BaseAI.InStealthRange(this))
			{
				this.RaiseSkill(Skills.SkillType.Sneak, 1f);
				return;
			}
			this.RaiseSkill(Skills.SkillType.Sneak, 0.1f);
		}
	}

	// Token: 0x06000231 RID: 561 RVA: 0x00011CDC File Offset: 0x0000FEDC
	private void UpdateStealth(float dt)
	{
		this.m_stealthFactorUpdateTimer += dt;
		if (this.m_stealthFactorUpdateTimer > 0.5f)
		{
			this.m_stealthFactorUpdateTimer = 0f;
			this.m_stealthFactorTarget = 0f;
			if (this.IsCrouching())
			{
				this.m_lastStealthPosition = base.transform.position;
				float skillFactor = this.m_skills.GetSkillFactor(Skills.SkillType.Sneak);
				float lightFactor = StealthSystem.instance.GetLightFactor(base.GetCenterPoint());
				this.m_stealthFactorTarget = Mathf.Lerp(0.5f + lightFactor * 0.5f, 0.2f + lightFactor * 0.4f, skillFactor);
				this.m_stealthFactorTarget = Mathf.Clamp01(this.m_stealthFactorTarget);
				this.m_seman.ModifyStealth(this.m_stealthFactorTarget, ref this.m_stealthFactorTarget);
				this.m_stealthFactorTarget = Mathf.Clamp01(this.m_stealthFactorTarget);
			}
			else
			{
				this.m_stealthFactorTarget = 1f;
			}
		}
		this.m_stealthFactor = Mathf.MoveTowards(this.m_stealthFactor, this.m_stealthFactorTarget, dt / 4f);
		this.m_nview.GetZDO().Set("Stealth", this.m_stealthFactor);
	}

	// Token: 0x06000232 RID: 562 RVA: 0x00011E00 File Offset: 0x00010000
	public override float GetStealthFactor()
	{
		if (!this.m_nview.IsValid())
		{
			return 0f;
		}
		if (this.m_nview.IsOwner())
		{
			return this.m_stealthFactor;
		}
		return this.m_nview.GetZDO().GetFloat("Stealth", 0f);
	}

	// Token: 0x06000233 RID: 563 RVA: 0x00011E50 File Offset: 0x00010050
	public override bool InAttack()
	{
		if (this.m_animator.IsInTransition(0))
		{
			return this.m_animator.GetNextAnimatorStateInfo(0).tagHash == Humanoid.m_animatorTagAttack || this.m_animator.GetNextAnimatorStateInfo(1).tagHash == Humanoid.m_animatorTagAttack;
		}
		return this.m_animator.GetCurrentAnimatorStateInfo(0).tagHash == Humanoid.m_animatorTagAttack || this.m_animator.GetCurrentAnimatorStateInfo(1).tagHash == Humanoid.m_animatorTagAttack;
	}

	// Token: 0x06000234 RID: 564 RVA: 0x00011EE2 File Offset: 0x000100E2
	public override float GetEquipmentMovementModifier()
	{
		return this.m_equipmentMovementModifier;
	}

	// Token: 0x06000235 RID: 565 RVA: 0x00011EEA File Offset: 0x000100EA
	protected override float GetJogSpeedFactor()
	{
		return 1f + this.m_equipmentMovementModifier;
	}

	// Token: 0x06000236 RID: 566 RVA: 0x00011EF8 File Offset: 0x000100F8
	protected override float GetRunSpeedFactor()
	{
		float num = 1f;
		float skillFactor = this.m_skills.GetSkillFactor(Skills.SkillType.Run);
		return (num + skillFactor * 0.25f) * (1f + this.m_equipmentMovementModifier * 1.5f);
	}

	// Token: 0x06000237 RID: 567 RVA: 0x00011F34 File Offset: 0x00010134
	public override bool InMinorAction()
	{
		return (this.m_animator.IsInTransition(1) ? this.m_animator.GetNextAnimatorStateInfo(1) : this.m_animator.GetCurrentAnimatorStateInfo(1)).tagHash == Player.m_animatorTagMinorAction;
	}

	// Token: 0x06000238 RID: 568 RVA: 0x00011F78 File Offset: 0x00010178
	public override bool GetRelativePosition(out ZDOID parent, out Vector3 relativePos, out Vector3 relativeVel)
	{
		if (this.m_attached && this.m_attachPoint)
		{
			ZNetView componentInParent = this.m_attachPoint.GetComponentInParent<ZNetView>();
			if (componentInParent && componentInParent.IsValid())
			{
				parent = componentInParent.GetZDO().m_uid;
				relativePos = componentInParent.transform.InverseTransformPoint(base.transform.position);
				relativeVel = Vector3.zero;
				return true;
			}
		}
		return base.GetRelativePosition(out parent, out relativePos, out relativeVel);
	}

	// Token: 0x06000239 RID: 569 RVA: 0x00011FF9 File Offset: 0x000101F9
	public override Skills GetSkills()
	{
		return this.m_skills;
	}

	// Token: 0x0600023A RID: 570 RVA: 0x00012001 File Offset: 0x00010201
	public override float GetRandomSkillFactor(Skills.SkillType skill)
	{
		return this.m_skills.GetRandomSkillFactor(skill);
	}

	// Token: 0x0600023B RID: 571 RVA: 0x0001200F File Offset: 0x0001020F
	public override float GetSkillFactor(Skills.SkillType skill)
	{
		return this.m_skills.GetSkillFactor(skill);
	}

	// Token: 0x0600023C RID: 572 RVA: 0x00012020 File Offset: 0x00010220
	protected override void DoDamageCameraShake(HitData hit)
	{
		if (GameCamera.instance && hit.GetTotalPhysicalDamage() > 0f)
		{
			float num = Mathf.Clamp01(hit.GetTotalPhysicalDamage() / base.GetMaxHealth());
			GameCamera.instance.AddShake(base.transform.position, 50f, this.m_baseCameraShake * num, false);
		}
	}

	// Token: 0x0600023D RID: 573 RVA: 0x0001207C File Offset: 0x0001027C
	protected override bool ToggleEquiped(ItemDrop.ItemData item)
	{
		if (!item.IsEquipable())
		{
			return false;
		}
		if (this.InAttack())
		{
			return true;
		}
		if (item.m_shared.m_equipDuration <= 0f)
		{
			if (base.IsItemEquiped(item))
			{
				base.UnequipItem(item, true);
			}
			else
			{
				base.EquipItem(item, true);
			}
		}
		else if (base.IsItemEquiped(item))
		{
			this.QueueUnequipItem(item);
		}
		else
		{
			this.QueueEquipItem(item);
		}
		return true;
	}

	// Token: 0x0600023E RID: 574 RVA: 0x000120E8 File Offset: 0x000102E8
	public void GetActionProgress(out string name, out float progress)
	{
		if (this.m_equipQueue.Count > 0)
		{
			Player.EquipQueueData equipQueueData = this.m_equipQueue[0];
			if (equipQueueData.m_duration > 0.5f)
			{
				if (equipQueueData.m_equip)
				{
					name = "$hud_equipping " + equipQueueData.m_item.m_shared.m_name;
				}
				else
				{
					name = "$hud_unequipping " + equipQueueData.m_item.m_shared.m_name;
				}
				progress = Mathf.Clamp01(equipQueueData.m_time / equipQueueData.m_duration);
				return;
			}
		}
		name = null;
		progress = 0f;
	}

	// Token: 0x0600023F RID: 575 RVA: 0x00012180 File Offset: 0x00010380
	private void UpdateEquipQueue(float dt)
	{
		if (this.m_equipQueuePause > 0f)
		{
			this.m_equipQueuePause -= dt;
			this.m_zanim.SetBool("equipping", false);
			return;
		}
		this.m_zanim.SetBool("equipping", this.m_equipQueue.Count > 0);
		if (this.m_equipQueue.Count == 0)
		{
			return;
		}
		Player.EquipQueueData equipQueueData = this.m_equipQueue[0];
		if (equipQueueData.m_time == 0f && equipQueueData.m_duration >= 1f)
		{
			this.m_equipStartEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
		}
		equipQueueData.m_time += dt;
		if (equipQueueData.m_time > equipQueueData.m_duration)
		{
			this.m_equipQueue.RemoveAt(0);
			if (equipQueueData.m_equip)
			{
				base.EquipItem(equipQueueData.m_item, true);
			}
			else
			{
				base.UnequipItem(equipQueueData.m_item, true);
			}
			this.m_equipQueuePause = 0.3f;
		}
	}

	// Token: 0x06000240 RID: 576 RVA: 0x00012288 File Offset: 0x00010488
	private void QueueEquipItem(ItemDrop.ItemData item)
	{
		if (item == null)
		{
			return;
		}
		if (this.IsItemQueued(item))
		{
			this.RemoveFromEquipQueue(item);
			return;
		}
		Player.EquipQueueData equipQueueData = new Player.EquipQueueData();
		equipQueueData.m_item = item;
		equipQueueData.m_equip = true;
		equipQueueData.m_duration = item.m_shared.m_equipDuration;
		this.m_equipQueue.Add(equipQueueData);
	}

	// Token: 0x06000241 RID: 577 RVA: 0x000122DC File Offset: 0x000104DC
	private void QueueUnequipItem(ItemDrop.ItemData item)
	{
		if (item == null)
		{
			return;
		}
		if (this.IsItemQueued(item))
		{
			this.RemoveFromEquipQueue(item);
			return;
		}
		Player.EquipQueueData equipQueueData = new Player.EquipQueueData();
		equipQueueData.m_item = item;
		equipQueueData.m_equip = false;
		equipQueueData.m_duration = item.m_shared.m_equipDuration;
		this.m_equipQueue.Add(equipQueueData);
	}

	// Token: 0x06000242 RID: 578 RVA: 0x0001232F File Offset: 0x0001052F
	public override void AbortEquipQueue()
	{
		this.m_equipQueue.Clear();
	}

	// Token: 0x06000243 RID: 579 RVA: 0x0001233C File Offset: 0x0001053C
	public override void RemoveFromEquipQueue(ItemDrop.ItemData item)
	{
		if (item == null)
		{
			return;
		}
		foreach (Player.EquipQueueData equipQueueData in this.m_equipQueue)
		{
			if (equipQueueData.m_item == item)
			{
				this.m_equipQueue.Remove(equipQueueData);
				break;
			}
		}
	}

	// Token: 0x06000244 RID: 580 RVA: 0x000123A4 File Offset: 0x000105A4
	public bool IsItemQueued(ItemDrop.ItemData item)
	{
		if (item == null)
		{
			return false;
		}
		using (List<Player.EquipQueueData>.Enumerator enumerator = this.m_equipQueue.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_item == item)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000245 RID: 581 RVA: 0x00012404 File Offset: 0x00010604
	public void ResetCharacter()
	{
		this.m_guardianPowerCooldown = 0f;
		Player.ResetSeenTutorials();
		this.m_knownRecipes.Clear();
		this.m_knownStations.Clear();
		this.m_knownMaterial.Clear();
		this.m_uniques.Clear();
		this.m_trophies.Clear();
		this.m_skills.Clear();
		this.m_knownBiome.Clear();
		this.m_knownTexts.Clear();
	}

	// Token: 0x04000116 RID: 278
	private float m_rotatePieceTimer;

	// Token: 0x04000117 RID: 279
	private float m_baseValueUpdatetimer;

	// Token: 0x04000118 RID: 280
	private const int dataVersion = 24;

	// Token: 0x04000119 RID: 281
	private float m_equipQueuePause;

	// Token: 0x0400011A RID: 282
	public static Player m_localPlayer = null;

	// Token: 0x0400011B RID: 283
	private static List<Player> m_players = new List<Player>();

	// Token: 0x0400011C RID: 284
	public static bool m_debugMode = false;

	// Token: 0x0400011D RID: 285
	[Header("Player")]
	public float m_maxPlaceDistance = 5f;

	// Token: 0x0400011E RID: 286
	public float m_maxInteractDistance = 5f;

	// Token: 0x0400011F RID: 287
	public float m_scrollSens = 4f;

	// Token: 0x04000120 RID: 288
	public float m_staminaRegen = 5f;

	// Token: 0x04000121 RID: 289
	public float m_staminaRegenTimeMultiplier = 1f;

	// Token: 0x04000122 RID: 290
	public float m_staminaRegenDelay = 1f;

	// Token: 0x04000123 RID: 291
	public float m_runStaminaDrain = 10f;

	// Token: 0x04000124 RID: 292
	public float m_sneakStaminaDrain = 5f;

	// Token: 0x04000125 RID: 293
	public float m_swimStaminaDrainMinSkill = 5f;

	// Token: 0x04000126 RID: 294
	public float m_swimStaminaDrainMaxSkill = 2f;

	// Token: 0x04000127 RID: 295
	public float m_dodgeStaminaUsage = 10f;

	// Token: 0x04000128 RID: 296
	public float m_weightStaminaFactor = 0.1f;

	// Token: 0x04000129 RID: 297
	public float m_autoPickupRange = 2f;

	// Token: 0x0400012A RID: 298
	public float m_maxCarryWeight = 300f;

	// Token: 0x0400012B RID: 299
	public float m_encumberedStaminaDrain = 10f;

	// Token: 0x0400012C RID: 300
	public float m_hardDeathCooldown = 10f;

	// Token: 0x0400012D RID: 301
	public float m_baseCameraShake = 4f;

	// Token: 0x0400012E RID: 302
	public float m_toolUseDelay = 0.25f;

	// Token: 0x0400012F RID: 303
	public EffectList m_drownEffects = new EffectList();

	// Token: 0x04000130 RID: 304
	public EffectList m_spawnEffects = new EffectList();

	// Token: 0x04000131 RID: 305
	public EffectList m_removeEffects = new EffectList();

	// Token: 0x04000132 RID: 306
	public EffectList m_dodgeEffects = new EffectList();

	// Token: 0x04000133 RID: 307
	public EffectList m_autopickupEffects = new EffectList();

	// Token: 0x04000134 RID: 308
	public EffectList m_skillLevelupEffects = new EffectList();

	// Token: 0x04000135 RID: 309
	public EffectList m_equipStartEffects = new EffectList();

	// Token: 0x04000136 RID: 310
	public GameObject m_placeMarker;

	// Token: 0x04000137 RID: 311
	public GameObject m_tombstone;

	// Token: 0x04000138 RID: 312
	public GameObject m_valkyrie;

	// Token: 0x04000139 RID: 313
	public Sprite m_textIcon;

	// Token: 0x0400013A RID: 314
	private Skills m_skills;

	// Token: 0x0400013B RID: 315
	private PieceTable m_buildPieces;

	// Token: 0x0400013C RID: 316
	private bool m_noPlacementCost;

	// Token: 0x0400013D RID: 317
	private bool m_hideUnavailable;

	// Token: 0x0400013E RID: 318
	private HashSet<string> m_knownRecipes = new HashSet<string>();

	// Token: 0x0400013F RID: 319
	private Dictionary<string, int> m_knownStations = new Dictionary<string, int>();

	// Token: 0x04000140 RID: 320
	private HashSet<string> m_knownMaterial = new HashSet<string>();

	// Token: 0x04000141 RID: 321
	private HashSet<string> m_shownTutorials = new HashSet<string>();

	// Token: 0x04000142 RID: 322
	private HashSet<string> m_uniques = new HashSet<string>();

	// Token: 0x04000143 RID: 323
	private HashSet<string> m_trophies = new HashSet<string>();

	// Token: 0x04000144 RID: 324
	private HashSet<Heightmap.Biome> m_knownBiome = new HashSet<Heightmap.Biome>();

	// Token: 0x04000145 RID: 325
	private Dictionary<string, string> m_knownTexts = new Dictionary<string, string>();

	// Token: 0x04000146 RID: 326
	private float m_stationDiscoverTimer;

	// Token: 0x04000147 RID: 327
	private bool m_debugFly;

	// Token: 0x04000148 RID: 328
	private bool m_godMode;

	// Token: 0x04000149 RID: 329
	private bool m_ghostMode;

	// Token: 0x0400014A RID: 330
	private float m_lookPitch;

	// Token: 0x0400014B RID: 331
	private const float m_baseHP = 25f;

	// Token: 0x0400014C RID: 332
	private const float m_baseStamina = 75f;

	// Token: 0x0400014D RID: 333
	private const int m_maxFoods = 3;

	// Token: 0x0400014E RID: 334
	private const float m_foodDrainPerSec = 0.1f;

	// Token: 0x0400014F RID: 335
	private float m_foodUpdateTimer;

	// Token: 0x04000150 RID: 336
	private float m_foodRegenTimer;

	// Token: 0x04000151 RID: 337
	private List<Player.Food> m_foods = new List<Player.Food>();

	// Token: 0x04000152 RID: 338
	private float m_stamina = 100f;

	// Token: 0x04000153 RID: 339
	private float m_maxStamina = 100f;

	// Token: 0x04000154 RID: 340
	private float m_staminaRegenTimer;

	// Token: 0x04000155 RID: 341
	private string m_guardianPower = "";

	// Token: 0x04000156 RID: 342
	private float m_guardianPowerCooldown;

	// Token: 0x04000157 RID: 343
	private StatusEffect m_guardianSE;

	// Token: 0x04000158 RID: 344
	private float m_lastToolUseTime;

	// Token: 0x04000159 RID: 345
	private GameObject m_placementMarkerInstance;

	// Token: 0x0400015A RID: 346
	private GameObject m_placementGhost;

	// Token: 0x0400015B RID: 347
	private Player.PlacementStatus m_placementStatus = Player.PlacementStatus.Invalid;

	// Token: 0x0400015C RID: 348
	private int m_placeRotation;

	// Token: 0x0400015D RID: 349
	private int m_placeRayMask;

	// Token: 0x0400015E RID: 350
	private int m_placeGroundRayMask;

	// Token: 0x0400015F RID: 351
	private int m_placeWaterRayMask;

	// Token: 0x04000160 RID: 352
	private int m_removeRayMask;

	// Token: 0x04000161 RID: 353
	private int m_interactMask;

	// Token: 0x04000162 RID: 354
	private int m_autoPickupMask;

	// Token: 0x04000163 RID: 355
	private List<Player.EquipQueueData> m_equipQueue = new List<Player.EquipQueueData>();

	// Token: 0x04000164 RID: 356
	private GameObject m_hovering;

	// Token: 0x04000165 RID: 357
	private Character m_hoveringCreature;

	// Token: 0x04000166 RID: 358
	private float m_lastHoverInteractTime;

	// Token: 0x04000167 RID: 359
	private bool m_pvp;

	// Token: 0x04000168 RID: 360
	private float m_updateCoverTimer;

	// Token: 0x04000169 RID: 361
	private float m_coverPercentage;

	// Token: 0x0400016A RID: 362
	private bool m_underRoof = true;

	// Token: 0x0400016B RID: 363
	private float m_nearFireTimer;

	// Token: 0x0400016C RID: 364
	private bool m_isLoading;

	// Token: 0x0400016D RID: 365
	private float m_queuedAttackTimer;

	// Token: 0x0400016E RID: 366
	private float m_queuedSecondAttackTimer;

	// Token: 0x0400016F RID: 367
	private float m_queuedDodgeTimer;

	// Token: 0x04000170 RID: 368
	private Vector3 m_queuedDodgeDir = Vector3.zero;

	// Token: 0x04000171 RID: 369
	private bool m_inDodge;

	// Token: 0x04000172 RID: 370
	private bool m_dodgeInvincible;

	// Token: 0x04000173 RID: 371
	private CraftingStation m_currentStation;

	// Token: 0x04000174 RID: 372
	private Ragdoll m_ragdoll;

	// Token: 0x04000175 RID: 373
	private Piece m_hoveringPiece;

	// Token: 0x04000176 RID: 374
	private string m_emoteState = "";

	// Token: 0x04000177 RID: 375
	private int m_emoteID;

	// Token: 0x04000178 RID: 376
	private bool m_intro;

	// Token: 0x04000179 RID: 377
	private bool m_firstSpawn = true;

	// Token: 0x0400017A RID: 378
	private bool m_crouchToggled;

	// Token: 0x0400017B RID: 379
	private bool m_autoRun;

	// Token: 0x0400017C RID: 380
	private bool m_safeInHome;

	// Token: 0x0400017D RID: 381
	private ShipControlls m_shipControl;

	// Token: 0x0400017E RID: 382
	private bool m_attached;

	// Token: 0x0400017F RID: 383
	private string m_attachAnimation = "";

	// Token: 0x04000180 RID: 384
	private bool m_sleeping;

	// Token: 0x04000181 RID: 385
	private Transform m_attachPoint;

	// Token: 0x04000182 RID: 386
	private Vector3 m_detachOffset = Vector3.zero;

	// Token: 0x04000183 RID: 387
	private int m_modelIndex;

	// Token: 0x04000184 RID: 388
	private Vector3 m_skinColor = Vector3.one;

	// Token: 0x04000185 RID: 389
	private Vector3 m_hairColor = Vector3.one;

	// Token: 0x04000186 RID: 390
	private bool m_teleporting;

	// Token: 0x04000187 RID: 391
	private bool m_distantTeleport;

	// Token: 0x04000188 RID: 392
	private float m_teleportTimer;

	// Token: 0x04000189 RID: 393
	private float m_teleportCooldown;

	// Token: 0x0400018A RID: 394
	private Vector3 m_teleportFromPos;

	// Token: 0x0400018B RID: 395
	private Quaternion m_teleportFromRot;

	// Token: 0x0400018C RID: 396
	private Vector3 m_teleportTargetPos;

	// Token: 0x0400018D RID: 397
	private Quaternion m_teleportTargetRot;

	// Token: 0x0400018E RID: 398
	private Heightmap.Biome m_currentBiome;

	// Token: 0x0400018F RID: 399
	private float m_biomeTimer;

	// Token: 0x04000190 RID: 400
	private int m_baseValue;

	// Token: 0x04000191 RID: 401
	private int m_comfortLevel;

	// Token: 0x04000192 RID: 402
	private float m_drownDamageTimer;

	// Token: 0x04000193 RID: 403
	private float m_timeSinceTargeted;

	// Token: 0x04000194 RID: 404
	private float m_timeSinceSensed;

	// Token: 0x04000195 RID: 405
	private float m_stealthFactorUpdateTimer;

	// Token: 0x04000196 RID: 406
	private float m_stealthFactor;

	// Token: 0x04000197 RID: 407
	private float m_stealthFactorTarget;

	// Token: 0x04000198 RID: 408
	private Vector3 m_lastStealthPosition = Vector3.zero;

	// Token: 0x04000199 RID: 409
	private float m_wakeupTimer = -1f;

	// Token: 0x0400019A RID: 410
	private float m_timeSinceDeath = 999999f;

	// Token: 0x0400019B RID: 411
	private float m_runSkillImproveTimer;

	// Token: 0x0400019C RID: 412
	private float m_swimSkillImproveTimer;

	// Token: 0x0400019D RID: 413
	private float m_sneakSkillImproveTimer;

	// Token: 0x0400019E RID: 414
	private float m_equipmentMovementModifier;

	// Token: 0x0400019F RID: 415
	private static int crouching = 0;

	// Token: 0x040001A0 RID: 416
	protected static int m_attackMask = 0;

	// Token: 0x040001A1 RID: 417
	protected static int m_animatorTagDodge = Animator.StringToHash("dodge");

	// Token: 0x040001A2 RID: 418
	protected static int m_animatorTagCutscene = Animator.StringToHash("cutscene");

	// Token: 0x040001A3 RID: 419
	protected static int m_animatorTagCrouch = Animator.StringToHash("crouch");

	// Token: 0x040001A4 RID: 420
	protected static int m_animatorTagMinorAction = Animator.StringToHash("minoraction");

	// Token: 0x040001A5 RID: 421
	protected static int m_animatorTagEmote = Animator.StringToHash("emote");

	// Token: 0x040001A6 RID: 422
	private List<PieceTable> m_tempOwnedPieceTables = new List<PieceTable>();

	// Token: 0x040001A7 RID: 423
	private List<Transform> m_tempSnapPoints1 = new List<Transform>();

	// Token: 0x040001A8 RID: 424
	private List<Transform> m_tempSnapPoints2 = new List<Transform>();

	// Token: 0x040001A9 RID: 425
	private List<Piece> m_tempPieces = new List<Piece>();

	// Token: 0x02000124 RID: 292
	public enum RequirementMode
	{
		// Token: 0x04000FE0 RID: 4064
		CanBuild,
		// Token: 0x04000FE1 RID: 4065
		IsKnown,
		// Token: 0x04000FE2 RID: 4066
		CanAlmostBuild
	}

	// Token: 0x02000125 RID: 293
	public class Food
	{
		// Token: 0x060010B7 RID: 4279 RVA: 0x00076A4A File Offset: 0x00074C4A
		public bool CanEatAgain()
		{
			return this.m_health < this.m_item.m_shared.m_food / 2f;
		}

		// Token: 0x04000FE3 RID: 4067
		public string m_name = "";

		// Token: 0x04000FE4 RID: 4068
		public ItemDrop.ItemData m_item;

		// Token: 0x04000FE5 RID: 4069
		public float m_health;

		// Token: 0x04000FE6 RID: 4070
		public float m_stamina;
	}

	// Token: 0x02000126 RID: 294
	public class EquipQueueData
	{
		// Token: 0x04000FE7 RID: 4071
		public ItemDrop.ItemData m_item;

		// Token: 0x04000FE8 RID: 4072
		public bool m_equip = true;

		// Token: 0x04000FE9 RID: 4073
		public float m_time;

		// Token: 0x04000FEA RID: 4074
		public float m_duration;
	}

	// Token: 0x02000127 RID: 295
	private enum PlacementStatus
	{
		// Token: 0x04000FEC RID: 4076
		Valid,
		// Token: 0x04000FED RID: 4077
		Invalid,
		// Token: 0x04000FEE RID: 4078
		BlockedbyPlayer,
		// Token: 0x04000FEF RID: 4079
		NoBuildZone,
		// Token: 0x04000FF0 RID: 4080
		PrivateZone,
		// Token: 0x04000FF1 RID: 4081
		MoreSpace,
		// Token: 0x04000FF2 RID: 4082
		NoTeleportArea,
		// Token: 0x04000FF3 RID: 4083
		ExtensionMissingStation,
		// Token: 0x04000FF4 RID: 4084
		WrongBiome,
		// Token: 0x04000FF5 RID: 4085
		NeedCultivated,
		// Token: 0x04000FF6 RID: 4086
		NotInDungeon
	}
}
