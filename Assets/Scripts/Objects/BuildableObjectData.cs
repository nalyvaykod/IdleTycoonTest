using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildableObjectData", menuName = "Building/Buildable Object Data")]
public class BuildableObjectData : ScriptableObject
{
    public string objectName;
    public double cost;
    public GameObject prefab;
    [TextArea(3, 5)]
    public string description;
}