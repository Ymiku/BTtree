using UnityEngine;
using System.Collections;
namespace BT.Ex {
	
	public class BTCheckToThink : BTConditional {
		private int _thinkTimeID;
		public override void Activate (BTDatabase database) {
			base.Activate (database);
			_database.SetData<float> ("thinkTime",0f);
			_thinkTimeID = _database.GetDataId ("thinkTime");
		}
		
		public override bool Check () {
			float thinkTime = _database.GetData<float>(_thinkTimeID);
			if (thinkTime > 0) {
				thinkTime -= Time.deltaTime;
				_database.SetData<float> (_thinkTimeID, thinkTime);
				return true;
			} else {
				return false;
			}

		}
		
	}
}