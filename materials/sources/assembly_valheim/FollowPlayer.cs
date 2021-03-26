using System;
using UnityEngine;

// Token: 0x02000039 RID: 57
public class FollowPlayer : MonoBehaviour
{
	// Token: 0x06000427 RID: 1063 RVA: 0x000219EC File Offset: 0x0001FBEC
	private void LateUpdate()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (Player.m_localPlayer == null || mainCamera == null)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		if (this.m_follow == FollowPlayer.Type.Camera || GameCamera.InFreeFly())
		{
			vector = mainCamera.transform.position;
		}
		else
		{
			vector = Player.m_localPlayer.transform.position;
		}
		if (this.m_lockYPos)
		{
			vector.y = base.transform.position.y;
		}
		if (vector.y > this.m_maxYPos)
		{
			vector.y = this.m_maxYPos;
		}
		base.transform.position = vector;
	}

	// Token: 0x04000424 RID: 1060
	public FollowPlayer.Type m_follow = FollowPlayer.Type.Camera;

	// Token: 0x04000425 RID: 1061
	public bool m_lockYPos;

	// Token: 0x04000426 RID: 1062
	public bool m_followCameraInFreefly;

	// Token: 0x04000427 RID: 1063
	public float m_maxYPos = 1000000f;

	// Token: 0x02000139 RID: 313
	public enum Type
	{
		// Token: 0x04001056 RID: 4182
		Player,
		// Token: 0x04001057 RID: 4183
		Camera
	}
}
