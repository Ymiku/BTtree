using UnityEngine;
using System.Collections;
using BT;
using BT.Ex;

public class BTUsualAI : BTTree {
	public BTUsualAI(CharacterManager characterManager)
	:base(characterManager){}
	public override BTNode Construct () {

		BTComposite root = new BTSelector ();
		BTComposite basic = new BTSelector ();
		root.AddChild (basic);
		BTComposite thinkSeq = new BTSequence ();
		BTComposite battleSeq = new BTSequence ();
		BTComposite searchSeq = new BTSequence ();
		BTComposite idleSel = new BTWeightRandomSelector ();
		basic.AddChild(thinkSeq);
		basic.AddChild(battleSeq);
		basic.AddChild(searchSeq);
		basic.AddChild(idleSel);

		BTConditional thinkCon = new BTCheckToThink ();
		BTAction thinkAct = new BTActionBlank ();
		thinkSeq.AddChild (thinkCon);
		thinkSeq.AddChild (thinkAct);

		BTConditional targetCheckCon = new BTCheckTargetExist();
		BTComposite battleSel = new BTSelector ();
		battleSeq.AddChild (targetCheckCon);
		battleSeq.AddChild (battleSel);
		BTComposite fearSeq = new BTSelector ();
		BTComposite attackPar = new BTParallelSelector ();
		battleSel.AddChild (fearSeq);
		battleSel.AddChild (attackPar);
		BTConditional alliesNumCheck = new BTCompareNum (BTSpecialNum.AlliesNum,3,BTNumCompareOpt.Less);
		BTAction fearAct = new BTActionFear ();
		fearSeq.AddChild (alliesNumCheck);
		fearSeq.AddChild (fearAct);
		BTAction moveAct = new BTActionMove ();
		BTAction attackAct = new BTActionAttack ();
		attackPar.AddChild (moveAct);
		attackPar.AddChild (attackAct);

		BTConditional alertTimeCheck = new BTCompareNum ("alertTime",0,BTNumCompareOpt.Greater);
		BTComposite searchPar = new BTParallelSelector ();
		searchSeq.AddChild (alertTimeCheck);
		searchSeq.AddChild (searchPar);
		BTAction searchAct = new BTActionSearch ();
		BTAction searchMoveAct = new BTActionMove ();
		searchPar.AddChild (searchAct);
		searchPar.AddChild (searchMoveAct);

		BTAction idleMoveAct = new BTActionMove ();
		BTAction idleAct = new BTActionBlank ();
		idleSel.AddChild (idleMoveAct);
		idleSel.AddChild (idleAct);
		return root;
	}
}