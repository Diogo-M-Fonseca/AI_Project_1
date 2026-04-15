using UnityEngine;
using System.Collections.Generic;

//enum of all module types stored in the same scritp as the module class, recomendation from colleague
public enum ModuleType 
{
    Habitat,
    Laboratory,
    Storage,
    Technical
}

public enum ModuleState
{
    Normal,
    Dangerous,
    Blocked
}

public class Module : MonoBehaviour
{
    [SerializeField] private ModuleType type;
    [SerializeField] private int capacity = 10;

    private ModuleState state = ModuleState.Normal;

    private List<GameObject> agentsInside = new List<GameObject>();

    public ModuleType Type { get { return type; } }
    
    public ModuleState State { get { return state; } }

    public bool HasSpace => agentsInside.Count < capacity;

    public void Enter(GameObject agent)
    {
        if (!agentsInside.Contains(agent))
        {
            agentsInside.Add(agent);
        }
    }

    public void Exit(GameObject agent)
    {
        agentsInside.Remove(agent);
    }

    public void SetState(ModuleState newState)
    {
        state = newState;
    }
}
