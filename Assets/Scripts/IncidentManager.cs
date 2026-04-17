using UnityEngine;
using System.Collections.Generic;

public class IncidentManager : MonoBehaviour
{
    //Singleton for global access
    private static IncidentManager instance;

    //List of all incidents
    private List<Incident> incidents = new List<Incident>();

    //bool to indentify if evacuation is active
    private bool evacuation;
    public static bool EvacuationActive => instance != null && instance.evacuation;

    //Timer to spawn a new incident
    private float spawnTimer;

    private void Awake () 
    { 
        //guarantee singleton
        instance = this; 
    }

    private void Update()
    {
        //Start timer
        spawnTimer += Time.deltaTime;

        //Activates evacuation if 4 or more incidents are happening
        if (!evacuation && incidents.Count > 4)
        {
            evacuation = true;
        }

        //Updates all active incidents
        UpdateIncidents();

        //spawns a new incident every 15s
        if (spawnTimer > 15f)
        {
            spawnTimer = 0f;
            TriggerRandomIncident();
        }

    }

    /// <summary>
    /// Apllyies the incident and changes module state
    /// </summary>
    /// <param name="module"></param>
    /// <param name="type"></param>
    private void ApplyIncident(Module module, IncidentType type)
    {
        //Escape modules are imune
        if (module.IsEscape) return;

        //Appllies state based on incident type
        switch (type)
        {
            case IncidentType.Fire:
                module.SetState(ModuleState.Fire);
                break;

            case IncidentType.OxygenLeak:
                module.SetState(ModuleState.NoOxigen);
                break;

            case IncidentType.PowerFailure:
                module.SetState(ModuleState.NoPower);
                break;
        }
    }

    /// <summary>
    /// Creates/Stores incident and apllies it
    /// </summary>
    /// <param name="type"></param>
    /// <param name="module"></param>
    private void TriggerIncident(IncidentType type, Module module)
    {
        //creates new incident and stores it
        incidents.Add(new Incident(type, module));

        //Applies the effect immediately to the module
        ApplyIncident(module, type);

        Debug.Log("[INCIDENT] " + type +" at " + module.Type +" (" + module.gameObject.name + ")");
    }

    /// <summary>
    /// Chooses random module and incident to apply
    /// </summary>
    private void TriggerRandomIncident()
    {
        //gets all modules in the scene
        Module[] modules = FindObjectsOfType<Module>();

        //if there is none return
        if (modules.Length == 0) return;

        //avoids double incidents in the same module
        Module[] valid = System.Array.FindAll(modules, m => m.State == ModuleState.Normal);

        //if there is none return
        if (valid.Length == 0) return;

        //picks a random module
        Module randomModule = valid[Random.Range(0, valid.Length)];

        //picks random incident
        IncidentType type = (IncidentType)Random.Range(0, 3);

        //Apllies said random incident on random module
        TriggerIncident(type, randomModule);
    }

    /// <summary>
    /// spreads incident to nearby module
    /// </summary>
    /// <param name="incident"></param>
    private void Spread(Incident incident)
    {
        //Wait a bit before starting to spread
        if (incident.Timer < 5f) return;

        //Get all modules
        Module[] modules = FindObjectsOfType<Module>();

        
        for (int i = 0; i < modules.Length; i++)
        {
            //Escape modules are imune
            if (modules[i].IsEscape) continue;

            //Calculates distance from origin of incident
            float dist = Vector3.Distance(incident.Origin.transform.position, 
                modules[i].transform.position);

            //Spreads only to nearby safe modules
            if (dist < 10f && modules[i].State == ModuleState.Normal 
                && !modules[i].RecentlyRepaired)
            {
                if (incident.Type == IncidentType.Fire)
                {
                    modules[i].SetState(ModuleState.Fire);
                }

                if (incident.Type == IncidentType.OxygenLeak)
                {
                    modules[i].SetState(ModuleState.NoOxigen);
                }
            }
        }
    }

    /// <summary>
    ///Updates all incidents and applies spreading logic
    /// </summary>
    private void UpdateIncidents()
    {
        // Updates all incidents and applies spreading logic
        for (int i = 0; i < incidents.Count; i++)
        {
            incidents[i].Tick();
            Spread(incidents[i]);
        }
    }
}
