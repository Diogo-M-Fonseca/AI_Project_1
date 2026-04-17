using UnityEngine;

public class Incident 
{
    private IncidentType type;

    //origin of incident
    private Module origin;

    //time since the start of the incident
    private float timer;

    //public getters
    public IncidentType Type { get { return type; } }
    public float Timer { get { return timer; } }
    public Module Origin { get { return origin; } }

    public Incident(IncidentType type, Module origin)
    {
        //inicializes incident information
        this.type = type;
        this.origin = origin;
        this.timer = 0;
    }

    public void Tick()
    {
        //increases incident duration over time
        timer += Time.deltaTime;
    }

}
