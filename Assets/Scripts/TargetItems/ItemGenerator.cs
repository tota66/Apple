using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerator : MonoBehaviour {
	public List<GameObject> itemApplePrefab;
	public List<GameObject> itemSuperApplePrefab;
	public Glow2Camera _camera;

	enum ItemId {
		APPLE,
		SUPER_APPLE,
	}
	private float interval = 0.5f;
	private float time = 0.0f;
	private int stageLevel = 0;
	private int count = 0;

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

	public void start(int level) {
		stageLevel = level;
	}

	public void reset() {
		count = 0;
		_camera.resetGlowObjects();
	}

	public void UpdateGenerator() {
		time += Time.deltaTime;
		if (time >= interval) {
			generate();
			time = 0.0f;
			count += 1;
		}
	}

	private ItemId chooseItemId() {
		switch (stageLevel) {
		case 1:
			return ItemId.APPLE;
			break;
		case 2:
			if (count % 6 == 5) {
				return ItemId.SUPER_APPLE;
			} else {
				return ItemId.APPLE;
			}
			break;
		default:
			return ItemId.APPLE;
			break;
		}
	}

	private void generate() {
		var x = Random.Range(minX, maxX);
		var itemId = chooseItemId();

		GameObject item = null;
		int idx = 0;
		switch (itemId) {
		case ItemId.APPLE:
			idx = Random.Range(0, itemApplePrefab.Count);
			item = Instantiate(itemApplePrefab[idx], new Vector3(x, y, 0.0f), Quaternion.identity) as GameObject;
			break;
		case ItemId.SUPER_APPLE:
			idx = Random.Range(0, itemSuperApplePrefab.Count);
			item = Instantiate(itemSuperApplePrefab[idx], new Vector3(x, y, 0.0f), Quaternion.identity) as GameObject;
			GameObject aura = item.transform.Find("Aura").gameObject;
			_camera.AddGrowObject(aura);
			break;
		default:
			break;
		}

		if (item != null) {
			GameManager.Instance.AddTargetItem(item.GetComponent<TargetItem>());
		}
	}
}
