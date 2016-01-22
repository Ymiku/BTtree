using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
namespace BT {

	/// <summary>
	/// BTTree is where the behavior tree should be constructed.
	/// </summary>
	public class BTTree:ScriptableObject {
		public string note;
		public BTNode _root;
		public BTNode root {get {return _root;}
			set{_root = value;}
		}
		protected BTDatabase _database = new BTDatabase();

		public List<EditorData> _editorData = new List<EditorData>();
		public Vector2 _editorOffset = Vector2.zero;


		public BTTree(CharacterManager characterManager)
		{
			_database.characterManager = characterManager;
		}
		public void Initialize () {
			_root = Construct();
			if (_root.name == null) {
				_root.name = "Root";
			}
			_root.Activate(_database);
		}
		public static BTTree CloneTree( BTTree behaviorTree )
		{
			BTTree treeClone = Instantiate<BTTree>( behaviorTree );
			treeClone.root = CloneNode( behaviorTree.root );
			return treeClone;
		}
		static BTNode CloneNode( BTNode node )
		{
			BTNode nodeClone = Instantiate<BTNode>( node );
			
			if ( node is BTDecorator )
			{
				( (BTDecorator)nodeClone ).child = CloneNode( ( (BTDecorator)node ).child );
			}
			else if ( node is BTComposite )
			{
				List<BTNode> cloneChildren = new List<BTNode>();
				foreach ( BTNode child in( (BTComposite)node ).children )
				{
					cloneChildren.Add( (BTNode)CloneNode( child ) );
				}
				
				( (BTComposite)nodeClone ).children = cloneChildren;
			}
			
			return nodeClone;
		}
		public void SetData<T> (string dataName, T data) {
			_database.SetData<T> (dataName,data);
		}

		public void Update () {
			_database.characterManager.localEventManager.Move (0f,0f);
			_root.Tick();
		}

		/// <summary>
		/// Init this tree by constructing the behavior tree.
		/// Root node should be returned.
		/// </summary>
		public virtual BTNode Construct () {
			_database = new BTDatabase();//GetComponent<BTDatabase>();
			if (_database == null) {
				_database = new BTDatabase();// gameObject.AddComponent<BTDatabase>();
			}

			return null;
		}
	}

}