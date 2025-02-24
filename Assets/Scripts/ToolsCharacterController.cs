using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsCharacterController : MonoBehaviour
{
    CharacterController2D character;
    Rigidbody2D rb;

    [Header("Tool Range Settings")]
    [SerializeField] float maxToolDistance = 3f;   // how close the player must be
    [SerializeField] float facingThreshold = 0.2f; // dot-product threshold for "facing"

    private void Awake() 
    {
        character = GetComponent<CharacterController2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            UseTool();
        }
    }

    private void UseTool()
    {
        // 1) Convert mouse position → world → tile coords
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int clickedTile = WorldToTile(mouseWorldPos);

        // 2) Find occupant on that tile
        ToolHit occupant = TileObjectManager.Instance.GetOccupant(clickedTile);
        if (occupant == null)
        {
            // Nothing to break on this tile
            return;
        }

        // 3) Check distance
        float distance = Vector2.Distance(rb.position, occupant.transform.position);
        if (distance > maxToolDistance)
        {
            // Too far to break
            return;
        }

        // 4) (Optional) Check facing
        //    If you want to remove facing checks entirely, just comment this out.
        Vector2 dirToObject = (occupant.transform.position - (Vector3)rb.position).normalized;
        float dot = Vector2.Dot(dirToObject, character.lastMotionVector);
        if (dot < facingThreshold)
        {
            // Not facing enough
            return;
        }

        // 5) All checks pass, break the object
        occupant.Hit();
    }

    // Convert world pos to tile coords (flooring x/y)
    private Vector2Int WorldToTile(Vector2 worldPos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x),
            Mathf.FloorToInt(worldPos.y)
        );
    }
}
