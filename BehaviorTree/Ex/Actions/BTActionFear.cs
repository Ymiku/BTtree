using UnityEngine;
using System.Collections;

namespace BT.Ex {
	
	public class BTActionFear : BTAction {
		private CharacterManager _characterManager;
		public override void Activate (BTDatabase database) {
			base.Activate (database);
			_characterManager = _database.characterManager;
		}
		
		protected override void Enter () {
			base.Enter ();
		}
		
		protected override BTResult Execute () {

			return BTResult.Success;
		}
	}
}