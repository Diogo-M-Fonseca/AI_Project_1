using UnityEngine;

public class IncidentManager : MonoBehaviour
{
    private static IncidentManager instance;

    private List<Incident> incidents = new List<Incident>();

    private float spawnTimer;

    private void Awake () 
    { 
        instance = this; 
    }

}
