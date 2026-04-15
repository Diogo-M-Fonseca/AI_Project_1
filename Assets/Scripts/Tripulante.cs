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
    private void Move(Modulos mod)
    {
        if (mod == null) return;

        //Usa o "setdestination" do navmesh para informar o tripulante para onde tem que se mover
        agent.SetDestination(mod.transform.position);
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
        int r = Random.Range(0, 3);

        ModuleType type =
            r == 0 ? ModuleType.Laboratory :
            r == 1 ? ModuleType.Habitat :
                     ModuleType.Storage;

        var options = System.Array.FindAll(modulos, m => m.Type == type);

        if (options.Length == 0) return;

        moduloTarget = options[Random.Range(0, options.Length)];

        ChangeState(estado.Moving);
    }



}
