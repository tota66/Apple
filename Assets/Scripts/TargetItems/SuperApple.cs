using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperApple : TargetItem {

	// Use this for initialization
	void Awake () {
		Init();
	}
	
	// Update is called once per frame
	void Update () {
		move();
		remove();
	}

	protected void Init() {
		base.Init();
	}

	protected override void move() {
		base.move();
	}

	public override int GetItemId() {
		return 2;
	}

	public override int GetScore() {
		return 5;
	}
}
