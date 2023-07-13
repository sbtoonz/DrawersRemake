// Decompiled with JetBrains decompiler
// Type: DrawerContainer
// Assembly: ItemDrawers, Version=0.0.3.0, Culture=neutral, PublicKeyToken=null
// MVID: 2CF7DD10-2A21-4A9E-B555-B0ACD6A3467F
// Assembly location: C:\Users\zarboz\Desktop\SavingMods\OdinPlus-ItemDrawers_Remake-0.0.6\plugins\ItemDrawers.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ItemDrawers_Remake;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DrawerContainer : Container, Hoverable, Interactable
{
  public Image _image;
  public TextMeshProUGUI _text;
  private Piece _piece;
  private ItemDrop.ItemData _item;
  public int _quantity;
  public bool _loading;
  public int _pickupDelay = 10;
  public LayerMask _pickupMask;
  public int RetrieveRadius = 10;
  public bool RetreiveEnabled;
  public int MaxItems = 9999;

  public void Awake()
  {
    m_nview = gameObject.GetComponent<ZNetView>();
    _piece = GetComponent<Piece>();
    _image.gameObject.SetActive(false);
    if ((UnityEngine.Object) m_nview != (UnityEngine.Object) null && m_nview.GetZDO() != null)
    {
      name = string.Format("Container_{0}", m_nview.GetZDO().m_uid);
      m_name = "piece_chest_drawer";
      m_nview = m_nview;
      m_inventory = new Inventory(name, (Sprite) null, 1, 1);
      m_inventory.m_onChanged = m_inventory.m_onChanged + new Action(OnInventoryChanged);
      m_nview.Register<int>("DrawerDrop", new Action<long, int>(RPC_Drop));
      m_nview.Register<ZDOID>("DrawerDropResponse", new Action<long, ZDOID>(RPC_DropResponse));
      m_nview.Register("DrawerClear", new Action<long>(RPC_Clear));
      m_nview.Register<string, int>("DrawerAddItem", new Action<long, string, int>(RPC_AddItem));
      m_nview.Register<string, int>("DrawerAddItemResponse", new Action<long, string, int>(RPC_AddItemResponse));
    }
    WearNTear component = GetComponent<WearNTear>();
    if (component != null)
      component.m_onDestroyed += new Action(((Container) this).OnDestroyed);
    InvokeRepeating("CheckForChanges", 0.0f, 1f);
  }

  private void RetreiveItems()
  {
    if (_quantity >= MaxItems || !RetreiveEnabled || _item == null || !(bool) (UnityEngine.Object) m_nview || !m_nview.IsOwner())
      return;
    if (_pickupDelay > 0)
    {
      --_pickupDelay;
    }
    else
    {
      foreach (Collider collider in Physics.OverlapSphere(transform.position + Vector3.up, (float) RetrieveRadius, (int) _pickupMask))
      {
        if ((bool) (UnityEngine.Object) collider.attachedRigidbody)
        {
          ItemDrop component1 = collider.attachedRigidbody.GetComponent<ItemDrop>();
          if (!((UnityEngine.Object) component1 == (UnityEngine.Object) null) && !((UnityEngine.Object) component1.m_itemData.m_dropPrefab != (UnityEngine.Object) _item.m_dropPrefab))
          {
            ZNetView component2 = component1.GetComponent<ZNetView>();
            if ((bool) (UnityEngine.Object) component2 && component2.IsValid())
            {
              if (!component1.CanPickup())
              {
                component1.RequestOwn();
              }
              else
              {
                AddItem(component1.m_itemData.m_dropPrefab.name, component1.m_itemData.m_stack);
                ZNetScene.instance.Destroy(component1.gameObject);
              }
            }
          }
        }
      }
    }
  }

  private void CheckForChanges()
  {
    if (!m_nview.IsValid())
      return;
    Load();
    RetreiveItems();
    UpdateVisuals();
  }

  public new string GetHoverName() => !(bool) (UnityEngine.Object) _piece ? string.Empty : Localization.instance.Localize(_piece.m_name);

  public new string GetHoverText()
  {
    string text = _item != null ? string.Format("<color=aqua>{0}</color> x {1}", (object) _item.m_shared.m_name, (object) _quantity) : "$piece_container_empty";
    if (!ZInput.IsGamepadActive())
    {
      KeyCode keyCode = ItemDrawersMod._configKeyDepositAll.Value;
      string str1 = keyCode.ToString();
      string str2;
      if (_quantity > 0)
      {
        string str3 = text + "\n[<color=yellow><b>$KEY_Use</b></color>] Take Stack";
        keyCode = ItemDrawersMod._configKeyWithdrawOne.Value;
        string str4 = keyCode.ToString();
        str2 = str3 + "\n[<color=yellow><b>" + str4 + "+$KEY_Use</b></color>] Take One";
      }
      else
      {
        keyCode = ItemDrawersMod._configKeyClear.Value;
        string str5 = keyCode.ToString();
        str2 = text + "\n[<color=yellow><b>" + str5 + "+$KEY_Use</b></color>] Clear Item";
      }
      text = str2 + "\n[<color=yellow><b>" + str1 + "+$KEY_Use</b></color>] Deposit All";
    }
    return Localization.instance.Localize(text);
  }

  public new bool Interact(Humanoid user, bool hold, bool alt)
  {
    if (_item == null || (UnityEngine.Object) user != (UnityEngine.Object) Player.m_localPlayer || !PrivateArea.CheckAccess(transform.position))
      return false;
    return !ZInput.IsGamepadActive() ? OnKeyboardInteract(Player.m_localPlayer) : OnGamepadInteract(Player.m_localPlayer);
  }

  private bool OnGamepadInteract(Player player)
  {
    bool blocking = player.m_blocking;
    bool crouchToggled = player.m_crouchToggled;
    return ProcessInputInternal(player, blocking, crouchToggled);
  }

  private bool OnKeyboardInteract(Player player)
  {
    bool key1 = Input.GetKey(ItemDrawersMod._configKeyDepositAll.Value);
    bool key2 = Input.GetKey(_quantity > 0 ? ItemDrawersMod._configKeyWithdrawOne.Value : ItemDrawersMod._configKeyClear.Value);
    return ProcessInputInternal(player, key1, key2);
  }

  private bool ProcessInputInternal(Player player, bool mod0, bool mod1)
  {
    if (mod0)
      return UseItem((Humanoid) player, _item);
    if (mod1 && _quantity <= 0)
      m_nview.InvokeRPC("DrawerClear");
    else if (_quantity > 0)
      m_nview.InvokeRPC("DrawerDrop", (object) (mod1 ? 1 : _item.m_shared.m_maxStackSize));
    return true;
  }

  public new bool UseItem(Humanoid user, ItemDrop.ItemData item)
  {
    if (item.m_shared.m_maxStackSize <= 1 || _item != null && !((UnityEngine.Object) _item.m_dropPrefab == (UnityEngine.Object) item.m_dropPrefab))
      return false;
    int num = user.GetInventory().CountItems(item.m_shared.m_name);
    if (num <= 0)
      return false;
    m_nview.InvokeRPC("DrawerAddItem", (object) item.m_dropPrefab.name, (object) num);
    return true;
  }

  private void UpdateInventory()
  {
    if (m_inventory.m_inventory.Count > 0)
    {
      ItemDrop.ItemData itemData = m_inventory.m_inventory[0];
      if (itemData.m_shared.m_name == _item.m_shared.m_name)
      {
        itemData.m_stack = _quantity;
        itemData.m_shared.m_maxStackSize = MaxItems;
      }
      else
        _addItem();
    }
    else
      _addItem();

    void _addItem()
    {
      m_inventory.m_inventory.Clear();
      ItemDrop.ItemData itemData = _item.Clone();
      itemData.m_stack = _quantity;
      itemData.m_shared = CommonUtils.Clone<ItemDrop.ItemData.SharedData>(itemData.m_shared);
      itemData.m_shared.m_maxStackSize = MaxItems;
      m_inventory.m_inventory.Add(itemData);
    }
  }

  private ItemDrop Drop(int quantity)
  {
    if (_quantity <= 0)
      return (ItemDrop) null;
    GameObject dropPrefab = _item.m_dropPrefab;
    int stack = Mathf.Min(quantity, _quantity);
    _quantity -= stack;
    _pickupDelay = 5;
    UpdateInventory();
    Vector3 position = transform.position + transform.forward * -0.5f;
    GameObject gameObject = Instantiate<GameObject>(dropPrefab, position, Quaternion.identity);
    gameObject.GetComponent<Rigidbody>().velocity = Vector3.up * 4f + transform.forward * 4f;
    ItemDrop component = gameObject.GetComponent<ItemDrop>();
    component.SetStack(stack);
    return component;
  }

  public int AddItem(string name, int stack)
  {
    GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(name);
    if ((UnityEngine.Object) itemPrefab == (UnityEngine.Object) null)
    {
      ZLog.Log((object) ("Failed to find item prefab " + name));
      return 0;
    }
    ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
    {
      ZLog.Log((object) ("Invalid item " + name));
      return 0;
    }
    if (_item != null && (UnityEngine.Object) _item.m_dropPrefab != (UnityEngine.Object) itemPrefab)
    {
      ZLog.Log((object) ("Cannot add to container " + name));
      return 0;
    }
    stack = Math.Min(stack, MaxItems - _quantity);
    _item = component.m_itemData;
    _item.m_dropPrefab = itemPrefab;
    _item.m_gridPos = Vector2i.zero;
    _quantity += stack;
    UpdateInventory();
    OnContainerChanged();
    return stack;
  }

  private void OnContainerChanged()
  {
    if (_loading || !m_nview.IsOwner())
      return;
    Save();
  }

  private void OnInventoryChanged()
  {
    _quantity = m_inventory.m_inventory.Count <= 0 ? 0 : Math.Max(0, _quantity - (_quantity - m_inventory.m_inventory[0].m_stack));
    OnContainerChanged();
    UpdateVisuals();
  }

  private void RPC_AddItem(long playerId, string prefabName, int quantity)
  {
    if (!m_nview.IsOwner())
      return;
    int num = AddItem(prefabName, quantity);
    if (num > 0)
      m_nview.InvokeRPC(playerId, "DrawerAddItemResponse", (object) prefabName, (object) num);
  }

  private void RPC_AddItemResponse(long _, string prefabName, int quantity)
  {
    if (!(bool) (UnityEngine.Object) Player.m_localPlayer || quantity <= 0)
      return;
    ItemDrop component = ObjectDB.instance.GetItemPrefab(prefabName).GetComponent<ItemDrop>();
    if (component != null)
    {
      string name = component.m_itemData.m_shared.m_name;
      Player.m_localPlayer.GetInventory().RemoveItem(name, quantity);
    }
  }

  private void RPC_Clear(long _)
  {
    if (!m_nview.IsOwner() || _item == null)
      return;
    Clear();
    OnContainerChanged();
  }

  private void RPC_Drop(long playerId, int quantity)
  {
    if (!m_nview.IsOwner() || _item == null)
      return;
    ItemDrop itemDrop = Drop(quantity);
    if (itemDrop != null)
    {
      OnContainerChanged();
      ZDOID uid = itemDrop.m_nview.GetZDO().m_uid;
      m_nview.InvokeRPC(playerId, "DrawerDropResponse", (object) uid);
    }
  }

  private void RPC_DropResponse(long _, ZDOID drop)
  {
    GameObject instance = ZNetScene.instance.FindInstance(drop);
    if (!((UnityEngine.Object) instance != (UnityEngine.Object) null))
      return;
    ItemDrop component = instance.GetComponent<ItemDrop>();
    if (!((UnityEngine.Object) component == (UnityEngine.Object) null) && Player.m_localPlayer.GetInventory().AddItem(component.m_itemData))
    {
      Player.m_localPlayer.ShowPickupMessage(component.m_itemData, component.m_itemData.m_stack);
      ZNetScene.instance.Destroy(instance);
    }
  }

  private void UpdateVisuals()
  {
    if ((UnityEngine.Object) _image != (UnityEngine.Object) null)
    {
      bool flag = _item != null && ((IEnumerable<Sprite>) _item.m_shared.m_icons).Count<Sprite>() > 0;
      _image.gameObject.SetActive(flag);
      if (flag)
        _image.sprite = _item.m_shared.m_icons[0];
    }
    if (!((UnityEngine.Object) _text != (UnityEngine.Object) null))
      return;
    _text.SetText(_quantity > 0 ? _quantity.ToString() : string.Empty);
  }

  private void Clear()
  {
    _quantity = 0;
    _item = (ItemDrop.ItemData) null;
    m_inventory.m_inventory.Clear();
    UpdateVisuals();
  }

  private void Save()
  {
    ZPackage zpackage = new ZPackage();
    zpackage.Write(0);
    if (_item != null)
    {
      zpackage.Write((UnityEngine.Object) _item.m_dropPrefab == (UnityEngine.Object) null ? "" : _item.m_dropPrefab.name);
      zpackage.Write(_quantity);
    }
    else
      zpackage.Write("");
    string base64 = zpackage.GetBase64();
    m_nview.GetZDO().Set("items", base64);
    m_lastRevision = m_nview.GetZDO().DataRevision;
  }

  private void Load()
  {
    if ((int) m_nview.GetZDO().DataRevision == (int) m_lastRevision)
      return;
    Clear();
    string base64String = m_nview.GetZDO().GetString("items");
    if (string.IsNullOrEmpty(base64String))
      return;
    ZPackage zpackage = new ZPackage(base64String);
    zpackage.ReadInt();
    string name = zpackage.ReadString();
    if (!string.IsNullOrEmpty(name))
    {
      int stack = zpackage.ReadInt();
      _loading = true;
      AddItem(name, stack);
      _loading = false;
      m_lastRevision = m_nview.GetZDO().DataRevision;
    }
  }

  public void OnDestroyed()
  {
    if (!m_nview.IsOwner() || _item == null)
      return;
    do
      ;
    while (_quantity > 0 && (bool) (UnityEngine.Object) Drop(_item.m_shared.m_maxStackSize));
  }
}
