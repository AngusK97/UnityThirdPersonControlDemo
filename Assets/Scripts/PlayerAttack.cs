using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Animator animator;
    public InputManager inputManager;
    public string attackParamString;
    public string doComboParamString;
    
    private int _attackParamHash;
    private int _doComboParamHash;

    private bool _canDoCombo;
    
    private void Start()
    {
        _attackParamHash = Animator.StringToHash(attackParamString);
        _doComboParamHash = Animator.StringToHash(doComboParamString);
    }

    void Update()
    {
        if (inputManager.playerControls.Attack.LightAttack.triggered)
        {
            if (!_canDoCombo)
            {
                animator.SetTrigger(_attackParamHash);
            }
            else
            {
                animator.SetBool(_doComboParamHash, true);
            }
        }
    }

    private void LateUpdate()
    {
        animator.ResetTrigger(_attackParamHash);
    }
    
    
    //-----------------------------------------------------------------------------------------------
    // Animation Events
    //-----------------------------------------------------------------------------------------------

    private void SetCanDoCombo()
    {
        _canDoCombo = true;
    }

    private void SetCanNotDoCombo()
    {
        _canDoCombo = false;
    }

    private void SetNotDoCombo()
    {
        animator.SetBool(_doComboParamHash, false);
    }

    private void SetIsNotAttacking()
    {
        animator.SetBool("isAttacking", false);
    }
}
