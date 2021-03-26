using System;
using UnityEngine;

// Token: 0x020000B3 RID: 179
public class TerrainLod : MonoBehaviour
{
	// Token: 0x06000BEF RID: 3055 RVA: 0x000550A9 File Offset: 0x000532A9
	private void Awake()
	{
		this.m_hmap = base.GetComponent<Heightmap>();
	}

	// Token: 0x06000BF0 RID: 3056 RVA: 0x000550B8 File Offset: 0x000532B8
	private void Update()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		if (ZNet.GetConnectionStatus() != ZNet.ConnectionStatus.Connected)
		{
			return;
		}
		Vector3 position = mainCamera.transform.position;
		if (Utils.DistanceXZ(position, this.m_lastPoint) > this.m_updateStepDistance)
		{
			this.m_lastPoint = new Vector3(Mathf.Round(position.x / this.m_hmap.m_scale) * this.m_hmap.m_scale, 0f, Mathf.Round(position.z / this.m_hmap.m_scale) * this.m_hmap.m_scale);
			this.m_needRebuild = true;
		}
		if (this.m_needRebuild && HeightmapBuilder.instance.IsTerrainReady(this.m_lastPoint, this.m_hmap.m_width, this.m_hmap.m_scale, this.m_hmap.m_isDistantLod, WorldGenerator.instance))
		{
			base.transform.position = this.m_lastPoint;
			this.m_hmap.Regenerate();
			this.m_needRebuild = false;
		}
	}

	// Token: 0x04000B12 RID: 2834
	public float m_updateStepDistance = 256f;

	// Token: 0x04000B13 RID: 2835
	private Heightmap m_hmap;

	// Token: 0x04000B14 RID: 2836
	private Vector3 m_lastPoint = new Vector3(99999f, 0f, 99999f);

	// Token: 0x04000B15 RID: 2837
	private bool m_needRebuild = true;
}
