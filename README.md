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


CharacterManager is a class that contains player information，such as HP，MP and component
this is My define

using UnityEngine;

using System.Collections;

[RequireComponent(typeof(InputManager),typeof(CharacterAttributes),typeof(AIManager))]

public class CharacterManager : MonoBehaviour {

	public enum CharacterType
	
	{
	
		Player,
		
		Allies,
		
		Enemy,
		
		Boss,
		
		Other
	
	}
	
	public CharacterType characterType;
	
	public CharacterBase characterBase;
	
	[HideInInspector]
	
	public CharacterAttributes characterAttributes;
	
	public LocalEventManager localEventManager;
	
	private PhysicsManager physicsManager;
	
	private InputManager inputManager;
	
	private AIManager aIManager;
	
	private DamageManager damageManager;
	
	[HideInInspector]
	
	public Transform downCillider;
	
	public int staticSceneNum = -1;
	
	// Use this for initialization
	
	void Awake()
	
	{
	
		localEventManager = new LocalEventManager();
	
		physicsManager = transform.Find("DownCollider").GetComponent<PhysicsManager>();
	
		characterAttributes = GetComponent<CharacterAttributes>();
	
		inputManager = GetComponent<InputManager>();
	
		aIManager = GetComponent<AIManager>();
	
		damageManager = GetComponent<DamageManager>();
	
		inputManager.localEventManager = localEventManager;
	
		downCillider = transform.Find("DownCollider");
	
		characterBase = new CharacterBase (gameObject);
	
		if (staticSceneNum >= 0)
	
			characterBase.selfSceneNum = staticSceneNum;
	
		if (characterType == CharacterType.Player) {
	
			characterBase.isPlayer = true;
	
			GameManager.Instance.Player = gameObject;
	
			DontDestroyOnLoad (this.gameObject);
	
		}
	
	}
	
	void Start()
	
	{
	
		characterBase.RegistEvent();
	
	}
	
	void Update()
	
	{
	
		characterBase.Update();
	
	}
	
	public void Reset()
	
	{
	
		characterBase.Reset ();
	
		characterAttributes.Reset ();
	
		damageManager.Reset ();
	
		switch (characterType) {
	
		case CharacterType.Enemy:
	
			GetComponent<AIManager> ().enabled = true;
	
			GetComponent<InputManager> ().enabled = false;
	
			characterBase.camp = 1;
	
			break;
	
		case CharacterType.Allies:
	
			GetComponent<AIManager> ().enabled = true;
	
			GetComponent<InputManager> ().enabled = false;
	
			GetComponent<CharacterManager> ().characterBase.camp = 0;
	
			break;
	
		case CharacterType.Player:
	
			aIManager.enabled = false;
	
			GetComponent<InputManager> ().enabled = true;
	
			GetComponent<CharacterManager> ().characterBase.camp = 0;
	
			break;
	
		}
	
	}
}


