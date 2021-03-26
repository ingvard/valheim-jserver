using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000056 RID: 86
public class InventoryGui : MonoBehaviour
{
	// Token: 0x1700000A RID: 10
	// (get) Token: 0x06000539 RID: 1337 RVA: 0x0002C1C2 File Offset: 0x0002A3C2
	public static InventoryGui instance
	{
		get
		{
			return InventoryGui.m_instance;
		}
	}

	// Token: 0x0600053A RID: 1338 RVA: 0x0002C1CC File Offset: 0x0002A3CC
	private void Awake()
	{
		InventoryGui.m_instance = this;
		this.m_animator = base.GetComponent<Animator>();
		this.m_inventoryRoot.gameObject.SetActive(true);
		this.m_container.gameObject.SetActive(false);
		this.m_splitPanel.gameObject.SetActive(false);
		this.m_trophiesPanel.SetActive(false);
		this.m_variantDialog.gameObject.SetActive(false);
		this.m_skillsDialog.gameObject.SetActive(false);
		this.m_textsDialog.gameObject.SetActive(false);
		this.m_playerGrid = this.m_player.GetComponentInChildren<InventoryGrid>();
		this.m_containerGrid = this.m_container.GetComponentInChildren<InventoryGrid>();
		InventoryGrid playerGrid = this.m_playerGrid;
		playerGrid.m_onSelected = (Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier>)Delegate.Combine(playerGrid.m_onSelected, new Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier>(this.OnSelectedItem));
		InventoryGrid playerGrid2 = this.m_playerGrid;
		playerGrid2.m_onRightClick = (Action<InventoryGrid, ItemDrop.ItemData, Vector2i>)Delegate.Combine(playerGrid2.m_onRightClick, new Action<InventoryGrid, ItemDrop.ItemData, Vector2i>(this.OnRightClickItem));
		InventoryGrid containerGrid = this.m_containerGrid;
		containerGrid.m_onSelected = (Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier>)Delegate.Combine(containerGrid.m_onSelected, new Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier>(this.OnSelectedItem));
		InventoryGrid containerGrid2 = this.m_containerGrid;
		containerGrid2.m_onRightClick = (Action<InventoryGrid, ItemDrop.ItemData, Vector2i>)Delegate.Combine(containerGrid2.m_onRightClick, new Action<InventoryGrid, ItemDrop.ItemData, Vector2i>(this.OnRightClickItem));
		this.m_craftButton.onClick.AddListener(new UnityAction(this.OnCraftPressed));
		this.m_craftCancelButton.onClick.AddListener(new UnityAction(this.OnCraftCancelPressed));
		this.m_dropButton.onClick.AddListener(new UnityAction(this.OnDropOutside));
		this.m_takeAllButton.onClick.AddListener(new UnityAction(this.OnTakeAll));
		this.m_repairButton.onClick.AddListener(new UnityAction(this.OnRepairPressed));
		this.m_splitSlider.onValueChanged.AddListener(new UnityAction<float>(this.OnSplitSliderChanged));
		this.m_splitCancelButton.onClick.AddListener(new UnityAction(this.OnSplitCancel));
		this.m_splitOkButton.onClick.AddListener(new UnityAction(this.OnSplitOk));
		VariantDialog variantDialog = this.m_variantDialog;
		variantDialog.m_selected = (Action<int>)Delegate.Combine(variantDialog.m_selected, new Action<int>(this.OnVariantSelected));
		this.m_recipeListBaseSize = this.m_recipeListRoot.rect.height;
		this.m_trophieListBaseSize = this.m_trophieListRoot.rect.height;
		this.m_minStationLevelBasecolor = this.m_minStationLevelText.color;
		this.m_tabCraft.interactable = false;
		this.m_tabUpgrade.interactable = true;
	}

	// Token: 0x0600053B RID: 1339 RVA: 0x0002C47D File Offset: 0x0002A67D
	private void OnDestroy()
	{
		InventoryGui.m_instance = null;
	}

	// Token: 0x0600053C RID: 1340 RVA: 0x0002C488 File Offset: 0x0002A688
	private void Update()
	{
		bool @bool = this.m_animator.GetBool("visible");
		if (!@bool)
		{
			this.m_hiddenFrames++;
		}
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null || localPlayer.IsDead() || localPlayer.InCutscene() || localPlayer.IsTeleporting())
		{
			this.Hide();
			return;
		}
		if (this.m_craftTimer < 0f && (Chat.instance == null || !Chat.instance.HasFocus()) && !global::Console.IsVisible() && !Menu.IsVisible() && TextViewer.instance && !TextViewer.instance.IsVisible() && !localPlayer.InCutscene() && !GameCamera.InFreeFly() && !Minimap.IsOpen())
		{
			if (this.m_trophiesPanel.activeSelf && (ZInput.GetButtonDown("JoyButtonB") || Input.GetKeyDown(KeyCode.Escape)))
			{
				this.m_trophiesPanel.SetActive(false);
			}
			else if (this.m_skillsDialog.gameObject.activeSelf && (ZInput.GetButtonDown("JoyButtonB") || Input.GetKeyDown(KeyCode.Escape)))
			{
				this.m_skillsDialog.gameObject.SetActive(false);
			}
			else if (this.m_textsDialog.gameObject.activeSelf && (ZInput.GetButtonDown("JoyButtonB") || Input.GetKeyDown(KeyCode.Escape)))
			{
				this.m_textsDialog.gameObject.SetActive(false);
			}
			else if (@bool)
			{
				if (ZInput.GetButtonDown("Inventory") || ZInput.GetButtonDown("JoyButtonB") || ZInput.GetButtonDown("JoyButtonY") || Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("Use"))
				{
					ZInput.ResetButtonStatus("Inventory");
					ZInput.ResetButtonStatus("JoyButtonB");
					ZInput.ResetButtonStatus("JoyButtonY");
					ZInput.ResetButtonStatus("Use");
					this.Hide();
				}
			}
			else if (ZInput.GetButtonDown("Inventory") || ZInput.GetButtonDown("JoyButtonY"))
			{
				ZInput.ResetButtonStatus("Inventory");
				ZInput.ResetButtonStatus("JoyButtonY");
				localPlayer.ShowTutorial("inventory", true);
				this.Show(null);
			}
		}
		if (@bool)
		{
			this.m_hiddenFrames = 0;
			this.UpdateGamepad();
			this.UpdateInventory(localPlayer);
			this.UpdateContainer(localPlayer);
			this.UpdateItemDrag();
			this.UpdateCharacterStats(localPlayer);
			this.UpdateInventoryWeight(localPlayer);
			this.UpdateContainerWeight();
			this.UpdateRecipe(localPlayer, Time.deltaTime);
			this.UpdateRepair();
		}
	}

	// Token: 0x0600053D RID: 1341 RVA: 0x0002C6F4 File Offset: 0x0002A8F4
	private void UpdateGamepad()
	{
		if (!this.m_inventoryGroup.IsActive())
		{
			return;
		}
		if (ZInput.GetButtonDown("JoyTabLeft"))
		{
			this.SetActiveGroup(this.m_activeGroup - 1);
		}
		if (ZInput.GetButtonDown("JoyTabRight"))
		{
			this.SetActiveGroup(this.m_activeGroup + 1);
		}
		if (this.m_activeGroup == 0 && !this.IsContainerOpen())
		{
			this.SetActiveGroup(1);
		}
		if (this.m_activeGroup == 3)
		{
			this.UpdateRecipeGamepadInput();
		}
	}

	// Token: 0x0600053E RID: 1342 RVA: 0x0002C76C File Offset: 0x0002A96C
	private void SetActiveGroup(int index)
	{
		index = Mathf.Clamp(index, 0, this.m_uiGroups.Length - 1);
		this.m_activeGroup = index;
		for (int i = 0; i < this.m_uiGroups.Length; i++)
		{
			this.m_uiGroups[i].SetActive(i == this.m_activeGroup);
		}
	}

	// Token: 0x0600053F RID: 1343 RVA: 0x0002C7BC File Offset: 0x0002A9BC
	private void UpdateCharacterStats(Player player)
	{
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		this.m_playerName.text = playerProfile.GetName();
		float bodyArmor = player.GetBodyArmor();
		this.m_armor.text = bodyArmor.ToString();
		this.m_pvp.interactable = player.CanSwitchPVP();
		player.SetPVP(this.m_pvp.isOn);
	}

	// Token: 0x06000540 RID: 1344 RVA: 0x0002C820 File Offset: 0x0002AA20
	private void UpdateInventoryWeight(Player player)
	{
		int num = Mathf.CeilToInt(player.GetInventory().GetTotalWeight());
		int num2 = Mathf.CeilToInt(player.GetMaxCarryWeight());
		if (num <= num2)
		{
			this.m_weight.text = num + "/" + num2;
			return;
		}
		if (Mathf.Sin(Time.time * 10f) > 0f)
		{
			this.m_weight.text = string.Concat(new object[]
			{
				"<color=red>",
				num,
				"</color>/",
				num2
			});
			return;
		}
		this.m_weight.text = num + "/" + num2;
	}

	// Token: 0x06000541 RID: 1345 RVA: 0x0002C8E4 File Offset: 0x0002AAE4
	private void UpdateContainerWeight()
	{
		if (this.m_currentContainer == null)
		{
			return;
		}
		int num = Mathf.CeilToInt(this.m_currentContainer.GetInventory().GetTotalWeight());
		this.m_containerWeight.text = num.ToString();
	}

	// Token: 0x06000542 RID: 1346 RVA: 0x0002C928 File Offset: 0x0002AB28
	private void UpdateInventory(Player player)
	{
		Inventory inventory = player.GetInventory();
		this.m_playerGrid.UpdateInventory(inventory, player, this.m_dragItem);
	}

	// Token: 0x06000543 RID: 1347 RVA: 0x0002C950 File Offset: 0x0002AB50
	private void UpdateContainer(Player player)
	{
		if (!this.m_animator.GetBool("visible"))
		{
			return;
		}
		if (this.m_currentContainer && this.m_currentContainer.IsOwner())
		{
			this.m_currentContainer.SetInUse(true);
			this.m_container.gameObject.SetActive(true);
			this.m_containerGrid.UpdateInventory(this.m_currentContainer.GetInventory(), null, this.m_dragItem);
			this.m_containerName.text = Localization.instance.Localize(this.m_currentContainer.GetInventory().GetName());
			if (this.m_firstContainerUpdate)
			{
				this.m_containerGrid.ResetView();
				this.m_firstContainerUpdate = false;
			}
			if (Vector3.Distance(this.m_currentContainer.transform.position, player.transform.position) > this.m_autoCloseDistance)
			{
				this.CloseContainer();
				return;
			}
		}
		else
		{
			this.m_container.gameObject.SetActive(false);
		}
	}

	// Token: 0x06000544 RID: 1348 RVA: 0x0002CA4C File Offset: 0x0002AC4C
	private RectTransform GetSelectedGamepadElement()
	{
		RectTransform gamepadSelectedElement = this.m_playerGrid.GetGamepadSelectedElement();
		if (gamepadSelectedElement)
		{
			return gamepadSelectedElement;
		}
		if (this.m_container.gameObject.activeSelf)
		{
			return this.m_containerGrid.GetGamepadSelectedElement();
		}
		return null;
	}

	// Token: 0x06000545 RID: 1349 RVA: 0x0002CA90 File Offset: 0x0002AC90
	private void UpdateItemDrag()
	{
		if (this.m_dragGo)
		{
			if (ZInput.IsGamepadActive() && !ZInput.IsMouseActive())
			{
				RectTransform selectedGamepadElement = this.GetSelectedGamepadElement();
				if (selectedGamepadElement)
				{
					Vector3[] array = new Vector3[4];
					selectedGamepadElement.GetWorldCorners(array);
					this.m_dragGo.transform.position = array[2] + new Vector3(0f, 32f, 0f);
				}
				else
				{
					this.m_dragGo.transform.position = new Vector3(-99999f, 0f, 0f);
				}
			}
			else
			{
				this.m_dragGo.transform.position = Input.mousePosition;
			}
			Image component = this.m_dragGo.transform.Find("icon").GetComponent<Image>();
			Text component2 = this.m_dragGo.transform.Find("name").GetComponent<Text>();
			Text component3 = this.m_dragGo.transform.Find("amount").GetComponent<Text>();
			component.sprite = this.m_dragItem.GetIcon();
			component2.text = this.m_dragItem.m_shared.m_name;
			component3.text = ((this.m_dragAmount > 1) ? this.m_dragAmount.ToString() : "");
			if (Input.GetMouseButton(1))
			{
				this.SetupDragItem(null, null, 1);
			}
		}
	}

	// Token: 0x06000546 RID: 1350 RVA: 0x0002CBF0 File Offset: 0x0002ADF0
	private void OnTakeAll()
	{
		if (Player.m_localPlayer.IsTeleporting())
		{
			return;
		}
		if (this.m_currentContainer)
		{
			this.SetupDragItem(null, null, 1);
			Inventory inventory = this.m_currentContainer.GetInventory();
			Player.m_localPlayer.GetInventory().MoveAll(inventory);
		}
	}

	// Token: 0x06000547 RID: 1351 RVA: 0x0002CC3C File Offset: 0x0002AE3C
	private void OnDropOutside()
	{
		if (this.m_dragGo)
		{
			ZLog.Log("Drop item " + this.m_dragItem.m_shared.m_name);
			if (!this.m_dragInventory.ContainsItem(this.m_dragItem))
			{
				this.SetupDragItem(null, null, 1);
				return;
			}
			if (Player.m_localPlayer.DropItem(this.m_dragInventory, this.m_dragItem, this.m_dragAmount))
			{
				this.m_moveItemEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
				this.SetupDragItem(null, null, 1);
				this.UpdateCraftingPanel(false);
			}
		}
	}

	// Token: 0x06000548 RID: 1352 RVA: 0x0002CCE5 File Offset: 0x0002AEE5
	private void OnRightClickItem(InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos)
	{
		if (item != null && Player.m_localPlayer)
		{
			Player.m_localPlayer.UseItem(grid.GetInventory(), item, true);
		}
	}

	// Token: 0x06000549 RID: 1353 RVA: 0x0002CD08 File Offset: 0x0002AF08
	private void OnSelectedItem(InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer.IsTeleporting())
		{
			return;
		}
		if (this.m_dragGo)
		{
			this.m_moveItemEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
			bool flag = localPlayer.IsItemEquiped(this.m_dragItem);
			bool flag2 = item != null && localPlayer.IsItemEquiped(item);
			Vector2i gridPos = this.m_dragItem.m_gridPos;
			if ((this.m_dragItem.m_shared.m_questItem || (item != null && item.m_shared.m_questItem)) && this.m_dragInventory != grid.GetInventory())
			{
				return;
			}
			if (!this.m_dragInventory.ContainsItem(this.m_dragItem))
			{
				this.SetupDragItem(null, null, 1);
				return;
			}
			localPlayer.RemoveFromEquipQueue(item);
			localPlayer.RemoveFromEquipQueue(this.m_dragItem);
			localPlayer.UnequipItem(this.m_dragItem, false);
			localPlayer.UnequipItem(item, false);
			bool flag3 = grid.DropItem(this.m_dragInventory, this.m_dragItem, this.m_dragAmount, pos);
			if (this.m_dragItem.m_stack < this.m_dragAmount)
			{
				this.m_dragAmount = this.m_dragItem.m_stack;
			}
			if (flag)
			{
				ItemDrop.ItemData itemAt = grid.GetInventory().GetItemAt(pos.x, pos.y);
				if (itemAt != null)
				{
					localPlayer.EquipItem(itemAt, false);
				}
				if (localPlayer.GetInventory().ContainsItem(this.m_dragItem))
				{
					localPlayer.EquipItem(this.m_dragItem, false);
				}
			}
			if (flag2)
			{
				ItemDrop.ItemData itemAt2 = this.m_dragInventory.GetItemAt(gridPos.x, gridPos.y);
				if (itemAt2 != null)
				{
					localPlayer.EquipItem(itemAt2, false);
				}
				if (localPlayer.GetInventory().ContainsItem(item))
				{
					localPlayer.EquipItem(item, false);
				}
			}
			if (flag3)
			{
				this.SetupDragItem(null, null, 1);
				this.UpdateCraftingPanel(false);
				return;
			}
		}
		else if (item != null)
		{
			if (mod == InventoryGrid.Modifier.Move)
			{
				if (item.m_shared.m_questItem)
				{
					return;
				}
				if (this.m_currentContainer != null)
				{
					localPlayer.RemoveFromEquipQueue(item);
					localPlayer.UnequipItem(item, true);
					if (grid.GetInventory() == this.m_currentContainer.GetInventory())
					{
						localPlayer.GetInventory().MoveItemToThis(grid.GetInventory(), item);
					}
					else
					{
						this.m_currentContainer.GetInventory().MoveItemToThis(localPlayer.GetInventory(), item);
					}
					this.m_moveItemEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
					return;
				}
				if (Player.m_localPlayer.DropItem(localPlayer.GetInventory(), item, item.m_stack))
				{
					this.m_moveItemEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
					return;
				}
			}
			else
			{
				if (mod == InventoryGrid.Modifier.Split && item.m_stack > 1)
				{
					this.ShowSplitDialog(item, grid.GetInventory());
					return;
				}
				this.SetupDragItem(item, grid.GetInventory(), item.m_stack);
			}
		}
	}

	// Token: 0x0600054A RID: 1354 RVA: 0x0002CFD7 File Offset: 0x0002B1D7
	public static bool IsVisible()
	{
		return InventoryGui.m_instance && InventoryGui.m_instance.m_hiddenFrames <= 1;
	}

	// Token: 0x0600054B RID: 1355 RVA: 0x0002CFF7 File Offset: 0x0002B1F7
	public bool IsContainerOpen()
	{
		return this.m_currentContainer != null;
	}

	// Token: 0x0600054C RID: 1356 RVA: 0x0002D008 File Offset: 0x0002B208
	public void Show(Container container)
	{
		Hud.HidePieceSelection();
		this.m_animator.SetBool("visible", true);
		this.SetActiveGroup(1);
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			this.SetupCrafting();
		}
		this.m_currentContainer = container;
		this.m_hiddenFrames = 0;
		if (localPlayer)
		{
			this.m_openInventoryEffects.Create(localPlayer.transform.position, Quaternion.identity, null, 1f);
		}
		Gogan.LogEvent("Screen", "Enter", "Inventory", 0L);
	}

	// Token: 0x0600054D RID: 1357 RVA: 0x0002D094 File Offset: 0x0002B294
	public void Hide()
	{
		if (!this.m_animator.GetBool("visible"))
		{
			return;
		}
		this.m_craftTimer = -1f;
		this.m_animator.SetBool("visible", false);
		this.m_trophiesPanel.SetActive(false);
		this.m_variantDialog.gameObject.SetActive(false);
		this.m_skillsDialog.gameObject.SetActive(false);
		this.m_textsDialog.gameObject.SetActive(false);
		this.m_splitPanel.gameObject.SetActive(false);
		this.SetupDragItem(null, null, 1);
		if (this.m_currentContainer)
		{
			this.m_currentContainer.SetInUse(false);
			this.m_currentContainer = null;
		}
		if (Player.m_localPlayer)
		{
			this.m_closeInventoryEffects.Create(Player.m_localPlayer.transform.position, Quaternion.identity, null, 1f);
		}
		Gogan.LogEvent("Screen", "Exit", "Inventory", 0L);
	}

	// Token: 0x0600054E RID: 1358 RVA: 0x0002D194 File Offset: 0x0002B394
	private void CloseContainer()
	{
		if (this.m_dragInventory != null && this.m_dragInventory != Player.m_localPlayer.GetInventory())
		{
			this.SetupDragItem(null, null, 1);
		}
		if (this.m_currentContainer)
		{
			this.m_currentContainer.SetInUse(false);
			this.m_currentContainer = null;
		}
		this.m_splitPanel.gameObject.SetActive(false);
		this.m_firstContainerUpdate = true;
		this.m_container.gameObject.SetActive(false);
	}

	// Token: 0x0600054F RID: 1359 RVA: 0x0002D20D File Offset: 0x0002B40D
	private void SetupCrafting()
	{
		this.UpdateCraftingPanel(true);
	}

	// Token: 0x06000550 RID: 1360 RVA: 0x0002D218 File Offset: 0x0002B418
	private void UpdateCraftingPanel(bool focusView = false)
	{
		Player localPlayer = Player.m_localPlayer;
		if (!localPlayer.GetCurrentCraftingStation() && !localPlayer.NoCostCheat())
		{
			this.m_tabCraft.interactable = false;
			this.m_tabUpgrade.interactable = true;
			this.m_tabUpgrade.gameObject.SetActive(false);
		}
		else
		{
			this.m_tabUpgrade.gameObject.SetActive(true);
		}
		List<Recipe> recipes = new List<Recipe>();
		localPlayer.GetAvailableRecipes(ref recipes);
		this.UpdateRecipeList(recipes);
		if (this.m_availableRecipes.Count <= 0)
		{
			this.SetRecipe(-1, focusView);
			return;
		}
		if (this.m_selectedRecipe.Key != null)
		{
			int selectedRecipeIndex = this.GetSelectedRecipeIndex();
			this.SetRecipe(selectedRecipeIndex, focusView);
			return;
		}
		this.SetRecipe(0, focusView);
	}

	// Token: 0x06000551 RID: 1361 RVA: 0x0002D2D4 File Offset: 0x0002B4D4
	private void UpdateRecipeList(List<Recipe> recipes)
	{
		Player localPlayer = Player.m_localPlayer;
		this.m_availableRecipes.Clear();
		foreach (GameObject obj in this.m_recipeList)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_recipeList.Clear();
		if (this.InCraftTab())
		{
			bool[] array = new bool[recipes.Count];
			for (int i = 0; i < recipes.Count; i++)
			{
				Recipe recipe = recipes[i];
				array[i] = localPlayer.HaveRequirements(recipe, false, 1);
			}
			for (int j = 0; j < recipes.Count; j++)
			{
				if (array[j])
				{
					this.AddRecipeToList(localPlayer, recipes[j], null, true);
				}
			}
			for (int k = 0; k < recipes.Count; k++)
			{
				if (!array[k])
				{
					this.AddRecipeToList(localPlayer, recipes[k], null, false);
				}
			}
		}
		else
		{
			List<KeyValuePair<Recipe, ItemDrop.ItemData>> list = new List<KeyValuePair<Recipe, ItemDrop.ItemData>>();
			List<KeyValuePair<Recipe, ItemDrop.ItemData>> list2 = new List<KeyValuePair<Recipe, ItemDrop.ItemData>>();
			for (int l = 0; l < recipes.Count; l++)
			{
				Recipe recipe2 = recipes[l];
				if (recipe2.m_item.m_itemData.m_shared.m_maxQuality > 1)
				{
					this.m_tempItemList.Clear();
					localPlayer.GetInventory().GetAllItems(recipe2.m_item.m_itemData.m_shared.m_name, this.m_tempItemList);
					foreach (ItemDrop.ItemData itemData in this.m_tempItemList)
					{
						if (itemData.m_quality < itemData.m_shared.m_maxQuality && localPlayer.HaveRequirements(recipe2, false, itemData.m_quality + 1))
						{
							list.Add(new KeyValuePair<Recipe, ItemDrop.ItemData>(recipe2, itemData));
						}
						else
						{
							list2.Add(new KeyValuePair<Recipe, ItemDrop.ItemData>(recipe2, itemData));
						}
					}
				}
			}
			foreach (KeyValuePair<Recipe, ItemDrop.ItemData> keyValuePair in list)
			{
				this.AddRecipeToList(localPlayer, keyValuePair.Key, keyValuePair.Value, true);
			}
			foreach (KeyValuePair<Recipe, ItemDrop.ItemData> keyValuePair2 in list2)
			{
				this.AddRecipeToList(localPlayer, keyValuePair2.Key, keyValuePair2.Value, false);
			}
		}
		float num = (float)this.m_recipeList.Count * this.m_recipeListSpace;
		num = Mathf.Max(this.m_recipeListBaseSize, num);
		this.m_recipeListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num);
	}

	// Token: 0x06000552 RID: 1362 RVA: 0x0002D5BC File Offset: 0x0002B7BC
	private void AddRecipeToList(Player player, Recipe recipe, ItemDrop.ItemData item, bool canCraft)
	{
		int count = this.m_recipeList.Count;
		GameObject element = UnityEngine.Object.Instantiate<GameObject>(this.m_recipeElementPrefab, this.m_recipeListRoot);
		element.SetActive(true);
		(element.transform as RectTransform).anchoredPosition = new Vector2(0f, (float)count * -this.m_recipeListSpace);
		Image component = element.transform.Find("icon").GetComponent<Image>();
		component.sprite = recipe.m_item.m_itemData.GetIcon();
		component.color = (canCraft ? Color.white : new Color(1f, 0f, 1f, 0f));
		Text component2 = element.transform.Find("name").GetComponent<Text>();
		string text = Localization.instance.Localize(recipe.m_item.m_itemData.m_shared.m_name);
		if (recipe.m_amount > 1)
		{
			text = text + " x" + recipe.m_amount;
		}
		component2.text = text;
		component2.color = (canCraft ? Color.white : new Color(0.66f, 0.66f, 0.66f, 1f));
		GuiBar component3 = element.transform.Find("Durability").GetComponent<GuiBar>();
		if (item != null && item.m_shared.m_useDurability && item.m_durability < item.GetMaxDurability())
		{
			component3.gameObject.SetActive(true);
			component3.SetValue(item.GetDurabilityPercentage());
		}
		else
		{
			component3.gameObject.SetActive(false);
		}
		Text component4 = element.transform.Find("QualityLevel").GetComponent<Text>();
		if (item != null)
		{
			component4.gameObject.SetActive(true);
			component4.text = item.m_quality.ToString();
		}
		else
		{
			component4.gameObject.SetActive(false);
		}
		element.GetComponent<Button>().onClick.AddListener(delegate
		{
			this.OnSelectedRecipe(element);
		});
		this.m_recipeList.Add(element);
		this.m_availableRecipes.Add(new KeyValuePair<Recipe, ItemDrop.ItemData>(recipe, item));
	}

	// Token: 0x06000553 RID: 1363 RVA: 0x0002D804 File Offset: 0x0002BA04
	private void OnSelectedRecipe(GameObject button)
	{
		int index = this.FindSelectedRecipe(button);
		this.SetRecipe(index, false);
	}

	// Token: 0x06000554 RID: 1364 RVA: 0x0002D824 File Offset: 0x0002BA24
	private void UpdateRecipeGamepadInput()
	{
		if (this.m_availableRecipes.Count > 0)
		{
			if (ZInput.GetButtonDown("JoyLStickDown"))
			{
				this.SetRecipe(Mathf.Min(this.m_availableRecipes.Count - 1, this.GetSelectedRecipeIndex() + 1), true);
			}
			if (ZInput.GetButtonDown("JoyLStickUp"))
			{
				this.SetRecipe(Mathf.Max(0, this.GetSelectedRecipeIndex() - 1), true);
			}
		}
	}

	// Token: 0x06000555 RID: 1365 RVA: 0x0002D890 File Offset: 0x0002BA90
	private int GetSelectedRecipeIndex()
	{
		int result = 0;
		for (int i = 0; i < this.m_availableRecipes.Count; i++)
		{
			if (this.m_availableRecipes[i].Key == this.m_selectedRecipe.Key && this.m_availableRecipes[i].Value == this.m_selectedRecipe.Value)
			{
				result = i;
			}
		}
		return result;
	}

	// Token: 0x06000556 RID: 1366 RVA: 0x0002D900 File Offset: 0x0002BB00
	private void SetRecipe(int index, bool center)
	{
		ZLog.Log("Setting selected recipe " + index);
		for (int i = 0; i < this.m_recipeList.Count; i++)
		{
			bool active = i == index;
			this.m_recipeList[i].transform.Find("selected").gameObject.SetActive(active);
		}
		if (center && index >= 0)
		{
			this.m_recipeEnsureVisible.CenterOnItem(this.m_recipeList[index].transform as RectTransform);
		}
		if (index < 0)
		{
			this.m_selectedRecipe = new KeyValuePair<Recipe, ItemDrop.ItemData>(null, null);
			this.m_selectedVariant = 0;
			return;
		}
		KeyValuePair<Recipe, ItemDrop.ItemData> selectedRecipe = this.m_availableRecipes[index];
		if (selectedRecipe.Key != this.m_selectedRecipe.Key || selectedRecipe.Value != this.m_selectedRecipe.Value)
		{
			this.m_selectedRecipe = selectedRecipe;
			this.m_selectedVariant = 0;
		}
	}

	// Token: 0x06000557 RID: 1367 RVA: 0x0002D9EC File Offset: 0x0002BBEC
	private void UpdateRecipe(Player player, float dt)
	{
		CraftingStation currentCraftingStation = player.GetCurrentCraftingStation();
		if (currentCraftingStation)
		{
			this.m_craftingStationName.text = Localization.instance.Localize(currentCraftingStation.m_name);
			this.m_craftingStationIcon.gameObject.SetActive(true);
			this.m_craftingStationIcon.sprite = currentCraftingStation.m_icon;
			int level = currentCraftingStation.GetLevel();
			this.m_craftingStationLevel.text = level.ToString();
			this.m_craftingStationLevelRoot.gameObject.SetActive(true);
		}
		else
		{
			this.m_craftingStationName.text = Localization.instance.Localize("$hud_crafting");
			this.m_craftingStationIcon.gameObject.SetActive(false);
			this.m_craftingStationLevelRoot.gameObject.SetActive(false);
		}
		if (this.m_selectedRecipe.Key)
		{
			this.m_recipeIcon.enabled = true;
			this.m_recipeName.enabled = true;
			this.m_recipeDecription.enabled = true;
			ItemDrop.ItemData value = this.m_selectedRecipe.Value;
			int num = (value != null) ? (value.m_quality + 1) : 1;
			bool flag = num <= this.m_selectedRecipe.Key.m_item.m_itemData.m_shared.m_maxQuality;
			int num2 = (value != null) ? value.m_variant : this.m_selectedVariant;
			this.m_recipeIcon.sprite = this.m_selectedRecipe.Key.m_item.m_itemData.m_shared.m_icons[num2];
			string text = Localization.instance.Localize(this.m_selectedRecipe.Key.m_item.m_itemData.m_shared.m_name);
			if (this.m_selectedRecipe.Key.m_amount > 1)
			{
				text = text + " x" + this.m_selectedRecipe.Key.m_amount;
			}
			this.m_recipeName.text = text;
			this.m_recipeDecription.text = Localization.instance.Localize(ItemDrop.ItemData.GetTooltip(this.m_selectedRecipe.Key.m_item.m_itemData, num, true));
			if (value != null)
			{
				this.m_itemCraftType.gameObject.SetActive(true);
				if (value.m_quality >= value.m_shared.m_maxQuality)
				{
					this.m_itemCraftType.text = Localization.instance.Localize("$inventory_maxquality");
				}
				else
				{
					string text2 = Localization.instance.Localize(value.m_shared.m_name);
					this.m_itemCraftType.text = Localization.instance.Localize("$inventory_upgrade", new string[]
					{
						text2,
						(value.m_quality + 1).ToString()
					});
				}
			}
			else
			{
				this.m_itemCraftType.gameObject.SetActive(false);
			}
			this.m_variantButton.gameObject.SetActive(this.m_selectedRecipe.Key.m_item.m_itemData.m_shared.m_variants > 1 && this.m_selectedRecipe.Value == null);
			this.SetupRequirementList(num, player, flag);
			int requiredStationLevel = this.m_selectedRecipe.Key.GetRequiredStationLevel(num);
			CraftingStation requiredStation = this.m_selectedRecipe.Key.GetRequiredStation(num);
			if (requiredStation != null && flag)
			{
				this.m_minStationLevelIcon.gameObject.SetActive(true);
				this.m_minStationLevelText.text = requiredStationLevel.ToString();
				if (currentCraftingStation == null || currentCraftingStation.GetLevel() < requiredStationLevel)
				{
					this.m_minStationLevelText.color = ((Mathf.Sin(Time.time * 10f) > 0f) ? Color.red : this.m_minStationLevelBasecolor);
				}
				else
				{
					this.m_minStationLevelText.color = this.m_minStationLevelBasecolor;
				}
			}
			else
			{
				this.m_minStationLevelIcon.gameObject.SetActive(false);
			}
			bool flag2 = player.HaveRequirements(this.m_selectedRecipe.Key, false, num);
			bool flag3 = this.m_selectedRecipe.Value != null || player.GetInventory().HaveEmptySlot();
			bool flag4 = !requiredStation || (currentCraftingStation && currentCraftingStation.CheckUsable(player, false));
			this.m_craftButton.interactable = (((flag2 && flag4) || player.NoCostCheat()) && flag3 && flag);
			Text componentInChildren = this.m_craftButton.GetComponentInChildren<Text>();
			if (num > 1)
			{
				componentInChildren.text = Localization.instance.Localize("$inventory_upgradebutton");
			}
			else
			{
				componentInChildren.text = Localization.instance.Localize("$inventory_craftbutton");
			}
			UITooltip component = this.m_craftButton.GetComponent<UITooltip>();
			if (!flag3)
			{
				component.m_text = Localization.instance.Localize("$inventory_full");
			}
			else if (!flag2)
			{
				component.m_text = Localization.instance.Localize("$msg_missingrequirement");
			}
			else if (!flag4)
			{
				component.m_text = Localization.instance.Localize("$msg_missingstation");
			}
			else
			{
				component.m_text = "";
			}
		}
		else
		{
			this.m_recipeIcon.enabled = false;
			this.m_recipeName.enabled = false;
			this.m_recipeDecription.enabled = false;
			this.m_qualityPanel.gameObject.SetActive(false);
			this.m_minStationLevelIcon.gameObject.SetActive(false);
			this.m_craftButton.GetComponent<UITooltip>().m_text = "";
			this.m_variantButton.gameObject.SetActive(false);
			this.m_itemCraftType.gameObject.SetActive(false);
			for (int i = 0; i < this.m_recipeRequirementList.Length; i++)
			{
				InventoryGui.HideRequirement(this.m_recipeRequirementList[i].transform);
			}
			this.m_craftButton.interactable = false;
		}
		if (this.m_craftTimer < 0f)
		{
			this.m_craftProgressPanel.gameObject.SetActive(false);
			this.m_craftButton.gameObject.SetActive(true);
			return;
		}
		this.m_craftButton.gameObject.SetActive(false);
		this.m_craftProgressPanel.gameObject.SetActive(true);
		this.m_craftProgressBar.SetMaxValue(this.m_craftDuration);
		this.m_craftProgressBar.SetValue(this.m_craftTimer);
		this.m_craftTimer += dt;
		if (this.m_craftTimer >= this.m_craftDuration)
		{
			this.DoCrafting(player);
			this.m_craftTimer = -1f;
		}
	}

	// Token: 0x06000558 RID: 1368 RVA: 0x0002E03C File Offset: 0x0002C23C
	private void SetupRequirementList(int quality, Player player, bool allowedQuality)
	{
		int i = 0;
		if (allowedQuality)
		{
			foreach (Piece.Requirement req in this.m_selectedRecipe.Key.m_resources)
			{
				if (InventoryGui.SetupRequirement(this.m_recipeRequirementList[i].transform, req, player, true, quality))
				{
					i++;
				}
			}
		}
		while (i < this.m_recipeRequirementList.Length)
		{
			InventoryGui.HideRequirement(this.m_recipeRequirementList[i].transform);
			i++;
		}
	}

	// Token: 0x06000559 RID: 1369 RVA: 0x0002E0B0 File Offset: 0x0002C2B0
	private void SetupUpgradeItem(Recipe recipe, ItemDrop.ItemData item)
	{
		if (item == null)
		{
			this.m_upgradeItemIcon.sprite = recipe.m_item.m_itemData.m_shared.m_icons[this.m_selectedVariant];
			this.m_upgradeItemName.text = Localization.instance.Localize(recipe.m_item.m_itemData.m_shared.m_name);
			this.m_upgradeItemNextQuality.text = ((recipe.m_item.m_itemData.m_shared.m_maxQuality > 1) ? "1" : "");
			this.m_itemCraftType.text = Localization.instance.Localize("$inventory_new");
			this.m_upgradeItemDurability.gameObject.SetActive(recipe.m_item.m_itemData.m_shared.m_useDurability);
			if (recipe.m_item.m_itemData.m_shared.m_useDurability)
			{
				this.m_upgradeItemDurability.SetValue(1f);
				return;
			}
		}
		else
		{
			this.m_upgradeItemIcon.sprite = item.GetIcon();
			this.m_upgradeItemName.text = Localization.instance.Localize(item.m_shared.m_name);
			this.m_upgradeItemNextQuality.text = item.m_quality.ToString();
			this.m_upgradeItemDurability.gameObject.SetActive(item.m_shared.m_useDurability);
			if (item.m_shared.m_useDurability)
			{
				this.m_upgradeItemDurability.SetValue(item.GetDurabilityPercentage());
			}
			if (item.m_quality >= item.m_shared.m_maxQuality)
			{
				this.m_itemCraftType.text = Localization.instance.Localize("$inventory_maxquality");
				return;
			}
			this.m_itemCraftType.text = Localization.instance.Localize("$inventory_upgrade");
		}
	}

	// Token: 0x0600055A RID: 1370 RVA: 0x0002E278 File Offset: 0x0002C478
	public static bool SetupRequirement(Transform elementRoot, Piece.Requirement req, Player player, bool craft, int quality)
	{
		Image component = elementRoot.transform.Find("res_icon").GetComponent<Image>();
		Text component2 = elementRoot.transform.Find("res_name").GetComponent<Text>();
		Text component3 = elementRoot.transform.Find("res_amount").GetComponent<Text>();
		UITooltip component4 = elementRoot.GetComponent<UITooltip>();
		if (req.m_resItem != null)
		{
			component.gameObject.SetActive(true);
			component2.gameObject.SetActive(true);
			component3.gameObject.SetActive(true);
			component.sprite = req.m_resItem.m_itemData.GetIcon();
			component.color = Color.white;
			component4.m_text = Localization.instance.Localize(req.m_resItem.m_itemData.m_shared.m_name);
			component2.text = Localization.instance.Localize(req.m_resItem.m_itemData.m_shared.m_name);
			int num = player.GetInventory().CountItems(req.m_resItem.m_itemData.m_shared.m_name);
			int amount = req.GetAmount(quality);
			if (amount <= 0)
			{
				InventoryGui.HideRequirement(elementRoot);
				return false;
			}
			component3.text = amount.ToString();
			if (num < amount)
			{
				component3.color = ((Mathf.Sin(Time.time * 10f) > 0f) ? Color.red : Color.white);
			}
			else
			{
				component3.color = Color.white;
			}
		}
		return true;
	}

	// Token: 0x0600055B RID: 1371 RVA: 0x0002E3F4 File Offset: 0x0002C5F4
	public static void HideRequirement(Transform elementRoot)
	{
		Image component = elementRoot.transform.Find("res_icon").GetComponent<Image>();
		Text component2 = elementRoot.transform.Find("res_name").GetComponent<Text>();
		Component component3 = elementRoot.transform.Find("res_amount").GetComponent<Text>();
		elementRoot.GetComponent<UITooltip>().m_text = "";
		component.gameObject.SetActive(false);
		component2.gameObject.SetActive(false);
		component3.gameObject.SetActive(false);
	}

	// Token: 0x0600055C RID: 1372 RVA: 0x0002E478 File Offset: 0x0002C678
	private void DoCrafting(Player player)
	{
		if (this.m_craftRecipe == null)
		{
			return;
		}
		int num = (this.m_craftUpgradeItem != null) ? (this.m_craftUpgradeItem.m_quality + 1) : 1;
		if (num > this.m_craftRecipe.m_item.m_itemData.m_shared.m_maxQuality)
		{
			return;
		}
		if (!player.HaveRequirements(this.m_craftRecipe, false, num) && !player.NoCostCheat())
		{
			return;
		}
		if (this.m_craftUpgradeItem != null && !player.GetInventory().ContainsItem(this.m_craftUpgradeItem))
		{
			return;
		}
		if (this.m_craftUpgradeItem == null && !player.GetInventory().HaveEmptySlot())
		{
			return;
		}
		if (this.m_craftRecipe.m_item.m_itemData.m_shared.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(this.m_craftRecipe.m_item.m_itemData.m_shared.m_dlc))
		{
			player.Message(MessageHud.MessageType.Center, "$msg_dlcrequired", 0, null);
			return;
		}
		int variant = this.m_craftVariant;
		if (this.m_craftUpgradeItem != null)
		{
			variant = this.m_craftUpgradeItem.m_variant;
			player.UnequipItem(this.m_craftUpgradeItem, true);
			player.GetInventory().RemoveItem(this.m_craftUpgradeItem);
		}
		long playerID = player.GetPlayerID();
		string playerName = player.GetPlayerName();
		if (player.GetInventory().AddItem(this.m_craftRecipe.m_item.gameObject.name, this.m_craftRecipe.m_amount, num, variant, playerID, playerName) != null)
		{
			if (!player.NoCostCheat())
			{
				player.ConsumeResources(this.m_craftRecipe.m_resources, num);
			}
			this.UpdateCraftingPanel(false);
		}
		CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
		if (currentCraftingStation)
		{
			currentCraftingStation.m_craftItemDoneEffects.Create(player.transform.position, Quaternion.identity, null, 1f);
		}
		else
		{
			this.m_craftItemDoneEffects.Create(player.transform.position, Quaternion.identity, null, 1f);
		}
		Game.instance.GetPlayerProfile().m_playerStats.m_crafts++;
		Gogan.LogEvent("Game", "Crafted", this.m_craftRecipe.m_item.m_itemData.m_shared.m_name, (long)num);
	}

	// Token: 0x0600055D RID: 1373 RVA: 0x0002E6AC File Offset: 0x0002C8AC
	private int FindSelectedRecipe(GameObject button)
	{
		for (int i = 0; i < this.m_recipeList.Count; i++)
		{
			if (this.m_recipeList[i] == button)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x0600055E RID: 1374 RVA: 0x0002E6E6 File Offset: 0x0002C8E6
	private void OnCraftCancelPressed()
	{
		if (this.m_craftTimer >= 0f)
		{
			this.m_craftTimer = -1f;
		}
	}

	// Token: 0x0600055F RID: 1375 RVA: 0x0002E700 File Offset: 0x0002C900
	private void OnCraftPressed()
	{
		if (!this.m_selectedRecipe.Key)
		{
			return;
		}
		this.m_craftRecipe = this.m_selectedRecipe.Key;
		this.m_craftUpgradeItem = this.m_selectedRecipe.Value;
		this.m_craftVariant = this.m_selectedVariant;
		this.m_craftTimer = 0f;
		if (this.m_craftRecipe.m_craftingStation)
		{
			CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
			if (currentCraftingStation)
			{
				currentCraftingStation.m_craftItemEffects.Create(Player.m_localPlayer.transform.position, Quaternion.identity, null, 1f);
				return;
			}
		}
		else
		{
			this.m_craftItemEffects.Create(Player.m_localPlayer.transform.position, Quaternion.identity, null, 1f);
		}
	}

	// Token: 0x06000560 RID: 1376 RVA: 0x0002E7CB File Offset: 0x0002C9CB
	private void OnRepairPressed()
	{
		this.RepairOneItem();
		this.UpdateRepair();
	}

	// Token: 0x06000561 RID: 1377 RVA: 0x0002E7DC File Offset: 0x0002C9DC
	private void UpdateRepair()
	{
		if (Player.m_localPlayer.GetCurrentCraftingStation() == null && !Player.m_localPlayer.NoCostCheat())
		{
			this.m_repairPanel.gameObject.SetActive(false);
			this.m_repairPanelSelection.gameObject.SetActive(false);
			this.m_repairButton.gameObject.SetActive(false);
			return;
		}
		this.m_repairButton.gameObject.SetActive(true);
		this.m_repairPanel.gameObject.SetActive(true);
		this.m_repairPanelSelection.gameObject.SetActive(true);
		if (this.HaveRepairableItems())
		{
			this.m_repairButton.interactable = true;
			this.m_repairButtonGlow.gameObject.SetActive(true);
			Color color = this.m_repairButtonGlow.color;
			color.a = 0.5f + Mathf.Sin(Time.time * 5f) * 0.5f;
			this.m_repairButtonGlow.color = color;
			return;
		}
		this.m_repairButton.interactable = false;
		this.m_repairButtonGlow.gameObject.SetActive(false);
	}

	// Token: 0x06000562 RID: 1378 RVA: 0x0002E8EC File Offset: 0x0002CAEC
	private void RepairOneItem()
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
		if (currentCraftingStation == null && !Player.m_localPlayer.NoCostCheat())
		{
			return;
		}
		if (currentCraftingStation && !currentCraftingStation.CheckUsable(Player.m_localPlayer, false))
		{
			return;
		}
		this.m_tempWornItems.Clear();
		Player.m_localPlayer.GetInventory().GetWornItems(this.m_tempWornItems);
		foreach (ItemDrop.ItemData itemData in this.m_tempWornItems)
		{
			if (this.CanRepair(itemData))
			{
				itemData.m_durability = itemData.GetMaxDurability();
				if (currentCraftingStation)
				{
					currentCraftingStation.m_repairItemDoneEffects.Create(currentCraftingStation.transform.position, Quaternion.identity, null, 1f);
				}
				Player.m_localPlayer.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_repaired", new string[]
				{
					itemData.m_shared.m_name
				}), 0, null);
				return;
			}
		}
		Player.m_localPlayer.Message(MessageHud.MessageType.Center, "No more item to repair", 0, null);
	}

	// Token: 0x06000563 RID: 1379 RVA: 0x0002EA24 File Offset: 0x0002CC24
	private bool HaveRepairableItems()
	{
		if (Player.m_localPlayer == null)
		{
			return false;
		}
		CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
		if (currentCraftingStation == null && !Player.m_localPlayer.NoCostCheat())
		{
			return false;
		}
		if (currentCraftingStation && !currentCraftingStation.CheckUsable(Player.m_localPlayer, false))
		{
			return false;
		}
		this.m_tempWornItems.Clear();
		Player.m_localPlayer.GetInventory().GetWornItems(this.m_tempWornItems);
		foreach (ItemDrop.ItemData item in this.m_tempWornItems)
		{
			if (this.CanRepair(item))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000564 RID: 1380 RVA: 0x0002EAEC File Offset: 0x0002CCEC
	private bool CanRepair(ItemDrop.ItemData item)
	{
		if (Player.m_localPlayer == null)
		{
			return false;
		}
		if (!item.m_shared.m_canBeReparied)
		{
			return false;
		}
		if (Player.m_localPlayer.NoCostCheat())
		{
			return true;
		}
		CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
		if (currentCraftingStation == null)
		{
			return false;
		}
		Recipe recipe = ObjectDB.instance.GetRecipe(item);
		return !(recipe == null) && (!(recipe.m_craftingStation == null) || !(recipe.m_repairStation == null)) && ((recipe.m_repairStation != null && recipe.m_repairStation.m_name == currentCraftingStation.m_name) || (recipe.m_craftingStation != null && recipe.m_craftingStation.m_name == currentCraftingStation.m_name)) && currentCraftingStation.GetLevel() >= recipe.m_minStationLevel;
	}

	// Token: 0x06000565 RID: 1381 RVA: 0x0002EBD0 File Offset: 0x0002CDD0
	private void SetupDragItem(ItemDrop.ItemData item, Inventory inventory, int amount)
	{
		if (this.m_dragGo)
		{
			UnityEngine.Object.Destroy(this.m_dragGo);
			this.m_dragGo = null;
			this.m_dragItem = null;
			this.m_dragInventory = null;
			this.m_dragAmount = 0;
		}
		if (item != null)
		{
			this.m_dragGo = UnityEngine.Object.Instantiate<GameObject>(this.m_dragItemPrefab, base.transform);
			this.m_dragItem = item;
			this.m_dragInventory = inventory;
			this.m_dragAmount = amount;
			this.m_moveItemEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
			UITooltip.HideTooltip();
		}
	}

	// Token: 0x06000566 RID: 1382 RVA: 0x0002EC68 File Offset: 0x0002CE68
	private void ShowSplitDialog(ItemDrop.ItemData item, Inventory fromIventory)
	{
		this.m_splitSlider.minValue = 1f;
		this.m_splitSlider.maxValue = (float)item.m_stack;
		this.m_splitSlider.value = (float)Mathf.CeilToInt((float)item.m_stack / 2f);
		this.m_splitIcon.sprite = item.GetIcon();
		this.m_splitIconName.text = Localization.instance.Localize(item.m_shared.m_name);
		this.m_splitPanel.gameObject.SetActive(true);
		this.m_splitItem = item;
		this.m_splitInventory = fromIventory;
		this.OnSplitSliderChanged(this.m_splitSlider.value);
	}

	// Token: 0x06000567 RID: 1383 RVA: 0x0002ED16 File Offset: 0x0002CF16
	private void OnSplitSliderChanged(float value)
	{
		this.m_splitAmount.text = (int)value + "/" + (int)this.m_splitSlider.maxValue;
	}

	// Token: 0x06000568 RID: 1384 RVA: 0x0002ED45 File Offset: 0x0002CF45
	private void OnSplitCancel()
	{
		this.m_splitItem = null;
		this.m_splitInventory = null;
		this.m_splitPanel.gameObject.SetActive(false);
	}

	// Token: 0x06000569 RID: 1385 RVA: 0x0002ED66 File Offset: 0x0002CF66
	private void OnSplitOk()
	{
		this.SetupDragItem(this.m_splitItem, this.m_splitInventory, (int)this.m_splitSlider.value);
		this.m_splitItem = null;
		this.m_splitInventory = null;
		this.m_splitPanel.gameObject.SetActive(false);
	}

	// Token: 0x0600056A RID: 1386 RVA: 0x0002EDA5 File Offset: 0x0002CFA5
	public void OnOpenSkills()
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		this.m_skillsDialog.Setup(Player.m_localPlayer);
		Gogan.LogEvent("Screen", "Enter", "Skills", 0L);
	}

	// Token: 0x0600056B RID: 1387 RVA: 0x0002EDDA File Offset: 0x0002CFDA
	public void OnOpenTexts()
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		this.m_textsDialog.Setup(Player.m_localPlayer);
		Gogan.LogEvent("Screen", "Enter", "Texts", 0L);
	}

	// Token: 0x0600056C RID: 1388 RVA: 0x0002EE0F File Offset: 0x0002D00F
	public void OnOpenTrophies()
	{
		this.m_trophiesPanel.SetActive(true);
		this.UpdateTrophyList();
		Gogan.LogEvent("Screen", "Enter", "Trophies", 0L);
	}

	// Token: 0x0600056D RID: 1389 RVA: 0x0002EE39 File Offset: 0x0002D039
	public void OnCloseTrophies()
	{
		this.m_trophiesPanel.SetActive(false);
	}

	// Token: 0x0600056E RID: 1390 RVA: 0x0002EE48 File Offset: 0x0002D048
	private void UpdateTrophyList()
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		foreach (GameObject obj in this.m_trophyList)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_trophyList.Clear();
		List<string> trophies = Player.m_localPlayer.GetTrophies();
		float num = 0f;
		for (int i = 0; i < trophies.Count; i++)
		{
			string text = trophies[i];
			GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(text);
			if (itemPrefab == null)
			{
				ZLog.LogWarning("Missing trophy prefab:" + text);
			}
			else
			{
				ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_trophieElementPrefab, this.m_trophieListRoot);
				gameObject.SetActive(true);
				RectTransform rectTransform = gameObject.transform as RectTransform;
				rectTransform.anchoredPosition = new Vector2((float)component.m_itemData.m_shared.m_trophyPos.x * this.m_trophieListSpace, (float)component.m_itemData.m_shared.m_trophyPos.y * -this.m_trophieListSpace);
				num = Mathf.Min(num, rectTransform.anchoredPosition.y - this.m_trophieListSpace);
				string text2 = Localization.instance.Localize(component.m_itemData.m_shared.m_name);
				if (text2.EndsWith(" trophy"))
				{
					text2 = text2.Remove(text2.Length - 7);
				}
				rectTransform.Find("icon_bkg/icon").GetComponent<Image>().sprite = component.m_itemData.GetIcon();
				rectTransform.Find("name").GetComponent<Text>().text = text2;
				rectTransform.Find("description").GetComponent<Text>().text = Localization.instance.Localize(component.m_itemData.m_shared.m_name + "_lore");
				this.m_trophyList.Add(gameObject);
			}
		}
		ZLog.Log("SIZE " + num);
		float size = Mathf.Max(this.m_trophieListBaseSize, -num);
		this.m_trophieListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
		this.m_trophyListScroll.value = 1f;
	}

	// Token: 0x0600056F RID: 1391 RVA: 0x0002F0AC File Offset: 0x0002D2AC
	public void OnShowVariantSelection()
	{
		this.m_variantDialog.Setup(this.m_selectedRecipe.Key.m_item.m_itemData);
		Gogan.LogEvent("Screen", "Enter", "VariantSelection", 0L);
	}

	// Token: 0x06000570 RID: 1392 RVA: 0x0002F0E4 File Offset: 0x0002D2E4
	private void OnVariantSelected(int index)
	{
		ZLog.Log("Item variant selected " + index);
		this.m_selectedVariant = index;
	}

	// Token: 0x06000571 RID: 1393 RVA: 0x0002F102 File Offset: 0x0002D302
	public bool InUpradeTab()
	{
		return !this.m_tabUpgrade.interactable;
	}

	// Token: 0x06000572 RID: 1394 RVA: 0x0002F112 File Offset: 0x0002D312
	public bool InCraftTab()
	{
		return !this.m_tabCraft.interactable;
	}

	// Token: 0x06000573 RID: 1395 RVA: 0x0002F122 File Offset: 0x0002D322
	public void OnTabCraftPressed()
	{
		this.m_tabCraft.interactable = false;
		this.m_tabUpgrade.interactable = true;
		this.UpdateCraftingPanel(false);
	}

	// Token: 0x06000574 RID: 1396 RVA: 0x0002F143 File Offset: 0x0002D343
	public void OnTabUpgradePressed()
	{
		this.m_tabCraft.interactable = true;
		this.m_tabUpgrade.interactable = false;
		this.UpdateCraftingPanel(false);
	}

	// Token: 0x040005B2 RID: 1458
	private List<ItemDrop.ItemData> m_tempItemList = new List<ItemDrop.ItemData>();

	// Token: 0x040005B3 RID: 1459
	private List<ItemDrop.ItemData> m_tempWornItems = new List<ItemDrop.ItemData>();

	// Token: 0x040005B4 RID: 1460
	private static InventoryGui m_instance;

	// Token: 0x040005B5 RID: 1461
	[Header("Gamepad")]
	public UIGroupHandler m_inventoryGroup;

	// Token: 0x040005B6 RID: 1462
	public UIGroupHandler[] m_uiGroups = new UIGroupHandler[0];

	// Token: 0x040005B7 RID: 1463
	private int m_activeGroup = 1;

	// Token: 0x040005B8 RID: 1464
	[Header("Other")]
	public Transform m_inventoryRoot;

	// Token: 0x040005B9 RID: 1465
	public RectTransform m_player;

	// Token: 0x040005BA RID: 1466
	public RectTransform m_container;

	// Token: 0x040005BB RID: 1467
	public GameObject m_dragItemPrefab;

	// Token: 0x040005BC RID: 1468
	public Text m_containerName;

	// Token: 0x040005BD RID: 1469
	public Button m_dropButton;

	// Token: 0x040005BE RID: 1470
	public Button m_takeAllButton;

	// Token: 0x040005BF RID: 1471
	public float m_autoCloseDistance = 4f;

	// Token: 0x040005C0 RID: 1472
	[Header("Crafting dialog")]
	public Button m_tabCraft;

	// Token: 0x040005C1 RID: 1473
	public Button m_tabUpgrade;

	// Token: 0x040005C2 RID: 1474
	public GameObject m_recipeElementPrefab;

	// Token: 0x040005C3 RID: 1475
	public RectTransform m_recipeListRoot;

	// Token: 0x040005C4 RID: 1476
	public Scrollbar m_recipeListScroll;

	// Token: 0x040005C5 RID: 1477
	public float m_recipeListSpace = 30f;

	// Token: 0x040005C6 RID: 1478
	public float m_craftDuration = 2f;

	// Token: 0x040005C7 RID: 1479
	public Text m_craftingStationName;

	// Token: 0x040005C8 RID: 1480
	public Image m_craftingStationIcon;

	// Token: 0x040005C9 RID: 1481
	public RectTransform m_craftingStationLevelRoot;

	// Token: 0x040005CA RID: 1482
	public Text m_craftingStationLevel;

	// Token: 0x040005CB RID: 1483
	public Text m_recipeName;

	// Token: 0x040005CC RID: 1484
	public Text m_recipeDecription;

	// Token: 0x040005CD RID: 1485
	public Image m_recipeIcon;

	// Token: 0x040005CE RID: 1486
	public GameObject[] m_recipeRequirementList = new GameObject[0];

	// Token: 0x040005CF RID: 1487
	public Button m_variantButton;

	// Token: 0x040005D0 RID: 1488
	public Button m_craftButton;

	// Token: 0x040005D1 RID: 1489
	public Button m_craftCancelButton;

	// Token: 0x040005D2 RID: 1490
	public Transform m_craftProgressPanel;

	// Token: 0x040005D3 RID: 1491
	public GuiBar m_craftProgressBar;

	// Token: 0x040005D4 RID: 1492
	[Header("Repair")]
	public Button m_repairButton;

	// Token: 0x040005D5 RID: 1493
	public Transform m_repairPanel;

	// Token: 0x040005D6 RID: 1494
	public Image m_repairButtonGlow;

	// Token: 0x040005D7 RID: 1495
	public Transform m_repairPanelSelection;

	// Token: 0x040005D8 RID: 1496
	[Header("Upgrade")]
	public Image m_upgradeItemIcon;

	// Token: 0x040005D9 RID: 1497
	public GuiBar m_upgradeItemDurability;

	// Token: 0x040005DA RID: 1498
	public Text m_upgradeItemName;

	// Token: 0x040005DB RID: 1499
	public Text m_upgradeItemQuality;

	// Token: 0x040005DC RID: 1500
	public GameObject m_upgradeItemQualityArrow;

	// Token: 0x040005DD RID: 1501
	public Text m_upgradeItemNextQuality;

	// Token: 0x040005DE RID: 1502
	public Text m_upgradeItemIndex;

	// Token: 0x040005DF RID: 1503
	public Text m_itemCraftType;

	// Token: 0x040005E0 RID: 1504
	public RectTransform m_qualityPanel;

	// Token: 0x040005E1 RID: 1505
	public Button m_qualityLevelDown;

	// Token: 0x040005E2 RID: 1506
	public Button m_qualityLevelUp;

	// Token: 0x040005E3 RID: 1507
	public Text m_qualityLevel;

	// Token: 0x040005E4 RID: 1508
	public Image m_minStationLevelIcon;

	// Token: 0x040005E5 RID: 1509
	private Color m_minStationLevelBasecolor;

	// Token: 0x040005E6 RID: 1510
	public Text m_minStationLevelText;

	// Token: 0x040005E7 RID: 1511
	public ScrollRectEnsureVisible m_recipeEnsureVisible;

	// Token: 0x040005E8 RID: 1512
	[Header("Variants dialog")]
	public VariantDialog m_variantDialog;

	// Token: 0x040005E9 RID: 1513
	[Header("Skills dialog")]
	public SkillsDialog m_skillsDialog;

	// Token: 0x040005EA RID: 1514
	[Header("Texts dialog")]
	public TextsDialog m_textsDialog;

	// Token: 0x040005EB RID: 1515
	[Header("Split dialog")]
	public Transform m_splitPanel;

	// Token: 0x040005EC RID: 1516
	public Slider m_splitSlider;

	// Token: 0x040005ED RID: 1517
	public Text m_splitAmount;

	// Token: 0x040005EE RID: 1518
	public Button m_splitCancelButton;

	// Token: 0x040005EF RID: 1519
	public Button m_splitOkButton;

	// Token: 0x040005F0 RID: 1520
	public Image m_splitIcon;

	// Token: 0x040005F1 RID: 1521
	public Text m_splitIconName;

	// Token: 0x040005F2 RID: 1522
	[Header("Character stats")]
	public Transform m_infoPanel;

	// Token: 0x040005F3 RID: 1523
	public Text m_playerName;

	// Token: 0x040005F4 RID: 1524
	public Text m_armor;

	// Token: 0x040005F5 RID: 1525
	public Text m_weight;

	// Token: 0x040005F6 RID: 1526
	public Text m_containerWeight;

	// Token: 0x040005F7 RID: 1527
	public Toggle m_pvp;

	// Token: 0x040005F8 RID: 1528
	[Header("Trophies")]
	public GameObject m_trophiesPanel;

	// Token: 0x040005F9 RID: 1529
	public RectTransform m_trophieListRoot;

	// Token: 0x040005FA RID: 1530
	public float m_trophieListSpace = 30f;

	// Token: 0x040005FB RID: 1531
	public GameObject m_trophieElementPrefab;

	// Token: 0x040005FC RID: 1532
	public Scrollbar m_trophyListScroll;

	// Token: 0x040005FD RID: 1533
	[Header("Effects")]
	public EffectList m_moveItemEffects = new EffectList();

	// Token: 0x040005FE RID: 1534
	public EffectList m_craftItemEffects = new EffectList();

	// Token: 0x040005FF RID: 1535
	public EffectList m_craftItemDoneEffects = new EffectList();

	// Token: 0x04000600 RID: 1536
	public EffectList m_openInventoryEffects = new EffectList();

	// Token: 0x04000601 RID: 1537
	public EffectList m_closeInventoryEffects = new EffectList();

	// Token: 0x04000602 RID: 1538
	private InventoryGrid m_playerGrid;

	// Token: 0x04000603 RID: 1539
	private InventoryGrid m_containerGrid;

	// Token: 0x04000604 RID: 1540
	private Animator m_animator;

	// Token: 0x04000605 RID: 1541
	private Container m_currentContainer;

	// Token: 0x04000606 RID: 1542
	private bool m_firstContainerUpdate = true;

	// Token: 0x04000607 RID: 1543
	private KeyValuePair<Recipe, ItemDrop.ItemData> m_selectedRecipe;

	// Token: 0x04000608 RID: 1544
	private List<ItemDrop.ItemData> m_upgradeItems = new List<ItemDrop.ItemData>();

	// Token: 0x04000609 RID: 1545
	private int m_selectedVariant;

	// Token: 0x0400060A RID: 1546
	private Recipe m_craftRecipe;

	// Token: 0x0400060B RID: 1547
	private ItemDrop.ItemData m_craftUpgradeItem;

	// Token: 0x0400060C RID: 1548
	private int m_craftVariant;

	// Token: 0x0400060D RID: 1549
	private List<GameObject> m_recipeList = new List<GameObject>();

	// Token: 0x0400060E RID: 1550
	private List<KeyValuePair<Recipe, ItemDrop.ItemData>> m_availableRecipes = new List<KeyValuePair<Recipe, ItemDrop.ItemData>>();

	// Token: 0x0400060F RID: 1551
	private GameObject m_dragGo;

	// Token: 0x04000610 RID: 1552
	private ItemDrop.ItemData m_dragItem;

	// Token: 0x04000611 RID: 1553
	private Inventory m_dragInventory;

	// Token: 0x04000612 RID: 1554
	private int m_dragAmount = 1;

	// Token: 0x04000613 RID: 1555
	private ItemDrop.ItemData m_splitItem;

	// Token: 0x04000614 RID: 1556
	private Inventory m_splitInventory;

	// Token: 0x04000615 RID: 1557
	private float m_craftTimer = -1f;

	// Token: 0x04000616 RID: 1558
	private float m_recipeListBaseSize;

	// Token: 0x04000617 RID: 1559
	private int m_hiddenFrames = 9999;

	// Token: 0x04000618 RID: 1560
	private List<GameObject> m_trophyList = new List<GameObject>();

	// Token: 0x04000619 RID: 1561
	private float m_trophieListBaseSize;
}
