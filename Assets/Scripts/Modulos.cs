using UnityEngine;

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

    private ModuleState estado = ModuleState.Normal;

    private List<GameObject> agentsInside = new List<GameObject>();

    public ModuleType Type { get { return type; } }
    
    public ModuleState estado { get { return state; } }

    public bool HashSpace => agentsInside.Count < capacity;

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

    public void SetState(ModuleState novoEstado)
    {
        estado = novoEstado;
    }
}
