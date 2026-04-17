using UnityEngine;
using System.Collections.Generic;

public class IncidentManager : MonoBehaviour
{
    private static IncidentManager instance;

    private List<Incident> incidents = new List<Incident>();

    private float spawnTimer;

    private void Awake () 
    { 
        instance = this; 
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        UpdateIncidents();

        if (spawnTimer > 15f)
        {
            spawnTimer = 0f;
            TriggerRandomIncident();
        }
    }


    private void ApplyIncident(Module module, IncidentType type)
    {
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


    private void TriggerIncident(IncidentType type, Module module)
    {
        incidents.Add(new Incident(type, module));

        ApplyIncident(module, type);
        Debug.Log("[INCIDENT] " + type +" at " + module.Type +" (" + module.gameObject.name + ")");
    }


    private void TriggerRandomIncident()
    {
        Module[] modules = FindObjectsOfType<Module>();

        if (modules.Length == 0) return;

        //avoids double incidents in the same module
        Module[] valid = System.Array.FindAll(modules, m => m.State == ModuleState.Normal);

        if (valid.Length == 0) return;

        Module randomModule = valid[Random.Range(0, valid.Length)];

        IncidentType type = (IncidentType)Random.Range(0, 3);

        TriggerIncident(type, randomModule);
        
    }

    private void Spread(Incident incident)
    {
        if (incident.Timer < 5f) return;

        Module[] modules = FindObjectsOfType<Module>();

        for (int i = 0; i < modules.Length; i++)
        {
            float dist = Vector3.Distance(incident.Origin.transform.position, modules[i].transform.position);

            if (dist < 10f && modules[i].State == ModuleState.Normal && !modules[i].RecentlyRepaired)
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

    private void UpdateIncidents()
    {
        for (int i = 0; i < incidents.Count; i++)
        {
            incidents[i].Tick();
            Spread(incidents[i]);
        }
    }
}
