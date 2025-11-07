// AvatarData.cs
using System;

public enum Gender { Male, Female }

[Serializable]
public class AvatarData
{
    // Datos básicos
    public Gender selectedGender;
    public int skinColorIndex;
    public string hairId;
    
    // Blend Shapes
    public float bodyFat;
    public float bodyMuscle;
    public float breastSize;
    
    // --- ROPA COMPLETA (NUEVO) ---
    // Hat (Gorra/Sombrero)
    public string clothingItemHatName;
    public int clothingItemHatVariation;
    
    // Top (Camisa/Camiseta/Parte Superior)
    public string clothingItemTopName;
    public int clothingItemTopVariation;
    
    // Bottom (Pantalones/Falda)
    public string clothingItemBottomName;
    public int clothingItemBottomVariation;
    
    // Shoes (Zapatos)
    public string clothingItemShoesName;
    public int clothingItemShoesVariation;
    
    // Glasses (Gafas)
    public string clothingItemGlassesName;
    public int clothingItemGlassesVariation;
    
    // --- CONSTRUCTOR POR DEFECTO ---
    public AvatarData()
    {
        selectedGender = Gender.Female;
        skinColorIndex = 0;
        hairId = "DefaultHair";
        
        bodyFat = 0.5f;
        bodyMuscle = 0.3f;
        breastSize = 0.5f;
        
        // Valores por defecto para ropa (vacíos = sin ropa)
        clothingItemHatName = "";
        clothingItemHatVariation = 0;
        
        clothingItemTopName = "";
        clothingItemTopVariation = 0;
        
        clothingItemBottomName = "";
        clothingItemBottomVariation = 0;
        
        clothingItemShoesName = "";
        clothingItemShoesVariation = 0;
        
        clothingItemGlassesName = "";
        clothingItemGlassesVariation = 0;
    }
}