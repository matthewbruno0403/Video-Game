using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    Transform player;
    [SerializeField] float speed = 10f;
    [SerializeField] float pickUpDistance = 1.5f;
    [SerializeField] float timeToLeave = 6000f;

    public Item itemData;
    public int quantity = 1;

    [HideInInspector] private bool isDropping = false;
    [HideInInspector] private Vector3 dropDestination;

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
        dropDestination = transform.position + facingDirection * tilesToDrop;
    }

    private void Start()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager instance is null at start!");
            return;
        }
        if (GameManager.instance.player == null)
        {
            Debug.LogError("GameManager's player reference is null at Start!");
            return;
        }
        player = GameManager.instance.player.transform;

        // Find the InventoryManager
        inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogError("No InventoryManager found in scene!");
        }
    }
  
    private void Update()
    {
        // 1) Handle dropping animation first
        if (isDropping)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                dropDestination,
                speed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, dropDestination) < 0.1f)
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
            bool success = inventoryManager.AddItem(itemData, quantity);
            if (!success)
            {
                Debug.Log("Inventory is full! Can't pick up item.");
                // Slight nudge away from player to avoid stuck overlap
                transform.position += (transform.position - player.position).normalized * 1f;
                return;
            }
            Destroy(gameObject);
        }
    }
}
