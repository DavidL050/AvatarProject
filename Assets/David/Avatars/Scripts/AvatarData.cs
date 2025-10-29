// AvatarData.cs

using System; // Necesario para [Serializable]

// Definimos un tipo de dato específico para el género. Es más limpio y seguro que usar un string o un int.
public enum Gender { Male, Female }

// [Serializable] es la clave para que Unity pueda convertir esta clase a JSON para guardarla.
[Serializable]
public class AvatarData
{
    public Gender selectedGender; // Para saber qué modelo cargar en la siguiente escena

    // --- Blend Shapes ---
    // Añade una variable public float por cada slider que tengas.
    public float bodyFat;
    public float bodyMuscle;
    public float breastSize; // Este valor solo se usará si el género es Female
    // public float noseWidth; // Ejemplo de cómo añadir más
    // public float jawHeight; // Ejemplo de cómo añadir más
}