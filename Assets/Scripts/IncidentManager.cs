using UnityEngine;

public class IncidentManager : MonoBehaviour
{
    private static IncidentManager instance;

    private List<Incident> incidents = new List<Incident>();

    private float spawnTimer;

    private void Awake () 
    { 
        instance = this; 
    }


    private void ApplyIncident(Module module, IncidentType type)
    {
        switch (type)
        {
            case IncidentType.Fire:
                module.SetState(ModuleState.Blocked);
                break;

            case IncidentType.OxygenLeak:
                module.SetState(ModuleState.Dangerous);
                break;

            case IncidentType.PowerFailure:
                module.SetState(ModuleState.Blocked);
                break;
        }
    }


    private void TriggerIncident(IncidentType type, Module module)
    {
        incidents.Add(new Incident(type, module));

        ApplyIncident(module, type);
    }

    private void UpdateIncidents()
    {
        for (int i = 0; i < incidents.Count; i++)
        {
            incidents[i].Tick();
        }
    }


    private void TriggerRandomIncident()
    {
        Module[] modules = FindObjectsOfType<Module>();

        if (modules.Length == 0) return;

        Module randomModule = modules[Random.Range(0, modules.Length)];

        IncidentType type = (IncidentType)Random.Range(0. 3);

        TriggerRandomIncident(type, modules);
        
    }
}
