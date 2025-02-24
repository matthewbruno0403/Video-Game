using UnityEngine;

public class CutTreeTop : MonoBehaviour
{
    [SerializeField] GameObject pickUpDrop;
    [SerializeField] int dropCount = 5;
    [SerializeField] float spread = 0.7f;

    
    // Called by the stump script the first time the player chops
    public void ChopTop()
    {
        // 1) Spawn items (logs, branches, etc.) from the top half
        for (int i = 0; i < dropCount; i++)
        {
            Vector3 position = transform.position;
            position.x += spread * Random.value - spread / 2;
            position.y += spread * Random.value - spread / 2;
            Instantiate(pickUpDrop, position, Quaternion.identity);
        }

        // 2) Destroy the top half
        Destroy(gameObject);
    }
}
