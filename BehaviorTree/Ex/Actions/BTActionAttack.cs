using UnityEngine;
using System.Collections;

namespace BT.Ex {
	
	public class BTActionAttack : BTAction {
		private CharacterManager _characterManager;
		private EquipmentStatus _EquipmentStatus;
		public override void Activate (BTDatabase database) {
			base.Activate (database);
			_characterManager = _database.characterManager;
		}
		
		protected override void Enter () {
			base.Enter ();
		}
		
		protected override BTResult Execute () {
			_characterManager.localEventManager.Attack ();
			return BTResult.Success;
		}
	}
}