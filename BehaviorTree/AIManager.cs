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
