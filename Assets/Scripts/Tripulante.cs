using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Tripulante : MonoBehaviour
{
    private NavMeshAgent agent;

    //Health value of each tripulante
    private float health = 100f;
    private const float maxHealth = 100f;

    //current state
    private AgentState state;

    //collection of all modulos present in the map
    private Module[] modules;

    //modulo that the tripulante searches for
    private Module targetModule;

    private float timer;

    //Necessitys of tripulante
    private float energy = 100f;
    private float resources = 0f;
    private float greenNeed = 100f;

    private const float maxEnergy = 100f;


    private void Awake()
    {
        //Get NavMeshAgent as soon as possible
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        //Saves all existeng modules
        modules = FindObjectsOfType<Module>();

        //Starts in Idle
        ChangeState(AgentState.Idle);
    }

    void Update()
    {
        //if it has died avoids starting Update again
        if (health <= 0f) return;

        timer += Time.deltaTime;

        //Updates needs and health before anything else
        UpdateNeeds();
        UpdateHealth();

        //If target module IsInDanger responds accordingly
        if (IsInDanger())
        {
            ChangeState(AgentState.RespondingToIncident);
        }

        //If evacuation is Active responds accordingly
        if (IncidentManager.EvacuationActive && state != AgentState.Evacuating)
        {
            ChangeState(AgentState.Evacuating);
        }

        //State machine 
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
    /// Moves tripulante to a Module
    /// </summary>
    /// <param name="mod"></param>
    private void Move(Module mod)
    {
        if (mod == null) return;

        //Uses SetDestination of NavMesh to move the tripulante 
        agent.SetDestination(mod.transform.position);
    }
    /// <summary>
    /// Controles and manages tripulante movement
    /// </summary>
    private void UpdateMoving()
    {
        // if target module dosent exist cancel movement
        if (targetModule == null) return;

        // if module becomes dangerous cancel movement
        if (targetModule.State != ModuleState.Normal)
        {
            agent.ResetPath();
            ChangeState(AgentState.Idle);
            return;
        }

        //waits for NavMeshCalculations to finish
        if (agent.pathPending) return;

        //If arrived at destiny
        if (agent.remainingDistance < 1f)
        {
           //Only enters if module isnt in danger
          if (targetModule.State == ModuleState.Normal)
          {
              targetModule.Enter(gameObject);
          }
              //Decide what to do inside module
              if (targetModule.Type == ModuleType.Habitat)
                  ChangeState(AgentState.Resting);
              else
                   ChangeState(AgentState.Working);
        }
    }

    /// <summary>
    /// Changes tripulantes state and handles transitions
    /// </summary>
    /// <param name="novoEstado"></param>
    private void ChangeState(AgentState novoEstado)
    {
        //avoid loop
        if (state == novoEstado) return;

        //is tripulante was already moving change path
        if (state == AgentState.Moving)
        {
            agent.ResetPath();
        }

        //Cancel all other decisions if needing to evacuate
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

    /// <summary>
    /// Imeadiatly beggins action when Entering specific states
    /// </summary>
    /// <param name="novoEstado"></param>
    private void OnEnterState(AgentState novoEstado)
    {
        //OnEnter moving imeadiatly star moving
        if (novoEstado == AgentState.Moving)
            Move(targetModule);
    }


    /// <summary>
    /// Picks next task based on necessitys and module avaliability
    /// </summary>
    private void PickNextTask()
    {
        ModuleType targetType;
        
        //Decides next task based on necessitys
        //the higher on this if statement the higher its priority
        if (energy < 30f)
            targetType = ModuleType.Habitat;
        else if (resources < 60f)
            targetType = ModuleType.Storage;
        else if (greenNeed < 40f)
            targetType = ModuleType.Storage;
        else 
            targetType = ModuleType.Laboratory;

        //Searches for valid modules
        //this representation style "m => m.Type .etc" was inspierd by colleague
        Module[] options = System.Array.FindAll(modules,
            m => m.Type == targetType && 
            m.State == ModuleState.Normal && m.HasSpace);
        
        if (options.Length == 0) return;

        //In valid options chooses one randomly
        targetModule = options[Random.Range(0, options.Length)];

        ChangeState(AgentState.Moving);
    }

    /// <summary>
    /// Working action of tripulante
    /// </summary>
    private void UpdateWorking()
    {
        if (timer > 3f)
        {
            //Creates resources and consumes energy
            resources += 20f;
            energy -= 10f;

            ChangeState(AgentState.Idle);
        }
    }

    /// <summary>
    /// Resting action of tripulante
    /// </summary>
    private void UpdateResting()
    {
        if (timer > 5f)
        {
            //Regains Energy
            energy = Mathf.Min(maxEnergy, energy + 40f);

            ChangeState(AgentState.Idle);
        }
    }

    /// <summary>
    /// Decrease needs values overtime
    /// </summary>
    private void UpdateNeeds()
    {
        //Consuming energy and greenNeed
        energy -= Time.deltaTime * 2f;
        greenNeed -= Time.deltaTime * 1.5f;
        
        //Clamp values of necessitys
        energy = Mathf.Clamp(energy, 0f, maxEnergy);
        resources = Mathf.Clamp(resources, 0f, 100f);
        greenNeed = Mathf.Clamp(greenNeed, 0f, 100f);
    }

    /// <summary>
    /// Manages decisions during emergency
    /// </summary>
    private void UpdateEmergency()
    {
        //This method was made with help from AI
        //searches for safe module
        Module safeModule = FindSafeModule();

        //if there is nun cancel
        if (safeModule == null) return;

        targetModule = safeModule;

        ChangeState(AgentState.Moving);
    }

    /// <summary>
    /// Checks all modules and returns an array of the safe ones
    /// </summary>
    /// <returns></returns>
    private Module FindSafeModule()
    {
        //This method was made with help from AI
        //Checks all modules and makes an array of the safe ones
        Module[] safemodules = System.Array.FindAll(modules, 
            m => m.State == ModuleState.Normal && m.HasSpace);

        //if there is none return null
        if (safemodules.Length == 0) return null;

        return safemodules[Random.Range(0, safemodules.Length)];
    }

    /// <summary>
    /// Verifies if module is in danger
    /// </summary>
    /// <returns></returns>
    private bool IsInDanger()
    {
        if (targetModule == null) return false;

        //only fire and NoOxigen is danger
        return targetModule.State == ModuleState.Fire ||
               targetModule.State == ModuleState.NoOxigen;
    }

    /// <summary>
    /// Checks all modules and returns an array of the Escape ones
    /// </summary>
    /// <returns></returns>
    private Module FindEscapeModule()
    {
        //Checks all modules and makes an array of the Escape ones
        Module[] escapes = System.Array.FindAll(modules,
            m => m.Type == ModuleType.Escape &&
             m.State == ModuleState.Normal);

        //if there is none return null
        if (escapes.Length == 0) return null;

        return escapes[Random.Range(0, escapes.Length)];
    }

    /// <summary>
    /// Orders tripulante to evacuate
    /// </summary>
    private void Evacuate()
    {
        //if there is no target choose exit
        if (targetModule == null)
        {
            targetModule = FindEscapeModule();

            if (targetModule != null)
                agent.SetDestination(targetModule.transform.position);

            return;
        }

        //awaits Navmesh calculations to end
        if (agent.pathPending) return;
        //if already there cancel
        if (agent.remainingDistance > 1f) return;

        //When arriving there Deactivate
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Manages tripulantes health
    /// </summary>
    private void UpdateHealth()
    {
        if (targetModule == null) return;

        //Each module state deals diffrent damage
        switch (targetModule.State)
        {
            case ModuleState.Fire:
                health -= Time.deltaTime * 30f;
                break;
            case ModuleState.NoOxigen:
                health -= Time.deltaTime * 15f;
                break;
        }

        health = Mathf.Clamp(health, 0f, maxHealth);

        if (health <= 0f)
        {
            Die();
        }
    }
    /// <summary>
    /// Kills Tripulante
    /// </summary>
    private void Die()
    {
        Debug.Log("Tripulante morreu");

        //Exits module when dead 
        //to avoid Full rooms with nobody inside
        if (targetModule != null)
        {
            targetModule.Exit(gameObject);
        }
        Destroy(gameObject);
    }
}
