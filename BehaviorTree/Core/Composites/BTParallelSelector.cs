using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace BT {
	
	/// <summary>
	/// BTParallelSelector is a composite that:
	/// 
	/// It will return false as soon as one of its child tasks return false. 
	/// It will return true only if all its child tasks return true;
	/// </summary>
	public class BTParallelSelector : BTComposite {
		private List<int> _completeChildNumList = new List<int>();

		public override BTResult Tick () {
			for (int i=0; i<children.Count; i++) {
				if(_completeChildNumList.Contains(i))
					continue;
				BTNode child = children[i];
				
				switch (child.Tick()) {
				case BTResult.Running:
					isRunning = true;
					continue;
				case BTResult.Success:
					child.Clear();
					_completeChildNumList.Add(i);
					continue;
				case BTResult.Failed:	
					child.Clear();
					_completeChildNumList.Add(i);
					return BTResult.Failed;
				}
			}

			if (_completeChildNumList.Count == children.Count) {
				isRunning = false;
				_completeChildNumList.Clear ();
				return BTResult.Success;
			}
			return BTResult.Running;
		}
		
		public override void Clear () {
			base.Clear();
			if (isRunning) {
				for (int i = 0; i < children.Count; i++) {
					if(!_completeChildNumList.Contains(i))
						children[i].Clear();
				}
			}
			_completeChildNumList.Clear ();
		}
	}
}