using System.Collections.Generic;
using UnityEngine;

public class ComboTrie
{
    private class TrieNode
    {
        public Dictionary<InputCommand, TrieNode> children = new Dictionary<InputCommand, TrieNode>();
        public ComboMove comboMove;
        public float maxSequenceTime; // Precomputed max time for the combo sequence
    }

    private readonly TrieNode root;

    public ComboTrie(List<ComboMappingAdvanced> comboMappings, Dictionary<ComboMove, float> comboMaxSequenceTimes)
    {
        root = new TrieNode();
        BuildTrie(comboMappings, comboMaxSequenceTimes);
    }

    private void BuildTrie(List<ComboMappingAdvanced> comboMappings, Dictionary<ComboMove, float> comboMaxSequenceTimes)
    {
        foreach (var mapping in comboMappings)
        {
            TrieNode currentNode = root;
            foreach (var inputQueue in mapping.comboMove.sequence)
            {
                if (!currentNode.children.TryGetValue(inputQueue.input, out TrieNode nextNode))
                {
                    nextNode = new TrieNode();
                    currentNode.children[inputQueue.input] = nextNode;
                }
                currentNode = nextNode;
            }
            currentNode.comboMove = mapping.comboMove;
            currentNode.maxSequenceTime = comboMaxSequenceTimes[mapping.comboMove];
        }
    }

    public ComboMove MatchCombo(List<InputQueue> inputHistory, float maxAllowedTime)
    {
        // Loop through each possible starting position in the input history
        for (int startIdx = 0; startIdx < inputHistory.Count; startIdx++)
        {
            TrieNode currentNode = root;
            float accumulatedTime = 0f;
        
            // Try to match from this starting position
            for (int i = startIdx; i < inputHistory.Count; i++)
            {
                InputCommand input = inputHistory[i].input;
                accumulatedTime += inputHistory[i].Delay;

                // Check if the input matches the current path in the trie
                if (!currentNode.children.TryGetValue(input, out currentNode))
                    break; // No matching path from this starting point

                // If we reached a complete combo move within the max allowed time, return it
                if (currentNode.comboMove != null && accumulatedTime <= currentNode.maxSequenceTime)
                    return currentNode.comboMove;
            }
        }

        // No matching combo found in the current input history
        return null;
    }

}
