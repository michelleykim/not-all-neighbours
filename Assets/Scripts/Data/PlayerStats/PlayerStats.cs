using System;
using UnityEngine;

namespace NotAllNeighbours.Data.Stats
{
  /// <summary>
  /// Data structure for player's mental state statistics
  /// </summary>
  [Serializable]
  public class PlayerStats
  {
    [Header("Mental State")]
    [SerializeField]
    [Range(0f, 100f)]
    [Tooltip("How 'normal' you appear to neighbors (high = trusted, low = ostracized)")]
    private float sanity = 100f;

    [SerializeField]
    [Range(0f, 100f)]
    [Tooltip("Your actual grasp on reality (high = see through lies, low = susceptible to gaslighting)")]
    private float clarity = 100f;

    // Properties
    public float Sanity
    {
      get => sanity;
      set => sanity = Mathf.Clamp(value, 0f, 100f);
    }

    public float Clarity
    {
      get => clarity;
      set => clarity = Mathf.Clamp(value, 0f, 100f);
    }

    /// <summary>
    /// Modify sanity by a given amount
    /// </summary>
    public void ModifySanity(float amount)
    {
      Sanity += amount;
    }

    /// <summary>
    /// Modify clarity by a given amount
    /// </summary>
    public void ModifyClarity(float amount)
    {
      Clarity += amount;
    }

    /// <summary>
    /// Reset stats to default values
    /// </summary>
    public void Reset()
    {
      sanity = 100f;
      clarity = 100f;
    }
  }
}