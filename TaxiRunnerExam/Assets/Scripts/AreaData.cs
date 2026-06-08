using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// AreaData.cs
//
// Serializable data container for each driveable area.
// Assign these in the AreaSelectManager Inspector, exactly like VehicleData.
//
// AREAS:
//   Index 0 — Soweto    — free, slow, low fares
//   Index 1 — Sandton   — R500, medium speed, higher fares
//   Index 2 — CBD       — R1000, fast, highest fares
// ─────────────────────────────────────────────────────────────────────────────

[System.Serializable]
public class AreaData
{
    public string areaName;

    [Tooltip("Unlock cost in Rands. 0 = free.")]
    public int cost;

    [Tooltip("Multiplier applied to GameManager.baseSpeed at run start. 1 = no change.")]
    public float speedMultiplier;

    [Tooltip("Multiplier applied to all passenger fare values. 1 = no change.")]
    public float fareMultiplier;

    [Tooltip("Short description shown on the area select screen.")]
    [TextArea(1, 2)]
    public string description;
}