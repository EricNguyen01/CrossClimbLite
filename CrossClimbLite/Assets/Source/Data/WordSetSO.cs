using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace CrossClimbLite
{
    [CreateAssetMenu(fileName = "WordSetSO", menuName = "Scriptable Objects/WordSetSO")]
    public class WordSetSO : ScriptableObject
    {
        [Header("CSV Source")]

        [HelpBox("Make sure the CSV contains the WORD, HINT, and CHARACTERS column with the exact names.", UnityEngine.UIElements.HelpBoxMessageType.Warning)]
        [SerializeField]
        [FormerlySerializedAs("wordHintCSV")]
        private TextAsset wordHintCSV;

        [SerializeField]
        [FormerlySerializedAs("wordColumnName")]
        private string wordColumnName = "WORD";

        [SerializeField]
        [FormerlySerializedAs("hintColumnName")]
        private string hintColumnName = "HINT";

        [SerializeField]
        [FormerlySerializedAs("charactersColumnName")]
        private string charactersColumnName = "CHARACTERS";

        [Header("Words Hints List")]

        [SerializeField]
        [Min(1)]
        [FormerlySerializedAs("entriesToShow")]
        private int entriesToShow = 40;

        [SerializeField]
        [HideInInspector]
        [FormerlySerializedAs("wordHintList")]
        private List<WordHintStruct> wordHintList = new List<WordHintStruct>();

        [Serializable]
        private struct WordHintStruct
        {
            [SerializeField]
            public string word;

            [SerializeField]
            public string hint;

            [SerializeField]
            public int wordLength;
        }

        private void GenerateWordSetsListFromCSV()
        {
            if(!wordHintCSV)
            {
                Debug.LogWarning("WordSetSO: No CSV file assigned.");

                return;
            } 

            wordHintList.Clear();

            // Check for empty csv or data rows
            // if csv only has less than 1 line (row), then it only has a header row or is empty, thus log warning and return
            var lines = wordHintCSV.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length <= 1)
            {
                Debug.LogWarning("CSV file appears empty or has no data rows.");

                return;
            }

            // Get first row (line) -> split by commas -> add splitted words (column headers) to list
            // Get index of the required WORD, HINT, and CHARACTERS columns in the headers list and if any index returns -1, log error and return
            var headers = lines[0].Trim().Split(',').Select(h => h.Trim().ToUpper()).ToList();

            int wordIndex = headers.IndexOf(wordColumnName);

            if(wordIndex == -1) wordIndex = headers.IndexOf("WORD"); //fallback to default

            int hintIndex = headers.IndexOf(hintColumnName);

            if(hintIndex == -1) hintIndex = headers.IndexOf("HINT"); //fallback to default

            int charIndex = headers.IndexOf(charactersColumnName);

            if(charIndex == -1) charIndex = headers.IndexOf("CHARACTERS"); //fallback to default

            if(charIndex == -1) charIndex = headers.IndexOf("CHARACTER"); //fallback to singular form

            if (charIndex == -1) charIndex = headers.IndexOf("FILTERED BY CHARACTER"); 

            if (charIndex == -1) charIndex = headers.IndexOf("WORD LENGTH"); //fallback to LENGTH

            if (wordIndex == -1 || hintIndex == -1 || charIndex == -1)
            {
                Debug.LogError("Invalid CSV file. Required columns: WORD, HINT, CHARACTERS.");

                return;
            }

            try
            {
                if(lines.Length > 251)
                {
                    int total = lines.Length - 1;

                    for (int i = 1; i < lines.Length; i++)
                    {
                        // Show progress bar every 250 rows (so it doesn’t slow parsing)
                        if (i % 250 == 0)
                        {
                            float progress = (float)i / total;

                            EditorUtility.DisplayProgressBar(
                                "Parsing CSV...",
                                $"Processing row {i} of {total}",
                                progress
                            );
                        }
                    }
                }

                // Create WordHintStruct for each data row and add to wordHintList
                for (int i = 1; i < lines.Length; i++)
                {
                    var cells = SafelyParseCSVLine(lines[i]);

                    if (cells.Count <= Mathf.Max(wordIndex, hintIndex, charIndex))
                        continue;

                    string word = cells[wordIndex].Trim();

                    string hint = cells[hintIndex].Trim();

                    string charValue = cells[charIndex].Trim();

                    if (string.IsNullOrEmpty(word))
                        continue;

                    // Remove any non-digit characters just in case (quotes, spaces, etc.)
                    //charValue = new string(charValue.Where(char.IsDigit).ToArray());

                    int wordLength = word.Length;

                    /*if (!string.IsNullOrEmpty(charValue))
                        int.TryParse(charValue, out wordLength);

                    if (wordLength != word.Length)
                    {
                        wordLength = word.Length;
                    }*/

                    wordHintList.Add(new WordHintStruct
                    {
                        word = word,

                        hint = hint,

                        wordLength = wordLength
                    });
                }

                Debug.Log($"Successfully parsed {wordHintList.Count} entries from CSV.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error while parsing CSV: " + ex.Message);
            }
            finally
            {
                // Always clear the progress bar even if an error occurs
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// CSV-safe line splitter (handles quotes, commas, etc.)
        /// </summary>
        private List<string> SafelyParseCSVLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            char quoteChar = '\0';
            string current = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                // Handle starting or ending quotes
                if ((c == '"' || c == '\''))
                {
                    if (!inQuotes)
                    {
                        inQuotes = true;
                        quoteChar = c; // Track which quote style started the field
                    }
                    else if (quoteChar == c)
                    {
                        // Check for escaped quotes ('', "")
                        if (i + 1 < line.Length && line[i + 1] == quoteChar)
                        {
                            current += c; // Add one literal quote
                            i++; // Skip next
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current += c; // Different quote inside quoted section (e.g., "it's")
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.Trim());
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            // Add last field
            result.Add(current.Trim());

            return result;
        }

        //EDITOR....................................................................................................................

#if UNITY_EDITOR

        [CustomEditor(typeof(WordSetSO))]
        private class WordSetSOEditor : Editor
        {
            private WordSetSO wordSetSO;

            private SerializedProperty csvFileProp;

            private SerializedProperty wordColumnNameProp;

            private SerializedProperty hintColumnNameProp;

            private SerializedProperty charactersColumnNameProp;

            private SerializedProperty entriesToShowProp;

            private SerializedProperty wordHintListProp;

            private bool isWordHintListFoldoutOpened = true;

            private void OnEnable()
            {
                wordSetSO = target as WordSetSO;

                csvFileProp = serializedObject.FindProperty("wordHintCSV");

                wordColumnNameProp = serializedObject.FindProperty("wordColumnName");

                hintColumnNameProp = serializedObject.FindProperty("hintColumnName");

                charactersColumnNameProp = serializedObject.FindProperty("charactersColumnName");

                entriesToShowProp = serializedObject.FindProperty("entriesToShow");

                wordHintListProp = serializedObject.FindProperty("wordHintList");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                if(csvFileProp != null) EditorGUILayout.PropertyField(csvFileProp);

                if(wordColumnNameProp != null) EditorGUILayout.PropertyField(wordColumnNameProp);

                if(hintColumnNameProp != null) EditorGUILayout.PropertyField(hintColumnNameProp);

                if(charactersColumnNameProp != null) EditorGUILayout.PropertyField(charactersColumnNameProp);

                EditorGUILayout.Space();

                if (entriesToShowProp != null) EditorGUILayout.PropertyField(entriesToShowProp);

                EditorGUILayout.Space();

                DrawReadOnlyList(wordHintListProp, wordSetSO.entriesToShow, ref isWordHintListFoldoutOpened);

                EditorGUILayout.Space(15);

                EditorGUILayout.HelpBox("Use the button to validate and generate words and hints list from the CSV file.", MessageType.Info);
            
                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                {
                    if (GUILayout.Button("Generate Words List From CSV"))//On Generate Grid button pressed:...
                    {
                        wordSetSO.GenerateWordSetsListFromCSV();

                        EditorUtility.SetDirty(this);
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }

            private void DrawReadOnlyList(SerializedProperty listProp, int entriesToShow, ref bool foldout)
            {
                if(entriesToShow < 1) entriesToShow = 1;

                // Toggle foldout
                foldout = EditorGUILayout.Foldout(foldout, $"Parsed Word Hint List ({listProp.arraySize} entries)", true);

                if (!foldout) return;

                EditorGUI.indentLevel++;

                int size = listProp.arraySize;

                if (size == 0)
                {
                    EditorGUILayout.LabelField("No entries parsed yet.");

                    EditorGUI.indentLevel--;

                    return;
                }

                int maxDisplay = Mathf.Min(size, entriesToShow);

                for (int i = 0; i < maxDisplay; i++)
                {
                    var element = listProp.GetArrayElementAtIndex(i);

                    if (element == null) continue;

                    // Find subfields
                    var wordProp = element.FindPropertyRelative("word");

                    var hintProp = element.FindPropertyRelative("hint");

                    var lengthProp = element.FindPropertyRelative("wordLength");

                    // Draw box-like element
                    EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.LabelField($"Element {i}", EditorStyles.boldLabel);

                    EditorGUI.indentLevel++;

                    DrawReadOnlyField("Word", wordProp?.stringValue);

                    DrawReadOnlyField("Hint", hintProp?.stringValue, true); // multi-line

                    DrawReadOnlyField("Word Length", lengthProp?.intValue.ToString());

                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndVertical();
                }

                if (size > maxDisplay)
                {
                    EditorGUILayout.HelpBox($"Showing first {maxDisplay} of {size} entries for performance.", MessageType.Info);
                }

                EditorGUI.indentLevel--;
            }

            private void DrawReadOnlyField(string label, string value, bool multiLine = false)
            {
                EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);

                GUI.enabled = false; // make it look read-only

                if (multiLine)
                {
                    EditorGUILayout.TextArea(value ?? string.Empty, GUILayout.MinHeight(40));
                }
                else
                {
                    EditorGUILayout.TextField(value ?? string.Empty);
                }

                GUI.enabled = true; // restore GUI state
            }
        }
    }
#endif
}
