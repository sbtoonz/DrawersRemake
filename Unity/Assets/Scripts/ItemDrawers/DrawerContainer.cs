using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DrawerContainer : MonoBehaviour
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
}