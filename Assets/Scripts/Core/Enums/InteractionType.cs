namespace NotAllNeighbours.Core.Enums
{
  /// <summary>
  /// Types of interactions available in the game
  /// According to GDD: 5 core interaction types only
  /// </summary>
  public enum InteractionType
  {
    /// <summary>
    /// Basic inspection - displays text description (Left Click)
    /// </summary>
    Examine,

    /// <summary>
    /// Collect evidence via photography (Right Click)
    /// GDD: Right-click to photograph evidence (max 5/day)
    /// </summary>
    Collect,

    /// <summary>
    /// Initiate dialogue with NPCs (Left Click)
    /// GDD: Triggers dialogue system with Sanity/Clarity gating
    /// </summary>
    Talk,

    /// <summary>
    /// Detailed inspection with camera zoom (Left Click)
    /// GDD: Some evidence only photographable when zoomed
    /// </summary>
    Investigate,

    /// <summary>
    /// Room transition (Left Click)
    /// GDD: Scene loading with fade effects
    /// </summary>
    Door
  }
}