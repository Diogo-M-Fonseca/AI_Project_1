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


    private void TriggerRandomIncident()
    {
        Module[] modules = FindObjectsOfType<Module>();

        if (modules.Length == 0) return;

        Module randomModule = modules[Random.Range(0, modules.Length)];

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

            if (dist < 10f && modules[i].State == ModuleState.Normal)
            {
                if (incident.Type == IncidentType.Fire)
                {
                    modules[i].SetState(ModuleState.Blocked);
                }

                if (incident.Type == IncidentType.OxygenLeak)
                {
                    modules[i].SetState(ModuleState.Dangerous);
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
