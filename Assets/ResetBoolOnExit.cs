using UnityEngine;

public class ResetBoolOnExit : StateMachineBehaviour
{
    public string boolName = "isAttacking";

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(boolName, false);
    }
}