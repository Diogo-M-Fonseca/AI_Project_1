using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Tripulante : MonoBehaviour
{
    private NavMeshAgent agent;

    //current estado
    private Estados estado;

    //collection of all modulos present in the map
    private Module[] modules;

    //modulo that the tripulante searches for
    private Module targetModule;

    private float timer;

    private float energy = 100f;
    private float resources = 0f;

    private const float maxEnergy = 100f;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        modulos = FindObjectsOfType<modulos>();
        ChangeState(AgentState.Idle);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Timer.deltaTime;
        UpdateNeeds();

        switch (estado)
        {
            case AgentState.Idle:
                DecideNextTask();
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


    private void ChangeState(Estados novoEstado)
    {
        if (targetModule != null)
            targetModule.Exit(gameObject);

        estado = novoEstado;
        Timer = 0f;

        OnEnterEstado(novoEstado);
    }

    private void OnEnterEstado(Estados novoEstado)
    {
        switch (novoEstado)
        {
            case AgentState.Moving:
                MoveTo(targetModule);
                break;
        }

    }

    private void PickNextTask()
    {
        ModuleType targetType;

        if (energy < 30f)
            targetType = ModuleType.Habitat;
        else if (resources > 60f)
            targetType = ModuleType.Storage;
        else
            targetType = ModuleType.Laboratory;

        module[] options = System.Array.FindAll(modules,
            m => m.Type == targetType && m.state == ModuleState.Normal && m.HashSpace);
        
        if (options.Lenght == 0) return;

        targetModule = options[Random.Range(0, options.Length)];

        ChangeState(Estados.Moving);
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
    }

}
