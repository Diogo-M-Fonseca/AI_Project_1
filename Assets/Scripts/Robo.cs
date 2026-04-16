using UnityEngine;

public class Robo : MonoBehaviour
{
    private NavMeshAgent agent;
    private AgentState state;

    private Module[] modules;
    private Module targetModule;

    private float battery = 100f;
    private float timer;



    private void ChangeBattery()
    {
        battery -= Time.deltaTime * 1.5f;
        
        if (battery < 20f && state != AgentState.Charging)
        {
             ChangeState()  
        }
    }

    private void ChangeState(AgentState novoEstado)
    {
        if (state == novoEstado) return;

        if (state == AgentState.Moving)
        {
            agent.ResetPath();
        }

        state = novoEstado;
        timer = 0f;

        OnEnterState()
    }

    private void OnEnterState(AgentState novoEstado)
    {
        switch (novoEstado)
        {
            case AgentState.Moving:
                if (targetModule != null)
                {
                    agent.SetDestination(targetModule.transform.position);
                }
                break;
            case AgentState.Charging:
                if (targetModule != null)
                {
                    agent.SetDestination(targetModule.transform.position);
                }
                break;
        }
    }
}
