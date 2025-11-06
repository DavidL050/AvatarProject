// AvatarData.cs
using System; // Necesario para [Serializable]

// Definimos un tipo de dato específico para el género. Es más limpio y seguro que usar un string o un int.
public enum Gender { Male, Female }

// [Serializable] es la clave para que Unity pueda convertir esta clase a JSON para guardarla.
[Serializable]
public class AvatarData
{
    public Gender selectedGender; // Para saber qué modelo cargar en la siguiente escena

    // --- VARIABLES QUE FALTABAN ---
    public int skinColorIndex; // ¡AÑADIDA! Para guardar el color de piel
    public string hairId;      // ¡AÑADIDA! Para guardar qué peinado se eligió
    public string shirtId;     // ¡AÑADIDA! Para guardar la camisa

    // --- Blend Shapes ---
    // Añade una variable public float por cada slider que tengas.
    public float bodyFat;
    public float bodyMuscle;
    public float breastSize; // Este valor solo se usará si el género es Female

    // --- CONSTRUCTOR POR DEFECTO ---
    // Se usa para crear un avatar la primera vez que juega un usuario.
    public AvatarData()
    {
        selectedGender = Gender.Female;
        skinColorIndex = 0;
        hairId = "DefaultHair"; // Puedes cambiar estos valores por defecto
        shirtId = "DefaultShirt";
        bodyFat = 0.5f;
        bodyMuscle = 0.3f;
        breastSize = 0.5f;
    }
}