using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Tripulante : MonoBehaviour
{
    private NavMeshAgent agent;

    //current estado
    private AgentState state;

    //collection of all modulos present in the map
    private Module[] modules;

    //modulo that the tripulante searches for
    private Module targetModule;

    private float timer;

    private float energy = 100f;
    private float resources = 0f;
    private float greenNeed = 100f;

    private const float maxEnergy = 100f;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        modules = FindObjectsOfType<Module>();
        ChangeState(AgentState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        UpdateNeeds();

        if (IsInDanger())
        {
            ChangeState(AgentState.RespondingToIncident);
        }

        if (IncidentManager.EvacuationActive && state != AgentState.Evacuating)
        {
            ChangeState(AgentState.Evacuating);
        }

        switch (state)
        {
            case AgentState.Idle:
                PickNextTask();
                break;

            case AgentState.Moving:
                UpdateMoving();
                break;

            case AgentState.Working:
                UpdateWorking();
                break;

            case AgentState.Resting:
                UpdateResting();
                break;

            case AgentState.RespondingToIncident:
                UpdateEmergency();
                break;

            case AgentState.Evacuating:
                Evacuate();
                break;
        }
        
    }

    /// <summary>
    /// method to send the tripulant to a specific modulo
    /// </summary>
    /// <param name="mod"></param>
    private void Move(Module mod)
    {
        if (mod == null) return;

        //Usa o "setdestination" do navmesh para informar o tripulante para onde tem que se mover
        agent.SetDestination(mod.transform.position);
    }

    private void UpdateMoving()
    {
        if (targetModule == null) return;

        if (targetModule.State != ModuleState.Normal)
        {
            agent.ResetPath();
            ChangeState(AgentState.Idle);
            return;
        }

        if (agent.pathPending) return;

        if (agent.remainingDistance < 1f)
        {
            // entrou no módulo
            targetModule.Enter(gameObject);

            if (targetModule.Type == ModuleType.Habitat)
                ChangeState(AgentState.Resting);
            else
                ChangeState(AgentState.Working);
        }
    }


    private void ChangeState(AgentState novoEstado)
    {
        //avoid loop
        if (state == novoEstado) return;

        if (state == AgentState.Moving)
        {
            agent.ResetPath();
        }

        if (novoEstado == AgentState.Evacuating)
        {
            targetModule = null;
        }

        //agent only exits modules after it has stoped moving
        if (targetModule != null && state != AgentState.Moving)
            targetModule.Exit(gameObject);

        state = novoEstado;
        timer = 0f;

        OnEnterState(novoEstado);
    }

    private void OnEnterState(AgentState novoEstado)
    {
        switch (novoEstado)
        {
            case AgentState.Moving:
                Move(targetModule);
                break;
        }

    }

    private void PickNextTask()
    {
        ModuleType targetType;

        if (energy < 30f)
            targetType = ModuleType.Habitat;
        else if (resources < 60f)
            targetType = ModuleType.Storage;
        else if (greenNeed < 40f)
            targetType = ModuleType.Storage;
        else 
            targetType = ModuleType.Laboratory;

        Module[] options = System.Array.FindAll(modules,
            m => m.Type == targetType && 
            m.State == ModuleState.Normal && m.HasSpace);
        
        if (options.Length == 0) return;

        targetModule = options[Random.Range(0, options.Length)];

        ChangeState(AgentState.Moving);
    }


    private void UpdateWorking()
    {
        if (timer > 3f)
        {
            resources += 20f;
            energy -= 10f;

            ChangeState(AgentState.Idle);
        }
    }

    private void UpdateResting()
    {
        if (timer > 5f)
        {
            energy = Mathf.Min(maxEnergy, energy + 40f);

            ChangeState(AgentState.Idle);
        }
    }

    private void UpdateNeeds()
    {
        energy -= Time.deltaTime * 2f;

        energy = Mathf.Clamp(energy, 0f, maxEnergy);
        resources = Mathf.Clamp(resources, 0f, 100f);
        greenNeed -= Time.deltaTime * 1.5f;
        greenNeed = Mathf.Clamp(greenNeed, 0f, 100f);
    }

    // helped by AI
    private void UpdateEmergency()
    {

        Module safeModule = FindSafeModule();

        if (safeModule == null) return;

        targetModule = safeModule;

        ChangeState(AgentState.Moving);
    }

    // helped by AI
    private Module FindSafeModule()
    {
        Module[] safemodules = System.Array.FindAll(modules, 
            m => m.State == ModuleState.Normal && m.HasSpace);

        if(safemodules.Length == 0) return null;

        return safemodules[Random.Range(0, safemodules.Length)];
    }

    private bool IsInDanger()
    {
        if (targetModule == null) return false;

        return targetModule.State == ModuleState.Fire ||
               targetModule.State == ModuleState.NoOxigen;
    }

    private Module FindEscapeModule()
    {
        Module[] escapes = System.Array.FindAll(modules,
            m => m.Type == ModuleType.Escape &&
             m.State == ModuleState.Normal);

        if (escapes.Length == 0) return null;

        return escapes[Random.Range(0, escapes.Length)];
    }

    private void Evacuate()
    {
        if (targetModule == null)
        {
            targetModule = FindEscapeModule();

            if (targetModule != null)
            {
                agent.SetDestination(targetModule.transform.position);
            }
            return;
        }

        if (agent.pathPending) return;
        if (agent.remainingDistance < 1f)
        {
            gameObject.SetActive(false);
        }
    }

}
