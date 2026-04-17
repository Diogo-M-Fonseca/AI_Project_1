using UnityEngine;
using System.Collections.Generic;

//enum of all module types stored in the same scritp as the module class, recomendation from colleague
public enum ModuleType 
{
    Habitat,
    GreenHouse,
    Laboratory,
    Storage,
    Technical
}

public enum ModuleState
{
    Normal,
    NoOxigen,
    Fire,
    NoPower
}

public class Module : MonoBehaviour
{
    [SerializeField] private ModuleType type;
    [SerializeField] private int capacity = 10;

    private Renderer renderer;
    private Color originalColor;

    private GameObject roboWorking;

    public bool IsAssigned => roboWorking != null;

    private ModuleState state = ModuleState.Normal;

    private List<GameObject> agentsInside = new List<GameObject>();

    public ModuleType Type { get { return type; } }
    
    public ModuleState State { get { return state; } }

    public bool HasSpace => agentsInside.Count < capacity;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();

        originalColor = renderer.material.color;
        Debug.Log("Renderer: " + renderer);
    }

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
        Debug.Log("Module state changed to: " + newState);
        VisualChanger();
    }

    private void VisualChanger()
    {
        switch (state)
        {

            case ModuleState.Normal:
                renderer.material.color = originalColor;
                break;

            case ModuleState.NoOxigen:
                renderer.material.color = Color.white;
                break;

            case ModuleState.Fire:
                renderer.material.color = Color.red;
                break;

            case ModuleState.NoPower:
                renderer.material.color = Color.Lerp(originalColor, Color.black, 0.7f);
                break;
        }
    }

    public bool TryAssign(GameObject robot)
    {
        if (roboWorking != null) return false;

        roboWorking = robot;
        return true;
    }

    public void Release()
    {
        roboWorking = null;
    }
}
