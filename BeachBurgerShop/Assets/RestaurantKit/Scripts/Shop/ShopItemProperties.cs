using UnityEngine;
using System.Collections;
using TMPro;

public class ShopItemProperties : MonoBehaviour {
	
	///*************************************************************************///
	/// A very simple value holder for different shop items.
	// You can easily add/edit item properties via this controller.
	///*************************************************************************///

	public int itemIndex;
	public int itemPrice;
	public GameObject priceTag;

	void Awake (){
		priceTag.GetComponent<TextMeshPro>().text = "$" + itemPrice;
	}
}