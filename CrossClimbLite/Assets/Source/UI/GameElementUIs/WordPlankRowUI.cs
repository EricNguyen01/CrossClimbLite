using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;
using System.Runtime.CompilerServices;

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
        private WordPlankDragHandleUI leftDragHandleUIRoot;

        [SerializeField]
        private WordPlankDragHandleUI rightDragHandleUIRoot;

        [field: Header("Required Letter Slot UI Prefabs")]

        [SerializeField]
        private PlankLetterSlotUI letterSlotUIPrefabToSpawn;

        private List<PlankLetterSlotUI> letterSlotsUISpawned = new List<PlankLetterSlotUI>();

        [field: Header("Letter Slots Spawn Specifications")]

        [field: SerializeField]
        public HorizontalLayoutGroup horizontalLayoutToSpawnLetterSlotsUnder { get; private set; }

        //the non-UI word plank row Modal (where plank row logic is stored)
        private WordPlankRow wordPlankRowLinked;

        private WordPlankRowUI plankDragVisualObject;

        private GameGridUI parentGridUI;

        private int plankUISiblingIndex = 0;

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

        public override void InitGameElementUI(GameElementBase wordPlankRowToLink, GameElementUIBase parentHoldingUIToLink)
        {
            base.InitGameElementUI(wordPlankRowToLink);

            if (!wordPlankRowToLink) return;

            if (wordPlankRowToLink is not WordPlankRow) return;

            wordPlankRowLinked = wordPlankRowToLink as WordPlankRow;

            wordPlankRowLinked.ConnectGameElementUI(this);

            InitChildrenLetterSlotsUI();

            if (wordPlankRowLinked.isPlankLocked)
            {
                UpdateUI_OnGameElementModalLocked(true);
            }

            if (leftDragHandleUIRoot)
            {
                leftDragHandleUIRoot.InitGameElementUI(wordPlankRowLinked);

                leftDragHandleUIRoot.InitDragHandleUI(this);

                if (wordPlankRowLinked.isPlankLocked) leftDragHandleUIRoot.UpdateUI_OnGameElementModalLocked(true);
            }

            if (rightDragHandleUIRoot)
            {
                rightDragHandleUIRoot.InitGameElementUI(wordPlankRowLinked);

                rightDragHandleUIRoot.InitDragHandleUI(this);

                if (wordPlankRowLinked.isPlankLocked) rightDragHandleUIRoot.UpdateUI_OnGameElementModalLocked(true);
            }

            plankUISiblingIndex = transform.GetSiblingIndex();

            if (!parentHoldingUIToLink) return;

            if(parentHoldingUIToLink is not GameGridUI) return;

            parentGridUI = parentHoldingUIToLink as GameGridUI;
        }

        public void InitChildrenLetterSlotsUI()
        {
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
                    letterSlotUI.InitGameElementUI(wordPlankRowLinked.letterSlotsInWordPlank[i], this);

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

        public void SetPlankUICanvasGroupData(bool blockRaycast, bool interactable, float enableAlpha = 1.0f, float disableAlpha = 0.0f)
        {
            if (!elementCanvasGroup) return;

            elementCanvasGroup.alpha = enableAlpha;

            elementCanvasGroup.blocksRaycasts = blockRaycast;

            elementCanvasGroup.interactable = interactable;
        }

        // DRAG HANDLER INTERFACE FUNCS............................................................................................

        private RectTransform vertLayoutParentRect;

        private Vector2 dragVisualLocalPos;

        private float mouseYLocalRectBeginDrag;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if(!enabled) return;

            CreatePlankDragVisualObject(false);

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

            if (parentGridUI)
            {
                parentGridUI.SetAllPlankUIsCanvasGroupData(true, false, 1.0f, 1.0f);
            }

            plankDragVisualObject.gameObject.SetActive(true);

            if(!vertLayoutParentRect) vertLayoutParentRect = transform.parent.GetComponent<RectTransform>();

            Vector2 mousePosLocalInRect;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(vertLayoutParentRect, eventData.position,
                                                                    parentRootCanvas.worldCamera ? parentRootCanvas.worldCamera : Camera.main,
                                                                    out mousePosLocalInRect);

            mouseYLocalRectBeginDrag = mousePosLocalInRect.y;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 mousePosLocalInRect;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(vertLayoutParentRect, eventData.position, 
                                                                    parentRootCanvas.worldCamera ? parentRootCanvas.worldCamera : Camera.main, 
                                                                    out mousePosLocalInRect);

            float mouseYLocalRectDragOffset = mousePosLocalInRect.y - mouseYLocalRectBeginDrag;

            plankDragVisualObject.transform.localPosition += new Vector3(0.0f, mouseYLocalRectDragOffset, 0.0f);

            Vector3 dragVisualLocalPosInParentVertLayout;

            dragVisualLocalPosInParentVertLayout = vertLayoutParentRect.transform.InverseTransformPoint(plankDragVisualObject.transform.position);

            if(dragVisualLocalPosInParentVertLayout.y > parentGridUI.plankUISpawned[0].transform.localPosition.y)
            {
                plankDragVisualObject.transform.localPosition = plankDragVisualObject.transform.InverseTransformPoint(parentGridUI.plankUISpawned[0].transform.position);
            }
            else if(dragVisualLocalPosInParentVertLayout.y < parentGridUI.plankUISpawned[parentGridUI.plankUISpawned.Count - 1].transform.localPosition.y)
            {
                plankDragVisualObject.transform.localPosition = plankDragVisualObject.transform.InverseTransformPoint(parentGridUI.plankUISpawned[parentGridUI.plankUISpawned.Count - 1].transform.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!enabled) return;

            DestroyPlankDragVisualObject();

            if (parentGridUI)
            {
                parentGridUI.SetAllPlankUIsCanvasGroupData(true, true, 1.0f, 1.0f);
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

        private void CreatePlankDragVisualObject(bool activeOnCreated)
        {
            if (!enabled) return;

            DestroyPlankDragVisualObject();

            plankDragVisualObject = Instantiate(gameObject, transform).GetComponent<WordPlankRowUI>();

            RectTransform plankDragRect = plankDragVisualObject.GetComponent<RectTransform>();

            if(plankDragRect) plankDragRect.sizeDelta = gameElementUIRect.sizeDelta;

            plankDragVisualObject.gameObject.name = "PlankDragVisualObject";

            if (plankDragVisualObject.elementCanvasGroup)
            {
                plankDragVisualObject.elementCanvasGroup.blocksRaycasts = false;

                plankDragVisualObject.elementCanvasGroup.interactable = false;

                plankDragVisualObject.elementCanvasGroup.ignoreParentGroups = true;
            }

            plankDragVisualObject.enabled = false;

            foreach(PlankLetterSlotUI childSlotUI in plankDragVisualObject.GetComponentsInChildren<PlankLetterSlotUI>())
            {
                if (!childSlotUI) continue;

                if (Application.isPlaying) Destroy(childSlotUI);

                else if (Application.isEditor) DestroyImmediate(childSlotUI);
            }

            if (plankDragVisualObject.rightDragHandleUIRoot)
            {
                if (Application.isPlaying) Destroy(plankDragVisualObject.rightDragHandleUIRoot);

                else if (Application.isEditor) DestroyImmediate(plankDragVisualObject.rightDragHandleUIRoot);
            }

            if (plankDragVisualObject.leftDragHandleUIRoot)
            {
                if (Application.isPlaying) Destroy(plankDragVisualObject.leftDragHandleUIRoot);

                else if (Application.isEditor) DestroyImmediate(plankDragVisualObject.leftDragHandleUIRoot);
            }

            plankDragVisualObject.gameObject.SetActive(activeOnCreated);
        }

        private void DestroyPlankDragVisualObject()
        {
            if (plankDragVisualObject)
            {
                if (Application.isPlaying) Destroy(plankDragVisualObject.gameObject);

                else if (Application.isEditor) DestroyImmediate(plankDragVisualObject.gameObject);

                plankDragVisualObject = null;
            }
        }
    }
}
