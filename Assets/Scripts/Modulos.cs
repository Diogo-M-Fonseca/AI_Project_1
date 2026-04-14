using UnityEngine;

//enum of all module types stored in the same scritp as the module class, recomendation from colleague
public enum ModuleType 
{
    Habitat,
    Laboratory,
    Storage,
    Technical
}

public class Modulos : MonoBehaviour
{
    [SerializeField] private ModuleType type;
    public ModuleType Type { get { return type; } }

}
