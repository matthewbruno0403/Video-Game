using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugHUDManager : MonoBehaviour
{
    public GameObject debugTextObject; // Assign this in the Inspector
    public TextMeshProUGUI debugText;
    public PlayerBiomeDetection playerBiomeDetection;
    public TimeManager timeManager;
    public ChunkManager chunkManager;
    public Transform player;

    private bool isDebugVisible = false;

    void Start()
    {
        debugTextObject.SetActive(false);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3)) // Toggle when F3 is pressed
        {
            isDebugVisible = !isDebugVisible;
            debugTextObject.SetActive(isDebugVisible);
        }

        if (isDebugVisible)
        {
            UpdateDebugText();
        }
    }

    void UpdateDebugText()
    {
        string biome = playerBiomeDetection.GetCurrentBiome();
        string time = FormatTime(timeManager.hour, timeManager.minute);
        string phase = timeManager.currentPhase.ToString();

        // ✅ Get the player's chunk using the new WorldToChunkCoord() function
        Vector2Int playerChunk = chunkManager.WorldToChunkCoord(player.position);
        
        // ✅ Get the number of loaded chunks
        int loadedChunksCount = chunkManager.GetLoadedChunkCount();


        debugText.text = $"Biome: {biome}\nTime: {time}\nPhase: {phase}\nChunk: {playerChunk}\nLoaded Chunks: {loadedChunksCount}";
    }

    string FormatTime(int hour, int minute)
    {
        string period = hour >= 12 ? "PM" : "AM";
        int displayHour = hour % 12;
        if (displayHour == 0) displayHour = 12; // Convert 0 to 12 for 12-hour format
        return $"{displayHour:D2}:{minute:D2} {period}";
    }

}
