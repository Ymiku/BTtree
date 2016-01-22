using UnityEngine;
using System.Collections;
namespace BT.Ex {
	
	public class BTCheckToAttack : BTConditional {
		private CharacterManager _targetManager;
		private Transform _trans;
		private EquipmentStatus _equipmentStatus;
		public override void Activate (BTDatabase database) {
			base.Activate (database);
			_trans = _database.characterManager.transform.Find ("DownCollider");
			_equipmentStatus = _database.characterManager.characterBase.equipmentStatus;
		}
		
		public override bool Check () {
			_targetManager = _database.GetData<CharacterManager> ("targetManager");
			if (_targetManager == null)
				return false;
			Transform targetTrans = _targetManager.transform.Find("DownCollider");
			return Mathf.Abs (_trans.position.x - targetTrans.position.x) < _equipmentStatus.Range.x &&
				Mathf.Abs (_trans.position.y - targetTrans.position.y) < _equipmentStatus.Range.y;
		}

	}
}