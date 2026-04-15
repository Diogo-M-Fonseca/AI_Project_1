using UnityEngine;
using UnityEngine.AI;

public class Tripulante : MonoBehaviour
{
    private NavMeshAgent agent;

    //current estado
    private Estados estado;

    //collection of all modulos present in the map
    private Modulos[] modulos;

    //modulo that the tripulante searches for
    private Modulos moduloTarget;

    private float Timer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        modulos = FindObjectsOfType<modulos>();
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Timer.deltaTime;

        switch (estado)
        {
            case AgentState.Idle:
                break;

            case AgentState.Moving:
                break;

            case AgentState.Working:
                break;

            case AgentState.Resting:
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


    private void ChangeEstado(Estados novoEstado)
    {
        estado = novoEstado;
        Timer = 0f;
    }

    private void OnEnterEstado(Estados novoEstado)
    {
        switch (novoEstado)
        {
            case AgentState.Idle:
                break;

            case AgentState.Moving:
                MoveTo(targetModule);
                break;

            case AgentState.Working:
                break;

            case AgentState.Resting:
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

        ChangeEstado(estado.Moving);
    }



}
