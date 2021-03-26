using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000110 RID: 272
public class WaterVolume : MonoBehaviour
{
	// Token: 0x06000FF8 RID: 4088 RVA: 0x00070578 File Offset: 0x0006E778
	private void Awake()
	{
		this.m_collider = base.GetComponent<BoxCollider>();
	}

	// Token: 0x06000FF9 RID: 4089 RVA: 0x00070586 File Offset: 0x0006E786
	private void Start()
	{
		this.DetectWaterDepth();
		this.SetupMaterial();
	}

	// Token: 0x06000FFA RID: 4090 RVA: 0x00070594 File Offset: 0x0006E794
	private void DetectWaterDepth()
	{
		if (this.m_heightmap)
		{
			float[] oceanDepth = this.m_heightmap.GetOceanDepth();
			this.m_normalizedDepth[0] = Mathf.Clamp01(oceanDepth[0] / 10f);
			this.m_normalizedDepth[1] = Mathf.Clamp01(oceanDepth[1] / 10f);
			this.m_normalizedDepth[2] = Mathf.Clamp01(oceanDepth[2] / 10f);
			this.m_normalizedDepth[3] = Mathf.Clamp01(oceanDepth[3] / 10f);
			return;
		}
		this.m_normalizedDepth[0] = this.m_forceDepth;
		this.m_normalizedDepth[1] = this.m_forceDepth;
		this.m_normalizedDepth[2] = this.m_forceDepth;
		this.m_normalizedDepth[3] = this.m_forceDepth;
	}

	// Token: 0x06000FFB RID: 4091 RVA: 0x0007064C File Offset: 0x0006E84C
	private void Update()
	{
		if (WaterVolume.m_waterUpdateFrame != Time.frameCount)
		{
			WaterVolume.m_waterUpdateFrame = Time.frameCount;
			this.UpdateWaterTime(Time.deltaTime);
		}
		this.UpdateFloaters();
		this.m_waterSurface.material.SetFloat(WaterVolume._WaterTime, WaterVolume.m_waterTime);
	}

	// Token: 0x06000FFC RID: 4092 RVA: 0x0007069C File Offset: 0x0006E89C
	private void UpdateWaterTime(float dt)
	{
		float num = this.m_menuWater ? Time.time : ZNet.instance.GetWrappedDayTimeSeconds();
		WaterVolume.m_waterTime += dt;
		if (Mathf.Abs(num - WaterVolume.m_waterTime) > 10f)
		{
			WaterVolume.m_waterTime = num;
		}
		WaterVolume.m_waterTime = Mathf.Lerp(WaterVolume.m_waterTime, num, 0.05f);
	}

	// Token: 0x06000FFD RID: 4093 RVA: 0x00070700 File Offset: 0x0006E900
	private void SetupMaterial()
	{
		if (this.m_forceDepth >= 0f)
		{
			this.m_waterSurface.material.SetFloatArray(WaterVolume._depth, new float[]
			{
				this.m_forceDepth,
				this.m_forceDepth,
				this.m_forceDepth,
				this.m_forceDepth
			});
		}
		else
		{
			this.m_waterSurface.material.SetFloatArray(WaterVolume._depth, this.m_normalizedDepth);
		}
		this.m_waterSurface.material.SetFloat(WaterVolume._UseGlobalWind, this.m_useGlobalWind ? 1f : 0f);
	}

	// Token: 0x06000FFE RID: 4094 RVA: 0x000707A0 File Offset: 0x0006E9A0
	public static float GetWaterLevel(Vector3 p, float waveFactor = 1f)
	{
		if (WaterVolume.m_waterVolumeMask == 0)
		{
			WaterVolume.m_waterVolumeMask = LayerMask.GetMask(new string[]
			{
				"WaterVolume"
			});
		}
		int num = Physics.OverlapSphereNonAlloc(p, 0f, WaterVolume.tempColliderArray, WaterVolume.m_waterVolumeMask);
		for (int i = 0; i < num; i++)
		{
			WaterVolume component = WaterVolume.tempColliderArray[i].GetComponent<WaterVolume>();
			if (component)
			{
				return component.GetWaterSurface(p, waveFactor);
			}
		}
		return -10000f;
	}

	// Token: 0x06000FFF RID: 4095 RVA: 0x00070814 File Offset: 0x0006EA14
	private float GetWaterSurface(Vector3 point, float waveFactor = 1f)
	{
		float wrappedDayTimeSeconds = ZNet.instance.GetWrappedDayTimeSeconds();
		float depth = this.Depth(point);
		float num = this.CalcWave(point, depth, wrappedDayTimeSeconds, waveFactor);
		float num2 = base.transform.position.y + num;
		if (Utils.LengthXZ(point) > 10500f && this.m_forceDepth < 0f)
		{
			num2 -= 100f;
		}
		return num2;
	}

	// Token: 0x06001000 RID: 4096 RVA: 0x00070875 File Offset: 0x0006EA75
	private float TrochSin(float x, float k)
	{
		return Mathf.Sin(x - Mathf.Cos(x) * k) * 0.5f + 0.5f;
	}

	// Token: 0x06001001 RID: 4097 RVA: 0x00070894 File Offset: 0x0006EA94
	private float CreateWave(Vector3 worldPos, float time, float waveSpeed, float waveLength, float waveHeight, Vector2 dir2d, float sharpness)
	{
		Vector3 normalized = new Vector3(dir2d.x, 0f, dir2d.y).normalized;
		Vector3 a = Vector3.Cross(normalized, Vector3.up);
		Vector3 vector = -(worldPos.z * normalized + worldPos.x * a);
		return (this.TrochSin(time * waveSpeed + vector.z * waveLength, sharpness) * this.TrochSin(time * waveSpeed * 0.123f + vector.x * 0.13123f * waveLength, sharpness) - 0.2f) * waveHeight;
	}

	// Token: 0x06001002 RID: 4098 RVA: 0x00070934 File Offset: 0x0006EB34
	private float CalcWave(Vector3 worldPos, float depth, Vector4 wind, float _WaterTime, float waveFactor)
	{
		Vector3 vector = new Vector3(wind.x, wind.y, wind.z);
		float w = wind.w;
		float num = Mathf.Lerp(0f, w, depth);
		float time = _WaterTime / 20f;
		float num2 = this.CreateWave(worldPos, time, 10f, 0.04f, 8f, new Vector2(vector.x, vector.z), 0.5f);
		float num3 = this.CreateWave(worldPos, time, 14.123f, 0.08f, 6f, new Vector2(1.0312f, 0.312f), 0.5f);
		float num4 = this.CreateWave(worldPos, time, 22.312f, 0.1f, 4f, new Vector2(-0.123f, 1.12f), 0.5f);
		float num5 = this.CreateWave(worldPos, time, 31.42f, 0.2f, 2f, new Vector2(0.423f, 0.124f), 0.5f);
		float num6 = this.CreateWave(worldPos, time, 35.42f, 0.4f, 1f, new Vector2(0.123f, -0.64f), 0.5f);
		float num7 = this.CreateWave(worldPos, time, 38.1223f, 1f, 0.8f, new Vector2(-0.523f, -0.64f), 0.7f);
		float num8 = this.CreateWave(worldPos, time, 41.1223f, 1.2f, 0.6f * waveFactor, new Vector2(0.223f, 0.74f), 0.8f);
		float num9 = this.CreateWave(worldPos, time, 51.5123f, 1.3f, 0.4f * waveFactor, new Vector2(0.923f, -0.24f), 0.9f);
		float num10 = this.CreateWave(worldPos, time, 54.2f, 1.3f, 0.3f * waveFactor, new Vector2(-0.323f, 0.44f), 0.9f);
		float num11 = this.CreateWave(worldPos, time, 56.123f, 1.5f, 0.2f * waveFactor, new Vector2(0.5312f, -0.812f), 0.9f);
		return (num2 + num3 + num4 + num5 + num6 + num7 + num8 + num9 + num10 + num11) * num;
	}

	// Token: 0x06001003 RID: 4099 RVA: 0x00070B64 File Offset: 0x0006ED64
	private float CalcWave(Vector3 worldPos, float depth, float _WaterTime, float waveFactor)
	{
		Vector4 wind = new Vector4(1f, 0f, 0f, 0f);
		Vector4 wind2 = new Vector4(1f, 0f, 0f, 0f);
		float t = 0f;
		if (this.m_useGlobalWind)
		{
			EnvMan.instance.GetWindData(out wind, out wind2, out t);
		}
		float a = this.CalcWave(worldPos, depth, wind, _WaterTime, waveFactor);
		float b = this.CalcWave(worldPos, depth, wind2, _WaterTime, waveFactor);
		return Mathf.Lerp(a, b, t);
	}

	// Token: 0x06001004 RID: 4100 RVA: 0x00070BE8 File Offset: 0x0006EDE8
	private float Depth(Vector3 point)
	{
		Vector3 vector = base.transform.InverseTransformPoint(point);
		float num = (vector.x + this.m_collider.bounds.size.x / 2f) / this.m_collider.bounds.size.x;
		float num2 = (vector.z + this.m_collider.bounds.size.z / 2f) / this.m_collider.bounds.size.z;
		num = Mathf.Clamp01(num);
		num2 = Mathf.Clamp01(num2);
		float a = Mathf.Lerp(this.m_normalizedDepth[3], this.m_normalizedDepth[2], num);
		float b = Mathf.Lerp(this.m_normalizedDepth[0], this.m_normalizedDepth[1], num);
		return Mathf.Lerp(a, b, num2);
	}

	// Token: 0x06001005 RID: 4101 RVA: 0x00070CC0 File Offset: 0x0006EEC0
	private void OnTriggerEnter(Collider collider)
	{
		IWaterInteractable component = collider.attachedRigidbody.GetComponent<IWaterInteractable>();
		if (component != null && !this.m_inWater.Contains(component))
		{
			this.m_inWater.Add(component);
		}
	}

	// Token: 0x06001006 RID: 4102 RVA: 0x00070CF8 File Offset: 0x0006EEF8
	private void UpdateFloaters()
	{
		if (this.m_inWater.Count == 0)
		{
			return;
		}
		IWaterInteractable waterInteractable = null;
		foreach (IWaterInteractable waterInteractable2 in this.m_inWater)
		{
			if (waterInteractable2.IsOwner())
			{
				Transform transform = waterInteractable2.GetTransform();
				if (transform)
				{
					float waterSurface = this.GetWaterSurface(transform.position, 1f);
					waterInteractable2.SetInWater(waterSurface);
				}
				else
				{
					waterInteractable = waterInteractable2;
				}
			}
		}
		if (waterInteractable != null)
		{
			this.m_inWater.Remove(waterInteractable);
		}
	}

	// Token: 0x06001007 RID: 4103 RVA: 0x00070D9C File Offset: 0x0006EF9C
	private void OnTriggerExit(Collider collider)
	{
		IWaterInteractable component = collider.attachedRigidbody.GetComponent<IWaterInteractable>();
		if (component != null)
		{
			component.SetInWater(-10000f);
			this.m_inWater.Remove(component);
		}
	}

	// Token: 0x04000EDD RID: 3805
	private static Collider[] tempColliderArray = new Collider[256];

	// Token: 0x04000EDE RID: 3806
	private float[] m_normalizedDepth = new float[4];

	// Token: 0x04000EDF RID: 3807
	private BoxCollider m_collider;

	// Token: 0x04000EE0 RID: 3808
	public MeshRenderer m_waterSurface;

	// Token: 0x04000EE1 RID: 3809
	public Heightmap m_heightmap;

	// Token: 0x04000EE2 RID: 3810
	public float m_forceDepth = -1f;

	// Token: 0x04000EE3 RID: 3811
	public bool m_menuWater;

	// Token: 0x04000EE4 RID: 3812
	public bool m_useGlobalWind = true;

	// Token: 0x04000EE5 RID: 3813
	private static float m_waterTime = 0f;

	// Token: 0x04000EE6 RID: 3814
	private static int m_waterUpdateFrame = 0;

	// Token: 0x04000EE7 RID: 3815
	private static int m_waterVolumeMask = 0;

	// Token: 0x04000EE8 RID: 3816
	private static int _WaterTime = Shader.PropertyToID("_WaterTime");

	// Token: 0x04000EE9 RID: 3817
	private static int _depth = Shader.PropertyToID("_depth");

	// Token: 0x04000EEA RID: 3818
	private static int _UseGlobalWind = Shader.PropertyToID("_UseGlobalWind");

	// Token: 0x04000EEB RID: 3819
	private List<IWaterInteractable> m_inWater = new List<IWaterInteractable>();
}
