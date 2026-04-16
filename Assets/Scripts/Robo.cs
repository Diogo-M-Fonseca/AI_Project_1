using UnityEngine;
using UnityEngine.AI;
using System;

public class Robo : MonoBehaviour
{
    private NavMeshAgent agent;
    private AgentState state;

    private Module[] modules;
    private Module targetModule;

    private float battery = 100f;
    private float maxBattery = 100f;

    private float timer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        modules = FindObjectsOfType<Module>();
        ChangeState(AgentState.Idle);
    }

    private void Update()
    {
        ChangeBattery();

        switch(state)
        {
            case AgentState.Idle:
                PickTask();
                break;

            case AgentState.Moving:
                break;

            case AgentState.Charging:
                ChangeBattery();
                break;
        }
    }


    private void ChangeBattery()
    {
        battery -= Time.deltaTime * 1.5f;

        battery -= Time.deltaTime * 1.5f;
        battery = Mathf.Clamp(battery, 0f, maxBattery);

        if (battery < 20f && state != AgentState.Charging)
        {
            targetModule = FindChargingStation();
             ChangeState(AgentState.Charging)  
        }
    }

    private void ChangeState(AgentState novoEstado)
    {
        if (state == novoEstado) return;

        if (state == AgentState.Moving)
        {
            agent.ResetPath();
        }

        state = novoEstado

        OnEnterState(novoEstado)
    }

    private void OnEnterState(AgentState novoEstado)
    {
        if (targetModule == null) return;

        if (newState == AgentState.Moving || newState == AgentState.Charging)
        {
            agent.SetDestination(targetModule.transform.position);
        }
    }

    private void UpdateMoving()
    {
        if (targetModule == null) return;

        if (agent.pathPending) return;

        if (agent.remainingDistance > 1f) return;

        if (targetModule.State != ModuleState.Normal)
        {
            //futuramente reparar o modulo
        }
        ChangeState(AgentState.Idle);
    }

    private void PickTask()
    {
        Module problem = FindProblemModule();

        if (problem != null)
        {
            targetModule = problem;
            ChangeState(AgentState.Moving);
            return;
        }

        targetModule = FindChargingStation();
        ChangeState(AgentState.Moving);
    }

    private Module FindChargingStation()
    {
        Module[] options = System.Array.FindAll(modules,
        m => m.Type == ModuleType.Technical &&
             m.State == ModuleState.Normal);

        if (options.Length == 0) return null;

        return options[Random.Range(0, options.Length)];
    }

    private Module FindProblemModule()
    {
        Module[] problematic = System.Array.FindAll(modules,
            m => m.State != ModuleState.Normal);

        if (problematic.Length == 0) return null;

        return problematic[Random.Range(0, problematic.Length)];
    }

    private void Charging()
    {
        if (targetModule == null) return;

        if (agent.pathPending) return;

        if (agent.remainingDistance > 1f) return;

        battery += Time.deltaTime * 25f;
        battery = Mathf.Clamp(battery, 0f, maxBattery);

        if (battery >= maxBattery)
        {
            ChangeState(AgentState.Idle);
        }
    }
}
