using UnityEngine;
using System.Collections;

namespace BT.Ex {
	
	public class BTActionSearch : BTAction {
		private CharacterManager _characterManager;
		private Transform _downCollider;
		public override void Activate (BTDatabase database) {
			base.Activate (database);
			_characterManager = _database.characterManager;
			_downCollider = _characterManager.transform.Find ("DownCollider");
		}
		
		protected override void Enter () {
			base.Enter ();
		}
		
		protected override BTResult Execute () {
			LCEnemy targetLC = GameManager.Instance.FindNearestEnemy (_downCollider.position,_characterManager.characterBase.camp);
			if (targetLC != null) {
				_database.SetData<CharacterManager> ("targetManager", targetLC.gameObject.GetComponent<CharacterManager> ());
				return BTResult.Success;
			} else {
				return BTResult.Failed;
			}
		}
	}
}