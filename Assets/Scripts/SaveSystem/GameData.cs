using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public double money;
    public int level;
    public int xp;
    public Dictionary<string, int> inventoryItems; 
    public List<BuiltObjectData> builtObjects; 
    public List<EquipmentSaveData> equipmentData; 
}

[System.Serializable]
public class BuiltObjectData
{
    public string objectId; 
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW; 
}

[System.Serializable]
public class EquipmentSaveData
{
    public string equipmentId; 
    public int currentLevel; 
}