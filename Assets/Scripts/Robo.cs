using UnityEngine;
using UnityEngine.AI;

public class Robo : MonoBehaviour
{
    //navmesh agent conecting to navmesh sistem
    private NavMeshAgent agent;

    //current state
    private AgentState state;

    //collection of all modules present in the map
    private Module[] modules;

    ////module that the robo searches for
    private Module targetModule;

    //Robo batery
    private float battery = 100f;
    private float maxBattery = 100f;

    //Timers utilized for cooldowns 
    private float repairTimer;
    private float timer;

    private void Awake()
    {
        //Get NavMeshAgent as soon as possible
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        //Saves all existeng modules
        modules = FindObjectsOfType<Module>();
        //Starts in Idle
        ChangeState(AgentState.Idle);
    }

    private void Update()
    {
        //Loses batery overtime
        ChangeBattery();

        //State machine
        switch(state)
        {
            case AgentState.Idle:
                PickTask();
                break;

            case AgentState.Moving:
                UpdateMoving();
                break;

            case AgentState.Charging:
                Charging();
                break;

            case AgentState.RespondingToIncident:
                Repair();
                break;
        }
    }

    /// <summary>
    /// Decreses battery and triggers charging decision
    /// </summary>
    private void ChangeBattery()
    {   
        //Consumes battery overtime
        battery -= Time.deltaTime;

        //garantees that it never goes bellow 0f
        battery = Mathf.Clamp(battery, 0f, maxBattery);

        //if battery is too low sendin robo charging
        if (battery < 20f && state != AgentState.Charging)
        {
            targetModule = FindChargingStation();

            if (targetModule != null)
            {
                ChangeState(AgentState.Charging);
            }
            else
            {
                ChangeState(AgentState.Idle); // fallback recomended by colleague
            }
        }
    }

    /// <summary>
    /// Changes robo state and handles transitions
    /// </summary>
    /// <param name="novoEstado"></param>
    private void ChangeState(AgentState novoEstado)
    {   
        //if already in same state return
        if (state == novoEstado) return;

        //if agent still moving change path
        if (state == AgentState.Moving)
        {
            agent.ResetPath();
        }

        state = novoEstado;
        repairTimer = 0f;

        OnEnterState(novoEstado);
    }

    /// <summary>
    /// Imeadiatly beggins action when Entering specific states
    /// </summary>
    /// <param name="novoEstado"></param>
    private void OnEnterState(AgentState novoEstado)
    {
        //if there is no destiny return
        if (targetModule == null) return;

        //if state requires movement SetDestination
        if (novoEstado == AgentState.Moving || novoEstado == AgentState.Charging 
            || novoEstado == AgentState.RespondingToIncident)
        {
            agent.SetDestination(targetModule.transform.position);
        }
    }

    /// <summary>
    /// Controles and manages robo movement
    /// </summary>
    private void UpdateMoving()
    {
        if (targetModule == null) return;

        //wait for navMesh calculations to finish
        if (agent.pathPending) return;

        //if already there return
        if (agent.remainingDistance > 1f) return;

        //if arrived at module with problem fix it  
        if (targetModule.State != ModuleState.Normal)
        {
            ChangeState(AgentState.RespondingToIncident);
            return;
        }

        //return to idle if nothing to fix
        ChangeState(AgentState.Idle);
    }

    /// <summary>
    /// Picks next task based on battery 
    /// and problems in modules
    /// </summary>
    private void PickTask()
    {
        //searches for modules with problems
        Module problem = FindProblemModule();

        //if one is found
        if (problem != null)
        {   
            //trys to "reserve" it
            //to avoid more than one robot in the same module
            if (problem.TryAssign(gameObject))
            {
                targetModule = problem;
                ChangeState(AgentState.Moving);
                return;
            }
            
        }

        //if no problem is found go charge
        targetModule = FindChargingStation();
        ChangeState(AgentState.Moving);
    }

    /// <summary>
    /// Checks all modules and returns an array of the Tecnical ones
    /// </summary>
    /// <returns></returns>
    private Module FindChargingStation()
    {
        //Checks all modules and makes an array of the Tecnical ones
        Module[] options = System.Array.FindAll(modules,
        m => m.Type == ModuleType.Technical &&
             m.State == ModuleState.Normal);

        //if none return null
        if (options.Length == 0) return null;

        return options[Random.Range(0, options.Length)];
    }

    /// <summary>
    /// //Checks all modules and makes an array of avaliable ones with a problem
    /// </summary>
    /// <returns></returns>
    private Module FindProblemModule()
    {
        //Checks all modules and makes an array of
        //avaliable ones with a problem
        Module[] problematic = System.Array.FindAll(modules,
            m => m.State != ModuleState.Normal && !m.IsAssigned);

        //if none return null
        if (problematic.Length == 0) return null;

        return problematic[Random.Range(0, problematic.Length)];
    }

    /// <summary>
    /// Charges robo overtime
    /// </summary>
    private void Charging()
    {
        //if there is no target return
        if (targetModule == null) return;

        //wait for navmesh calculations to end
        if (agent.pathPending) return;

        //if already there return
        if (agent.remainingDistance > 1f) return;

        //increase battery overtime
        battery += Time.deltaTime * 25f;
        //guarantees that battery never excedes maxBattery
        battery = Mathf.Clamp(battery, 0f, maxBattery);

        //if already charged enter idle
        if (battery >= maxBattery)
        {
            ChangeState(AgentState.Idle);
        }
    }

    /// <summary>
    /// Repairs target module
    /// </summary>
    private void Repair()
    {
        //if there is no target return
        if (targetModule == null) return;

        //wait for navmesh calculations to end
        if (agent.pathPending) return;

        //if already there return
        if (agent.remainingDistance > 1f) return;

        //starts repair timer
        repairTimer += Time.deltaTime;

        //after 3 seconds
        if (repairTimer > 3f)
        {
            //Resolves incident
            targetModule.SetState(ModuleState.Normal);

            //creates cooldown
            //to avoid imeaditly reactivating incident
            //on the same module
            targetModule.Repaired();

            //Liberates module for other robots
            targetModule.Release();

            repairTimer = 0f;

            //goes idle
            ChangeState(AgentState.Idle);
        }
    }
}
