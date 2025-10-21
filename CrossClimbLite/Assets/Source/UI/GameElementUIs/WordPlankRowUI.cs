using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class WordPlankRowUI : GameElementUIBase, IBeginDragHandler, IDragHandler, IEndDragHandler   
    {
        [Header("Plank State Colors")]
        [SerializeField]
        private Color normalStateColor = Color.white;

        [SerializeField]
        private Color lockedColor = Color.gray;

        [SerializeField]
        private Color selectedColor = Color.magenta;

        [SerializeField]
        private Color keywordColor = Color.yellow;
        
        [Header("Plank Visual")]

        [SerializeField]
        private Image wordPlankRowBackgroundImage;

        [Header("Drag Handles Visuals")]

        [SerializeField]
        private Image leftDragHandleImage;

        [SerializeField]
        private Image rightDragHandleImage;

        [field: Header("Letter Slots Spawn Specifications")]

        [SerializeField]
        private PlankLetterSlotUI letterSlotUIPrefabToSpawn;

        private List<PlankLetterSlotUI> letterSlotsUISpawned = new List<PlankLetterSlotUI>();

        [field: SerializeField]
        public HorizontalLayoutGroup horizontalLayoutToSpawnLetterSlotsUnder { get; private set; }

        //the non-UI word plank row Modal (where plank row logic is stored)
        private WordPlankRow wordPlankRowLinked;

        private GameObject wordPlankDragVisualObject;

        private bool isKeyword = false;

        private bool isDragable = false;

        protected override void Awake()
        {
            base.Awake();

            if (!wordPlankRowBackgroundImage)
            {
                TryGetComponent<Image>(out wordPlankRowBackgroundImage);
            }

            if(!horizontalLayoutToSpawnLetterSlotsUnder)
            {
                horizontalLayoutToSpawnLetterSlotsUnder = GetComponent<HorizontalLayoutGroup>();

                if (!horizontalLayoutToSpawnLetterSlotsUnder)
                {
                    horizontalLayoutToSpawnLetterSlotsUnder = GetComponentInChildren<HorizontalLayoutGroup>();
                }
            }
        }

        public override void InitGameElementUI(GameElementBase wordPlankRowToLink)
        {
            base.InitGameElementUI(wordPlankRowToLink);

            if (!wordPlankRowToLink) return;

            if (wordPlankRowToLink is not WordPlankRow) return;

            wordPlankRowLinked = wordPlankRowToLink as WordPlankRow;

            InitChildrenLetterSlotsUI();

            if (wordPlankRowLinked.isPlankLocked)
            {
                UpdateUI_OnGameElementModalLocked(true);
            }

            WordPlankDragHandleUI dragHandleUI;

            if (leftDragHandleImage)
            {
                if (!leftDragHandleImage.GetComponent<WordPlankDragHandleUI>())
                {
                    dragHandleUI = leftDragHandleImage.gameObject.AddComponent<WordPlankDragHandleUI>();

                    dragHandleUI.InitGameElementUI(wordPlankRowLinked);

                    dragHandleUI.InitDragHandleUI(this);

                    if(wordPlankRowLinked.isPlankLocked) dragHandleUI.UpdateUI_OnGameElementModalLocked(true);
                }
            }

            if (rightDragHandleImage)
            {
                if (!rightDragHandleImage.GetComponent<WordPlankDragHandleUI>())
                {
                    dragHandleUI = rightDragHandleImage.gameObject.AddComponent<WordPlankDragHandleUI>();

                    dragHandleUI.InitGameElementUI(wordPlankRowLinked);

                    dragHandleUI.InitDragHandleUI(this);

                    if (wordPlankRowLinked.isPlankLocked) dragHandleUI.UpdateUI_OnGameElementModalLocked(true);
                }
            }
        }

        private void InitChildrenLetterSlotsUI()
        {
            if (!enabled) return;

            if (!wordPlankRowLinked) return;

            if (!letterSlotUIPrefabToSpawn)
            {
                Debug.LogError("Trying to spawn letter slot UI children for word plank row UI: " + name +
                               " but no letter slot UI prefab ref is assigned! Cannot spawn letter slot UI children.");
                return;
            }

            if (wordPlankRowLinked.letterSlotsInWordPlank == null || wordPlankRowLinked.letterSlotsInWordPlank.Length == 0) return;

            if (letterSlotsUISpawned != null && letterSlotsUISpawned.Count > 0)
            {
                RemoveAllChildrenLetterSlotsUI();
            }

            if (letterSlotsUISpawned == null) letterSlotsUISpawned = new List<PlankLetterSlotUI>();

            else letterSlotsUISpawned.Clear();

            for (int i = 0; i < wordPlankRowLinked.letterSlotsInWordPlank.Length; i++)
            {
                if (!wordPlankRowLinked.letterSlotsInWordPlank[i]) continue;

                GameObject letterSlotUIObj = null;

                if (horizontalLayoutToSpawnLetterSlotsUnder)
                    letterSlotUIObj = Instantiate(letterSlotUIPrefabToSpawn.gameObject, horizontalLayoutToSpawnLetterSlotsUnder.transform);

                else letterSlotUIObj = Instantiate(letterSlotUIPrefabToSpawn.gameObject, transform);

                if (!letterSlotUIObj) continue;

                letterSlotUIObj.name = letterSlotUIObj.name + "_" + i;

                PlankLetterSlotUI letterSlotUI = letterSlotUIObj.GetComponent<PlankLetterSlotUI>();

                if (letterSlotUI)
                {
                    letterSlotUI.InitGameElementUI(wordPlankRowLinked.letterSlotsInWordPlank[i]);

                    letterSlotsUISpawned.Add(letterSlotUI);
                }
            }
        }

        private void RemoveAllChildrenLetterSlotsUI()
        {
            if(letterSlotsUISpawned == null || letterSlotsUISpawned.Count == 0) return;

            for (int i = 0; i < letterSlotsUISpawned.Count; i++)
            {
                if (!letterSlotsUISpawned[i]) continue;

                if(Application.isEditor) DestroyImmediate(letterSlotsUISpawned[i].gameObject);

                else if(Application.isPlaying) Destroy(letterSlotsUISpawned[i].gameObject);
            }
        }

        public void UpdateUI_PlankModalIsKeyword(bool isKeyword)
        {
            this.isKeyword = isKeyword;

            if (!wordPlankRowBackgroundImage) return;

            if (isLocked)
            {
                wordPlankRowBackgroundImage.color = lockedColor;

                return;
            }

            if (isKeyword)
            {
                wordPlankRowBackgroundImage.color = keywordColor;

                return;
            }

            wordPlankRowBackgroundImage.color = normalStateColor;
        }

        public override void UpdateUI_OnGameElementModalSelected(bool isSelected)
        {
            base.UpdateUI_OnGameElementModalSelected(isSelected);

            if (!enabled) return;

            if (!wordPlankRowBackgroundImage) return;

            if (wordPlankRowLinked && wordPlankRowLinked.isPlankKeyword)
            {
                if(wordPlankRowBackgroundImage.color != keywordColor)
                    wordPlankRowBackgroundImage.color = keywordColor;

                return;
            }

            if (isSelected)
            {
                wordPlankRowBackgroundImage.color = selectedColor;

                return;
            }

            wordPlankRowBackgroundImage.color = normalStateColor;
        }

        public override void UpdateUI_OnGameElementModalLocked(bool isLocked)
        {
            base.UpdateUI_OnGameElementModalLocked(isLocked);

            isDragable = isLocked;

            if (!enabled) return;

            if (isLocked)
            {
                if (wordPlankRowBackgroundImage)
                    wordPlankRowBackgroundImage.color = lockedColor;

                return;
            }

            if (wordPlankRowBackgroundImage)
            {
                if (wordPlankRowLinked.isPlankKeyword)
                    wordPlankRowBackgroundImage.color = keywordColor;

                else
                    wordPlankRowBackgroundImage.color = normalStateColor;
            }
        }

        // DRAG HANDLER INTERFACE FUNCS............................................................................................

        public void OnBeginDrag(PointerEventData eventData)
        {
            
        }

        public void OnDrag(PointerEventData eventData)
        {
            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            
        }

        private void CreateDragVisualObject()
        {
            if (wordPlankDragVisualObject) return;


        }
    }
}
