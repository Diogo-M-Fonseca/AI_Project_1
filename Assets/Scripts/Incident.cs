using UnityEngine;

public class Incident 
{
    private IncidentType type;
    private Module origin;
    private float timer;

    public IncidentType Type { get { return type; } }
    public float Timer { get { return timer; } }
    public Module Origin { get { return origin; } }

    public Incident(IncidentType type, Module origin)
    {
        this.type = type;
        this.origin = origin;
        this.timer = 0;
    }

    public void Tick()
    {
        timer += Time.deltaTime;
    }

}
