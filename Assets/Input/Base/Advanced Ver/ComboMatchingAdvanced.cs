using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public struct ComboMappingAdvanced
{
    public ComboMove comboMove;
    public ScriptableObject action;
}

public class ComboMatchingAdvanced : MonoBehaviour
{
    #region Vars

    public List<ComboInput> AllInputs; // List of all Input
    public List<ComboMappingAdvanced> comboMappings; // Mapping of ComboMove to Action

    [FoldoutGroup("Debug")]
    [SerializeField] private Dictionary<ComboMove, IComboCommand> comboActions;

    [FoldoutGroup("Debug/Setup")] 
    [CanBeNull] public ComboChain comboChain;
    [FoldoutGroup("Debug")]
    [SerializeField, ReadOnly] private float timeBetweenInputs, maxDelay; // Time gap between last two inputs
    [FoldoutGroup("Debug")]
    [SerializeField] private List<InputQueue> inputHistory = new List<InputQueue>(); // Track recent inputs as InputQueue
    
    
    private ComboTrie comboTrie;
    private Dictionary<ComboMove, float> comboMaxSequenceTimes;
    private float lastInputTime;
    
    #endregion

    #region Unity Methods

    private void Awake()
    {
        comboActions = new Dictionary<ComboMove, IComboCommand>();
        comboMaxSequenceTimes = new Dictionary<ComboMove, float>();

        foreach (var mapping in comboMappings)
        {
            if (mapping.action is IComboCommand command)
            {
                comboActions[mapping.comboMove] = command;
                comboMaxSequenceTimes[mapping.comboMove] = CalculateMaxSequenceTime(mapping.comboMove.sequence);
            }
            else
            {
                Debug.LogWarning($"{mapping.action.name} does not implement IComboCommand and won't be added.");
            }
        }

        comboTrie = new ComboTrie(comboMappings, comboMaxSequenceTimes);
        SetMaxDelay();
    }

    private void Update()
    {
        InputCommand? playerInput = GetPlayerInput();

        if (!playerInput.HasValue && inputHistory.Count <= 0) return;
        if (playerInput.HasValue)
        {
            if (inputHistory.Count > 0)
            {
                timeBetweenInputs = Time.time - lastInputTime;
            }
            lastInputTime = Time.time;

            // Add new input with time delay
            var input = new InputQueue { input = playerInput.Value, Delay = timeBetweenInputs };
            
            inputHistory.Add(input);
            Chain_AddMove(input);

            CheckComboMatches();
        }

        if (inputHistory.Count > 0 && Time.time - lastInputTime > maxDelay)
        {
            ResetCombo();
        }
    }

    #endregion

    #region Combo Chain Ults

    public void Chain_AddMove(InputQueue input)
    {
        if(comboChain == null) return;
        comboChain.Chain_AddMove(input);
    }
    
    public void Chain_AddCombineMove(IComboCommand action, int actionLength)
    {
        if(comboChain == null) return;
        comboChain.Chain_AddCombineMove(action, actionLength);
    }

    #endregion

    #region Combo Matching

    private void CheckComboMatches()
    {
        ComboMove matchedCombo = comboTrie.MatchCombo(inputHistory, timeBetweenInputs);

        if (matchedCombo != null)
        {
            ExecuteCombo(matchedCombo);
            ResetCombo();
        }
    }

    private void ExecuteCombo(ComboMove combo)
    {
        if (comboActions.TryGetValue(combo, out IComboCommand action))
        {
            Chain_AddCombineMove(action, combo.sequence.Count);
            action.Execute();
        }
    }

    private void ResetCombo()
    {
        // Clear the input history and reset other variables
        inputHistory.Clear();
        lastInputTime = 0f;
        timeBetweenInputs = 0f;
    }

    #endregion
    
    #region Input Handling and Calculate Shiet

    private InputCommand? GetPlayerInput()
    {
        foreach (ComboInput comboInput in AllInputs)
        {
            if (Input.GetKeyDown(comboInput.keyCode))
                return comboInput.input;
        }
        return null;
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
        foreach (var mapping in comboMappings)
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
    
    #endregion
}