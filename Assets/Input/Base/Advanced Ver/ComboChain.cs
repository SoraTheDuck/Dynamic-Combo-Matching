using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ComboChain : MonoBehaviour
{
    public string debug;
    public List<ComboMappingAdvanced> CombineComboMappings; // Mappings of combo moves to actions
    
    [FoldoutGroup("Debug")] 
    [SerializeField] private Dictionary<ComboMove, IComboCommand> comboActions;
    [FoldoutGroup("Debug")] 
    [SerializeField] private List<InputQueue> CombinedInputHistory = new List<InputQueue>(); // Track recent inputs
    [FoldoutGroup("Debug")]
    [SerializeField] private float timeBetweenInputs, maxDelay; // Time delay tracking

    private ComboTrie comboTrie;
    private Dictionary<ComboMove, float> comboMaxSequenceTimes;
    private float lastInputTime;

    private void Awake()
    {
        comboActions = new Dictionary<ComboMove, IComboCommand>();
        comboMaxSequenceTimes = new Dictionary<ComboMove, float>();

        // Initialize comboActions and maxSequenceTimes
        foreach (var mapping in CombineComboMappings)
        {
            if (mapping.action is IComboCommand command)
            {
                comboActions[mapping.comboMove] = command;
                comboMaxSequenceTimes[mapping.comboMove] = CalculateMaxSequenceTime(mapping.comboMove.sequence);
            }
            else
            {
                Debug.LogWarning($"{mapping.comboMove.name} does not have a valid action and won't be added.");
            }
        }

        comboTrie = new ComboTrie(CombineComboMappings, comboMaxSequenceTimes);
        SetMaxDelay();
    }

    public void Chain_AddMove(InputQueue input)
    {
        if (CombinedInputHistory.Count > 0)
        {
            timeBetweenInputs = Time.time - lastInputTime;
        }
        lastInputTime = Time.time;

        input.Delay = timeBetweenInputs;
        CombinedInputHistory.Add(input);
        debug = input.input + " || " + input.Delay;

        // Check for combined combo matches using ComboTrie
        CheckCombineComboMatches();

        // Reset input history if the delay exceeds the max delay
        if (Time.time - lastInputTime > maxDelay)
        {
            ResetCombo();
        }
    }

    public void Chain_AddCombineMove(IComboCommand action, int comboSequenceLength)
    {
        if (CombinedInputHistory.Count >= comboSequenceLength)
        {
            CombinedInputHistory.RemoveRange(CombinedInputHistory.Count - comboSequenceLength, comboSequenceLength);
        }

        var input = new InputQueue
        {
            action = action,
            Delay = 0 // Reset delay for the new combined combo
        };
        CombinedInputHistory.Add(input);

        lastInputTime = Time.time;

        CheckCombineComboMatches();
    }

    private void CheckCombineComboMatches()
    {
        ComboMove matchedCombo = comboTrie.MatchCombo(CombinedInputHistory, timeBetweenInputs);

        if (matchedCombo != null)
        {
            ExecuteCombinedCombo(matchedCombo);
            ResetCombo();
        }
    }

    private void ExecuteCombinedCombo(ComboMove comboMove)
    {
        if (comboMove == null)
        {
            Debug.LogError("ComboMove is null!");
            return;
        }

        if (comboActions.TryGetValue(comboMove, out IComboCommand action))
        {
            Chain_AddCombineMove(action, comboMove.sequence.Count);
            action.Execute();

            Debug.Log($"Executed combined combo: {comboMove.name}");
        }
        else
        {
            Debug.LogWarning($"No action found for combined combo: {comboMove.name}");
        }
    }

    private void ResetCombo()
    {
        CombinedInputHistory.Clear();
        lastInputTime = 0f;
        timeBetweenInputs = 0f;
    }

    private float CalculateMaxSequenceTime(List<InputQueue> sequence)
    {
        float maxTime = 0f;
        foreach (var input in sequence)
        {
            maxTime += input.Delay;
        }
        return maxTime;
    }

    private void SetMaxDelay()
    {
        maxDelay = 0f;
        foreach (var mapping in CombineComboMappings)
        {
            foreach (var inputQueue in mapping.comboMove.sequence)
            {
                if (inputQueue.Delay > maxDelay)
                {
                    maxDelay = inputQueue.Delay;
                }
            }
        }
    }
}
