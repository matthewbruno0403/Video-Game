using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    Transform player;
    [SerializeField] float speed = 10f;
    [SerializeField] float pickUpDistance = 1.5f;
    [SerializeField] float timeToLeave = 300f;

    public Item itemData;
    public int quantity = 1;

    [HideInInspector] private bool isDropping = false;
    // [HideInInspector] private Vector3 dropDestination;
    private Vector3 dropStartPos;
    private Vector3 dropEndPos;
    private float dropTime = 0f;
    private float dropDuration = 0.4f;
    private float arcHeight = 0.3f;


    private InventoryManager inventoryManager;

    // Called by WorldItemDropper (or your static method) to set item data
    public void Setup(Item newItem, int newQuantity)
    {
        itemData = newItem;
        quantity = newQuantity;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && itemData != null && itemData.icon != null)
        {
            sr.sprite = itemData.icon;
        }
    }

    // NEW: This method starts the "drop" animation
    public void StartDropping(Vector3 facingDirection, float tilesToDrop = 2f)
    {
        isDropping = true;
        // The item is already at the player's position; 
        // move it 'tilesToDrop' units in the facing direction
        // dropDestination = transform.position + facingDirection * tilesToDrop;
        dropStartPos = transform.position;
        dropEndPos = dropStartPos + facingDirection * tilesToDrop;
        dropTime = 0f;
    }

    private void Start()
    {
        if (GameManager.instance == null)
        {
            return;
        }
        if (GameManager.instance.player == null)
        {
            return;
        }
        player = GameManager.instance.player.transform;

        // Find the InventoryManager
        inventoryManager = FindObjectOfType<InventoryManager>();
    }
  
    private void Update()
    {
        // 1) Handle dropping animation first
        if (isDropping)
        {
            dropTime += Time.deltaTime;
            float t = dropTime / dropDuration;
            t = Mathf.Clamp01(t);

            Vector3 newPos = Vector3.Lerp(dropStartPos, dropEndPos, t);
            float heightOffset = arcHeight * Mathf.Sin(Mathf.PI * t);
            newPos.y += heightOffset;

            transform.position = newPos;

            if (t >= 1f)
            {
                // Done dropping
                isDropping = false;
            }
            return; // Skip pickup logic while dropping
        }
        
        // 2) Otherwise, handle normal "pick up" logic
        timeToLeave -= Time.deltaTime;
        if (timeToLeave < 0f) 
        {
            Destroy(gameObject);
            return;
        }
    
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > pickUpDistance)
        {
            return;
        }

        bool canFit = inventoryManager.HasSpaceFor(itemData, quantity);
        if (!canFit)
        {
            return; // Inventory full
        }

        // Magnet toward the player
        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            speed * Time.deltaTime
        );

        // If close enough, pick up
        if (distance < 0.1f)
        {
            int usedSlot = inventoryManager.AddItem(itemData, quantity);
            if (usedSlot < 0)
            {
                Debug.Log("Inventory is full! Can't pick up item.");
                // Slight nudge away from player to avoid stuck overlap
                transform.position += (transform.position - player.position).normalized * 1f;
                return;
            }

        // Force the hotbar to refresh and re-equip the active slot if the item landed there
        HotbarUI hotbarUI = FindObjectOfType<HotbarUI>();
        if (hotbarUI != null)
        {
            hotbarUI.RefreshHotbar();
            if (usedSlot == hotbarUI.activeSlotIndex)
            {
                hotbarUI.SetActiveSlot(hotbarUI.activeSlotIndex);
            }
        }
            Destroy(gameObject);
        }
    }
}