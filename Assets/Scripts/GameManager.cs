using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
	private static GameManager instance;

	public Player player;
	public ItemGenerator itemGenerator;

	private bool isGameStarted = false;
	private bool isGenerateEnd = false;
	private bool isGameOver = false;
	private int score = 0;
	private int prevScore = 0;
	private float time = 0.0f;
	private float limitTime = 10.0f;

	private int[] obs;
	private float minX = -7.0f;
	private float maxX = 7.0f;
	private float minY = -5.0f;
	private float maxY = 7.0f;
	private int stageWidth = 0;
	private int stageHeight = 0;
	private List<TargetItem> targetItemList;


	public static GameManager Instance {
		get {
			if (instance == null) {
				GameObject go = new GameObject("GameManager");
				instance = go.AddComponent<GameManager>();

				instance.Init();
			}
			return instance;
		}
	}

	void Init() {
		stageWidth = (int)(maxX - minX) + 1;
		stageHeight = (int)(maxY - minY) + 1;
		Debug.Log("stagewidth: " + stageWidth + " stageHeight: " + stageHeight);

		obs = new int[stageWidth * stageHeight + 1];
		targetItemList = new List<TargetItem>();

		Debug.Log("Game Manager Initialized.");
	}

	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		itemGenerator = GameObject.FindGameObjectWithTag("ItemGenerator").GetComponent<ItemGenerator>();
	}

	public void PlayerAction(int actionId) {
		switch (actionId) {
		case 0:
			player.GoLeft();
			break;
		case 1:
			player.GoRight();
			break;
		default:
			break;
		}
	}

	public void AddTargetItem(TargetItem item) {
		targetItemList.Add(item);
	}

	public void RemoveTargetItem(TargetItem item) {
		targetItemList.Remove(item);
	}

	public void AddScore(int gain) {
		score += gain;
	}

	public int GetScore() {
		return score;
	}

	private void resetObs() {
		int size = stageWidth * stageHeight + 1;
		for (int i = 0; i < size; ++i) {
			obs[i] = 0;
		}
	}

	private void updateObs() {
		if (targetItemList == null) {
			return;
		}

		resetObs();

		// target item position
		foreach (var item in targetItemList) {
			var posId = item.GetPositionId();
			obs[posId] = Mathf.Max(obs[posId], item.GetItemId());
			//Debug.Log(item.GetItemId());
		}

		// player position
		int size = stageWidth * stageHeight;
		obs[size] = player.GetPositionId();
	}

	private int[] getObs() {
		return obs;
	}

	public float GetMinX() {
		return minX;
	}
	public float GetMaxX() {
		return maxX;
	}
	public float GetMinY() {
		return minY;
	}
	public float GetMaxY() {
		return maxY;
	}
	public int GetStageWidth() {
		return stageWidth;
	}

	public void GameStart() {
		isGameStarted = true;
	}

	public void GameReset() {
		isGameStarted = false;
		isGenerateEnd = false;
		isGameOver = false;
		time = 0;
		score = 0;
		prevScore = 0;
		targetItemList.Clear();
	}

	public string GetResponseMessage(bool isOnlyObs) {
		var obs = getObs();
		var score = GetScore();
		//var reward = score - prevScore;
		var reward = score;
		int isDone = isGameOver ? 1 : 0;

		string str = "";
		int i = 0;
		foreach (var v in obs) {
			str += v.ToString();
			if (i != obs.Length - 1) {
				str += ",";
			}
			++i;
		}

		if (!isOnlyObs) {
			str += "o";

			str += reward.ToString();
			str += "r";

			str += isDone.ToString();
			str += "d";
		}

		prevScore = score;

		return str;
	}

	// Update is called once per frame
	void Update () {
		if (!isGameStarted) {
			if (Input.GetKey(KeyCode.S)) {
				GameStart();
			}
			player.Reset();
			return;
		}

		// update game states
		if (!isGenerateEnd) {
			itemGenerator.UpdateGenerator();
		}
		updateObs();

		time += Time.deltaTime;

		// game over
		if (time >= limitTime) {
			isGenerateEnd = true;
			// If there are no apples, game is over.
			if (targetItemList.Count <= 0) {
				isGameStarted = false;
				isGameOver = true;
				time = 0;
			}
		}
	}
}
