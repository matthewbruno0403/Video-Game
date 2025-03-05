using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class PlayerCombat : MonoBehaviour
{
    private CharacterController2D character;
    private Animator animator;

    // Attack config
    public float attackDuration = 0.3f;
    private bool isAttacking = false;

    void Awake()
    {
        character = GetComponent<CharacterController2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            ItemStack activeStack = InventoryManager.instance.hotbarUI.GetActiveSlotItem();
            if (activeStack != null && activeStack.item != null && activeStack.item.itemType == "Weapon")
            {
                StartCoroutine(PerformAttack());
            }
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;

        // 1) Determine facing direction
        int dir = 0; // let's say 0 = down
        Vector2 facing = character.facingDirection;

        // Example mapping: 0=down, 1=up, 2=left, 3=right
        if (facing.y > 0.5f)
        {
            dir = 1; // up
        }
        else if (facing.y < -0.5f)
        {
            dir = 0; // down
        }
        else if (facing.x < -0.5f)
        {
            dir = 2; // left
        }
        else if (facing.x > 0.5f)
        {
            dir = 3; // right
        }

        // 2) Pass these parameters to the Animator
        animator.SetInteger("attackDir", dir);
        animator.SetTrigger("Attack");   // Triggers the correct Attack state based on 'attackDir'

        // 3) Temporarily stop movement
        float originalSpeed = character.GetSpeed();
        character.SetSpeed(0f);

        // 4) Wait for the attack duration
        yield return new WaitForSeconds(attackDuration);

        // 5) Restore movement
        character.SetSpeed(originalSpeed);
        isAttacking = false;
    }
}
