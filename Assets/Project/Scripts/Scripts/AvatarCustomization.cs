// AvatarCustomization.cs - VERSIÓN COMPLETA CON PERSISTENCIA DE ROPA
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Text;
using UnityEditor;

namespace Sunbox.Avatars {
    public class AvatarCustomization : MonoBehaviour {
        #region Variables y Enums Originales del Asset
        private const string NAME = "[AvatarCustomization]";
        public enum AvatarGender { Male = 0, Female = 1 }
        public enum OutfitType { Top, Bottom, WholeOutfit, Shoes, Hat, Other }
        public enum HideBlendShapeIndex { Nothing = -1, OutfitHideShirt = 23, OutfitHidePants = 24, OutfitHideShoes = 25, OutfitHideHoodie = 38, OutfitHideShorts = 39, OutfitHideSleeveless = 40, OutfitHideBriefs = 41, OutfitHideDress = 42, OutfitHideSkirtMid = 43, OutfitHideFlats = 44 }
        [Flags] public enum AvatarExpression { None = 0, Anger = 1, Disgust = 2, Fear = 4, Joy = 8, Sadness = 16 }
        public const string SECTION_BODY_PARAMETERS = "Body Parameters";
        public const string SECTION_FACE_PARAMETERS = "Face Parameters";
        public const string SECTION_FEATURE_STYLES = "Features";
        public HideBlendShapeIndex[] HideBlendShapeIndices { get { if (_hideBlendShapeIndices == null) { _hideBlendShapeIndices = (HideBlendShapeIndex[])System.Enum.GetValues(typeof(HideBlendShapeIndex)); } return _hideBlendShapeIndices; } }
        public SkinnedMeshRenderer CurrentGenderSkinnedRenderer { get { if (CurrentGender == AvatarGender.Male) { _currentGenderSkinnedMeshRenderer = MaleBodyGEO; } if (CurrentGender == AvatarGender.Female) { _currentGenderSkinnedMeshRenderer = FemaleBodyGEO; } if(_currentGenderSkinnedMeshRenderer != null) _bodyBones = _currentGenderSkinnedMeshRenderer.bones; return _currentGenderSkinnedMeshRenderer; } }
        public GameObject CurrentBase { get { if (CurrentGender == AvatarGender.Male) { return MaleBase; } return FemaleBase; } }
        public Animator Animator { get { return GetComponent<Animator>(); } }
        public RuntimeAnimatorController AnimatorController { get { return Animator.runtimeAnimatorController; } set { Animator.runtimeAnimatorController = value; } }
        public TextAsset Preset;
        public GameObject MaleBase;
        public GameObject FemaleBase;
        public SkinnedMeshRenderer MaleBodyGEO;
        public SkinnedMeshRenderer FemaleBodyGEO;
        public AvatarReferences AvatarReferences;
        [AvatarGenderFieldAttribute("Gender", "gender")] public AvatarGender CurrentGender = AvatarGender.Male;
        [AvatarFloatFieldAttribute("Body Fat", "bodyFat", -100, 100, Section = SECTION_BODY_PARAMETERS)] public float BodyFat = 0f;
        [AvatarFloatFieldAttribute("Body Muscle", "bodyMuscle", 0, 100, Section = SECTION_BODY_PARAMETERS)] public float BodyMuscle = 0f;
        [AvatarFloatFieldAttribute("Body Height (m)", "bodyHeightMetres", 1.5f, 2.1f, 1.5f, 2.1f, IgnoreInPlayMode = true)] public float BodyHeight = 1.8f;
        [AvatarFloatFieldAttribute("Breast Size (Female only)", "breastSize", -100, 100, Section = SECTION_BODY_PARAMETERS)] public float BreastSize = 0f;
        [AvatarFloatFieldAttribute("Nose Length", "noseLength", -100, 100, Section = SECTION_FACE_PARAMETERS)] public float NoseLength = 0f;
        [AvatarFloatFieldAttribute("Lips Width", "lipsWidth", -100, 100, Section = SECTION_FACE_PARAMETERS)] public float LipsWidth = 0f;
        [AvatarFloatFieldAttribute("Jaw Width", "jawWidth", -100, 100, Section = SECTION_FACE_PARAMETERS)] public float JawWidth = 0f;
        [AvatarFloatFieldAttribute("Brow Width", "browWidth", -100, 100, Section = SECTION_FACE_PARAMETERS)] public float BrowWidth = 0f;
        [AvatarFloatFieldAttribute("Brow Height", "browHeight", -100, 100, Section = SECTION_FACE_PARAMETERS)] public float BrowHeight = 0f;
        [AvatarFloatFieldAttribute("Eyes Size", "eyesSize", -100, 100, Section = SECTION_FACE_PARAMETERS)] public float EyesSize = 0f;
        [AvatarFloatFieldAttribute("Eyes Closed Default", "eyesClosedDefault", 0, 100, Section = SECTION_FACE_PARAMETERS)] public float EyesClosedDefault = 0f;
        [AvatarIntegerField("Skin Material", "skinMaterialIndex", Section = SECTION_FEATURE_STYLES, ArrayDependancyField = "MaleSkinMaterials")] public int SkinMaterialIndex = 0;
        [AvatarIntegerField("Hair Style", "hairStyleIndex", Section = SECTION_FEATURE_STYLES, ArrayDependancyField = "HairItems")] public int HairStyleIndex = 0;
        [AvatarIntegerField("Hair Material", "hairMaterialIndex", Section = SECTION_FEATURE_STYLES, ArrayDependancyField = "HairItems", IsVariationField = true)] public int HairMaterialIndex = 0;
        [AvatarIntegerField("Facial Hair Style", "facialHairStyleIndex", Section = SECTION_FEATURE_STYLES, ArrayDependancyField = "FacialHairItems")] public int FacialHairStyleIndex = 0;
        [AvatarIntegerField("Facial Hair Material", "facialHairMaterialIndex", Section = SECTION_FEATURE_STYLES, ArrayDependancyField = "FacialHairItems", IsVariationField = true)] public int FacialHairMaterialIndex = 0;
        [AvatarIntegerField("Eye Material", "eyeMaterialIndex", Section = SECTION_FEATURE_STYLES, ArrayDependancyField = "EyeMaterials")] public int EyeMaterialIndex = 0;
        [AvatarIntegerField("Lashes Material", "lashesMaterialIndex", Section = SECTION_FEATURE_STYLES, ArrayDependancyField = "LashesMaterials")] public int LashesMaterialIndex = 0;
        [AvatarIntegerField("Brow Material", "browMaterialIndex", Section = SECTION_FEATURE_STYLES, ArrayDependancyField = "BrowMaterials")] public int BrowMaterialIndex = 0;
        [AvatarIntegerField("Nails Material", "nailsMaterialIndex", Section = SECTION_FEATURE_STYLES, ArrayDependancyField = "NailMaterials")] public int NailsMaterialIndex = 0;
        public ClothingItem ClothingItemHat; public int ClothingItemHatVariationIndex;
        public ClothingItem ClothingItemTop; public int ClothingItemTopVariationIndex;
        public ClothingItem ClothingItemBottom; public int ClothingItemBottomVariationIndex;
        public ClothingItem ClothingItemGlasses; public int ClothingItemGlassesVariationIndex;
        public ClothingItem ClothingItemShoes; public int ClothingItemShoesVariationIndex;
        [Range(0, 11.7f)] public float BlinkingInterval = 5f;
        [Range(0, 0.99f)] public float BlinkSpeed = 0.5f;
        public float ExpressionChangeSpeed = 1f;
        private HideBlendShapeIndex[] _hideBlendShapeIndices;
        private Slot[] _avatarSlots;
        private SkinnedMeshRenderer _currentGenderSkinnedMeshRenderer;
        private Transform[] _bodyBones;
        private Dictionary<SlotType, SkinnedMeshRenderer> _activeSkinnedClothingItems = new Dictionary<SlotType, SkinnedMeshRenderer>();
        private Dictionary<SlotType, UClothingItem> _currentInstantiatedClothingItems = new Dictionary<SlotType, UClothingItem>();
        private GameObject _facialHairObject;
        private GameObject _hairGameObject;
        private float _eyesClosed = 0;
        private float _eyesClosedExpression = 0;
        private float _adjustedBodyFat = 0;
        private float _adjustedBodyMuscle = 0;
        private float _blinkTimer = 0;
        #endregion

        #region Integración con Sistema de Juego
     public void ApplyData(AvatarData data)
{
    if (data == null)
    {
        Debug.LogError("Se intentó aplicar datos nulos.");
        return;
    }
    this.CurrentGender = (AvatarGender)data.selectedGender;
    this.BodyFat = data.bodyFat;
    this.BodyMuscle = data.bodyMuscle;
    this.BreastSize = data.breastSize;
    this.SkinMaterialIndex = data.skinColorIndex;
    this.HairStyleIndex = FindHairIndexFromId(data.hairId);


    ApplyClothingFromData(data);
    SetGender(CurrentGender, true);
    UpdateClothing();

    Debug.Log("Avatar reconstruido correctamente con su ropa y forma corporal.");
}
        public void SaveDataAndContinue() 
        { 
            if (GameManager.Instance == null) 
            { 
                Debug.LogError("GameManager no encontrado."); 
                return; 
            } 
            
            // ✅ OBTENER los datos actuales en lugar de crear nuevos
            AvatarData dataToSave = GameManager.Instance.CurrentAvatarData;
            
            // Si no hay datos previos, crear nuevos
            if (dataToSave == null)
            {
                dataToSave = new AvatarData();
                Debug.LogWarning("No había datos previos, creando nuevos AvatarData");
            }
            
            // Actualizar todos los campos
            dataToSave.selectedGender = (Gender)this.CurrentGender; 
            dataToSave.bodyFat = this.BodyFat; 
            dataToSave.bodyMuscle = this.BodyMuscle; 
            dataToSave.breastSize = this.BreastSize; 
            dataToSave.skinColorIndex = this.SkinMaterialIndex; 
            dataToSave.hairId = GetHairIdFromIndex(this.HairStyleIndex); 
            
            // Guardar ropa
            SaveClothingToData(dataToSave);
            
            // Actualizar en GameManager
            GameManager.Instance.SetCurrentAvatarData(dataToSave); 
            
            // Guardar a disco
            GameManager.Instance.SavePlayerData(); 
            
            Debug.Log("========== AVATAR GUARDADO COMPLETO ==========");
            
            // Cambiar de escena
            UnityEngine.SceneManagement.SceneManager.LoadScene("Proyecto_General"); 
        }

        private string GetHairIdFromIndex(int index) 
        { 
            var hairItems = AvatarReferences.HairItems; 
            if (hairItems == null || index < 0 || index >= hairItems.Length) 
            { 
                return null; 
            } 
            return hairItems[index].name; 
        }

        private int FindHairIndexFromId(string hairId) 
        { 
            var hairItems = AvatarReferences.HairItems; 
            if (hairItems == null || string.IsNullOrEmpty(hairId)) 
            { 
                return 0; 
            } 
            for (int i = 0; i < hairItems.Length; i++) 
            { 
                if (hairItems[i] != null && hairItems[i].name == hairId) 
                { 
                    return i; 
                } 
            } 
            return 0; 
        }
        #endregion

        #region Persistencia de Ropa
        /// <summary>
        /// Busca un ClothingItem por nombre en los items disponibles
        /// </summary>
        private ClothingItem FindClothingItemByName(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
            {
                return null;
            }

            if (AvatarReferences.AvailableClothingItems == null)
            {
                Debug.LogError("AvailableClothingItems es null - verifica AvatarReferences en el Inspector");
                return null;
            }

            // Búsqueda exacta primero
            foreach (var item in AvatarReferences.AvailableClothingItems)
            {
                if (item != null && item.Name == itemName)
                {
                    Debug.Log($"✓ ClothingItem encontrado: {itemName}");
                    return item;
                }
            }

            // Si no se encuentra, intentar búsqueda case-insensitive
            foreach (var item in AvatarReferences.AvailableClothingItems)
            {
                if (item != null && item.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning($"⚠ ClothingItem encontrado con diferente capitalización: '{itemName}' -> '{item.Name}'");
                    return item;
                }
            }

            // Log de todos los items disponibles para debugging
            Debug.LogError($"✗ ClothingItem '{itemName}' NO encontrado. Items disponibles:");
            for (int i = 0; i < AvatarReferences.AvailableClothingItems.Length; i++)
            {
                var item = AvatarReferences.AvailableClothingItems[i];
                if (item != null)
                {
                    Debug.Log($"  [{i}] {item.Name} (SlotType: {item.SlotType})");
                }
            }
            
            return null;
        }

        /// <summary>
        /// Aplica la ropa guardada desde AvatarData
        /// </summary>
        private void ApplyClothingFromData(AvatarData data)
        {
            if (data == null)
            {
                Debug.LogWarning("AvatarData es null, no se puede aplicar ropa");
                return;
            }

            Debug.Log($"========== APLICANDO ROPA DESDE DATOS GUARDADOS ==========");

            // Aplicar Hat
            if (!string.IsNullOrEmpty(data.clothingItemHatName))
            {
                ClothingItem hat = FindClothingItemByName(data.clothingItemHatName);
                if (hat != null)
                {
                    ClothingItemHat = hat;
                    ClothingItemHatVariationIndex = Mathf.Clamp(data.clothingItemHatVariation, 0, hat.Variations.Length - 1);
                    Debug.Log($"✓ Hat: {hat.Name} (Var: {ClothingItemHatVariationIndex})");
                }
                else
                {
                    ClothingItemHat = null;
                    Debug.LogWarning($"✗ Hat '{data.clothingItemHatName}' no encontrado - removido");
                }
            }
            else
            {
                ClothingItemHat = null;
                Debug.Log("○ Hat: ninguno");
            }

            // Aplicar Top
            if (!string.IsNullOrEmpty(data.clothingItemTopName))
            {
                ClothingItem top = FindClothingItemByName(data.clothingItemTopName);
                if (top != null)
                {
                    ClothingItemTop = top;
                    ClothingItemTopVariationIndex = Mathf.Clamp(data.clothingItemTopVariation, 0, top.Variations.Length - 1);
                    Debug.Log($"✓ Top: {top.Name} (Var: {ClothingItemTopVariationIndex})");
                }
                else
                {
                    ClothingItemTop = null;
                    Debug.LogWarning($"✗ Top '{data.clothingItemTopName}' no encontrado - removido");
                }
            }
            else
            {
                ClothingItemTop = null;
                Debug.Log("○ Top: ninguno");
            }

            // Aplicar Bottom
            if (!string.IsNullOrEmpty(data.clothingItemBottomName))
            {
                ClothingItem bottom = FindClothingItemByName(data.clothingItemBottomName);
                if (bottom != null)
                {
                    ClothingItemBottom = bottom;
                    ClothingItemBottomVariationIndex = Mathf.Clamp(data.clothingItemBottomVariation, 0, bottom.Variations.Length - 1);
                    Debug.Log($"✓ Bottom: {bottom.Name} (Var: {ClothingItemBottomVariationIndex})");
                }
                else
                {
                    ClothingItemBottom = null;
                    Debug.LogWarning($"✗ Bottom '{data.clothingItemBottomName}' no encontrado - removido");
                }
            }
            else
            {
                ClothingItemBottom = null;
                Debug.Log("○ Bottom: ninguno");
            }

            // Aplicar Shoes
            if (!string.IsNullOrEmpty(data.clothingItemShoesName))
            {
                ClothingItem shoes = FindClothingItemByName(data.clothingItemShoesName);
                if (shoes != null)
                {
                    ClothingItemShoes = shoes;
                    ClothingItemShoesVariationIndex = Mathf.Clamp(data.clothingItemShoesVariation, 0, shoes.Variations.Length - 1);
                    Debug.Log($"✓ Shoes: {shoes.Name} (Var: {ClothingItemShoesVariationIndex})");
                }
                else
                {
                    ClothingItemShoes = null;
                    Debug.LogWarning($"✗ Shoes '{data.clothingItemShoesName}' no encontrado - removido");
                }
            }
            else
            {
                ClothingItemShoes = null;
                Debug.Log("○ Shoes: ninguno");
            }

            // Aplicar Glasses
            if (!string.IsNullOrEmpty(data.clothingItemGlassesName))
            {
                ClothingItem glasses = FindClothingItemByName(data.clothingItemGlassesName);
                if (glasses != null)
                {
                    ClothingItemGlasses = glasses;
                    ClothingItemGlassesVariationIndex = Mathf.Clamp(data.clothingItemGlassesVariation, 0, glasses.Variations.Length - 1);
                    Debug.Log($"✓ Glasses: {glasses.Name} (Var: {ClothingItemGlassesVariationIndex})");
                }
                else
                {
                    ClothingItemGlasses = null;
                    Debug.LogWarning($"✗ Glasses '{data.clothingItemGlassesName}' no encontrado - removido");
                }
            }
            else
            {
                ClothingItemGlasses = null;
                Debug.Log("○ Glasses: ninguno");
            }

            Debug.Log($"========== FIN APLICACIÓN DE ROPA ==========");
        }

        /// <summary>
        /// Guarda la ropa actual en AvatarData
        /// </summary>
        private void SaveClothingToData(AvatarData data)
        {
            if (data == null)
            {
                Debug.LogError("AvatarData es null, no se puede guardar ropa");
                return;
            }

            Debug.Log($"========== GUARDANDO ROPA ACTUAL ==========");

            // Guardar Hat
            data.clothingItemHatName = ClothingItemHat?.Name ?? "";
            data.clothingItemHatVariation = ClothingItemHat != null ? ClothingItemHatVariationIndex : 0;
            Debug.Log($"Hat: '{data.clothingItemHatName}' (Var: {data.clothingItemHatVariation}) - Objeto: {(ClothingItemHat != null ? "✓" : "✗")}");

            // Guardar Top
            data.clothingItemTopName = ClothingItemTop?.Name ?? "";
            data.clothingItemTopVariation = ClothingItemTop != null ? ClothingItemTopVariationIndex : 0;
            Debug.Log($"Top: '{data.clothingItemTopName}' (Var: {data.clothingItemTopVariation}) - Objeto: {(ClothingItemTop != null ? "✓" : "✗")}");

            // Guardar Bottom
            data.clothingItemBottomName = ClothingItemBottom?.Name ?? "";
            data.clothingItemBottomVariation = ClothingItemBottom != null ? ClothingItemBottomVariationIndex : 0;
            Debug.Log($"Bottom: '{data.clothingItemBottomName}' (Var: {data.clothingItemBottomVariation}) - Objeto: {(ClothingItemBottom != null ? "✓" : "✗")}");

            // Guardar Shoes
            data.clothingItemShoesName = ClothingItemShoes?.Name ?? "";
            data.clothingItemShoesVariation = ClothingItemShoes != null ? ClothingItemShoesVariationIndex : 0;
            Debug.Log($"Shoes: '{data.clothingItemShoesName}' (Var: {data.clothingItemShoesVariation}) - Objeto: {(ClothingItemShoes != null ? "✓" : "✗")}");

            // Guardar Glasses
            data.clothingItemGlassesName = ClothingItemGlasses?.Name ?? "";
            data.clothingItemGlassesVariation = ClothingItemGlasses != null ? ClothingItemGlassesVariationIndex : 0;
            Debug.Log($"Glasses: '{data.clothingItemGlassesName}' (Var: {data.clothingItemGlassesVariation}) - Objeto: {(ClothingItemGlasses != null ? "✓" : "✗")}");

            Debug.Log($"========== FIN GUARDADO DE ROPA ==========");
        }
        #endregion

        #region Funciones Originales del Asset
        void Update() { UpdateBlinking_Internal(); }

        public void RandomizeBodyParameters(int seed = -1, bool ignoreHeight = true, bool unifiedHairColors = true) { if (seed != -1) { UnityEngine.Random.InitState(seed); } FieldInfo[] fields = typeof(AvatarCustomization).GetFields(); foreach (FieldInfo field in fields) { AvatarFieldAttribute avatarFieldAttribute = field.GetCustomAttribute<AvatarFieldAttribute>(); if (avatarFieldAttribute == null) { continue; } if (avatarFieldAttribute is AvatarFloatFieldAttribute) { AvatarFloatFieldAttribute floatFieldAttribute = (AvatarFloatFieldAttribute)avatarFieldAttribute; field.SetValue(this, UnityEngine.Random.Range(floatFieldAttribute.SourceMinValue, floatFieldAttribute.SourceMaxValue)); } } EyesClosedDefault = 20; if (ignoreHeight) BodyHeight = 1.8f; AvatarGender gender = UnityEngine.Random.value < 0.5f ? AvatarGender.Male : AvatarGender.Female; MaleBase.SetActive(CurrentGender == AvatarGender.Male); FemaleBase.SetActive(CurrentGender == AvatarGender.Female); if (CurrentGender == AvatarGender.Male) SkinMaterialIndex = UnityEngine.Random.Range(0, AvatarReferences.MaleSkinMaterials.Length); if (CurrentGender == AvatarGender.Female) SkinMaterialIndex = UnityEngine.Random.Range(0, AvatarReferences.FemaleSkinMaterials.Length); HairStyleIndex = UnityEngine.Random.Range(0, AvatarReferences.HairItems.Length); HairMaterialIndex = UnityEngine.Random.Range(0, AvatarReferences.HairItems[HairStyleIndex].Variations.Length); if (CurrentGender == AvatarGender.Male) { FacialHairStyleIndex = UnityEngine.Random.Range(0, AvatarReferences.FacialHairItems.Length); FacialHairMaterialIndex = UnityEngine.Random.Range(0, AvatarReferences.FacialHairItems[FacialHairStyleIndex].Variations.Length); } if (unifiedHairColors) { FacialHairMaterialIndex = HairMaterialIndex; BrowMaterialIndex = HairMaterialIndex; } EyeMaterialIndex = UnityEngine.Random.Range(0, AvatarReferences.EyeMaterials.Length); if (!unifiedHairColors) BrowMaterialIndex = UnityEngine.Random.Range(0, AvatarReferences.BrowMaterials.Length); UpdateCustomization(); }

        public void RandomizeClothing(int seed = -1, bool nudity = false) { if (seed != -1) UnityEngine.Random.InitState(seed); ClothingItemHat = AvatarReferences.AvailableClothingItems.RandomFirst(item => item.SlotType == SlotType.Hat); if (nudity) { ClothingItemTop = AvatarReferences.AvailableClothingItems.RandomFirst(item => item.SlotType == SlotType.Top); ClothingItemBottom = AvatarReferences.AvailableClothingItems.RandomFirst(item => item.SlotType == SlotType.Bottom); } else { ClothingItemTop = AvatarReferences.AvailableClothingItems.RandomFirst(item => item.SlotType == SlotType.Top && !item.IsEmpty); ClothingItemBottom = AvatarReferences.AvailableClothingItems.RandomFirst(item => item.SlotType == SlotType.Bottom && !item.IsEmpty); } ClothingItemGlasses = AvatarReferences.AvailableClothingItems.RandomFirst(item => item.SlotType == SlotType.Glasses); ClothingItemShoes = AvatarReferences.AvailableClothingItems.RandomFirst(item => item.SlotType == SlotType.Shoes); ClothingItemHatVariationIndex = ClothingItemHat == null || ClothingItemHat.Variations.Length <= 1 ? 0 : UnityEngine.Random.Range(0, ClothingItemHat.Variations.Length); ClothingItemTopVariationIndex = ClothingItemTop == null || ClothingItemTop.Variations.Length <= 1 ? 0 : UnityEngine.Random.Range(0, ClothingItemTop.Variations.Length); ClothingItemBottomVariationIndex = ClothingItemBottom == null || ClothingItemBottom.Variations.Length <= 1 ? 0 : UnityEngine.Random.Range(0, ClothingItemBottom.Variations.Length); ClothingItemGlassesVariationIndex = ClothingItemGlasses == null || ClothingItemGlasses.Variations.Length <= 1 ? 0 : UnityEngine.Random.Range(0, ClothingItemGlasses.Variations.Length); ClothingItemShoesVariationIndex = ClothingItemShoes == null || ClothingItemShoes.Variations.Length <= 1 ? 0 : UnityEngine.Random.Range(0, ClothingItemShoes.Variations.Length); UpdateClothing(); }

        public void AttachClothingItem(ClothingItem item, int variationIndex = 0) { if (item.SlotType == SlotType.Hat) { ClothingItemHat = item; ClothingItemHatVariationIndex = variationIndex; } if (item.SlotType == SlotType.Top) { ClothingItemTop = item; ClothingItemTopVariationIndex = variationIndex; } if (item.SlotType == SlotType.Bottom) { ClothingItemBottom = item; ClothingItemBottomVariationIndex = variationIndex; } if (item.SlotType == SlotType.Glasses) { ClothingItemGlasses = item; ClothingItemGlassesVariationIndex = variationIndex; } if (item.SlotType == SlotType.Shoes) { ClothingItemShoes = item; ClothingItemShoesVariationIndex = variationIndex; } UpdateClothing(); }

        public void SetClothingItemVariation(SlotType slotType, int value) { if (slotType == SlotType.Hat) ClothingItemHatVariationIndex = value; if (slotType == SlotType.Top) ClothingItemTopVariationIndex = value; if (slotType == SlotType.Bottom) ClothingItemBottomVariationIndex = value; if (slotType == SlotType.Glasses) ClothingItemGlassesVariationIndex = value; if (slotType == SlotType.Shoes) ClothingItemShoesVariationIndex = value; UpdateClothing(); }

        public ClothingItem GetClothingItemFromSlot(SlotType slotType) { if (slotType == SlotType.Hat) return ClothingItemHat; if (slotType == SlotType.Top) return ClothingItemTop; if (slotType == SlotType.Bottom) return ClothingItemBottom; if (slotType == SlotType.Glasses) return ClothingItemGlasses; if (slotType == SlotType.Shoes) return ClothingItemShoes; return null; }

        public int GetClothingItemVariationIndex(SlotType slotType) { if (slotType == SlotType.Hat) return ClothingItemHatVariationIndex; if (slotType == SlotType.Top) return ClothingItemTopVariationIndex; if (slotType == SlotType.Bottom) return ClothingItemBottomVariationIndex; if (slotType == SlotType.Glasses) return ClothingItemGlassesVariationIndex; if (slotType == SlotType.Shoes) return ClothingItemShoesVariationIndex; return 0; }

        public void SetGender(AvatarGender gender, bool force = false) { if (CurrentGender != gender || force) { CurrentGender = gender; _currentGenderSkinnedMeshRenderer = null; MaleBase.SetActive(CurrentGender == AvatarGender.Male); FemaleBase.SetActive(CurrentGender == AvatarGender.Female); UpdateCustomization(); UpdateClothing(); } }

        public void UpdateCustomization() { UpdateHeight_Internal(); UpdateBodyParameters_Internal(); UpdateFace_Internal(); UpdateFacialHair_Shape_Internal(); UpdateHair_Internal(); UpdateFacialHair_Internal(); UpdateMaterials_Internal(); UpdateBlinking_Internal(); }

        public void UpdateClothing() { UpdateClothing_Internal(); }

        public static string ToConfigString(AvatarCustomization instance) { const string CLOTHING_ITEM_KEY = "clothingItem"; StringBuilder sb = new StringBuilder(); FieldInfo[] fields = typeof(AvatarCustomization).GetFields(); foreach (FieldInfo fieldInfo in fields) { AvatarFieldAttribute fieldAttribute = fieldInfo.GetCustomAttribute<AvatarFieldAttribute>(); if (fieldAttribute != null) sb.AppendLine($"{fieldAttribute.Prefix}{fieldAttribute.VariableName}={fieldAttribute.GetValueString(fieldInfo, instance)}"); } if (instance.ClothingItemHat != null) sb.AppendLine($"{CLOTHING_ITEM_KEY}={instance.ClothingItemHat.Name}-{instance.ClothingItemHatVariationIndex}"); if (instance.ClothingItemTop != null) sb.AppendLine($"{CLOTHING_ITEM_KEY}={instance.ClothingItemTop.Name}-{instance.ClothingItemTopVariationIndex}"); if (instance.ClothingItemBottom != null) sb.AppendLine($"{CLOTHING_ITEM_KEY}={instance.ClothingItemBottom.Name}-{instance.ClothingItemBottomVariationIndex}"); if (instance.ClothingItemGlasses != null) sb.AppendLine($"{CLOTHING_ITEM_KEY}={instance.ClothingItemGlasses.Name}-{instance.ClothingItemGlassesVariationIndex}"); if (instance.ClothingItemShoes != null) sb.AppendLine($"{CLOTHING_ITEM_KEY}={instance.ClothingItemShoes.Name}-{instance.ClothingItemShoesVariationIndex}"); return sb.ToString(); }

        private void SetBlendShapes_Internal(float amount, int min, int max, SkinnedMeshRenderer body) { if (amount >= 0) { body.SetBlendShapeWeight(max, amount); body.SetBlendShapeWeight(min, 0); } else { body.SetBlendShapeWeight(min, -amount); body.SetBlendShapeWeight(max, 0); } }

        private void UpdateHair_Internal() { UHair[] existingHairs = CurrentBase.GetComponentsInChildren<UHair>(true); if (AvatarReferences.HairItems == null || AvatarReferences.HairItems.Length == 0) { foreach (UHair hair in existingHairs) SafeDestroyGameObject_Internal(hair.gameObject); return; } int index = Mathf.Clamp(HairStyleIndex, 0, AvatarReferences.HairItems.Length - 1); int materialIndex = AvatarReferences.HairItems[index].HasVariations() ? Mathf.Clamp(HairMaterialIndex, 0, AvatarReferences.HairItems[index].Variations.Length - 1) : 0; _hairGameObject = null; UHair hairInstance = null; foreach (UHair hair in existingHairs) { if (hair.HairItem == AvatarReferences.HairItems[index] && _hairGameObject == null) { _hairGameObject = hair.gameObject; continue; } SafeDestroyGameObject_Internal(hair.gameObject); } if (_hairGameObject == null) { _hairGameObject = Instantiate(AvatarReferences.HairItems[index].HairMesh).gameObject; _hairGameObject.transform.parent = CurrentBase.transform; hairInstance = _hairGameObject.AddComponent<UHair>(); hairInstance.HairItem = AvatarReferences.HairItems[index]; _avatarSlots = CurrentGenderSkinnedRenderer.transform.parent.GetComponentsInChildren<Slot>(); Slot slot = _avatarSlots.FirstOrDefault(s => s.SlotType == SlotType.Hair); _hairGameObject.transform.position = slot.BoneTransform.position; _hairGameObject.transform.rotation = slot.BoneTransform.rotation; _hairGameObject.transform.localScale = Vector3.one; _hairGameObject.transform.parent = slot.BoneTransform; } if (_hairGameObject != null) { hairInstance = _hairGameObject.GetComponent<UHair>(); MeshRenderer renderer = _hairGameObject.GetComponent<MeshRenderer>(); renderer.sharedMaterial = AvatarReferences.HairItems[index].HasVariations() ? AvatarReferences.HairItems[index].Variations[materialIndex] : null; } UClothingItem hatClothingInstance = CurrentBase.GetComponentsInChildren<UClothingItem>().FirstOrDefault(item => item.ClothingItem.SlotType == SlotType.Hat); if (hatClothingInstance != null && !hatClothingInstance.ClothingItem.IsEmpty && hairInstance.HairItem.HideHairWhenHatEquipped) { _hairGameObject.SetActive(hatClothingInstance == null); } if (hatClothingInstance != null) { hatClothingInstance.transform.localPosition = AvatarReferences.HairItems[index].HatOffset; } }

        private void UpdateFacialHair_Internal() { if (CurrentGender != AvatarGender.Male) { UFacialHair[] existingFacialHair = CurrentBase.GetComponentsInChildren<UFacialHair>(); foreach (UFacialHair facialHair in existingFacialHair) SafeDestroyGameObject_Internal(facialHair.gameObject); _facialHairObject = null; return; } UFacialHair[] existingFacialHairs = CurrentBase.GetComponentsInChildren<UFacialHair>(); if (AvatarReferences.FacialHairItems == null || AvatarReferences.FacialHairItems.Length == 0) { foreach (UFacialHair facialHair in existingFacialHairs) SafeDestroyGameObject_Internal(facialHair.gameObject); _facialHairObject = null; return; } int index = Mathf.Clamp(FacialHairStyleIndex, 0, AvatarReferences.FacialHairItems.Length - 1); if (AvatarReferences.FacialHairItems[index] == null || AvatarReferences.FacialHairItems[index].FacialHairmesh == null) return; int materialIndex = AvatarReferences.FacialHairItems[index].HasVariations() ? Mathf.Clamp(FacialHairMaterialIndex, 0, AvatarReferences.FacialHairItems[index].Variations.Length - 1) : 0; _facialHairObject = null; foreach (UFacialHair facialHair in existingFacialHairs) { if (facialHair.FacialHairItem == AvatarReferences.FacialHairItems[index]) { _facialHairObject = facialHair.gameObject; continue; } SafeDestroyGameObject_Internal(facialHair.gameObject); } if (_facialHairObject == null) { GameObject facialHairMeshObject = AvatarReferences.FacialHairItems[index].FacialHairmesh.gameObject; _facialHairObject = Instantiate(facialHairMeshObject); _facialHairObject.transform.SetParent(CurrentBase.transform, false); _facialHairObject.transform.localPosition = Vector3.zero; _facialHairObject.transform.localRotation = Quaternion.identity; _facialHairObject.transform.localScale = Vector3.one; UFacialHair facialHairComponent = _facialHairObject.GetComponent<UFacialHair>() ?? _facialHairObject.AddComponent<UFacialHair>(); facialHairComponent.FacialHairItem = AvatarReferences.FacialHairItems[index]; SkinnedMeshRenderer renderer = _facialHairObject.GetComponent<SkinnedMeshRenderer>(); if (renderer == null) { SafeDestroyGameObject_Internal(_facialHairObject); _facialHairObject = null; return; } renderer.bones = _bodyBones; renderer.rootBone = CurrentGenderSkinnedRenderer.rootBone; if (AvatarReferences.FacialHairItems[index].HasVariations()) renderer.sharedMaterial = AvatarReferences.FacialHairItems[index].Variations[materialIndex]; } if (_facialHairObject != null) { SkinnedMeshRenderer renderer = _facialHairObject.GetComponent<SkinnedMeshRenderer>(); if (renderer != null && AvatarReferences.FacialHairItems[index].HasVariations()) renderer.sharedMaterial = AvatarReferences.FacialHairItems[index].Variations[materialIndex]; if (!_facialHairObject.activeSelf) _facialHairObject.SetActive(true); } UpdateFacialHair_Shape_Internal(); }

        private void UpdateFacialHair_Shape_Internal() { if (_facialHairObject) { SkinnedMeshRenderer skinnedMeshRender = _facialHairObject.GetComponent<SkinnedMeshRenderer>(); SetBlendShapes_Internal(NoseLength, Constants.NOSELENGTH_MIN, Constants.NOSELENGTH_MAX, skinnedMeshRender); SetBlendShapes_Internal(LipsWidth, Constants.LIPSWIDTH_MIN, Constants.LIPSWIDTH_MAX, skinnedMeshRender); SetBlendShapes_Internal(JawWidth, Constants.JAWWIDTH_MIN, Constants.JAWWIDTH_MAX, skinnedMeshRender); SetBlendShapes_Internal(EyesSize, Constants.EYESIZE_MIN, Constants.EYESIZE_MAX, skinnedMeshRender); if (BodyMuscle + Mathf.Abs(BodyFat) <= 100) { SetBlendShapes_Internal(BodyFat, Constants.BODY_SKINNY, Constants.BODY_CHUBBY, skinnedMeshRender); skinnedMeshRender.SetBlendShapeWeight(Constants.BODY_MUSCLE, BodyMuscle); } else { _adjustedBodyFat = BodyFat * 100 / (Mathf.Abs(BodyFat) + BodyMuscle); _adjustedBodyMuscle = BodyMuscle * 100 / (Mathf.Abs(BodyFat) + BodyMuscle); SetBlendShapes_Internal(_adjustedBodyFat, Constants.BODY_SKINNY, Constants.BODY_CHUBBY, skinnedMeshRender); skinnedMeshRender.SetBlendShapeWeight(Constants.BODY_MUSCLE, _adjustedBodyMuscle); } } }

        private void UpdateBlinking_Internal() { if (_blinkTimer < 12 - BlinkingInterval + (1f - BlinkSpeed)) _blinkTimer += Time.deltaTime; else _blinkTimer = 0; if (_blinkTimer > 12 - BlinkingInterval) { float closedAmount = 1 - 2 * Mathf.Abs((_blinkTimer - (12 - BlinkingInterval)) / ((1f - BlinkSpeed)) - 0.5f); CurrentGenderSkinnedRenderer.SetBlendShapeWeight(Constants.EYES_BLINK, Mathf.Lerp(_eyesClosed, 100, closedAmount)); } else { CurrentGenderSkinnedRenderer.SetBlendShapeWeight(Constants.EYES_BLINK, _eyesClosed); } }

        private void UpdateFace_Internal() { SetBlendShapes_Internal(NoseLength, Constants.NOSELENGTH_MIN, Constants.NOSELENGTH_MAX, CurrentGenderSkinnedRenderer); SetBlendShapes_Internal(LipsWidth, Constants.LIPSWIDTH_MIN, Constants.LIPSWIDTH_MAX, CurrentGenderSkinnedRenderer); SetBlendShapes_Internal(BrowWidth, Constants.BROWWIDTH_MIN, Constants.BROWWIDTH_MAX, CurrentGenderSkinnedRenderer); SetBlendShapes_Internal(BrowHeight, Constants.BROWHEIGHT_MIN, Constants.BROWHEIGHT_MAX, CurrentGenderSkinnedRenderer); SetBlendShapes_Internal(EyesSize, Constants.EYESIZE_MIN, Constants.EYESIZE_MAX, CurrentGenderSkinnedRenderer); SetBlendShapes_Internal(JawWidth, Constants.JAWWIDTH_MIN, Constants.JAWWIDTH_MAX, CurrentGenderSkinnedRenderer); _eyesClosed = EyesClosedDefault * 0.7f + 15 + _eyesClosedExpression; UpdateFacialHair_Shape_Internal(); }

        private void UpdateMaterials_Internal() { Material[] sharedMaterials = CurrentGenderSkinnedRenderer.sharedMaterials; sharedMaterials[Constants.SKIN_MATERIAL_SLOT] = CurrentGender == AvatarGender.Male ? AvatarReferences.MaleSkinMaterials[SkinMaterialIndex] : AvatarReferences.FemaleSkinMaterials[SkinMaterialIndex]; sharedMaterials[Constants.LASHES_MATERIAL_SLOT] = AvatarReferences.LashesMaterials[LashesMaterialIndex]; sharedMaterials[Constants.NAILS_MATERIAL_SLOT] = AvatarReferences.NailMaterials[NailsMaterialIndex]; sharedMaterials[Constants.EYE_MATERIAL_SLOT] = AvatarReferences.EyeMaterials[EyeMaterialIndex]; sharedMaterials[Constants.BROWS_MATERIAL_SLOT] = AvatarReferences.BrowMaterials[BrowMaterialIndex]; CurrentGenderSkinnedRenderer.sharedMaterials = sharedMaterials; }

        private void UpdateHeight_Internal() { CurrentBase.transform.localScale = Vector3.one * GetBodyHeightScale_Internal(); }

        private void UpdateBodyParameters_Internal() { SetBlendShapes_Internal(BreastSize, Constants.BREASTSIZE_MIN, Constants.BREASTSIZE_MAX, CurrentGenderSkinnedRenderer); if (BodyMuscle + Mathf.Abs(BodyFat) <= 100) { SetBlendShapes_Internal(BodyFat, Constants.BODY_SKINNY, Constants.BODY_CHUBBY, CurrentGenderSkinnedRenderer); CurrentGenderSkinnedRenderer.SetBlendShapeWeight(Constants.BODY_MUSCLE, BodyMuscle); foreach (SkinnedMeshRenderer clothingPiece in _activeSkinnedClothingItems.Values) { SetBlendShapes_Internal(BodyFat, Constants.BODY_SKINNY, Constants.BODY_CHUBBY, clothingPiece); clothingPiece.SetBlendShapeWeight(Constants.BODY_MUSCLE, BodyMuscle); SetBlendShapes_Internal(BreastSize, Constants.BREASTSIZE_MIN, Constants.BREASTSIZE_MAX, clothingPiece); } } else { _adjustedBodyFat = BodyFat * 100 / (Mathf.Abs(BodyFat) + BodyMuscle); _adjustedBodyMuscle = BodyMuscle * 100 / (Mathf.Abs(BodyFat) + BodyMuscle); SetBlendShapes_Internal(_adjustedBodyFat, Constants.BODY_SKINNY, Constants.BODY_CHUBBY, CurrentGenderSkinnedRenderer); CurrentGenderSkinnedRenderer.SetBlendShapeWeight(Constants.BODY_MUSCLE, _adjustedBodyMuscle); foreach (SkinnedMeshRenderer clothingPiece in _activeSkinnedClothingItems.Values) { SetBlendShapes_Internal(_adjustedBodyFat, Constants.BODY_SKINNY, Constants.BODY_CHUBBY, clothingPiece); clothingPiece.SetBlendShapeWeight(Constants.BODY_MUSCLE, _adjustedBodyMuscle); SetBlendShapes_Internal(BreastSize, Constants.BREASTSIZE_MIN, Constants.BREASTSIZE_MAX, clothingPiece); } } }

        private float GetBodyHeightScale_Internal() { const float DEFAULT_HEIGHT = 1.8f; return BodyHeight / DEFAULT_HEIGHT; }

        private void FindClothingItemInstances_Internal() { _activeSkinnedClothingItems.Clear(); UClothingItem[] instanciatedClothingItems = CurrentBase.GetComponentsInChildren<UClothingItem>(); foreach (UClothingItem item in instanciatedClothingItems) item.IsEquipped = false; _avatarSlots = CurrentGenderSkinnedRenderer.transform.parent.GetComponentsInChildren<Slot>(); ResolveClothingItemHideSlots_Internal(ClothingItemHat); ResolveClothingItemHideSlots_Internal(ClothingItemTop); ResolveClothingItemHideSlots_Internal(ClothingItemBottom); ResolveClothingItemHideSlots_Internal(ClothingItemShoes); ResolveClothingItemHideSlots_Internal(ClothingItemGlasses); ResolveClothingItemSlot_Internal(instanciatedClothingItems, ClothingItemHat, SlotType.Hat, ClothingItemHatVariationIndex); ResolveClothingItemSlot_Internal(instanciatedClothingItems, ClothingItemTop, SlotType.Top, ClothingItemTopVariationIndex); ResolveClothingItemSlot_Internal(instanciatedClothingItems, ClothingItemBottom, SlotType.Bottom, ClothingItemBottomVariationIndex); ResolveClothingItemSlot_Internal(instanciatedClothingItems, ClothingItemShoes, SlotType.Shoes, ClothingItemShoesVariationIndex); ResolveClothingItemSlot_Internal(instanciatedClothingItems, ClothingItemGlasses, SlotType.Glasses, ClothingItemGlassesVariationIndex); foreach (UClothingItem item in CurrentBase.GetComponentsInChildren<UClothingItem>().Where(i => !i.IsEquipped)) SafeDestroyGameObject_Internal(item.gameObject); }

        private void ResolveClothingItemHideSlots_Internal(ClothingItem clothingItem) { if (clothingItem == null || clothingItem.HideSlots == null || clothingItem.HideSlots.Length == 0) return; if (clothingItem.HideSlots.Contains(SlotType.Bottom)) ClothingItemBottom = null; if (clothingItem.HideSlots.Contains(SlotType.Glasses)) ClothingItemGlasses = null; if (clothingItem.HideSlots.Contains(SlotType.Top)) ClothingItemTop = null; if (clothingItem.HideSlots.Contains(SlotType.Shoes)) ClothingItemShoes = null; if (clothingItem.HideSlots.Contains(SlotType.Hat)) ClothingItemHat = null; }

        private void ResolveClothingItemSlot_Internal(UClothingItem[] clothingInstances, ClothingItem clothingItem, SlotType slotType, int clothingItemIndex) { UClothingItem instance = clothingInstances.FirstOrDefault(item => item.ClothingItem == clothingItem); if (clothingItem == null) { if (instance != null) { SafeDestroyGameObject_Internal(instance.gameObject); if (_activeSkinnedClothingItems.ContainsKey(slotType)) _activeSkinnedClothingItems.Remove(slotType); } return; } if (clothingItem.SlotType != slotType) { clothingItem = null; return; } Slot slot = _avatarSlots.FirstOrDefault(s => s.SlotType == slotType); if (instance == null) { Mesh mesh = CurrentGender == AvatarGender.Male ? clothingItem.MaleMesh : clothingItem.FemaleMesh; GameObject instantiatedClothingItem = new GameObject($"{clothingItem.name}"); instance = instantiatedClothingItem.AddComponent<UClothingItem>(); MeshFilter meshFilter = instantiatedClothingItem.AddComponent<MeshFilter>(); instance.ClothingItem = clothingItem; instantiatedClothingItem.transform.parent = transform; meshFilter.mesh = mesh; if (slot.AttachmentType == AttachmentType.ParentToBone) { instantiatedClothingItem.transform.parent = slot.BoneTransform; instantiatedClothingItem.transform.position = slot.BoneTransform.position; instantiatedClothingItem.transform.rotation = slot.BoneTransform.rotation; instantiatedClothingItem.AddComponent<MeshRenderer>(); } if (slot.AttachmentType == AttachmentType.SkinnedToArmature) { instantiatedClothingItem.transform.parent = slot.BoneTransform; SkinnedMeshRenderer skinnedMeshRenderer = instantiatedClothingItem.AddComponent<SkinnedMeshRenderer>(); skinnedMeshRenderer.sharedMesh = mesh; skinnedMeshRenderer.bones = _bodyBones; _activeSkinnedClothingItems.Add(slot.SlotType, skinnedMeshRenderer); } } if (slot.AttachmentType == AttachmentType.ParentToBone) { Vector3 offset = Vector3.zero; if (_hairGameObject != null && slotType == SlotType.Hat) { UHair hairInstance = _hairGameObject.GetComponent<UHair>(); offset = hairInstance.HairItem.HatOffset; if (clothingItem.IsEmpty) hairInstance.gameObject.SetActive(true); else hairInstance.gameObject.SetActive(!hairInstance.HairItem.HideHairWhenHatEquipped); } instance.transform.localPosition = offset; } if (slot.AttachmentType == AttachmentType.SkinnedToArmature) { if (!_activeSkinnedClothingItems.ContainsKey(slotType)) _activeSkinnedClothingItems.Add(slotType, instance.GetComponent<SkinnedMeshRenderer>()); else _activeSkinnedClothingItems[slotType] = instance.GetComponent<SkinnedMeshRenderer>(); } instance.SetVariation(clothingItemIndex); if (clothingItem.HideShapeWeightIndex != HideBlendShapeIndex.Nothing) CurrentGenderSkinnedRenderer.SetBlendShapeWeight((int)clothingItem.HideShapeWeightIndex, 100); if (clothingItem.HideShapeWeightIndexSecondary != HideBlendShapeIndex.Nothing) CurrentGenderSkinnedRenderer.SetBlendShapeWeight((int)clothingItem.HideShapeWeightIndexSecondary, 100); instance.IsEquipped = true; }

        private void UpdateClothing_Internal() { ResetCostumization_Internal(); FindClothingItemInstances_Internal(); UpdateBodyParameters_Internal(); }

        private void ResetCostumization_Internal() { HideBlendShapeIndex[] indices = (HideBlendShapeIndex[])System.Enum.GetValues(typeof(HideBlendShapeIndex)); foreach (HideBlendShapeIndex index in indices) { if (index != HideBlendShapeIndex.Nothing) CurrentGenderSkinnedRenderer.SetBlendShapeWeight((int)index, 0); } }

        private void SafeDestroyGameObject_Internal(GameObject gameObject) { 
            #if UNITY_EDITOR
            if (Application.isPlaying == false) { 
                DestroyImmediate(gameObject); 
                return; 
            }
            #endif
            Destroy(gameObject); 
        }

        void OnApplicationQuit() { ResetCostumization_Internal(); }
        #endregion
    }

    #region Clases de Atributos Originales
    [AttributeUsage(AttributeTargets.Field)] public class AvatarFloatFieldAttribute : AvatarFieldAttribute { public float SourceMinValue, SourceMaxValue, DisplayMinValue, DisplayMaxValue; public AvatarFloatFieldAttribute(string displayName, string variableName, float sourceMinValue, float sourceMaxValue, float displayMinValue = 0, float displayMaxValue = 1) : base(displayName, variableName, "f_") { SourceMinValue = sourceMinValue; SourceMaxValue = sourceMaxValue; DisplayMinValue = displayMinValue; DisplayMaxValue = displayMaxValue; } }
    [AttributeUsage(AttributeTargets.Field)] public class AvatarIntegerFieldAttribute : AvatarFieldAttribute { public string ArrayDependancyField = null; public bool IsVariationField = false; public AvatarIntegerFieldAttribute(string displayName, string variableName) : base(displayName, variableName, "i_") { } public bool HasArrayDependancy() => ArrayDependancyField != null; }
    [AttributeUsage(AttributeTargets.Field)] public class AvatarColorFieldAttribute : AvatarFieldAttribute { public AvatarColorFieldAttribute(string displayName, string variableName) : base(displayName, variableName, "c_") { } }
    [AttributeUsage(AttributeTargets.Field)] public class AvatarGenderFieldAttribute : AvatarFieldAttribute { public AvatarGenderFieldAttribute(string displayName, string variableName) : base(displayName, variableName, "g_") { } }
    [AttributeUsage(AttributeTargets.Field)] public class AvatarFieldAttribute : Attribute { public string Section = "None"; public string DisplayName, VariableName, Prefix; public bool IgnoreInPlayMode; public AvatarFieldAttribute(string displayName, string variableName, string prefix) { DisplayName = displayName; VariableName = variableName; Prefix = prefix; } public virtual string GetValueString(FieldInfo fieldInfo, object instance) => fieldInfo.GetValue(instance).ToString(); }
    [AttributeUsage(AttributeTargets.Field)] public class AvatarTitleAttribute : Attribute { public string Title { get; } public AvatarTitleAttribute(string title) { Title = title; } }
    #endregion
}