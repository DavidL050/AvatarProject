using System;

public enum Gender { Male, Female }

[Serializable]
public class AvatarData
{
    // Datos básicos
    public Gender selectedGender;
    public int skinColorIndex;
    
    // Estilos y colores
    public int hairStyleIndex;     
    public int hairColorIndex;
    public int beardStyleIndex;    
    public int beardColorIndex;
    public int browColorIndex;     // ← NUEVO: Color de cejas

    // Blend Shapes
    public float bodyFat;
    public float bodyMuscle;
    public float breastSize;
    
    // Ropa
    public string clothingItemHatName;
    public int clothingItemHatVariation;
    public string clothingItemTopName;
    public int clothingItemTopVariation;
    public string clothingItemBottomName;
    public int clothingItemBottomVariation;
    public string clothingItemShoesName;
    public int clothingItemShoesVariation;
    public string clothingItemGlassesName;
    public int clothingItemGlassesVariation;
    
    public AvatarData()
    {
        selectedGender = Gender.Female;
        skinColorIndex = 0;
        
        hairStyleIndex = 0;
        hairColorIndex = 0;
        beardStyleIndex = 0;
        beardColorIndex = 0;
        browColorIndex = 0;  // ← NUEVO: Inicializar
        
        bodyFat = 0.5f;
        bodyMuscle = 0.3f;
        breastSize = 0.5f;
        
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
