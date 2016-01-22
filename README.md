# BTtree
BehaviorTree for unity

Based on Yifeng Wu's BehaviorTree Framework,improve the visulization tool.

Usage:

Creat a new btTree in Assets window

Find the editor window in Window-behaviorTreeEditor

Drag the tree asset into editor window

Add this Script to the player gameobject

using UnityEngine;

using System.Collections;

using BT;

public class AIManager : MonoBehaviour {

	public BTTree bt;
	
	public BTDatabase database = new BTDatabase();
	
	// Use this for initialization
	
	void Start () {
	
		database.characterManager = GetComponent<CharacterManager>();
		
		bt = BTTree.CloneTree (bt);
		
		bt.Initialize (database);
		
	}
	
	// Update is called once per frame
	
	void Update () {
	
		bt.Update ();
		
	}

}
