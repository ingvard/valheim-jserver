using System;

// Token: 0x020000D7 RID: 215
public interface Interactable
{
	// Token: 0x06000DCE RID: 3534
	bool Interact(Humanoid user, bool hold);

	// Token: 0x06000DCF RID: 3535
	bool UseItem(Humanoid user, ItemDrop.ItemData item);
}
