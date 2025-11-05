using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace CrossClimbLite
{
    [CreateAssetMenu(fileName = "WordSetDataSO", menuName = "Scriptable Objects/WordSetDataSO")]
    public class WordSetDataSO : ScriptableObject, ISerializationCallbackReceiver
    {
        [Header("CSV Source")]

        [Space(10)]

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

        [Space(10)]

        [SerializeField]
        [Min(1)]
        [FormerlySerializedAs("wordHintEntriesToShow")]
        private int wordHintEntriesToShow = 40;

        [SerializeField]
        [FormerlySerializedAs("wordHintList")]
        private List<WordHintStruct> wordHintList = new List<WordHintStruct>();

        [Header("Word Sets List")]

        [Space(10)]

        [SerializeField]
        [Min(1)]
        [FormerlySerializedAs("wordSetsEntriesToShow")]
        private int wordSetsEntriesToShow = 20;

        [SerializeField]
        private List<List<WordHintStruct>> allWordChains = new List<List<WordHintStruct>>();

        [Serializable]
        public class SingleWordChainWrapper
        {
            [SerializeField]
            [FormerlySerializedAs("singleWordChain")]
            private List<WordHintStruct> singleWordChain = new List<WordHintStruct>();

            public SingleWordChainWrapper(List<WordHintStruct> singleWordChain)
            {
                this.singleWordChain = singleWordChain;
            }

            public List<WordHintStruct> GetWordChain()
            {
                return singleWordChain;
            }
        }

        [SerializeField]
        [FormerlySerializedAs("allWordSetsList")]
        private List<SingleWordChainWrapper> allWordSetsList = new List<SingleWordChainWrapper>();

        [SerializeField]
        //Key: length group num | Value: index entry in allWordSetsList
        private Dictionary<int, int> wordSetsLengthGroupStartIndexDict = new Dictionary<int, int>();

        [SerializeField]
        [HideInInspector]
        private List<int> wordSetsLengthGroupStartIndexDictKeys = new List<int>();

        [SerializeField]
        [HideInInspector]
        private List<int> wordSetsLengthGroupStartIndexDictValues = new List<int>();

        [Serializable]
        public struct WordHintStruct
        {
            [SerializeField]
            public string word;

            [SerializeField]
            public string hint;

            [SerializeField]
            public int wordLength;
        }

        private bool GenerateWordsListFromCSV()
        {
            Debug.Log("Begins Generating Word Hint List from CSV");

            if(!wordHintCSV)
            {
                Debug.LogWarning("WordSetSO: No CSV file assigned.");

                return false;
            }

            if (wordHintList == null) wordHintList = new List<WordHintStruct>();

            else wordHintList.Clear();

            float timeStart = Time.realtimeSinceStartup;

            float timeTook = 0.0f;

            //PROCESS AND VALIDATE CSV FILE CONTENT.............................................................................................................

            // Check for empty csv or data rows
            // if csv only has less than 1 line (row), then it only has a header row or is empty, thus log warning and return
            var lines = wordHintCSV.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length <= 1)
            {
                Debug.LogWarning("CSV file appears empty or has no data rows.");

                return false;
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

                return false;
            }

            //BEGIN GENERATING WORD HINT LIST...............................................................................................................

            bool successfullyGenerated = true;

            try
            {
                if(Application.isEditor)
                {
                    if (lines.Length > 251)
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

                    int wordLength = word.Length;

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

                successfullyGenerated = false;
            }
            finally
            {
                // Always clear the progress bar even if an error occurs
                if (Application.isEditor) EditorUtility.ClearProgressBar();
            }

            //SORT WORD HINT LIST BY WORD LENGTH.................................................................................................................

            if (successfullyGenerated)
            {
                try
                {
                    if (Application.isEditor) EditorUtility.DisplayProgressBar("Sorting List...", "Sorting list by word legnth", 0.0f);

                    SortWordHintListByLength();

                    if (Application.isEditor) EditorUtility.DisplayProgressBar("Sorting List...", "Sorting list by word legnth", 1.0f);
                }
                finally
                {
                    if (Application.isEditor) EditorUtility.ClearProgressBar();
                }
            }

            timeTook = Time.realtimeSinceStartup - timeStart;

            timeTook = (float)Math.Round(timeTook, 2);

            Debug.Log($"Finished generating Word Hint list from CSV. Time took: {timeTook}s");

            return successfullyGenerated;
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

        private void SortWordHintListByLength()
        {
            if (wordHintList == null || wordHintList.Count == 0)
            {
                Debug.LogWarning("Cannot sort list by word length - word hint list is empty or uninitialized.");

                return;
            }

            wordHintList.Sort((a, b) =>
            {
                int compare = a.wordLength.CompareTo(b.wordLength);

                // Optional: stable tie-breaker (alphabetical by word)
                if (compare == 0)
                    compare = string.Compare(a.word, b.word, System.StringComparison.OrdinalIgnoreCase);

                return compare;
            });

            Debug.Log("Successfully sorted Word hint list by word length (shortest to longest).");
        }

        private bool GenerateWordChainsList()
        {
            float startTime = Time.realtimeSinceStartup;

            float timeTook = 0.0f;

            Debug.Log("Begins Generating All Word Sets List");

            if(wordHintList == null || wordHintList.Count == 0)
            {
                if (!GenerateWordsListFromCSV()) return false;
            }

            allWordChains.Clear();

            allWordChains = GroupByOneCharacterChains();

            if (allWordChains == null || allWordChains.Count == 0)
            {
                Debug.LogError("No Word Chains Could Be Generated. Please check error logs for more info.");

                timeTook = Time.realtimeSinceStartup - startTime;

                timeTook = (float)Math.Round(timeTook, 2);

                Debug.Log($"Word Sets List Generation Complete With Errors. Time took: {timeTook}");

                return false;
            }

            if (allWordSetsList == null) allWordSetsList = new List<SingleWordChainWrapper>();

            else allWordSetsList.Clear();

            for(int i = 0; i < allWordChains.Count; i++)
            {
                if (allWordChains[i] == null || allWordChains[i].Count == 0) continue;

                allWordSetsList.Add(new SingleWordChainWrapper(allWordChains[i]));
            }

            timeTook = Time.realtimeSinceStartup - startTime;

            timeTook = (float)Math.Round(timeTook, 2);

            Debug.Log($"Word Sets List Generation Complete Successfully. Time took: {timeTook}");

            return true;
        }

        /// <summary>
        /// Returns a list of lists (chains) where each sub-list or chain contains words that differ by only 1-char. 
        /// Each chain also has words that have the same number of characters length.
        /// </summary>
        /// <returns></returns>
        private List<List<WordHintStruct>> GroupByOneCharacterChains()
        {
            if (wordHintList == null || wordHintList.Count == 0)
            {
                Debug.LogWarning("Word list is empty — cannot form chains.");

                return new List<List<WordHintStruct>>();
            }

            //Group by word length
            var grouped = wordHintList.GroupBy(w => w.wordLength).OrderBy(g => g.Key).ToList();

            var allChains = new List<List<WordHintStruct>>();

            int totalGroups = grouped.Count;

            int processedGroups = 0;

            try
            {
                //iterates through each word length group (e.g group of 4 chars words then group of 5 chars words)
                foreach (var group in grouped)
                {
                    processedGroups++;

                    string currentGroupLog = $"Processing chars length group: {group.Key} chars words";

                    float progress = (float)processedGroups / totalGroups;

                    if (Application.isEditor) EditorUtility.DisplayProgressBar("Generating Word Chains", currentGroupLog, progress);

                    //init a list of WordHintStruct from the current word length group 
                    var words = new List<WordHintStruct>(group);

                    //init a hash set (no duplicate) of all the unique words in the current word length group
                    var unvisited = new HashSet<string>(words.Select(w => w.word));

                    //init a list of chains of words that differ by 1 char (each chain is a list and all chains are put in a bigger list)
                    //this chains list is only for this current word length iteration
                    var chainsForThisLength = new List<List<WordHintStruct>>();

                    //iterate through the unvisited words
                    while (unvisited.Count > 0)
                    {
                        // Start a new chain from the first univisited word
                        var startWord = words.FirstOrDefault(w => unvisited.Contains(w.word));

                        //unvisited words hash set is empty -> break and go to next word length group iteration
                        if (string.IsNullOrEmpty(startWord.word)) break;

                        //init the chain list for the picked univisited word 
                        var currentChain = new List<WordHintStruct> { startWord };

                        //remove picked word as its now visited
                        unvisited.Remove(startWord.word);

                        bool addedNew;

                        do
                        {
                            addedNew = false;//reset addedNew's status to avoid infinite while loop

                            WordHintStruct? nextWord = null;

                            //iterate through all the words in the wordhint list to find valid words with 1 char diff from the picked word above
                            foreach (var candidate in words)
                            {
                                //if current word already visited -> go next
                                if (!unvisited.Contains(candidate.word))
                                    continue;

                                /*
                                 * The ComputeEditDistance function below uses Levenshtein Distance algorithm (edit distance)
                                 * That is:
                                 * The minimum number of single-character insertions, deletions, or substitutions required to transform one word into another.
                                 * A distance of 1 means the words differ by exactly one character.
                                 * If for a current word being processed, no other words have a distance of 1 (differ by 1 char) to the current word,
                                 * the word-chain of that word is now finished and a new chain is started.
                                 */

                                //currentChain[^1] = currentChain[currentChain.Count - 1]
                                if (ComputeEditDistance(currentChain[^1].word, candidate.word) == 1)
                                {
                                    nextWord = candidate;//valid 1-char diff word found -> exit loop

                                    break;
                                }
                            }

                            //if a valid next word with distance of 1 (differ by 1 char) is found from above iteration ^
                            if (nextWord.HasValue)
                            {
                                currentChain.Add(nextWord.Value);//add to current chain of the word being processed

                                unvisited.Remove(nextWord.Value.word);//remove from visited words hash

                                addedNew = true;
                            }

                        } while (addedNew);//if previously added a new word to the current chain successfully -> continue loop

                        //else

                        //current chain has finished with all the possible 1-char diff words for the picked word
                        //add current chain to the list of chains for the current word length group and starts next chain for the next unvisited word
                        chainsForThisLength.Add(currentChain);
                    }

                    if (wordSetsLengthGroupStartIndexDict == null) wordSetsLengthGroupStartIndexDict = new Dictionary<int, int>();

                    if(allChains.Count <= 1) wordSetsLengthGroupStartIndexDict.TryAdd(group.Key, 0);

                    else wordSetsLengthGroupStartIndexDict.TryAdd(group.Key, allChains.Count);

                    //once all chains for the current word length group are found and processed -> add to the all chains list
                    //start next word length group iteration to find chains for next word length group
                    allChains.AddRange(chainsForThisLength);

                    Debug.Log($"Found {chainsForThisLength.Count} chain(s) for word length {group.Key}.");
                }

                Debug.Log($"Completed: {allChains.Count} total one-character-difference chains created.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error while generating chains: {ex.Message}");
            }
            finally
            {
                // Always clear progress bar
                if (Application.isEditor) EditorUtility.ClearProgressBar();
            }

            return allChains;
        }

        /// <summary>
        /// Checks the number of chars diff bt/ word string "a" and "b"
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int ComputeEditDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return int.MaxValue;

            if (Mathf.Abs(a.Length - b.Length) > 1)
                return int.MaxValue; // Early reject if lengths differ too much

            int[,] dp = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;

            for (int j = 0; j <= b.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = (a[i - 1] == b[j - 1]) ? 0 : 1;

                    dp[i, j] = Mathf.Min(
                        Mathf.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost
                    );
                }
            }

            return dp[a.Length, b.Length];
        }

        public List<WordHintStruct> GetRandomWordSetRuntime(int wordLengthRequired, int wordCountRequired)
        {
            List<WordHintStruct> finalWordSet = new List<WordHintStruct>();

            bool validRandomSetFound = false;

            if (allWordSetsList == null || 
               allWordSetsList.Count == 0 || 
               wordSetsLengthGroupStartIndexDict == null || 
               wordSetsLengthGroupStartIndexDict.Count == 0)
            {
                if(!GenerateWordChainsList()) return finalWordSet;
            }

            if (allWordSetsList == null || allWordSetsList.Count == 0) return finalWordSet;

            if (wordSetsLengthGroupStartIndexDict == null || wordSetsLengthGroupStartIndexDict.Count == 0) return finalWordSet;

            if (wordCountRequired < 3)
            {
                Debug.LogWarning($"Invalid word count provided: {wordCountRequired}. " +
                                 "Word count must not be less than 3 when getting random word chain!");

                wordCountRequired = 3;
            }

            if (wordLengthRequired < 3)
            {
                Debug.LogWarning($"Invalid word length provided: {wordLengthRequired}. " +
                                 "Word length must not be less than 3 when getting random word chain!");

                wordLengthRequired = 3;
            }

            if (!wordSetsLengthGroupStartIndexDict.ContainsKey(wordLengthRequired))
            {
                wordLengthRequired = wordSetsLengthGroupStartIndexDict.ElementAt(wordSetsLengthGroupStartIndexDict.Count - 1).Key;
            }

            int wordSetsRangeStart = wordSetsLengthGroupStartIndexDict[wordLengthRequired];

            int wordSetsRangeEnd = allWordSetsList.Count - 1;

            if (wordSetsLengthGroupStartIndexDict.ContainsKey(wordLengthRequired + 1))
            {
                wordSetsRangeEnd = wordSetsLengthGroupStartIndexDict[wordLengthRequired + 1] - 1;
            }

            try
            {
                List<SingleWordChainWrapper> wordSetsInLengthGroup = new List<SingleWordChainWrapper>();
                
                wordSetsInLengthGroup.AddRange(allWordSetsList.GetRange(wordSetsRangeStart, wordSetsRangeEnd - wordSetsRangeStart));

                int rand = UnityEngine.Random.Range(0, wordSetsInLengthGroup.Count);

                int previousRand = -1;

                int maxIteration = 30;

                int count = 0;

                while (count <= maxIteration && !validRandomSetFound)
                {
                    if (previousRand != -1 && previousRand == rand)
                    {
                        rand = UnityEngine.Random.Range(0, wordSetsInLengthGroup.Count);

                        count++;

                        continue;
                    }

                    previousRand = rand;

                    if (wordSetsInLengthGroup[rand] == null ||
                        wordSetsInLengthGroup[rand].GetWordChain() == null ||
                        wordSetsInLengthGroup[rand].GetWordChain().Count == 0 ||
                        wordSetsInLengthGroup[rand].GetWordChain().Count < wordCountRequired)
                    {
                        //fall back to finding the first valid word chain in all possible chains if all random picks before and at max iteration failed
                        if (count == maxIteration)
                        {
                            for (int i = 0; i < wordSetsInLengthGroup.Count; i++)
                            {
                                if (wordSetsInLengthGroup[i] == null ||
                                    wordSetsInLengthGroup[i].GetWordChain() == null ||
                                    wordSetsInLengthGroup[i].GetWordChain().Count == 0 ||
                                    wordSetsInLengthGroup[i].GetWordChain().Count < wordCountRequired) continue;

                                finalWordSet = wordSetsInLengthGroup[i].GetWordChain();

                                validRandomSetFound = true;

                                break;
                            }
                        }

                        rand = UnityEngine.Random.Range(0, wordSetsInLengthGroup.Count);

                        count++;

                        continue;
                    }

                    finalWordSet = wordSetsInLengthGroup[rand].GetWordChain();

                    validRandomSetFound = true;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Encountered Fatal Error while trying to generate a random word set. \n{ex}");
            }
            finally
            {
                if (validRandomSetFound)
                {
                    if (finalWordSet.Count > wordCountRequired)
                    {
                        finalWordSet = finalWordSet.GetRange(UnityEngine.Random.Range(0, finalWordSet.Count - wordCountRequired), wordCountRequired);
                    }
                }
                else
                {
                    Debug.LogWarning("Iterations could not find a valid word set." +
                                 "\nFalling back to the very first word set in current word length group as the final option.");

                    //fallback to first word chain 
                    finalWordSet = allWordSetsList[wordSetsRangeStart].GetWordChain();

                    if (finalWordSet == null || finalWordSet.Count == 0)
                    {
                        Debug.LogError("Invalid first word set (null or empty). Word Set Retrieval Failed!");
                    }
                    else
                    {
                        if (finalWordSet.Count < wordCountRequired)
                        {
                            Debug.LogError("Invalid word count in the first word set. Word Set Retrieved Partially!");
                        }
                        else if (finalWordSet.Count > wordCountRequired)
                        {
                            finalWordSet = finalWordSet.GetRange(UnityEngine.Random.Range(0, finalWordSet.Count - wordCountRequired), wordCountRequired);
                        }
                    }
                }
            }
        
            return finalWordSet;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (wordSetsLengthGroupStartIndexDict == null || wordSetsLengthGroupStartIndexDict.Count == 0) return;

            if (wordSetsLengthGroupStartIndexDictKeys == null) wordSetsLengthGroupStartIndexDictKeys = new List<int>();

            wordSetsLengthGroupStartIndexDictKeys.Clear();

            if(wordSetsLengthGroupStartIndexDictValues == null) wordSetsLengthGroupStartIndexDictValues = new List<int>();

            wordSetsLengthGroupStartIndexDictValues.Clear();

            foreach (KeyValuePair<int, int> pair in wordSetsLengthGroupStartIndexDict)
            {
                wordSetsLengthGroupStartIndexDictKeys.Add(pair.Key);

                wordSetsLengthGroupStartIndexDictValues.Add(pair.Value);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (wordSetsLengthGroupStartIndexDictKeys == null || wordSetsLengthGroupStartIndexDictValues == null) return;

            if (wordSetsLengthGroupStartIndexDictKeys.Count == 0 || wordSetsLengthGroupStartIndexDictValues.Count == 0) return;

            if (wordSetsLengthGroupStartIndexDictKeys.Count != wordSetsLengthGroupStartIndexDictValues.Count) return;

            if(wordSetsLengthGroupStartIndexDict == null) wordSetsLengthGroupStartIndexDict = new Dictionary<int, int>();

            wordSetsLengthGroupStartIndexDict.Clear();

            for(int i = 0; i <  wordSetsLengthGroupStartIndexDictKeys.Count; i++)
            {
                int key = wordSetsLengthGroupStartIndexDictKeys[i];

                int value = wordSetsLengthGroupStartIndexDictValues[i];

                wordSetsLengthGroupStartIndexDict.TryAdd(key, value);
            }
        }

        //EDITOR..................................................................................................................................................

#if UNITY_EDITOR

        [CustomEditor(typeof(WordSetDataSO))]
        private class WordSetDataSOEditor : Editor
        {
            private WordSetDataSO wordSetDataSO;

            private SerializedProperty csvFileProp;

            private SerializedProperty wordColumnNameProp;

            private SerializedProperty hintColumnNameProp;

            private SerializedProperty charactersColumnNameProp;

            private SerializedProperty wordHintEntriesToShowProp;

            private SerializedProperty wordHintListProp;

            private SerializedProperty wordSetsEntriesToShowProp;

            private SerializedProperty allWordSetsListProp;

            private bool isWordHintListFoldoutOpened = true;

            private bool isAllWordSetsFoldoutOpened = true;

            private Vector2 wordHintListScrollPos;

            private Vector2 wordSetsListScrollPos;

            private void OnEnable()
            {
                wordSetDataSO = target as WordSetDataSO;

                csvFileProp = serializedObject.FindProperty("wordHintCSV");

                wordColumnNameProp = serializedObject.FindProperty("wordColumnName");

                hintColumnNameProp = serializedObject.FindProperty("hintColumnName");

                charactersColumnNameProp = serializedObject.FindProperty("charactersColumnName");

                wordHintEntriesToShowProp = serializedObject.FindProperty("wordHintEntriesToShow");

                wordHintListProp = serializedObject.FindProperty("wordHintList");

                wordSetsEntriesToShowProp = serializedObject.FindProperty("wordSetsEntriesToShow");

                allWordSetsListProp = serializedObject.FindProperty("allWordSetsList");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                if(csvFileProp != null) EditorGUILayout.PropertyField(csvFileProp);

                if(wordColumnNameProp != null) EditorGUILayout.PropertyField(wordColumnNameProp);

                if(hintColumnNameProp != null) EditorGUILayout.PropertyField(hintColumnNameProp);

                if(charactersColumnNameProp != null) EditorGUILayout.PropertyField(charactersColumnNameProp);

                EditorGUILayout.Space();

                if (wordHintEntriesToShowProp != null) EditorGUILayout.PropertyField(wordHintEntriesToShowProp);

                EditorGUILayout.Space();

                DrawReadOnlyList(wordHintListProp, 
                                 wordSetDataSO.wordHintEntriesToShow, 
                                 true, 
                                 ref wordHintListScrollPos, 
                                 ref isWordHintListFoldoutOpened);

                EditorGUILayout.Space(15);

                EditorGUILayout.HelpBox("Use the button to validate and generate words and hints list from the CSV file.", MessageType.Info);
            
                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                {
                    if (GUILayout.Button("Generate Words List From CSV"))//On Generate Grid button pressed:...
                    {
                        wordSetDataSO.GenerateWordsListFromCSV();

                        EditorUtility.SetDirty(wordSetDataSO);
                    }
                }

                EditorGUILayout.Space(15);

                if (wordSetsEntriesToShowProp != null) EditorGUILayout.PropertyField(wordSetsEntriesToShowProp);

                EditorGUILayout.Space();

                DrawReadOnlyList(allWordSetsListProp, 
                                 wordSetDataSO.wordSetsEntriesToShow, 
                                 true, 
                                 ref wordSetsListScrollPos, 
                                 ref isAllWordSetsFoldoutOpened);

                EditorGUILayout.Space(15);

                EditorGUILayout.HelpBox("Use the button below to generate all possible word sets " +
                                        "where each set has words that differ by only 1 character from one another.",
                                        MessageType.Info);

                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                {
                    if (GUILayout.Button("Generate All Word Sets"))//On Generate Grid button pressed:...
                    {
                        wordSetDataSO.GenerateWordChainsList();

                        EditorUtility.SetDirty(wordSetDataSO);
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }

            private void DrawReadOnlyList(SerializedProperty listProp, int entriesToShow, bool canScroll, ref Vector2 scrollPos, ref bool foldout)
            {
                if (listProp == null) return;

                if(entriesToShow < 1) entriesToShow = listProp.arraySize;

                // Toggle foldout
                foldout = EditorGUILayout.Foldout(foldout, $"{listProp.displayName} ({listProp.arraySize} entries)", true);

                if (!foldout) return;

                EditorGUI.indentLevel++;

                int size = listProp.arraySize;

                if (size == 0)
                {
                    EditorGUILayout.LabelField("List is empty.");

                    EditorGUI.indentLevel--;

                    return;
                }

                int maxDisplay = Mathf.Min(size, entriesToShow);

                // Begin scrollable view
                //scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(500));
                if(canScroll) 
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(Mathf.Max(EditorGUIUtility.currentViewWidth, 500.0f)));

                for (int i = 0; i < maxDisplay; i++)
                {
                    var element = listProp.GetArrayElementAtIndex(i);

                    if (element == null) continue;

                    if (element.isArray)
                    {
                        DrawReadOnlyListNoScroll(element, element.arraySize, ref foldout);

                        continue;
                    }

                    // Find subfields
                    var wordProp = element.FindPropertyRelative("word");

                    var hintProp = element.FindPropertyRelative("hint");

                    var lengthProp = element.FindPropertyRelative("wordLength");

                    var singleWordChainsProp = element.FindPropertyRelative("singleWordChain");

                    // Draw box-like element
                    EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.LabelField($"Element {i}", EditorStyles.boldLabel);

                    EditorGUI.indentLevel++;

                    if(wordProp != null) DrawReadOnlyField("Word", wordProp?.stringValue);

                    if (hintProp != null) DrawReadOnlyField("Hint", hintProp?.stringValue, true); // multi-line

                    if(lengthProp != null) DrawReadOnlyField("Word Length", lengthProp?.intValue.ToString());

                    if(singleWordChainsProp != null) DrawReadOnlyListNoScroll(singleWordChainsProp, singleWordChainsProp.arraySize, ref foldout);

                    EditorGUI.indentLevel--;

                    EditorGUILayout.EndVertical();
                }

                if (size > maxDisplay)
                {
                    EditorGUILayout.HelpBox($"Showing first {maxDisplay} of {size} entries.", MessageType.Info);
                }

                if(canScroll) EditorGUILayout.EndScrollView();

                EditorGUI.indentLevel--;
            }

            private void DrawReadOnlyListNoScroll(SerializedProperty listProp, int entriesToShow, ref bool foldout)
            {
                Vector2 scrollPos = Vector2.zero;

                DrawReadOnlyList(listProp, entriesToShow, false, ref scrollPos, ref foldout);
            }

            private void DrawReadOnlyField(string label, string value, bool multiLine = false)
            {
                EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);

                GUI.enabled = false; // make it look read-only

                if (multiLine)
                {
                    // Wrap text manually inside a word-wrapped area
                    var textStyle = new GUIStyle(EditorStyles.textArea)
                    {
                        wordWrap = true,

                        stretchHeight = true,
                    };

                    if (string.IsNullOrEmpty(value)) value = string.Empty;

                    float textHeight = textStyle.CalcHeight(new GUIContent(value), EditorGUIUtility.currentViewWidth - 35);

                    float clampedHeight = Mathf.Min(textHeight + 14, 400); // max 300px

                    EditorGUILayout.TextArea(value ?? string.Empty, textStyle, 
                                             GUILayout.MinHeight(40), 
                                             GUILayout.Height(clampedHeight),
                                             GUILayout.ExpandHeight(true));
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
