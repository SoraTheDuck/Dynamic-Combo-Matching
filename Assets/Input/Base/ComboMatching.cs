using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public struct ComboMapping
{
    public ComboMove comboMove;
    public ScriptableObject action;
}

public class ComboMatching : MonoBehaviour
{
    public List<ComboInput> AllInputs; // List of all Input
    public List<ComboMapping> comboMappings; // Mapping of ComboMove to Action

    [FoldoutGroup("Debug")]
    [SerializeField] private Dictionary<ComboMove, IComboCommand> comboActions;

    [FoldoutGroup("Debug")]
    [SerializeField, ReadOnly] private float timeBetweenInputs, maxDelay; // Time gap between last two inputs
    [FoldoutGroup("Debug")]
    [SerializeField] private List<InputCommand> inputHistory = new List<InputCommand>(); // Track recent inputs
    
    // Calculate
    private float lastInputTime;

    #region Unity Methods

    private void Awake()
    {
        // Initialize dictionary and ensure only valid actions are added
        comboActions = new Dictionary<ComboMove, IComboCommand>();
        foreach (var mapping in comboMappings)
        {
            if (mapping.action is IComboCommand command)
                comboActions[mapping.comboMove] = command;
            else
                Debug.LogWarning($"{mapping.action.name} does not implement IComboCommand and won't be added.");
        }
        
        SetMaxDelay();
    }

    private void Update()
    {
        InputCommand? playerInput = GetPlayerInput();
        
        if(!playerInput.HasValue && inputHistory.Count <= 0) return;
        if (playerInput.HasValue)
        {
            // Calculate the time gap since the last input
            if (inputHistory.Count > 0)
            {
                timeBetweenInputs = Time.time - lastInputTime;
            }
            lastInputTime = Time.time;
            inputHistory.Add(playerInput.Value);

            // Only check for combo matches if we have enough inputs
            CheckComboMatches();
        }

        // Reset the combo if no input is detected within the allowed time
        if (inputHistory.Count > 0 && Time.time - lastInputTime > maxDelay)
        {
            ResetCombo();
        }
    }

    #endregion

    #region Combo Matching

    private void CheckComboMatches()
    {
        foreach (var mapping in comboMappings)
        {
            List<InputQueue> sequence = mapping.comboMove.sequence;

            // Skip if there aren't enough inputs in the history to match the combo sequence
            if (inputHistory.Count < sequence.Count)
                continue;

            // Calculate time required for this combo sequence
            float maxSequenceTime = CalculateMaxSequenceTime(sequence);

            // Check if the combo matches
            if (IsComboMatch(sequence, maxSequenceTime))
            {
                ExecuteCombo(mapping.comboMove);
                ResetCombo(); // Clear input history after a successful combo
                break;
            }
        }
    }

    private bool IsComboMatch(List<InputQueue> sequence, float maxSequenceTime)
    {
        float sequenceTime = 0f;
        bool isMatch = true;

        for (int i = 0; i < sequence.Count; i++)
        {
            InputQueue expectedInput = sequence[i];
            InputCommand actualInput = inputHistory[inputHistory.Count - sequence.Count + i];

            // Accumulate time delay for the sequence
            sequenceTime += expectedInput.Delay;

            // Check if inputs and timing match
            if (actualInput != expectedInput.input || sequenceTime > maxSequenceTime)
            {
                isMatch = false;
                break;
            }
        }

        return isMatch;
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

    private void ExecuteCombo(ComboMove combo)
    {
        if (comboActions.TryGetValue(combo, out IComboCommand action))
            action.Execute();
    }

    private void ResetCombo()
    {
        inputHistory.Clear();
        lastInputTime = 0f;
        timeBetweenInputs = 0f;
    }

    #endregion

    // Input Handling
    private InputCommand? GetPlayerInput()
    {
        foreach (ComboInput comboInput in AllInputs)
        {
            if (Input.GetKeyDown(comboInput.keyCode))
                return comboInput.input;
        }
        return null;
    }
    
    private void SetMaxDelay()
    {
        maxDelay = 0f; // Reset maxDelay to 0 before finding the maximum
        foreach (var mapping in comboMappings)
        {
            foreach (var inputQueue in mapping.comboMove.sequence)
            {
                // Update maxDelay if this input's delay is longer
                if (inputQueue.Delay > maxDelay)
                {
                    maxDelay = inputQueue.Delay;
                }
            }
        }
    }
}
