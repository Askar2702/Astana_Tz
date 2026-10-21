
using System;
using UnityEngine;

[Serializable]
public class StepCondition
{
    [SerializeField] private string _eventId;

    public string EventId => _eventId;
}
