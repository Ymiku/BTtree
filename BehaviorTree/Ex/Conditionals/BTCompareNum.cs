using UnityEngine;
using System.Collections;
namespace BT {
	
	/// <summary>
	/// BTCompareData is a conditional inheriting from BTConditional.
	/// 
	/// It performs comparison between the provided data with what's found in BTDatabase.
	/// It returns true if they are equal, false otherwise.
	/// </summary>
	public class BTCompareNum : BTConditional {
		private bool _isSpecial;
		private float _thisNum;
		private float _targetNum;
		private BTNumCompareOpt _numCompareOpt;

		private string _readDataName;
		private int _readDataID;
		private BTSpecialNum _specialName;
		public BTCompareNum(string readDataName,float targetNum,BTNumCompareOpt compareOpt) {
			_isSpecial = false;
			_readDataName = readDataName;
			_targetNum=targetNum;
			_numCompareOpt = compareOpt;
		}
		public BTCompareNum(BTSpecialNum specialName,float targetNum,BTNumCompareOpt compareOpt) {
			_isSpecial = true;
			_specialName = specialName;
			_targetNum=targetNum;
			_numCompareOpt = compareOpt;
		}
		public override void Activate (BTDatabase database) {
			base.Activate (database);
			if (!_isSpecial) {
				_readDataID = _database.GetDataId(_readDataName);
			}
		}
		
		public override bool Check () {
			if (_isSpecial) {
				switch (_specialName) {
				case BTSpecialNum.AlliesNum:
					if (_database.characterManager.characterType == CharacterManager.CharacterType.Allies) {
						_thisNum = GameManager.Instance.PlayerList.Count;
					} else {
						_thisNum = GameManager.Instance.Scene [GameManager.Instance.ThisSceneNum].EnemyList.Count;
					}
					break;
				}
			} else {
				_thisNum = _database.GetData<float>(_readDataID);
			}
			switch (_numCompareOpt) {
			case BTNumCompareOpt.Greater:
				return _thisNum > _targetNum;
				break;
			case BTNumCompareOpt.GreaterEquals:
				return _thisNum >= _targetNum;
				break;
			case BTNumCompareOpt.Equals:
				return _thisNum == _targetNum;
				break;
			case BTNumCompareOpt.LessEquals:
				return _thisNum <= _targetNum;
				break;
			case BTNumCompareOpt.Less:
				return _thisNum < _targetNum;
				break;
			}
			return false;
		}
	}
	
}