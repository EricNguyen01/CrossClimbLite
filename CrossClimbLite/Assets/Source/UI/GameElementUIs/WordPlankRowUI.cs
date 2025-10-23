using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class WordPlankRowUI : GameElementUIBase, IBeginDragHandler, IDragHandler, IEndDragHandler   
    {
        [SerializeField]
        [ReadOnlyInspector]
        private int plankUISiblingIndex = 0;

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
        private Image leftDragHandleRoot;

        [SerializeField]
        private Image rightDragHandleRoot;

        [field: Header("Required Letter Slot UI Prefabs")]

        [SerializeField]
        private PlankLetterSlotUI letterSlotUIPrefabToSpawn;

        private List<PlankLetterSlotUI> letterSlotsUISpawned = new List<PlankLetterSlotUI>();

        [field: Header("Letter Slots Spawn Specifications")]

        [field: SerializeField]
        public HorizontalLayoutGroup horizontalLayoutToSpawnLetterSlotsUnder { get; private set; }

        //the non-UI word plank row Modal (where plank row logic is stored)
        private WordPlankRow wordPlankRowLinked;

        private GameGridUI parentGridUI;

        private WordPlankRowUI plankDragVisualObject;

        private Canvas parentRootCanvas;

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

            parentRootCanvas = GetComponentInParent<Canvas>();
        }

        public override void InitGameElementUI(GameElementBase wordPlankRowToLink)
        {
            base.InitGameElementUI(wordPlankRowToLink);

            if (!wordPlankRowToLink) return;

            if (wordPlankRowToLink is not WordPlankRow) return;

            wordPlankRowLinked = wordPlankRowToLink as WordPlankRow;

            if (!wordPlankRowLinked.gameGridHoldingPlank)
            {
                gameObject.SetActive(false);

                enabled = false;

                return;
            }

            InitChildrenLetterSlotsUI();

            if (wordPlankRowLinked.isPlankLocked)
            {
                UpdateUI_OnGameElementModalLocked(true);
            }

            WordPlankDragHandleUI dragHandleUI;

            if (leftDragHandleRoot)
            {
                if (!leftDragHandleRoot.GetComponent<WordPlankDragHandleUI>())
                {
                    dragHandleUI = leftDragHandleRoot.gameObject.AddComponent<WordPlankDragHandleUI>();

                    dragHandleUI.InitGameElementUI(wordPlankRowLinked);

                    dragHandleUI.InitDragHandleUI(this);

                    if(wordPlankRowLinked.isPlankLocked) dragHandleUI.UpdateUI_OnGameElementModalLocked(true);
                }
            }

            if (rightDragHandleRoot)
            {
                if (!rightDragHandleRoot.GetComponent<WordPlankDragHandleUI>())
                {
                    dragHandleUI = rightDragHandleRoot.gameObject.AddComponent<WordPlankDragHandleUI>();

                    dragHandleUI.InitGameElementUI(wordPlankRowLinked);

                    dragHandleUI.InitDragHandleUI(this);

                    if (wordPlankRowLinked.isPlankLocked) dragHandleUI.UpdateUI_OnGameElementModalLocked(true);
                }
            }

            plankUISiblingIndex = transform.GetSiblingIndex();
        }

        public void SetParentHoldingGridUI(GameGridUI gameGridUI)
        {
            if (!gameGridUI) return;

            parentGridUI = gameGridUI;
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
            if(!enabled) return;

            CreatePlankDragVisualObject();

            if (!plankDragVisualObject) return;

            if (plankDragVisualObject.letterSlotsUISpawned != null && plankDragVisualObject.letterSlotsUISpawned.Count > 0)
            {
                for (int i = 0; i < plankDragVisualObject.letterSlotsUISpawned.Count; i++)
                {
                    if (!plankDragVisualObject.letterSlotsUISpawned[i]) continue;

                    if (i > letterSlotsUISpawned.Count - 1) break;

                    plankDragVisualObject.letterSlotsUISpawned[i].inputField.text = letterSlotsUISpawned[i].inputField.text;
                }
            }

            if (parentRootCanvas) plankDragVisualObject.transform.SetParent(parentRootCanvas.transform);

            else plankDragVisualObject.transform.SetParent(null);

            plankDragVisualObject.gameObject.SetActive(true);

            if (elementCanvasGroup)
            {
                elementCanvasGroup.alpha = 0.0f;

                elementCanvasGroup.blocksRaycasts = false;

                elementCanvasGroup.interactable = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!enabled) return;

            if (plankDragVisualObject)
            {
                if (plankDragVisualObject.gameObject.activeSelf) plankDragVisualObject.gameObject.SetActive(false);

                plankDragVisualObject.transform.SetParent(transform);

                plankDragVisualObject.transform.localPosition = Vector3.zero;
            }

            if (elementCanvasGroup)
            {
                elementCanvasGroup.alpha = 1.0f;

                elementCanvasGroup.blocksRaycasts = true;

                elementCanvasGroup.interactable = true;
            }

            if (eventData == null || !eventData.pointerEnter) return;

            WordPlankRowUI destinationPlank;  
            
            eventData.pointerEnter.TryGetComponent<WordPlankRowUI>(out destinationPlank);

            if (!destinationPlank)
            {
                WordPlankDragHandleUI destinationDragHandleUI;

                if (eventData.pointerEnter.TryGetComponent<WordPlankDragHandleUI>(out destinationDragHandleUI))
                {
                    if (destinationDragHandleUI && destinationDragHandleUI.parentWordPlankUI)
                        destinationPlank = destinationDragHandleUI.parentWordPlankUI;
                }
            }

            if (!destinationPlank) return;

            PlankSwapWith(destinationPlank);
        }

        public void PlankSwapWith(WordPlankRowUI plankToSwap)
        {
            if(!enabled) return;

            if (!plankToSwap || !plankToSwap.wordPlankRowLinked) return;

            if (plankToSwap.transform.parent != transform.parent) return;

            if (this == plankToSwap) return;

            if (wordPlankRowLinked.isPlankKeyword && !plankToSwap.wordPlankRowLinked.isPlankKeyword) return;

            transform.SetSiblingIndex(plankToSwap.transform.GetSiblingIndex());

            plankUISiblingIndex = transform.GetSiblingIndex();
        }

        private void CreatePlankDragVisualObject()
        {
            if (!enabled) return;

            if (plankDragVisualObject)
            {
                if(Application.isPlaying) Destroy(plankDragVisualObject.gameObject);

                else if(Application.isEditor) DestroyImmediate(plankDragVisualObject.gameObject);

                plankDragVisualObject = null;
            }

            if (!parentGridUI || !parentGridUI.wordPlankUIPrefabToSpawn) return;

            plankDragVisualObject = Instantiate(parentGridUI.wordPlankUIPrefabToSpawn, transform).GetComponent<WordPlankRowUI>();

            plankDragVisualObject.gameObject.name = "PlankDragVisualObject";

            plankDragVisualObject.InitGameElementUI(wordPlankRowLinked);

            plankDragVisualObject.enabled = false;

            if(plankDragVisualObject.letterSlotsUISpawned != null && plankDragVisualObject.letterSlotsUISpawned.Count > 0)
            {
                for (int i = 0; i < plankDragVisualObject.letterSlotsUISpawned.Count; i++)
                {
                    if (!plankDragVisualObject.letterSlotsUISpawned[i]) continue;

                    plankDragVisualObject.letterSlotsUISpawned[i].enabled = false;
                }
            }

            CanvasGroup dragVisualCanvasGroup;

            plankDragVisualObject.TryGetComponent<CanvasGroup>(out dragVisualCanvasGroup);

            if (dragVisualCanvasGroup)
            {
                dragVisualCanvasGroup.blocksRaycasts = false;

                dragVisualCanvasGroup.interactable = false;

                dragVisualCanvasGroup.ignoreParentGroups = true;
            }

            WordPlankDragHandleUI[] dragHandleUIComps = plankDragVisualObject.GetComponentsInChildren<WordPlankDragHandleUI>(); 

            if(dragHandleUIComps != null && dragHandleUIComps.Length > 0)
            {
                for(int i = 0; i < dragHandleUIComps.Length; i++)
                {
                    if(!dragHandleUIComps[i]) continue;

                    if (Application.isPlaying) Destroy(dragHandleUIComps[i]);

                    else if (Application.isEditor) DestroyImmediate(dragHandleUIComps[i]);
                }
            }

            plankDragVisualObject.gameObject.SetActive(false);
        }
    }
}
