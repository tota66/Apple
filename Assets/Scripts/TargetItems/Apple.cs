﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Apple : TargetItem {

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
		return 1;
	}

	public override int GetScore() {
		return 1;
	}
}
