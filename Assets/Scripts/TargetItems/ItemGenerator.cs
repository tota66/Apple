using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerator : MonoBehaviour {
	public List<GameObject> itemPrefab;

	enum ItemId {
		APPLE
	}
	private float interval = 0.5f;
	private float time = 0.0f;

	private float minX = -7.0f;
	private float maxX = 7.0f;
	private float y = 7.0f;

	// Use this for initialization
	void Start () {
		minX = GameManager.Instance.GetMinX();
		maxX = GameManager.Instance.GetMaxX();
		y = GameManager.Instance.GetMaxY();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void UpdateGenerator() {
		time += Time.deltaTime;
		if (time >= interval) {
			generate();
			time = 0.0f;
		}
	}

	private ItemId chooseItemId() {
		return ItemId.APPLE;
	}

	private void generate() {
		var x = Random.Range(minX, maxX);
		var itemId = chooseItemId();

		GameObject item = null;
		switch (itemId) {
		case ItemId.APPLE:
			item = Instantiate(itemPrefab[(int)itemId], new Vector3(x, y, 0.0f), Quaternion.identity) as GameObject;
			break;
		default:
			break;
		}

		if (item != null) {
			GameManager.Instance.AddTargetItem(item.GetComponent<TargetItem>());
		}
	}
}
