using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;

namespace BT
{
	public class BehaviorTreeEditor : EditorWindow
	{
		public static Type[] nodeTypes;
		public static String[] nodeTypeNames;

		private BTTree _behaviorTree;
		private Dictionary<int, EditorData> _editorData = new Dictionary<int, EditorData>();

		// Default settings.
		private const float _nodeWidth = 200.0f;
		private const float _nodeHeight = 100.0f;

		// Decorator Settings
		private const float _decoratorHeight = 20;

		// Compositor Settings
		private const float _compositorHeight = 50;

		// Leaf settings.
		private const float _leafHeight = 200.0f;

		// General settings.
		private static Vector2 offset = Vector2.zero;

		private Texture _plusIcon;
		private Texture _leftIcon;
		private Texture _rightIcon;

		/**
		 * @brief Add a menu item in the editor for creating new behavior tree assets.
		 *
		 * @todo
		 *     Create new asset in currently selected folder of the project view,
		 *     rather than always placing them in the root Assets folder.
		 *
		 * @todo
		 *     Check if asset already exists, because by default the AssetDatabase
		 *     will overwrite an existing asset of the same name.
		 */
		[MenuItem( "Assets/Create/Behavior Tree" )]
		public static void CreateBehaviorTreeAsset()
		{
			// Create behavior tree asset.
			BTTree behaviorTreeAsset = ScriptableObject.CreateInstance<BTTree>();
			int i = 0;
			while (true) {
				if(AssetDatabase.LoadAssetAtPath<BTTree>("Assets/BT"+i.ToString()+".asset"))
				{
					i++;
					if(i>10000)break;
				}
				else
				{
					break;
				}
			}
			AssetDatabase.CreateAsset( behaviorTreeAsset, "Assets/BT"+i.ToString()+".asset");

			// Add root node to behavior tree.
			BTNode newNode = ScriptableObject.CreateInstance<BTActionBlank>();
			newNode.id = newNode.GetInstanceID(); // generate a unique ID for the node.
			Debug.Log (behaviorTreeAsset._editorData);
			if (behaviorTreeAsset._editorData == null) {
				behaviorTreeAsset._editorData = new List<EditorData> ();
			}
			behaviorTreeAsset._editorData.Add( new EditorData( newNode, null ) );
			behaviorTreeAsset.root = newNode;
			AssetDatabase.AddObjectToAsset( newNode, behaviorTreeAsset );

			// Set asset as the active object.
			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = behaviorTreeAsset;
		}

		[MenuItem( "Window/Behavior Tree Editor" )]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow<BehaviorTreeEditor>();
		}

		void OnGUI()
		{
			// Create a box for selecting the behavior tree.
			// Ironically, this has to be outside of the try-catch
			// otherwise we'll get uncaught exceptions.
			_behaviorTree =
				(BTTree)EditorGUILayout.ObjectField(
					"Behavior Tree",
					_behaviorTree,
					typeof( BTTree ),
					false );

			try
			{
				// only draw editor if a behavior tree is selected
				if ( _behaviorTree )
				{
					// Perform initial setup.
					CreateTypeLists();
					LoadResources();
					BuildEditorData();

					CreateGUI();
				}
			}
			catch ( Exception e )
			{
				Debug.LogError( e );
			}
		}

		void CreateGUI()
		{
			BeginWindows();

			// Handle dragging the mouse in the background.
			if ( Event.current.type == EventType.MouseDrag )
			{
				offset += Event.current.delta;
				_behaviorTree._editorOffset = offset;
				Repaint();
			}

			DrawNode( _behaviorTree.root );
			EndWindows();
		}

		#region Draw Nodes
		void DrawNode( BTNode node )
		{
			if ( !node )
			{
				Debug.LogError( "Node is null!" );
				return;
			}

			// Handle children.
			if ( node is BTDecorator )
			{
				DrawDecorator( (BTDecorator)node );
			}
			else if ( node is BTComposite )
			{
				DrawCompositor( (BTComposite)node );
			}
			else
			{
				DrawLeaf( (BTLeaf)node );
			}
		}

		void DrawDecorator( BTDecorator decorator )
		{
			EditorData nodeData = _editorData[decorator.id];
			CreateNodeWindow( nodeData, _nodeWidth, _decoratorHeight );

			// ensure that the node has a child
			if ( !decorator.child )
			{
				decorator.child = CreateNodeWithParent( typeof( BTActionBlank ), decorator );
			}

			EditorData childData = _editorData[decorator.child.id];

			if ( DrawPlusButton( nodeData, childData ) )
			{
				BTDecorator newNode = (BTDecorator)CreateNodeWithParent( typeof( BTInverter ), decorator );
				newNode.child = decorator.child;
				decorator.child = newNode;
			}

			// handle drawing the child
			DrawNodeCurve( nodeData.rect, childData.rect );
			DrawNode( decorator.child );
		}

		void DrawCompositor( BTComposite compositor )
		{
			EditorData nodeData = _editorData[compositor.id];
			CreateNodeWindow( nodeData, _nodeWidth, _compositorHeight );

			for ( int index = 0; index < compositor.children.Count; ++index )
			{
				BTNode child = compositor.children[index];
				EditorData childData = _editorData[child.id];

				if ( DrawPlusButton( nodeData, childData ) )
				{
					BTDecorator newChild = (BTDecorator)CreateNodeWithParent( typeof( BTInverter ), compositor );
					newChild.child = child;
					compositor.children[index] = newChild;

					_editorData[newChild.id].parentIndex = index;

					child = newChild;
				}

				if ( index > 0 &&
				     DrawLeftButton( childData ) )
				{
					SwapCompositorChildren( compositor, index, index - 1 );
				}

				if ( index < compositor.children.Count - 1 &&
				     DrawRightButton( childData ) )
				{
					SwapCompositorChildren( compositor, index, index + 1 );
				}

				DrawNodeCurve( nodeData.rect, childData.rect, index, compositor.children.Count );
				DrawNode( child );
			}
		}

		void DrawLeaf( BTLeaf leaf )
		{
			EditorData nodeData = _editorData[leaf.id];
			CreateNodeWindow( nodeData, _nodeWidth, _leafHeight );
		}
		#endregion

		#region Startup Helpers
		void CreateTypeLists()
		{
			// get list of possible node types
			// i.e., all types that extend TreeNode and are not abstract
			nodeTypes = typeof( BTNode ).Assembly.GetTypes()
					.Where( type => type.IsSubclassOf( typeof( BTNode ) ) && !type.IsAbstract ).ToArray<Type>();
			nodeTypeNames = Array.ConvertAll<Type, String>( nodeTypes,
				new Converter<Type, String> (
					delegate( Type type )
					{
						if ( type.IsSubclassOf( typeof( BTDecorator ) ) )
						{
							return "Decorators/" + type.Name;
						}
						else if ( type.IsSubclassOf( typeof( BTComposite ) ) )
						{
							return "Compositors/" + type.Name;
						}
						else
						{
							return "Leaves/" + type.Name;
						}
					} ) );
		}

		void LoadResources()
		{
			_plusIcon = (Texture)Resources.Load( "plus" );
			_leftIcon = (Texture)Resources.Load( "arrow_left" );
			_rightIcon = (Texture)Resources.Load( "arrow_right" );
		}

		void BuildEditorData()
		{
			_editorData.Clear();

			foreach ( EditorData editorData in _behaviorTree._editorData )
			{
				_editorData.Add( editorData.id, editorData );
			}

			offset = _behaviorTree._editorOffset;
		}
		#endregion

		#region GUI Helpers
		Type CreateNodeTypeDropdown( BTNode node )
		{
			int selectedType = ( node != null ? Array.IndexOf<Type>( nodeTypes, node.GetType() ) : Array.IndexOf<Type>( nodeTypes, typeof( BTActionBlank ) ) );
			Rect rect = new Rect( 0, 0, _nodeWidth - 40.0f, 20.0f );
			selectedType = EditorGUI.Popup( rect, selectedType, nodeTypeNames );
			return nodeTypes[selectedType];
		}

		bool DrawPlusButton( EditorData parent, EditorData child )
		{
			float midX = ( ( parent.rect.x + _nodeWidth * 0.5f ) + ( child.rect.x + _nodeWidth * 0.5f ) ) * 0.5f;
			float midY = ( ( parent.rect.y + parent.rect.height ) + child.rect.y ) * 0.5f;

			return DrawEditorButton( new Vector2( midX + 2, midY - 8 ), _plusIcon );
		}

		bool DrawLeftButton( EditorData child )
		{
			float left = child.rect.x - 18;
			float top = child.rect.y;
			return DrawEditorButton( new Vector2( left, top ), _leftIcon );
		}

		bool DrawRightButton( EditorData child )
		{
			float left = child.rect.x - 18;
			float top = child.rect.y + ( child.parentIndex == 0 ? 0 : 18 );
			Vector2 position = new Vector2( left, top );

			return DrawEditorButton( position, _rightIcon );
		}

		bool DrawEditorButton( Vector2 position, Texture icon )
		{
			position += offset;

			GUIStyle style = new GUIStyle();
			style.margin = new RectOffset( 0, 0, 0, 0 );
			style.border = new RectOffset( 0, 0, 0, 0 );
			return GUI.Button( new Rect( position.x, position.y, 16.0f, 16.0f ), icon, style );
		}

		void CreateNodeWindow( EditorData nodeData, float width, float height )
		{
			nodeData.rect.width = width;
			nodeData.rect.height = height;

			Rect tempRect = nodeData.rect;
			tempRect.position += offset;
			Rect resultRect = GUI.Window( nodeData.id, tempRect, DrawNodeWindow, "" );
			resultRect.position -= offset;

			// If the node was moved, save the change and mark the asset as dirty.
			if ( resultRect != nodeData.rect )
			{
				nodeData.rect = resultRect;
				EditorUtility.SetDirty( _behaviorTree );
			}
		}

		void DrawNodeWindow( int id )
		{
			// Get EditorNode
			EditorData nodeData = _editorData[id];
			BTNode node = nodeData.node;

			Type selectedType = CreateNodeTypeDropdown( node );
			if ( selectedType != node.GetType() )
			{
				ReplaceNode( nodeData, selectedType );
				node = nodeData.node;
			}

			if ( node is BTComposite )
			{
				BTComposite compositor = (BTComposite)node;

				Rect rect = new Rect( 0, 20, _nodeWidth, 20 );
				if ( GUI.Button( rect, "Add Child" ) )
				{
					BTNode childNode = CreateNodeWithParent( typeof( BTActionBlank ), compositor );
					compositor.children.Add( childNode );
				}
			}
			else if ( node is BTLeaf )
			{
				if ( node == null )
				{
					// Passing null to Editor.CreateEditor() causes the whole editor to hard crash,
					// so we throw an exception to keep that from happening. node should never be
					// null in this case, anyway, but if it is we don't want the whole editor to shut down.
					throw new NullReferenceException();
				}

				Editor editor = Editor.CreateEditor( node );
				editor.OnInspectorGUI();
			}

			// Drag window last because otherwise you can't click on things
			GUI.DragWindow();
		}

		void DrawNodeCurve( Rect start, Rect end )
		{
			Vector3 startPos = new Vector3( start.x + start.width * 0.5f, start.y + start.height, 0 );
			Vector3 endPos = new Vector3( end.x + end.width * 0.5f, end.y, 0 );
			
			DrawNodeCurve( startPos, endPos );
		}

		/// <summary>
		/// Draws the bezier curve between two nodes, offsetting the starting node
		/// based on the number of children nodes and the index of the current child.
		/// </summary>
		/// <param name="start">A Rect describing the position of the starting node.</param>
		/// <param name="end">A Rect describing the position of the ending node.</param>
		/// <param name="index">The index of the end node in the parent node's children array.</param>
		/// <param name="count">The total number of children that the parent node has.</param>
		void DrawNodeCurve( Rect start, Rect end, int index, int count )
		{
			float incrementSize = start.width / count;
			float startX = start.x + incrementSize * index + incrementSize * 0.5f;
			Vector3 startPos = new Vector3( startX, start.y + start.height, 0 );
			Vector3 endPos = new Vector3( end.x + end.width * 0.5f, end.y, 0 );
			
			DrawNodeCurve( startPos, endPos );
		}

		void DrawNodeCurve( Vector3 startPos, Vector3 endPos )
		{
			startPos += new Vector3( offset.x, offset.y, 0.0f );
			endPos += new Vector3( offset.x, offset.y, 0.0f );

			// Calculate tangents.
			float dist = Vector3.Distance( startPos, endPos );
			float curvePower = dist * 0.25f;

			Vector3 startTan = startPos + Vector3.up * curvePower;
			Vector3 endTan = endPos + Vector3.down * curvePower;
			Color shadowCol = new Color( 0, 0, 0, 0.06f );
		
			// Draw a shadow.
			for ( int i = 0; i < 3; i++ )
			{
				Handles.DrawBezier( startPos, endPos, startTan, endTan, shadowCol, null, ( i + 1 ) * 5 );
			}

			Handles.DrawBezier( startPos, endPos, startTan, endTan, Color.black, null, 2 );
		}
		#endregion

		#region Create and Destroy Nodes
		/// <summary>
		/// Do NOT call this directly! It's a only helper function for the other node creation methods.
		/// </summary>
		/// <param name="nodeType"></param>
		/// <returns></returns>
		BTNode CreateNode( Type nodeType )
		{


			BTNode newNode = (BTNode)ScriptableObject.CreateInstance( nodeType );
			newNode.id = newNode.GetInstanceID(); // generate a unique ID for the node.

			AssetDatabase.AddObjectToAsset( newNode, _behaviorTree );
			EditorUtility.SetDirty( _behaviorTree );

			return newNode;
		}

		BTNode CreateNodeWithParent( Type nodeType, BTNode parent )
		{
			BTNode newNode = CreateNode( nodeType );
			EditorData nodeData = new EditorData( newNode, parent );
			nodeData.rect.position = _editorData[parent.id].rect.position + new Vector2( 0.0f, 100.0f );
			_behaviorTree._editorData.Add( nodeData );
			BuildEditorData();
			return newNode;
		}

		BTNode CreateNodeWithoutParent( Type nodeType )
		{
			BTNode newNode = CreateNode( nodeType );
			_behaviorTree._editorData.Add( new EditorData( newNode, null ) );
			BuildEditorData();
			return newNode;
		}

		void DeleteNode( BTNode node, bool deleteEditorData = true )
		{
			if ( node is BTDecorator && ( (BTDecorator)node ).child )
			{
				DeleteNode( ( (BTDecorator)node ).child );
			}
			else if ( node is BTComposite && ( (BTComposite)node ).children != null )
			{
				foreach ( BTNode child in ( (BTComposite)node ).children )
				{
					DeleteNode( child );
				}
			}

			if ( deleteEditorData )
			{
				// Remove the editor data from the list held by the behavior tree.
				_behaviorTree._editorData.Remove( _editorData[node.id] );

				// Remove the data from the local dict.
				_editorData.Remove( node.id );
			}

			DestroyImmediate( node, true );
		}

		void ReplaceNode( EditorData nodeData, Type newType )
		{
			BTNode oldNode = nodeData.node;
			BTNode newNode = CreateNode( newType );

			// Transfer editor data.
			newNode.id = nodeData.id;
			nodeData.node = newNode;

			// Swap children, then delete old node.
			if ( oldNode is BTDecorator && newNode is BTDecorator )
			{
				( (BTDecorator)newNode ).child = ( (BTDecorator)oldNode ).child;
				( (BTDecorator)oldNode ).child = null;
				_editorData[( (BTDecorator)newNode ).child.id].parent = ( (BTDecorator)newNode );

			}
			else if ( oldNode is BTComposite && newNode is BTComposite )
			{
				( (BTComposite)newNode ).children = ( (BTComposite)oldNode ).children;
				( (BTComposite)oldNode ).children = null;
				for (int i = 0; i < ( (BTComposite)newNode ).children.Count; i++) {
					_editorData[( (BTComposite)newNode ).children[i].id].parent = ( (BTComposite)newNode );
				}
			}
			else if ( oldNode is BTDecorator && newNode is BTComposite )
			{
				( (BTComposite)newNode ).children = new List<BTNode>{( (BTDecorator)oldNode ).child};
				( (BTDecorator)oldNode ).child = null;
				_editorData[( (BTComposite)newNode ).children[0].id].parent = ( (BTComposite)newNode );
			}
			else if ( oldNode is BTComposite && newNode is BTDecorator )
			{
				if(( (BTComposite)oldNode ).children.Count>0)
				{
					( (BTDecorator)newNode ).child =  ( (BTComposite)oldNode ).children[0];
					_editorData[( (BTDecorator)newNode ).child.id].parent = ( (BTDecorator)newNode );
				}
				( (BTComposite)oldNode ).children = null;

			}
			DeleteNode( oldNode, false );

			// Update parent's reference to the node.
			if ( nodeData.parent )
			{
				if ( nodeData.parent is BTDecorator )
				{
					BTDecorator parentDecorator = (BTDecorator)nodeData.parent;
					parentDecorator.child = nodeData.node;
				}
				else if ( nodeData.parent is BTComposite )
				{
					BTComposite parentCompositor = (BTComposite)nodeData.parent;
					parentCompositor.children[nodeData.parentIndex] = nodeData.node;
				}
				else
				{
					Debug.LogError( "Parent node is neither a Decorator nor a Compositor!" );
				}
			}
			else
			{
				// Node is root node.
				_behaviorTree.root = nodeData.node;
			}

			// Editor data will have changed, so we must rebuild
			// it before continuing with rendering the GUI.
			BuildEditorData();
		}
		#endregion

		#region General Utilities
		void SwapCompositorChildren( BTComposite compositor, int firstIndex, int secondIndex )
		{
			// Retrieve nodes and editor data.
			BTNode first = compositor.children[firstIndex];
			EditorData childData = _editorData[first.id];

			BTNode second = compositor.children[secondIndex];
			EditorData secondData = _editorData[second.id];

			// Swap the order of the children in the array.
			compositor.children[secondIndex] = first;
			compositor.children[firstIndex] = second;

			// Swap the positions of the two children's nodes.
			// TODO: Move the entire subtree under each node by the same amount.
			Vector2 tempPosition = secondData.rect.position;
			secondData.rect.position = childData.rect.position;
			childData.rect.position = tempPosition;

			// Update EditorData indices.
			secondData.parentIndex = firstIndex;
			childData.parentIndex = secondIndex;
		}
		#endregion
	}
}
#endif
namespace BT
{
	[Serializable]
	public class EditorData
	{
		public int id = 0;
		public BTNode node;
		public BTNode parent; /// If this is null then the node is the root node.
		public int parentIndex = 0; /// The index of the node in the parent's _children array (only used if parent is a compositor).
		public Rect rect = new Rect( 10, 10, 100, 100 );
		public bool foldout = true;

		public EditorData( BTNode node, BTNode parent )
		{
			this.node = node;
			this.parent = parent;
			this.id = node.id;

			if ( parent is BTComposite )
			{
				// EditorData is always created before adding the node
				// to the parent's list of children, so we can assume its
				// index with be _children.Count.
				parentIndex = ( (BTComposite)parent ).children.Count;
			}
		}
	}
}


