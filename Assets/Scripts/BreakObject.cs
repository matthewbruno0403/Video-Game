using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakObject : ToolHit
{
    [SerializeField] GameObject pickUpDrop;
    [SerializeField] int dropCount = 5;
    [SerializeField] float spread = 0.7f;
    
    
    public override void Hit()
    {
        for (int i = 0; i < dropCount; i++)
        {
            Vector3 position = transform.position;
            position.x += spread * UnityEngine.Random.value - spread / 2;
            position.y += spread * UnityEngine.Random.value - spread / 2;
            Instantiate(pickUpDrop, position, Quaternion.identity);
        }
        
        TileObjectManager.Instance.UnregisterObject(tileCoord);
        Destroy (gameObject);
    }
}
