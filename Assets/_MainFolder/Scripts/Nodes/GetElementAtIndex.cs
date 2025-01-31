using System.Collections.Generic;
using UnityEngine;
using ParadoxNotion;
using ParadoxNotion.Design;
using NodeCanvas.Framework;

[Category("Custom")]
public class GetElementAtIndexNode : ActionTask
{
    [RequiredField]
    public BBParameter<List<GameObject>> list; // Список объектов
    [RequiredField]
    public BBParameter<int> index; // Индекс для извлечения
    [BlackboardOnly]
    public BBParameter<GameObject> result; // Результат

    protected override void OnExecute()
    {
        // Проверяем, что список и индекс валидны
        if (list.value != null && index.value >= 0 && index.value < list.value.Count)
        {
            // Получаем элемент по индексу
            result.value = list.value[index.value];
            EndAction(true);
        }
        else
        {
            // Если индекс невалиден, завершение с ошибкой
            Debug.LogError("Invalid index or list is null");
            EndAction(false);
        }
    }
}
