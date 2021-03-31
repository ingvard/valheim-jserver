using System;
using UnityEngine;

// Token: 0x02000039 RID: 57
public class FollowPlayer : MonoBehaviour
{
	// Token: 0x06000428 RID: 1064 RVA: 0x00021AA0 File Offset: 0x0001FCA0
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

	// Token: 0x04000428 RID: 1064
	public FollowPlayer.Type m_follow = FollowPlayer.Type.Camera;

	// Token: 0x04000429 RID: 1065
	public bool m_lockYPos;

	// Token: 0x0400042A RID: 1066
	public bool m_followCameraInFreefly;

	// Token: 0x0400042B RID: 1067
	public float m_maxYPos = 1000000f;

	// Token: 0x02000139 RID: 313
	public enum Type
	{
		// Token: 0x0400105D RID: 4189
		Player,
		// Token: 0x0400105E RID: 4190
		Camera
	}
}
