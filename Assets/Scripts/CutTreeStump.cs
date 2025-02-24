using UnityEngine;

public class CutTreeStump : ToolHit
{
    [Header("References")]
    [SerializeField] CutTreeTop topScript; // Reference to the top script

    [Header("Stump Drops")]
    [SerializeField] GameObject pickUpDrop;
    [SerializeField] int dropCount = 2;
    [SerializeField] float spread = 0.7f;

    
    // Has the top been chopped yet?
    private bool topIsChopped = false;

    public override void Hit()
    {
        // 1) If the top still exists, chop it first
        if (!topIsChopped && topScript != null)
        {
            // Chop the top
            topScript.ChopTop();
            topIsChopped = true;
            topScript = null; 
            return; // End here. Next click will chop the stump.
        }

        // 2) If we reach here, the top is already gone. Chop the stump.
        for (int i = 0; i < dropCount; i++)
        {
            Vector3 position = transform.position;
            position.x += spread * Random.value - spread / 2;
            position.y += spread * Random.value - spread / 2;
            Instantiate(pickUpDrop, position, Quaternion.identity);
        }

        // Destroy the stump
        TileObjectManager.Instance.UnregisterObject(tileCoord);
        Destroy(gameObject);
    }
}
