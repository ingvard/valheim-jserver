using System;
using System.IO;
using UnityEngine;

// Token: 0x0200009E RID: 158
public class GameCamera : MonoBehaviour
{
	// Token: 0x17000027 RID: 39
	// (get) Token: 0x06000ADA RID: 2778 RVA: 0x0004DF01 File Offset: 0x0004C101
	public static GameCamera instance
	{
		get
		{
			return GameCamera.m_instance;
		}
	}

	// Token: 0x06000ADB RID: 2779 RVA: 0x0004DF08 File Offset: 0x0004C108
	private void Awake()
	{
		GameCamera.m_instance = this;
		this.m_camera = base.GetComponent<Camera>();
		this.m_listner = base.GetComponentInChildren<AudioListener>();
		this.m_camera.depthTextureMode = DepthTextureMode.DepthNormals;
		this.ApplySettings();
		if (!Application.isEditor)
		{
			this.m_mouseCapture = true;
		}
	}

	// Token: 0x06000ADC RID: 2780 RVA: 0x0004DF48 File Offset: 0x0004C148
	private void OnDestroy()
	{
		if (GameCamera.m_instance == this)
		{
			GameCamera.m_instance = null;
		}
	}

	// Token: 0x06000ADD RID: 2781 RVA: 0x0004DF5D File Offset: 0x0004C15D
	public void ApplySettings()
	{
		this.m_cameraShakeEnabled = (PlayerPrefs.GetInt("CameraShake", 1) == 1);
		this.m_shipCameraTilt = (PlayerPrefs.GetInt("ShipCameraTilt", 1) == 1);
	}

	// Token: 0x06000ADE RID: 2782 RVA: 0x0004DF90 File Offset: 0x0004C190
	private void LateUpdate()
	{
		float deltaTime = Time.deltaTime;
		if (Input.GetKeyDown(KeyCode.F11) || (this.m_freeFly && Input.GetKeyDown(KeyCode.Mouse1)))
		{
			GameCamera.ScreenShot();
		}
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			this.UpdateBaseOffset(localPlayer, deltaTime);
		}
		this.UpdateMouseCapture();
		this.UpdateCamera(deltaTime);
		this.UpdateListner();
	}

	// Token: 0x06000ADF RID: 2783 RVA: 0x0004DFF4 File Offset: 0x0004C1F4
	private void UpdateMouseCapture()
	{
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F1))
		{
			this.m_mouseCapture = !this.m_mouseCapture;
		}
		if (this.m_mouseCapture && !InventoryGui.IsVisible() && !Menu.IsVisible() && !Minimap.IsOpen() && !StoreGui.IsVisible() && !Hud.IsPieceSelectionVisible())
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			return;
		}
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = ZInput.IsMouseActive();
	}

	// Token: 0x06000AE0 RID: 2784 RVA: 0x0004E070 File Offset: 0x0004C270
	public static void ScreenShot()
	{
		DateTime now = DateTime.Now;
		Directory.CreateDirectory(Utils.GetSaveDataPath() + "/screenshots");
		string text = now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
		string text2 = now.ToString("yyyy-MM-dd");
		string text3 = string.Concat(new string[]
		{
			Utils.GetSaveDataPath(),
			"/screenshots/screenshot_",
			text2,
			"_",
			text,
			".png"
		});
		if (File.Exists(text3))
		{
			return;
		}
		ScreenCapture.CaptureScreenshot(text3);
		ZLog.Log("Screenshot saved:" + text3);
	}

	// Token: 0x06000AE1 RID: 2785 RVA: 0x0004E140 File Offset: 0x0004C340
	private void UpdateListner()
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer && !this.m_freeFly)
		{
			this.m_listner.transform.position = localPlayer.m_eye.position;
			return;
		}
		this.m_listner.transform.localPosition = Vector3.zero;
	}

	// Token: 0x06000AE2 RID: 2786 RVA: 0x0004E194 File Offset: 0x0004C394
	private void UpdateCamera(float dt)
	{
		if (this.m_freeFly)
		{
			this.UpdateFreeFly(dt);
			this.UpdateCameraShake(dt);
			return;
		}
		this.m_camera.fieldOfView = this.m_fov;
		this.m_skyCamera.fieldOfView = this.m_fov;
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			if ((!Chat.instance || !Chat.instance.HasFocus()) && !global::Console.IsVisible() && !InventoryGui.IsVisible() && !StoreGui.IsVisible() && !Menu.IsVisible() && !Minimap.IsOpen() && !localPlayer.InCutscene() && !localPlayer.InPlaceMode())
			{
				float minDistance = this.m_minDistance;
				float axis = Input.GetAxis("Mouse ScrollWheel");
				this.m_distance -= axis * this.m_zoomSens;
				float max = (localPlayer.GetControlledShip() != null) ? this.m_maxDistanceBoat : this.m_maxDistance;
				this.m_distance = Mathf.Clamp(this.m_distance, minDistance, max);
			}
			if (localPlayer.IsDead() && localPlayer.GetRagdoll())
			{
				Vector3 averageBodyPosition = localPlayer.GetRagdoll().GetAverageBodyPosition();
				base.transform.LookAt(averageBodyPosition);
			}
			else
			{
				Vector3 position;
				Quaternion rotation;
				this.GetCameraPosition(dt, out position, out rotation);
				base.transform.position = position;
				base.transform.rotation = rotation;
			}
			this.UpdateCameraShake(dt);
		}
	}

	// Token: 0x06000AE3 RID: 2787 RVA: 0x0004E2F0 File Offset: 0x0004C4F0
	private void GetCameraPosition(float dt, out Vector3 pos, out Quaternion rot)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null)
		{
			pos = base.transform.position;
			rot = base.transform.rotation;
			return;
		}
		Vector3 vector = this.GetOffsetedEyePos();
		float num = this.m_distance;
		if (localPlayer.InIntro())
		{
			vector = localPlayer.transform.position;
			num = this.m_flyingDistance;
		}
		Vector3 vector2 = -localPlayer.m_eye.transform.forward;
		if (this.m_smoothYTilt && !localPlayer.InIntro())
		{
			num = Mathf.Lerp(num, 1.5f, Utils.SmoothStep(0f, -0.5f, vector2.y));
		}
		Vector3 vector3 = vector + vector2 * num;
		this.CollideRay2(localPlayer.m_eye.position, vector, ref vector3);
		this.UpdateNearClipping(vector, vector3, dt);
		float waterLevel = WaterVolume.GetWaterLevel(vector3, 1f);
		if (vector3.y < waterLevel + this.m_minWaterDistance)
		{
			vector3.y = waterLevel + this.m_minWaterDistance;
			this.m_waterClipping = true;
		}
		else
		{
			this.m_waterClipping = false;
		}
		pos = vector3;
		rot = localPlayer.m_eye.transform.rotation;
		if (this.m_shipCameraTilt)
		{
			this.ApplyCameraTilt(localPlayer, dt, ref rot);
		}
	}

	// Token: 0x06000AE4 RID: 2788 RVA: 0x0004E43C File Offset: 0x0004C63C
	private void ApplyCameraTilt(Player player, float dt, ref Quaternion rot)
	{
		if (player.InIntro())
		{
			return;
		}
		Ship standingOnShip = player.GetStandingOnShip();
		float num = Mathf.Clamp01((this.m_distance - this.m_minDistance) / (this.m_maxDistanceBoat - this.m_minDistance));
		num = Mathf.Pow(num, 2f);
		float smoothTime = Mathf.Lerp(this.m_tiltSmoothnessShipMin, this.m_tiltSmoothnessShipMax, num);
		Vector3 up = Vector3.up;
		if (standingOnShip != null && standingOnShip.transform.up.y > 0f)
		{
			up = standingOnShip.transform.up;
		}
		else if (player.IsAttached())
		{
			up = player.GetVisual().transform.up;
		}
		Vector3 forward = player.m_eye.transform.forward;
		Vector3 target = Vector3.Lerp(up, Vector3.up, num * 0.5f);
		this.m_smoothedCameraUp = Vector3.SmoothDamp(this.m_smoothedCameraUp, target, ref this.m_smoothedCameraUpVel, smoothTime, 99f, dt);
		rot = Quaternion.LookRotation(forward, this.m_smoothedCameraUp);
	}

	// Token: 0x06000AE5 RID: 2789 RVA: 0x0004E540 File Offset: 0x0004C740
	private void UpdateNearClipping(Vector3 eyePos, Vector3 camPos, float dt)
	{
		float num = this.m_nearClipPlaneMax;
		Vector3 normalized = (camPos - eyePos).normalized;
		if (this.m_waterClipping || Physics.CheckSphere(camPos - normalized * this.m_nearClipPlaneMax, this.m_nearClipPlaneMax, this.m_blockCameraMask))
		{
			num = this.m_nearClipPlaneMin;
		}
		if (this.m_camera.nearClipPlane != num)
		{
			this.m_camera.nearClipPlane = num;
		}
	}

	// Token: 0x06000AE6 RID: 2790 RVA: 0x0004E5B8 File Offset: 0x0004C7B8
	private void CollideRay2(Vector3 eyePos, Vector3 offsetedEyePos, ref Vector3 end)
	{
		float num;
		if (this.RayTestPoint(eyePos, offsetedEyePos, (end - offsetedEyePos).normalized, Vector3.Distance(eyePos, end), out num))
		{
			float t = Utils.LerpStep(0.5f, 2f, num);
			Vector3 a = eyePos + (end - eyePos).normalized * num;
			Vector3 b = offsetedEyePos + (end - offsetedEyePos).normalized * num;
			end = Vector3.Lerp(a, b, t);
		}
	}

	// Token: 0x06000AE7 RID: 2791 RVA: 0x0004E654 File Offset: 0x0004C854
	private bool RayTestPoint(Vector3 point, Vector3 offsetedPoint, Vector3 dir, float maxDist, out float distance)
	{
		bool result = false;
		distance = maxDist;
		RaycastHit raycastHit;
		if (Physics.SphereCast(offsetedPoint, this.m_raycastWidth, dir, out raycastHit, maxDist, this.m_blockCameraMask))
		{
			distance = raycastHit.distance;
			result = true;
		}
		offsetedPoint + dir * distance;
		if (Physics.SphereCast(point, this.m_raycastWidth, dir, out raycastHit, maxDist, this.m_blockCameraMask))
		{
			if (raycastHit.distance < distance)
			{
				distance = raycastHit.distance;
			}
			result = true;
		}
		if (Physics.Raycast(point, dir, out raycastHit, maxDist, this.m_blockCameraMask))
		{
			float num = raycastHit.distance - this.m_nearClipPlaneMin;
			if (num < distance)
			{
				distance = num;
			}
			result = true;
		}
		return result;
	}

	// Token: 0x06000AE8 RID: 2792 RVA: 0x0004E70C File Offset: 0x0004C90C
	private bool RayTestPoint(Vector3 point, Vector3 dir, float maxDist, out Vector3 hitPoint)
	{
		RaycastHit raycastHit;
		if (Physics.SphereCast(point, 0.2f, dir, out raycastHit, maxDist, this.m_blockCameraMask))
		{
			hitPoint = point + dir * raycastHit.distance;
			return true;
		}
		if (Physics.Raycast(point, dir, out raycastHit, maxDist, this.m_blockCameraMask))
		{
			hitPoint = point + dir * (raycastHit.distance - 0.05f);
			return true;
		}
		hitPoint = Vector3.zero;
		return false;
	}

	// Token: 0x06000AE9 RID: 2793 RVA: 0x0004E798 File Offset: 0x0004C998
	private void UpdateFreeFly(float dt)
	{
		if (global::Console.IsVisible())
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		zero.x = Input.GetAxis("Mouse X");
		zero.y = Input.GetAxis("Mouse Y");
		zero.x += ZInput.GetJoyRightStickX() * 110f * dt;
		zero.y += -ZInput.GetJoyRightStickY() * 110f * dt;
		this.m_freeFlyYaw += zero.x;
		this.m_freeFlyPitch -= zero.y;
		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			this.m_freeFlySpeed *= 0.8f;
		}
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			this.m_freeFlySpeed *= 1.2f;
		}
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			this.m_freeFlySpeed *= 1.2f;
		}
		if (ZInput.GetButton("JoyTabLeft"))
		{
			this.m_camera.fieldOfView = Mathf.Max(this.m_freeFlyMinFov, this.m_camera.fieldOfView - dt * 20f);
		}
		if (ZInput.GetButton("JoyTabRight"))
		{
			this.m_camera.fieldOfView = Mathf.Min(this.m_freeFlyMaxFov, this.m_camera.fieldOfView + dt * 20f);
		}
		this.m_skyCamera.fieldOfView = this.m_camera.fieldOfView;
		if (ZInput.GetButton("JoyButtonY"))
		{
			this.m_freeFlySpeed += this.m_freeFlySpeed * 0.1f * dt * 10f;
		}
		if (ZInput.GetButton("JoyButtonX"))
		{
			this.m_freeFlySpeed -= this.m_freeFlySpeed * 0.1f * dt * 10f;
		}
		this.m_freeFlySpeed = Mathf.Clamp(this.m_freeFlySpeed, 1f, 1000f);
		if (ZInput.GetButtonDown("JoyLStick") || ZInput.GetButtonDown("SecondAttack"))
		{
			if (this.m_freeFlyLockon)
			{
				this.m_freeFlyLockon = null;
			}
			else
			{
				int mask = LayerMask.GetMask(new string[]
				{
					"Default",
					"static_solid",
					"terrain",
					"vehicle",
					"character",
					"piece",
					"character_net",
					"viewblock"
				});
				RaycastHit raycastHit;
				if (Physics.Raycast(base.transform.position, base.transform.forward, out raycastHit, 10000f, mask))
				{
					this.m_freeFlyLockon = raycastHit.collider.transform;
					this.m_freeFlyLockonOffset = this.m_freeFlyLockon.InverseTransformPoint(base.transform.position);
				}
			}
		}
		Vector3 vector = Vector3.zero;
		if (ZInput.GetButton("Left"))
		{
			vector -= Vector3.right;
		}
		if (ZInput.GetButton("Right"))
		{
			vector += Vector3.right;
		}
		if (ZInput.GetButton("Forward"))
		{
			vector += Vector3.forward;
		}
		if (ZInput.GetButton("Backward"))
		{
			vector -= Vector3.forward;
		}
		if (ZInput.GetButton("Jump"))
		{
			vector += Vector3.up;
		}
		if (ZInput.GetButton("Crouch"))
		{
			vector -= Vector3.up;
		}
		vector += Vector3.up * ZInput.GetJoyRTrigger();
		vector -= Vector3.up * ZInput.GetJoyLTrigger();
		vector += Vector3.right * ZInput.GetJoyLeftStickX();
		vector += -Vector3.forward * ZInput.GetJoyLeftStickY();
		if (ZInput.GetButtonDown("JoyButtonB") || ZInput.GetButtonDown("Block"))
		{
			this.m_freeFlySavedVel = vector;
		}
		float magnitude = this.m_freeFlySavedVel.magnitude;
		if (magnitude > 0.001f)
		{
			vector += this.m_freeFlySavedVel;
			if (vector.magnitude > magnitude)
			{
				vector = vector.normalized * magnitude;
			}
		}
		if (vector.magnitude > 1f)
		{
			vector.Normalize();
		}
		vector = base.transform.TransformVector(vector);
		vector *= this.m_freeFlySpeed;
		if (this.m_freeFlySmooth <= 0f)
		{
			this.m_freeFlyVel = vector;
		}
		else
		{
			this.m_freeFlyVel = Vector3.SmoothDamp(this.m_freeFlyVel, vector, ref this.m_freeFlyAcc, this.m_freeFlySmooth, 99f, dt);
		}
		if (this.m_freeFlyLockon)
		{
			this.m_freeFlyLockonOffset += this.m_freeFlyLockon.InverseTransformVector(this.m_freeFlyVel * dt);
			base.transform.position = this.m_freeFlyLockon.TransformPoint(this.m_freeFlyLockonOffset);
		}
		else
		{
			base.transform.position = base.transform.position + this.m_freeFlyVel * dt;
		}
		Quaternion quaternion = Quaternion.Euler(0f, this.m_freeFlyYaw, 0f) * Quaternion.Euler(this.m_freeFlyPitch, 0f, 0f);
		if (this.m_freeFlyLockon)
		{
			quaternion = this.m_freeFlyLockon.rotation * quaternion;
		}
		if (ZInput.GetButtonDown("JoyRStick") || ZInput.GetButtonDown("Attack"))
		{
			if (this.m_freeFlyTarget)
			{
				this.m_freeFlyTarget = null;
			}
			else
			{
				int mask2 = LayerMask.GetMask(new string[]
				{
					"Default",
					"static_solid",
					"terrain",
					"vehicle",
					"character",
					"piece",
					"character_net",
					"viewblock"
				});
				RaycastHit raycastHit2;
				if (Physics.Raycast(base.transform.position, base.transform.forward, out raycastHit2, 10000f, mask2))
				{
					this.m_freeFlyTarget = raycastHit2.collider.transform;
					this.m_freeFlyTargetOffset = this.m_freeFlyTarget.InverseTransformPoint(raycastHit2.point);
				}
			}
		}
		if (this.m_freeFlyTarget)
		{
			quaternion = Quaternion.LookRotation((this.m_freeFlyTarget.TransformPoint(this.m_freeFlyTargetOffset) - base.transform.position).normalized, Vector3.up);
		}
		if (this.m_freeFlySmooth <= 0f)
		{
			base.transform.rotation = quaternion;
			return;
		}
		Quaternion rotation = Utils.SmoothDamp(base.transform.rotation, quaternion, ref this.m_freeFlyRef, this.m_freeFlySmooth, 9999f, dt);
		base.transform.rotation = rotation;
	}

	// Token: 0x06000AEA RID: 2794 RVA: 0x0004EE3C File Offset: 0x0004D03C
	private void UpdateCameraShake(float dt)
	{
		this.m_shakeIntensity -= dt;
		if (this.m_shakeIntensity <= 0f)
		{
			this.m_shakeIntensity = 0f;
			return;
		}
		float num = this.m_shakeIntensity * this.m_shakeIntensity * this.m_shakeIntensity;
		this.m_shakeTimer += dt * Mathf.Clamp01(this.m_shakeIntensity) * this.m_shakeFreq;
		Quaternion rhs = Quaternion.Euler(Mathf.Sin(this.m_shakeTimer) * num * this.m_shakeMovement, Mathf.Cos(this.m_shakeTimer * 0.9f) * num * this.m_shakeMovement, 0f);
		base.transform.rotation = base.transform.rotation * rhs;
	}

	// Token: 0x06000AEB RID: 2795 RVA: 0x0004EEFC File Offset: 0x0004D0FC
	public void AddShake(Vector3 point, float range, float strength, bool continous)
	{
		if (!this.m_cameraShakeEnabled)
		{
			return;
		}
		float num = Vector3.Distance(point, base.transform.position);
		if (num > range)
		{
			return;
		}
		num = Mathf.Max(1f, num);
		float num2 = 1f - num / range;
		float num3 = strength * num2;
		if (num3 < this.m_shakeIntensity)
		{
			return;
		}
		this.m_shakeIntensity = num3;
		if (continous)
		{
			this.m_shakeTimer = Time.time * Mathf.Clamp01(strength) * this.m_shakeFreq;
			return;
		}
		this.m_shakeTimer = Time.time * Mathf.Clamp01(this.m_shakeIntensity) * this.m_shakeFreq;
	}

	// Token: 0x06000AEC RID: 2796 RVA: 0x0004EF90 File Offset: 0x0004D190
	private float RayTest(Vector3 point, Vector3 dir, float maxDist)
	{
		RaycastHit raycastHit;
		if (Physics.SphereCast(point, 0.2f, dir, out raycastHit, maxDist, this.m_blockCameraMask))
		{
			return raycastHit.distance;
		}
		return maxDist;
	}

	// Token: 0x06000AED RID: 2797 RVA: 0x0004EFC4 File Offset: 0x0004D1C4
	private Vector3 GetCameraBaseOffset(Player player)
	{
		if (player.InBed())
		{
			return player.GetHeadPoint() - player.transform.position;
		}
		if (player.IsAttached() || player.IsSitting())
		{
			return player.GetHeadPoint() + Vector3.up * 0.3f - player.transform.position;
		}
		return player.m_eye.transform.position - player.transform.position;
	}

	// Token: 0x06000AEE RID: 2798 RVA: 0x0004F04C File Offset: 0x0004D24C
	private void UpdateBaseOffset(Player player, float dt)
	{
		Vector3 cameraBaseOffset = this.GetCameraBaseOffset(player);
		this.m_currentBaseOffset = Vector3.SmoothDamp(this.m_currentBaseOffset, cameraBaseOffset, ref this.m_offsetBaseVel, 0.5f, 999f, dt);
		if (Vector3.Distance(this.m_playerPos, player.transform.position) > 20f)
		{
			this.m_playerPos = player.transform.position;
		}
		this.m_playerPos = Vector3.SmoothDamp(this.m_playerPos, player.transform.position, ref this.m_playerVel, this.m_smoothness, 999f, dt);
	}

	// Token: 0x06000AEF RID: 2799 RVA: 0x0004F0E0 File Offset: 0x0004D2E0
	private Vector3 GetOffsetedEyePos()
	{
		Player localPlayer = Player.m_localPlayer;
		if (!localPlayer)
		{
			return base.transform.position;
		}
		if (localPlayer.GetStandingOnShip() != null || localPlayer.IsAttached())
		{
			return localPlayer.transform.position + this.m_currentBaseOffset + this.GetCameraOffset(localPlayer);
		}
		return this.m_playerPos + this.m_currentBaseOffset + this.GetCameraOffset(localPlayer);
	}

	// Token: 0x06000AF0 RID: 2800 RVA: 0x0004F160 File Offset: 0x0004D360
	private Vector3 GetCameraOffset(Player player)
	{
		if (this.m_distance <= 0f)
		{
			return player.m_eye.transform.TransformVector(this.m_fpsOffset);
		}
		if (player.InBed())
		{
			return Vector3.zero;
		}
		Vector3 vector = player.UseMeleeCamera() ? this.m_3rdCombatOffset : this.m_3rdOffset;
		return player.m_eye.transform.TransformVector(vector);
	}

	// Token: 0x06000AF1 RID: 2801 RVA: 0x0004F1C7 File Offset: 0x0004D3C7
	public void ToggleFreeFly()
	{
		this.m_freeFly = !this.m_freeFly;
	}

	// Token: 0x06000AF2 RID: 2802 RVA: 0x0004F1D8 File Offset: 0x0004D3D8
	public void SetFreeFlySmoothness(float smooth)
	{
		this.m_freeFlySmooth = Mathf.Clamp(smooth, 0f, 1f);
	}

	// Token: 0x06000AF3 RID: 2803 RVA: 0x0004F1F0 File Offset: 0x0004D3F0
	public float GetFreeFlySmoothness()
	{
		return this.m_freeFlySmooth;
	}

	// Token: 0x06000AF4 RID: 2804 RVA: 0x0004F1F8 File Offset: 0x0004D3F8
	public static bool InFreeFly()
	{
		return GameCamera.m_instance && GameCamera.m_instance.m_freeFly;
	}

	// Token: 0x04000A2E RID: 2606
	private Vector3 m_playerPos = Vector3.zero;

	// Token: 0x04000A2F RID: 2607
	private Vector3 m_currentBaseOffset = Vector3.zero;

	// Token: 0x04000A30 RID: 2608
	private Vector3 m_offsetBaseVel = Vector3.zero;

	// Token: 0x04000A31 RID: 2609
	private Vector3 m_playerVel = Vector3.zero;

	// Token: 0x04000A32 RID: 2610
	public Vector3 m_3rdOffset = Vector3.zero;

	// Token: 0x04000A33 RID: 2611
	public Vector3 m_3rdCombatOffset = Vector3.zero;

	// Token: 0x04000A34 RID: 2612
	public Vector3 m_fpsOffset = Vector3.zero;

	// Token: 0x04000A35 RID: 2613
	public float m_flyingDistance = 15f;

	// Token: 0x04000A36 RID: 2614
	public LayerMask m_blockCameraMask;

	// Token: 0x04000A37 RID: 2615
	public float m_minDistance;

	// Token: 0x04000A38 RID: 2616
	public float m_maxDistance = 6f;

	// Token: 0x04000A39 RID: 2617
	public float m_maxDistanceBoat = 6f;

	// Token: 0x04000A3A RID: 2618
	public float m_raycastWidth = 0.35f;

	// Token: 0x04000A3B RID: 2619
	public bool m_smoothYTilt;

	// Token: 0x04000A3C RID: 2620
	public float m_zoomSens = 10f;

	// Token: 0x04000A3D RID: 2621
	public float m_inventoryOffset = 0.1f;

	// Token: 0x04000A3E RID: 2622
	public float m_nearClipPlaneMin = 0.1f;

	// Token: 0x04000A3F RID: 2623
	public float m_nearClipPlaneMax = 0.5f;

	// Token: 0x04000A40 RID: 2624
	public float m_fov = 65f;

	// Token: 0x04000A41 RID: 2625
	public float m_freeFlyMinFov = 5f;

	// Token: 0x04000A42 RID: 2626
	public float m_freeFlyMaxFov = 120f;

	// Token: 0x04000A43 RID: 2627
	public float m_tiltSmoothnessShipMin = 0.1f;

	// Token: 0x04000A44 RID: 2628
	public float m_tiltSmoothnessShipMax = 0.5f;

	// Token: 0x04000A45 RID: 2629
	public float m_shakeFreq = 10f;

	// Token: 0x04000A46 RID: 2630
	public float m_shakeMovement = 1f;

	// Token: 0x04000A47 RID: 2631
	public float m_smoothness = 0.1f;

	// Token: 0x04000A48 RID: 2632
	public float m_minWaterDistance = 0.3f;

	// Token: 0x04000A49 RID: 2633
	public Camera m_skyCamera;

	// Token: 0x04000A4A RID: 2634
	private float m_distance = 4f;

	// Token: 0x04000A4B RID: 2635
	private bool m_freeFly;

	// Token: 0x04000A4C RID: 2636
	private float m_shakeIntensity;

	// Token: 0x04000A4D RID: 2637
	private float m_shakeTimer;

	// Token: 0x04000A4E RID: 2638
	private bool m_cameraShakeEnabled = true;

	// Token: 0x04000A4F RID: 2639
	private bool m_mouseCapture;

	// Token: 0x04000A50 RID: 2640
	private Quaternion m_freeFlyRef = Quaternion.identity;

	// Token: 0x04000A51 RID: 2641
	private float m_freeFlyYaw;

	// Token: 0x04000A52 RID: 2642
	private float m_freeFlyPitch;

	// Token: 0x04000A53 RID: 2643
	private float m_freeFlySpeed = 20f;

	// Token: 0x04000A54 RID: 2644
	private float m_freeFlySmooth;

	// Token: 0x04000A55 RID: 2645
	private Vector3 m_freeFlySavedVel = Vector3.zero;

	// Token: 0x04000A56 RID: 2646
	private Transform m_freeFlyTarget;

	// Token: 0x04000A57 RID: 2647
	private Vector3 m_freeFlyTargetOffset = Vector3.zero;

	// Token: 0x04000A58 RID: 2648
	private Transform m_freeFlyLockon;

	// Token: 0x04000A59 RID: 2649
	private Vector3 m_freeFlyLockonOffset = Vector3.zero;

	// Token: 0x04000A5A RID: 2650
	private Vector3 m_freeFlyVel = Vector3.zero;

	// Token: 0x04000A5B RID: 2651
	private Vector3 m_freeFlyAcc = Vector3.zero;

	// Token: 0x04000A5C RID: 2652
	private Vector3 m_freeFlyTurnVel = Vector3.zero;

	// Token: 0x04000A5D RID: 2653
	private bool m_shipCameraTilt = true;

	// Token: 0x04000A5E RID: 2654
	private Vector3 m_smoothedCameraUp = Vector3.up;

	// Token: 0x04000A5F RID: 2655
	private Vector3 m_smoothedCameraUpVel = Vector3.zero;

	// Token: 0x04000A60 RID: 2656
	private AudioListener m_listner;

	// Token: 0x04000A61 RID: 2657
	private Camera m_camera;

	// Token: 0x04000A62 RID: 2658
	private bool m_waterClipping;

	// Token: 0x04000A63 RID: 2659
	private static GameCamera m_instance;
}
