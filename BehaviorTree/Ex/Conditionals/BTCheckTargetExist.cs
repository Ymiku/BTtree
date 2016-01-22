using UnityEngine;
using System.Collections;
namespace BT.Ex {
	
	public class BTCheckTargetExist : BTConditional {
		private CharacterManager _targetManager;
		private int _alertTimeID;
		public override void Activate (BTDatabase database) {
			base.Activate (database);

			_alertTimeID = _database.GetDataId ("alertTime");
			_database.SetData<float>(_alertTimeID,0f);
		}
		
		public override bool Check () {
			_targetManager = _database.GetData<CharacterManager> ("targetManager");
			float alertTime = _database.GetData<float> (_alertTimeID);
			if (alertTime > 0)
				_database.SetData<float> (_alertTimeID, alertTime -= Time.deltaTime);
			if (_targetManager == null)
				return false;
			if (_targetManager.characterBase.isDie == true)
				return false;
			_database.SetData<float>(_alertTimeID,5f);
			return true;
		}
		
	}
}