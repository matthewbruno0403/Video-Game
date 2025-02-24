using UnityEngine;

public static class WorldItemDropper
{
    public static void DropItem(ItemStack stack, Vector3 dropPosition)
    {
        // 1) Get the player's transform and controller
        GameObject playerObj = GameManager.instance.player; 
        Transform playerTransform = playerObj.transform;
        
        CharacterController2D controller = playerObj.GetComponent<CharacterController2D>();
        // If you added the FacingDirection property:
        Vector2 facing2D = controller.facingDirection;
        // Or if you just want to use lastMotionVector:
        // Vector2 facing2D = controller.lastMotionVector == Vector2.zero ? Vector2.down : controller.lastMotionVector;

        // Convert that 2D direction to a 3D vector for spawning
        Vector3 facing3D = new Vector3(facing2D.x, facing2D.y, 0f);

        // 2) Spawn at player's position (not mouse click)
        Vector3 playerPos = playerTransform.position;

        // Load your prefab
        GameObject itemDropPrefab = Resources.Load<GameObject>("WorldItem");
        if(itemDropPrefab == null)
        {
            Debug.LogError("WorldItem prefab not found!");
            return;
        }

        // Instantiate at the player's position
        GameObject droppedItem = GameObject.Instantiate(itemDropPrefab, playerPos, Quaternion.identity);

        // 3) Setup item data and start the "reverse magnet" drop
        PickUpItem pickup = droppedItem.GetComponent<PickUpItem>();
        if (pickup != null)
        {
            pickup.Setup(stack.item, stack.quantity);
            pickup.StartDropping(facing3D, 2f); // e.g., 2 tiles in that direction
        }
    }
}
