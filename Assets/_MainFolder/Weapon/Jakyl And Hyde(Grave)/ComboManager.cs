using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ComboManager : MonoBehaviour
{
    [System.Serializable]
    public class ComboStep
    {
        public string attackType;
        public float cooldown;
        public int damage;
        public float damageDelay;
        public UnityEvent onStepEvent;
    }

    [System.Serializable]
    public class MyCombo
    {
        public string comboType;             // Тип комбо (например, "Grave" или "Revolver")
        public string comboName;
        public List<ComboStep> steps;
        public UnityEvent onComboComplete;
    }

    public List<MyCombo> combos;            // Все доступные комбинации
    public float comboResetTime = 1.0f;     // Время сброса текущего комбо, если игрок не продолжил

    public Animator GraveAnimator;
    public Animator RevolverAnimator;

    private Dictionary<string, ComboState> comboStates = new Dictionary<string, ComboState>();

    private void Start()
    {
        // Инициализируем состояния для каждого типа комбо
        foreach (var combo in combos)
        {
            if (!comboStates.ContainsKey(combo.comboType))
            {
                comboStates[combo.comboType] = new ComboState();
            }
        }
    }

    private void Update()
    {
        foreach (var state in comboStates.Values)
        {
            // Сброс последовательности после времени бездействия
            if (state.currentSequence.Count > 0 && Time.time - state.lastAttackTime > comboResetTime)
            {
                state.Reset();
            }
        }
    }

    public void RegisterAttack(string attackType)
    {
        if (!comboStates.ContainsKey(attackType))
        {
            Debug.LogWarning($"Атака '{attackType}' не зарегистрирована в системах комбо!");
            return;
        }

        var state = comboStates[attackType];

        // Игнорируем, если атака на КД
        if (Time.time - state.lastAttackTime < GetCurrentCooldown(attackType))
        {
            Debug.Log($"Атака '{attackType}' на КД!");
            return;
        }

        state.lastAttackTime = Time.time; // Обновляем таймер

        // Добавляем атаку в последовательность
        state.currentSequence.Add(attackType);

        // Проверяем, подходит ли текущая последовательность под комбо
        MyCombo matchedCombo = CheckCombo(attackType, state.currentSequence);
        if (matchedCombo != null)
        {
            ExecuteComboStep(matchedCombo, state);
        }
        else
        {
            Debug.Log($"Атака '{attackType}' не подходит под активное комбо.");
            state.Reset(); // Сбрасываем последовательность для этого типа
        }
    }

    private MyCombo CheckCombo(string comboType, List<string> sequence)
    {
        foreach (var combo in combos)
        {
            if (combo.comboType != comboType)
                continue;

            if (sequence.Count > combo.steps.Count)
                continue;

            bool isMatch = true;
            for (int i = 0; i < sequence.Count; i++)
            {
                if (sequence[i] != combo.steps[i].attackType)
                {
                    isMatch = false;
                    break;
                }
            }

            if (isMatch)
                return combo;
        }

        return null;
    }

    private void ExecuteComboStep(MyCombo combo, ComboState state)
    {
        if (state.currentStepIndex < combo.steps.Count)
        {
            var currentStep = combo.steps[state.currentStepIndex];
            currentStep.onStepEvent?.Invoke();

            // Запуск анимации
            if (combo.comboType == "Grave")
            {
                GraveAnimator.SetTrigger("Punch" + (state.currentStepIndex + 1));
            }
            else if (combo.comboType == "Revolver")
            {
                RevolverAnimator.SetTrigger("Shoot" + (state.currentStepIndex + 1));
            }

            state.currentStepIndex++;

            // Если это последний шаг
            if (state.currentStepIndex >= combo.steps.Count)
            {
                combo.onComboComplete?.Invoke();
                state.Reset();
            }
        }
    }

    private float GetCurrentCooldown(string comboType)
    {
        if (comboStates.TryGetValue(comboType, out var state) && state.currentStepIndex > 0)
        {
            foreach (var combo in combos)
            {
                if (combo.comboType == comboType && state.currentStepIndex <= combo.steps.Count)
                {
                    return combo.steps[state.currentStepIndex - 1].cooldown;
                }
            }
        }
        return 0;
    }

    private class ComboState
    {
        public List<string> currentSequence = new List<string>();
        public float lastAttackTime;
        public int currentStepIndex;

        public void Reset()
        {
            currentSequence.Clear();
            currentStepIndex = 0;
        }
    }
}
