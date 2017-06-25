using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	private float speed = 3.0f;
	private int state = 0;
	private float minX;
	private float maxX;
	private float minY;
	private int stageWidth;
	private Vector3 initPosition;

	// Use this for initialization
	void Start () {
		minX = GameManager.Instance.GetMinX();
		maxX = GameManager.Instance.GetMaxX();
		minY = GameManager.Instance.GetMinY();
		stageWidth = GameManager.Instance.GetStageWidth();
		initPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.Z)) {
			GoLeft();
		}
		if (Input.GetKey(KeyCode.X)) {
			GoRight();
		}

		if (state == 1) {
			if (transform.position.x < maxX) {
				transform.position += Vector3.right * Time.deltaTime * speed;
			}
		} else if (state == -1) {
			if (minX < transform.position.x) {
				transform.position += Vector3.left * Time.deltaTime * speed;
			}
		}
	}

	public void GoRight() {
		state = 1;
	}

	public void GoLeft() {
		state = -1;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag != "TargetItem") {
			return;
		}
		GameManager.Instance.AddScore(1);

		TargetItem item = other.GetComponent<TargetItem>();
		GameManager.Instance.RemoveTargetItem(item);
		Destroy(other.gameObject);
	}

	public int GetPositionId() {
		int x = Mathf.RoundToInt(transform.position.x);
		int y = Mathf.RoundToInt(transform.position.y);
		int ix = x - Mathf.RoundToInt(minX);
		int iy = y - Mathf.RoundToInt(minY);
		//Debug.Log(ix + "," + iy);
		return iy * stageWidth + ix;
	}

	public void Reset() {
		transform.position = initPosition;
	}
}
