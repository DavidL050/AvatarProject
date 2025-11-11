

using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro; 

namespace Sunbox.Avatars
{
    public class UUIAvatarEditor : MonoBehaviour
    {
       
        const string CONTENT_STRING = " (Content)";
        const string CONTENT_STRING_HIDE = " (Content Hide)";

        public AvatarCustomization Avatar;

       
        public Camera Camera; 
        public Button RandomizeButton;

        [Space(20)]
        public Button MaleButton;
        public Button FemaleButton;

        [Space(20)]
        public Button BodyButton;
        public Button FaceButton;
        public Button FeaturesButton;
        public Button ClothingButton;

        [Space(20)]
        public GameObject SliderTemplate;
        public GameObject TitleTemplate;
        public GameObject ClothingItemDropdownTemplate;

        [Space(20)]
        public RectTransform ContentTransform;

        
        private Vector3 _cameraPosition;
        private Vector3 _cameraRotation;
        private Vector3 _zoomedPosition = new Vector3(-0.331f, 1.566f, 0.804f);
        private Vector3 _zoomedRotation = new Vector3(0, 172.5f, 0);
        private bool _isZoomed = false;

        private ClothingDropdownWrapper _hatClothingDropdown;
        private ClothingDropdownWrapper _topClothingDropdown;
        private ClothingDropdownWrapper _bottomClothingDropdown;
        private ClothingDropdownWrapper _shoesClothingDropdown;
        private ClothingDropdownWrapper _glassesClothingDropdown;

        private UClothingItem _hiddenGlasses;
        private List<SliderWrapper> _sliders = new List<SliderWrapper>();

       
        void Start() {
            
            _cameraPosition = Camera.transform.position;
            _cameraRotation = Camera.transform.eulerAngles;

            MaleButton.onClick.AddListener(() => Avatar.SetGender(AvatarCustomization.AvatarGender.Male, true));
            FemaleButton.onClick.AddListener(() => Avatar.SetGender(AvatarCustomization.AvatarGender.Female, true));

            BodyButton.onClick.AddListener(() => {
                ClearSectionContent_Internal();
                ShowSliderList_Internal(AvatarCustomization.SECTION_BODY_PARAMETERS);
                _isZoomed = false;
                
                if (_hiddenGlasses != null) {
                    _hiddenGlasses.gameObject.SetActive(true);
                }
            });
            FaceButton.onClick.AddListener(() => {
                ClearSectionContent_Internal();
                ShowSliderList_Internal(AvatarCustomization.SECTION_FACE_PARAMETERS);
                _isZoomed = true;

                _hiddenGlasses = Avatar.GetComponentsInChildren<UClothingItem>().FirstOrDefault(item => item.ClothingItem.SlotType == SlotType.Glasses);
                if (_hiddenGlasses != null) {
                    _hiddenGlasses.gameObject.SetActive(false);
                }
            });
            FeaturesButton.onClick.AddListener(() => {
                ClearSectionContent_Internal();
                ShowSliderList_Internal(AvatarCustomization.SECTION_FEATURE_STYLES);
                _isZoomed = false;

                if (_hiddenGlasses != null) {
                    _hiddenGlasses.gameObject.SetActive(true);
                }
            });
            ClothingButton.onClick.AddListener(() => {
                ClearSectionContent_Internal();
                ShowClothingList_Internal();
                _isZoomed = false;

                if (_hiddenGlasses != null) {
                    _hiddenGlasses.gameObject.SetActive(true);
                }
            });

            RandomizeButton.onClick.AddListener(() => {
                Avatar.RandomizeBodyParameters();
                Avatar.RandomizeClothing();
                _isZoomed = false;

                if (_hatClothingDropdown != null) {
                    _hatClothingDropdown.UpdateValues(false);
                }
                if (_topClothingDropdown != null) {
                    _topClothingDropdown.UpdateValues(false);
                }
                if (_bottomClothingDropdown != null) {
                    _bottomClothingDropdown.UpdateValues(false);
                }
                if (_shoesClothingDropdown != null) {
                    _shoesClothingDropdown.UpdateValues(false);
                }
                if (_glassesClothingDropdown != null) {
                    _glassesClothingDropdown.UpdateValues(false);
                }

                UpdateSliderList_Internal();
            });

            BodyButton.onClick.Invoke();

            SliderTemplate.SetActive(false);
            TitleTemplate.SetActive(false);
            ClothingItemDropdownTemplate.SetActive(false);
        }

        void Update() {
          
            Camera.transform.position = Vector3.Lerp(Camera.transform.position, _isZoomed ? _zoomedPosition : _cameraPosition, 2f * Time.deltaTime);
            Camera.transform.rotation = Quaternion.Lerp(Camera.transform.rotation, _isZoomed ? Quaternion.Euler(_zoomedRotation) : Quaternion.Euler(_cameraRotation), 2 * Time.deltaTime);
        }


        private void UpdateSliderList_Internal() {
            if (_sliders == null) {
                return;
            }

            FieldInfo[] fields = typeof(AvatarCustomization).GetFields();

            foreach (FieldInfo field in fields) {
                AvatarFieldAttribute avatarFieldAttribute = field.GetCustomAttribute<AvatarFieldAttribute>();
                if (avatarFieldAttribute == null) {
                    continue;
                }
                
                SliderWrapper slider = _sliders.FirstOrDefault(s => s.FieldName == field.Name);
                if (slider == null) {
                    continue;
                }

                slider.CurrentValue = slider.WholeNumbers ? (int) field.GetValue(Avatar) : (float) field.GetValue(Avatar);
            }
        }

        private void ShowSliderList_Internal(string section) {
            FieldInfo[] fields = typeof(AvatarCustomization).GetFields();

            foreach (FieldInfo field in fields) {
                AvatarFieldAttribute avatarFieldAttribute = field.GetCustomAttribute<AvatarFieldAttribute>();
                if (avatarFieldAttribute == null || avatarFieldAttribute.Section != section) {
                    continue;
                }

                AddSlider_Internal(field, fields);
            }

            SliderTemplate.SetActive(false);
            TitleTemplate.SetActive(false);
        }

        private void ClearSectionContent_Internal() {
            foreach (Transform child in ContentTransform) {
                if (child.name.Contains(CONTENT_STRING)) {
                    Destroy(child.gameObject);
                }
            }

            foreach (Transform child in ContentTransform) {
                if (child.name.Contains(CONTENT_STRING_HIDE)) {
                    child.gameObject.SetActive(false);
                }
            }

            _sliders.Clear();
        }

        private void ShowClothingList_Internal() {

            if (_hatClothingDropdown == null) {
                _hatClothingDropdown = new ClothingDropdownWrapper(ClothingItemDropdownTemplate, Avatar, SlotType.Hat).AttachTo(ContentTransform);
                _hatClothingDropdown.Title = "Hat";
            }
            else {
                _hatClothingDropdown.Toggle(true);
            }

            if (_topClothingDropdown == null) {
                _topClothingDropdown = new ClothingDropdownWrapper(ClothingItemDropdownTemplate, Avatar, SlotType.Top).AttachTo(ContentTransform);
                _topClothingDropdown.Title = "Torso/Top";
            }
            else {
                _topClothingDropdown.Toggle(true);
            }
            
            if (_bottomClothingDropdown == null) {
                _bottomClothingDropdown = new ClothingDropdownWrapper(ClothingItemDropdownTemplate, Avatar, SlotType.Bottom).AttachTo(ContentTransform);
                _bottomClothingDropdown.Title = "Pants/Bottom";
            }
            else {
                _bottomClothingDropdown.Toggle(true);
            }
            
            if (_glassesClothingDropdown == null) {
                _glassesClothingDropdown = new ClothingDropdownWrapper(ClothingItemDropdownTemplate, Avatar, SlotType.Glasses).AttachTo(ContentTransform);
                _glassesClothingDropdown.Title = "Glasses";
            }
            else {
                _glassesClothingDropdown.Toggle(true);
            }

            if (_shoesClothingDropdown == null) {
                _shoesClothingDropdown = new ClothingDropdownWrapper(ClothingItemDropdownTemplate, Avatar, SlotType.Shoes).AttachTo(ContentTransform);
                _shoesClothingDropdown.Title = "Shoes";
            }
            else {
                _shoesClothingDropdown.Toggle(true);
            }
        }

        private void AddSlider_Internal(FieldInfo field, FieldInfo[] fields) {
            AvatarFloatFieldAttribute floatFieldAttribute = field.GetCustomAttribute<AvatarFloatFieldAttribute>();
            if (floatFieldAttribute != null && !floatFieldAttribute.IgnoreInPlayMode) {
                SliderWrapper slider = new SliderWrapper(field.Name, SliderTemplate) {
                    Title = floatFieldAttribute.DisplayName,
                    CurrentValue = (float) field.GetValue(Avatar),
                    MinValue = floatFieldAttribute.SourceMinValue,
                    MaxValue = floatFieldAttribute.SourceMaxValue
                };

                slider.OnValueChanged = (value) => {
                    field.SetValue(Avatar, (float) value);
                    Avatar.UpdateCustomization();
                };

                slider.AttachTo(ContentTransform);

                _sliders.Add(slider);
            }

            AvatarIntegerFieldAttribute integerFieldAttribute = field.GetCustomAttribute<AvatarIntegerFieldAttribute>();
            if (integerFieldAttribute != null && integerFieldAttribute.HasArrayDependancy()) {
                
                FieldInfo[] referencesFieldInfos = Avatar.AvatarReferences.GetType().GetFields();
                Array dependancyArrayObject = (Array) referencesFieldInfos.First(f => f.Name == integerFieldAttribute.ArrayDependancyField).GetValue(Avatar.AvatarReferences);

                if (integerFieldAttribute.IsVariationField) {
                    IVariations variationsInterface = (IVariations) dependancyArrayObject.GetValue(0);
                    
                    SliderWrapper slider = new SliderWrapper(field.Name, SliderTemplate) {
                        CurrentValue = (int) field.GetValue(Avatar),
                        MinValue = 0,
                        MaxValue = variationsInterface.GetVariationsCount() - 1,
                        WholeNumbers = true
                    };

                    slider.OnValueChanged = (value) => {
                        field.SetValue(Avatar, (int) value);
                        Avatar.UpdateCustomization();
                        slider.Title = $"{integerFieldAttribute.DisplayName} ({variationsInterface.GetVariationName((int) value)})";
                    };

                    slider.AttachTo(ContentTransform);
                    slider.Title = $"{integerFieldAttribute.DisplayName} ({variationsInterface.GetVariationName((int) slider.CurrentValue)})";
                    _sliders.Add(slider);
                }
                else {
                    SliderWrapper slider = new SliderWrapper(field.Name, SliderTemplate) {
                        CurrentValue = (int) field.GetValue(Avatar),
                        MinValue = 0,
                        MaxValue = dependancyArrayObject.Length - 1,
                        WholeNumbers = true
                    };

                    slider.OnValueChanged = (value) => {
                        field.SetValue(Avatar, (int) value);
                        Avatar.UpdateCustomization();
                        slider.Title = $"{integerFieldAttribute.DisplayName} ({GetObjectName_Internal(dependancyArrayObject, (int) value)})";
                    };

                    slider.AttachTo(ContentTransform);
                    slider.Title = $"{integerFieldAttribute.DisplayName} ({GetObjectName_Internal(dependancyArrayObject, (int) slider.CurrentValue)})";
                    _sliders.Add(slider);
                }
            }
        }

        private string GetObjectName_Internal(Array array, int index) {
            object value = array.GetValue(index);
            if (value is Material material) {
                return material.name;
            }
            return value.ToString();
        }

        private void AddTitleText_Internal(string title) {
            GameObject titleInstance = Instantiate(TitleTemplate);
            titleInstance.transform.SetParent(ContentTransform, false);
            // CORRECCIÓN AQUÍ:
            if (titleInstance.GetComponent<TextMeshProUGUI>() != null)
            {
                titleInstance.GetComponent<TextMeshProUGUI>().text = title;
            }
        }

        

        public class ClothingDropdownWrapper
        {
            public string Title
            {
                get => _title;
                set
                {
                    _title = value;
                    if (_instance != null)
                    {
                        var titleText = _instance.GetComponentInChildren<TextMeshProUGUI>();
                        if(titleText != null) titleText.text = value;
                    }
                }
            }

            private string _title;
            private Slider _slider;
           
            private TMP_Dropdown _dropdown;

            private GameObject _clothingDropdownTemplate;
            private GameObject _instance;

            private AvatarCustomization _avatar;
            private ClothingItem[] _availableClothingItems;
            private SlotType _slotType;
            private Action<ClothingItem> _onClothingItemSelected;
            private ClothingItem _selectedClothingItem;
            private int _selectedClothingItemIndex;
            private LayoutElement _layoutElement;
            private const float MIN_HEIGHT = 60;
            private const float MAX_HEIGHT = 100;

            public ClothingDropdownWrapper(GameObject clothingDropdownTemplate, AvatarCustomization avatar, SlotType slotType)
            {
                _clothingDropdownTemplate = clothingDropdownTemplate;
                _avatar = avatar;
                _slotType = slotType;
                _availableClothingItems = _avatar.AvatarReferences.AvailableClothingItems.Where(item => item.SlotType == _slotType).ToArray();
            }

            public ClothingDropdownWrapper AttachTo(Transform transform)
            {
                _instance = Instantiate(_clothingDropdownTemplate);
                _layoutElement = _instance.GetComponent<LayoutElement>();
                // CORRECCIÓN AQUÍ: Buscamos el nuevo tipo de dropdown
                _dropdown = _instance.GetComponentInChildren<TMP_Dropdown>();
                _slider = _instance.GetComponentInChildren<Slider>();

                _selectedClothingItem = _avatar.GetClothingItemFromSlot(_slotType) != null ? _availableClothingItems.FirstOrDefault(item => item == _avatar.GetClothingItemFromSlot(_slotType)) : null;
                _selectedClothingItemIndex = _avatar.GetClothingItemVariationIndex(_slotType);

                UpdateSliderText_Internal(_selectedClothingItem != null ? _selectedClothingItem.Variations[_avatar.GetClothingItemVariationIndex(_slotType)].name : "");

                _dropdown.ClearOptions();
               
                List<TMP_Dropdown.OptionData> options = _avatar.AvatarReferences.AvailableClothingItems
                    .Where(item => item.SlotType == _slotType)
                    .Select(item => new TMP_Dropdown.OptionData(item.Name))
                    .ToList();
                _dropdown.AddOptions(options);

                _dropdown.onValueChanged.AddListener((index) => {
                    _selectedClothingItem = _availableClothingItems[index];
                    _selectedClothingItemIndex = index;
                    _avatar.SetClothingItemVariation(_slotType, 0);
                    _avatar.AttachClothingItem(_selectedClothingItem);
                    _avatar.UpdateClothing();
                    UpdateSlider_Internal();
                    UpdateSliderText_Internal(_selectedClothingItem.Variations[0].name);
                });

                _instance.transform.SetParent(transform, false);
                _instance.name += CONTENT_STRING_HIDE;
                _instance.SetActive(true);
                UpdateSlider_Internal();
                return this;
            }
            
 
            public void SetOnClothingItemSelected(Action<ClothingItem> onClothingItemSelected) {
                _onClothingItemSelected = onClothingItemSelected;
                if (_dropdown != null) {
                    _dropdown.onValueChanged.RemoveAllListeners();
                    _dropdown.onValueChanged.AddListener((index) => {
                        ClothingItem selectedClothingItem = _availableClothingItems[index];
                        _onClothingItemSelected.Invoke(selectedClothingItem);
                    });
                }
            }

            public void UpdateValues(bool notify = true) {
                ClothingItem clothingItem = _avatar.GetClothingItemFromSlot(_slotType);

                if (notify) {
                    _dropdown.value = clothingItem != null ? Array.IndexOf(_availableClothingItems, clothingItem) : 0;
                    _slider.value = (clothingItem != null ? _avatar.GetClothingItemVariationIndex(_slotType) : 0);
                }
                else {
                    _dropdown.SetValueWithoutNotify(clothingItem != null ? Array.IndexOf(_availableClothingItems, clothingItem) : 0);
                    _slider.SetValueWithoutNotify(clothingItem != null ? _avatar.GetClothingItemVariationIndex(_slotType) : 0);
                }

                _slider.gameObject.SetActive(clothingItem != null ? clothingItem.Variations.Length > 1 : false);
                _layoutElement.minHeight = clothingItem != null && clothingItem.Variations.Length > 1 ? MAX_HEIGHT : MIN_HEIGHT;

                Vector2 size = _instance.GetComponent<RectTransform>().sizeDelta;
                size.y = clothingItem != null && clothingItem.Variations.Length > 1 ? MAX_HEIGHT : MIN_HEIGHT;
                _instance.GetComponent<RectTransform>().sizeDelta = size;

                if (clothingItem != null) {
                    UpdateSliderText_Internal(clothingItem.Variations[_avatar.GetClothingItemVariationIndex(_slotType)].name);
                }
            }

            public void Toggle(bool visibility) {
                if (_instance != null) {
                    _instance.SetActive(visibility);
                }
            }

            private void UpdateSlider_Internal() {
                _slider.gameObject.SetActive(_selectedClothingItem != null ? _selectedClothingItem.Variations.Length > 1 : false);
                _layoutElement.minHeight = _selectedClothingItem != null && _selectedClothingItem.Variations.Length > 1 ? MAX_HEIGHT : MIN_HEIGHT;
                
                Vector2 size = _instance.GetComponent<RectTransform>().sizeDelta;
                size.y = _selectedClothingItem != null && _selectedClothingItem.Variations.Length > 1 ? MAX_HEIGHT : MIN_HEIGHT;
                _instance.GetComponent<RectTransform>().sizeDelta = size;
               
                _slider.minValue = 0;
                _slider.maxValue = _selectedClothingItem != null ? _selectedClothingItem.Variations.Length - 1 : 0;
                _slider.wholeNumbers = true;
                _slider.onValueChanged.RemoveAllListeners();
                _slider.onValueChanged.AddListener((value) => {
                    _avatar.SetClothingItemVariation(_slotType, (int) value);
                    _avatar.UpdateClothing();
                    UpdateSliderText_Internal(_selectedClothingItem.Variations[(int) value].name);
                });
            }
            
            private void UpdateSliderText_Internal(string text) {
               
                var sliderText = _slider.GetComponentInChildren<TextMeshProUGUI>();
                if(sliderText != null) sliderText.text = text;
            }
        }

        public class SliderWrapper
        {
            public string FieldName;
            public string Title
            {
                get => _title;
                set
                {
                    _title = value;
                    if (_instantiatedSlider != null)
                    {
                        // CORRECCIÓN ROBUSTA AQUÍ
                        var sliderText = _instantiatedSlider.GetComponentInChildren<TextMeshProUGUI>();
                        if (sliderText != null) sliderText.text = value;
                    }
                }
            }

            public float MinValue { get; set; }
            public float MaxValue { get; set; }
            public float CurrentValue
            {
                get => _currentValue;
                set
                {
                    _currentValue = value;
                    if (_slider != null) _slider.value = value;
                }
            }
            public UnityAction<float> OnValueChanged { set => _onValueChanged = value; }
            public bool WholeNumbers { get; set; } = false;

            private GameObject _sliderTemplate;
            private GameObject _instantiatedSlider;
            private Slider _slider;
            private UnityAction<float> _onValueChanged;
            private string _title;
            private float _currentValue;

            public SliderWrapper(string fieldName, GameObject sliderTemplate)
            {
                _sliderTemplate = sliderTemplate;
                FieldName = fieldName;
            }

            public void AttachTo(Transform transform)
            {
                _instantiatedSlider = Instantiate(_sliderTemplate);
                 if (_title == "Breast Size")
            {
                UISliderIdentifier identifier = _instantiatedSlider.AddComponent<UISliderIdentifier>();
                identifier.sliderID = SliderType.BreastSize;
                }
                
                // CORRECCIÓN ROBUSTA Y SEGURA AQUÍ
                TextMeshProUGUI sliderText = _instantiatedSlider.GetComponentInChildren<TextMeshProUGUI>();
                if (sliderText != null)
                {
                    sliderText.text = Title;
                }

                // Búsqueda robusta del Slider
                _slider = _instantiatedSlider.GetComponentInChildren<Slider>();
                if(_slider != null)
                {
                    _slider.wholeNumbers = WholeNumbers;
                    _slider.minValue = MinValue;
                    _slider.maxValue = MaxValue;
                    _slider.value = _currentValue;
                    _slider.onValueChanged.RemoveAllListeners();
                    _slider.onValueChanged.AddListener((value) => {
                        _currentValue = value;
                        _onValueChanged?.Invoke(value);
                    });
                }
                
                _instantiatedSlider.transform.SetParent(transform, false);
                _instantiatedSlider.name += CONTENT_STRING;
                _instantiatedSlider.SetActive(true);
            }
        }
    }
    
    public static class LinqExtensions
    {
        public static int FirstIndexMatch<T>(this IEnumerable<T> source, Predicate<T> match)
        {
            int index = 0;
            foreach (T item in source)
            {
                if (match(item))
                {
                    return index;
                }
                index++;
            }
            return -1; 
        }
    }
}
