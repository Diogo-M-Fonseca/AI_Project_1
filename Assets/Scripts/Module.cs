using UnityEngine;
using System.Collections.Generic;

//enum of all module types
//stored in the same scritp as the module class,
//recomendation from colleague
public enum ModuleType 
{
    Habitat,
    GreenHouse,
    Laboratory,
    Storage,
    Escape,
    Technical
}

//enum of all module States
//stored in the same scritp as the module class,
//recomendation from colleague
public enum ModuleState
{
    Normal,
    NoOxigen,
    Fire,
    NoPower
}

public class Module : MonoBehaviour
{
    //type of this module
    [SerializeField] private ModuleType type;
    //maximum capacity of this module
    [SerializeField] private int capacity = 10;

    //verifies if its and escape module
    public bool IsEscape => type == ModuleType.Escape;

    //visual component to help visualise module state
    private Renderer renderer;
    private Color originalColor;

    //robo currently working in this module
    private GameObject roboWorking;

    //bool to idientify
    //if there is already a robo working or not
    public bool IsAssigned => roboWorking != null;

    //cooldown until being able to suffer incidents
    //after being repaired
    private float repaircooldown = 0f;

    //bool to identify if it has been recently reapired
    public bool RecentlyRepaired => Time.time < repaircooldown;

    //State of this module
    private ModuleState state = ModuleState.Normal;

    //List of agents inside this module
    private List<GameObject> agentsInside = new List<GameObject>();

    //public getters of type and state
    public ModuleType Type { get { return type; } }
    public ModuleState State { get { return state; } }

    //bool to identify if it has space
    public bool HasSpace => agentsInside.Count < capacity;

    private void Awake()
    {
        //Gets module renderer to change color later
        renderer = GetComponent<Renderer>();

        //stores original color
        originalColor = renderer.material.color;

        Debug.Log("Renderer: " + renderer);
    }

    /// <summary>
    /// Adds agent to module it entered
    /// </summary>
    /// <param name="agent"></param>
    public void Enter(GameObject agent)
    {
        //adds agent to module if it isnt already added
        if (!agentsInside.Contains(agent))
        {
            agentsInside.Add(agent);
        }
    }

    /// <summary>
    /// Removes agent from module
    /// </summary>
    /// <param name="agent"></param>
    public void Exit(GameObject agent)
    {
        //removes agent from module
        agentsInside.Remove(agent);
    }

    /// <summary>
    /// Changes module state to new state
    /// </summary>
    /// <param name="newState"></param>
    public void SetState(ModuleState newState)
    {
        //Escape modules ignored
        if (IsEscape) return;

        //changes module state to new state
        state = newState;
        Debug.Log("Module state changed to: " + newState);

        //changes modules visual accordingly
        VisualChanger();
    }

    /// <summary>
    /// Changes module color depending on its sate
    /// </summary>
    private void VisualChanger()
    {
        //changes module color depending on its sate
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
                //picks module color and darkens it a bit
                renderer.material.color = Color.Lerp(originalColor, Color.black, 0.7f);
                break;
        }
    }

    /// <summary>
    /// Verifies if robo can be assigned to repair this module
    /// </summary>
    /// <param name="robot"></param>
    /// <returns></returns>
    public bool TryAssign(GameObject robot)
    {
        //if robo is already assign return false
        if (roboWorking != null) return false;

        //if no robo is working return true
        roboWorking = robot;
        return true;
    }

    /// <summary>
    /// Releases robo after repair
    /// </summary>
    public void Release()
    {
        roboWorking = null;
    }

    /// <summary>
    /// Aplies Repaired cooldown to module
    /// </summary>
    public void Repaired()
    {
        //Uses cooldonw to avoid imediate
        //re-incident in the same module
        repaircooldown = Time.time + 5f;
    }

    /// <summary>
    /// Cleans List to avoid invalid refrences
    /// </summary>
    private void OnDestroy()
    {
        //recomendation from colleague
        agentsInside.Clear();
    }

}
