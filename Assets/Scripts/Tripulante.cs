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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        modulos = FindObjectsofType<modulos>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
