using System.Collections.Generic;
using UnityEngine;

namespace SuperPhage
{
    public class SwimFieldState : SingletonBase<SwimFieldState>
    {
        public Vector3 SavedPhagePosition { get; private set; }
        public string CurrentBacteriaID { get; private set; }
        public bool HasSavedPhagePosition { get; private set; }
        public int PendingPhageAISpawnCount { get; private set; }

        private readonly HashSet<string> defeatedBacteriaIDs = new();
        private readonly List<Vector3> defeatedBacteriaPositions = new();
        public IReadOnlyList<Vector3> DefeatedBacteriaPositions => defeatedBacteriaPositions;
        public int DefeatedBacteriaCount => defeatedBacteriaIDs.Count;

        public Vector3 CurrentBacteriaPosition { get; private set; }

        public void SavePhagePosition(Vector3 position)
        {
            SavedPhagePosition = position;
            HasSavedPhagePosition = true;
        }

        public void SetCurrentBacteria(string bacteriaID, Vector3 bacteriaPosition)
        {
            CurrentBacteriaID = bacteriaID;
            CurrentBacteriaPosition = bacteriaPosition;
        }

        public void MarkCurrentBacteriaDefeated()
        {
            if (string.IsNullOrEmpty(CurrentBacteriaID)) return;

            defeatedBacteriaIDs.Add(CurrentBacteriaID);
            defeatedBacteriaPositions.Add(CurrentBacteriaPosition);

            CurrentBacteriaID = null;
        }

        public bool IsBacteriaDefeated(string bacteriaID)
        {
            return defeatedBacteriaIDs.Contains(bacteriaID);
        }

        public void AddPendingPhageAISpawn(int count)
        {
            PendingPhageAISpawnCount += Mathf.Max(0, count);
        }

        public int ConsumePendingPhageAISpawnCount()
        {
            int count = PendingPhageAISpawnCount;
            PendingPhageAISpawnCount = 0;
            return count;
        }

        public void Clear()
        {
            SavedPhagePosition = default;
            CurrentBacteriaID = null;
            HasSavedPhagePosition = false;
            PendingPhageAISpawnCount = 0;
            defeatedBacteriaIDs.Clear();
            defeatedBacteriaPositions.Clear();
        }
    }
}