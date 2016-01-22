using UnityEngine;
using System.Collections;

namespace BT.Ex {

	public class BTActionMove : BTAction {
		private Vector2 _dir;
		private int _targetManagerID;
		private Transform _thisDownTrans;
		private Transform _targetDownTrans;
		public BTActionMove () {

		}
		public override void Activate (BTDatabase database) {
			base.Activate (database);
			_targetManagerID = _database.GetDataId ("targetManager");
			_thisDownTrans = _database.characterManager.transform.Find ("DownCollider");
		}

		protected override void Enter () {
			base.Enter ();
		}

		protected override BTResult Execute () {
			_targetDownTrans = _database.characterManager.transform.Find ("DownCollider");
			_dir = new Vector2 (_targetDownTrans.position.x-_thisDownTrans.position.x,_targetDownTrans.position.y-_thisDownTrans.position.y);
			_database.characterManager.localEventManager.Move (_dir.x,_dir.y);
			return BTResult.Success;
		}

	}

}