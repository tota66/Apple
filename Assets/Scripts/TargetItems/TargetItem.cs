using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetItem : MonoBehaviour {
	private float speed = 1.0f;

	private float minX;
	private float minY;
	private int stageWidth;

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update () {
	}

	protected void Init() {
		minX = GameManager.Instance.GetMinX();
		minY = GameManager.Instance.GetMinY();
		stageWidth = GameManager.Instance.GetStageWidth();
	}

	protected virtual void move() {
		transform.position += Vector3.down * Time.deltaTime * speed;
	}

	protected void remove() {
		if (transform.position.y <= minY) {
			GameManager.Instance.RemoveTargetItem(this);
			Destroy(gameObject);
		}
	}

	public int GetPositionId() {
		int x = Mathf.RoundToInt(transform.position.x);
		int y = Mathf.RoundToInt(transform.position.y);
		int ix = x - Mathf.RoundToInt(minX);
		int iy = y - Mathf.RoundToInt(minY);
		//Debug.Log(ix + "," + iy);
		return iy * stageWidth + ix;
	}

	public virtual int GetItemId() {
		return 0;
	}
}
