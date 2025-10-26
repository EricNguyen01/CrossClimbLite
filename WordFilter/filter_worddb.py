import pandas as pd
import zipfile
import re
from tqdm import tqdm
from wordfreq import zipf_frequency

# === CONFIGURATION ===
CSV_PATH = "WordDB.csv"          # Input file
NAMES_ZIP = "names.zip"          # NLTK names list
OUTPUT_PATH = "Filtered_WordDB_ModernNoNames.csv"
FREQ_THRESHOLD = 2.3             # 3.0 = common modern English words

# === ASK USER FOR LENGTH RANGE (with validation) ===
DEFAULT_MIN, DEFAULT_MAX = 2, 15

try:
    MIN_LEN = int(input(f"Enter minimum word length (default {DEFAULT_MIN}): ").strip() or DEFAULT_MIN)
    MAX_LEN = int(input(f"Enter maximum word length (default {DEFAULT_MAX}): ").strip() or DEFAULT_MAX)

    # Validate values
    if MIN_LEN < 1 or MAX_LEN < 1 or MIN_LEN > MAX_LEN or MAX_LEN > 30:
        print(f"⚠️ Invalid range entered ({MIN_LEN}-{MAX_LEN}). Resetting to defaults ({DEFAULT_MIN}-{DEFAULT_MAX}).")
        MIN_LEN, MAX_LEN = DEFAULT_MIN, DEFAULT_MAX

except ValueError:
    print(f"⚠️ Invalid input detected. Using default range ({DEFAULT_MIN}-{DEFAULT_MAX}).")
    MIN_LEN, MAX_LEN = DEFAULT_MIN, DEFAULT_MAX

print(f"✅ Using word length range: {MIN_LEN}-{MAX_LEN}")

# === LOAD NAME LIST ===
def extract_words_from_zip(zip_path):
    word_set = set()
    with zipfile.ZipFile(zip_path, 'r') as z:
        for filename in z.namelist():
            with z.open(filename) as f:
                try:
                    for line in f.read().decode('utf-8').splitlines():
                        line = line.strip()
                        if line:
                            word_set.add(line.lower())
                except UnicodeDecodeError:
                    continue
    return word_set

print("Loading name corpus...")
name_words = extract_words_from_zip(NAMES_ZIP)

# === READ CSV ===
print("Reading CSV file...")
df = pd.read_csv(CSV_PATH)

if "WORD" not in df.columns:
    raise ValueError("Your CSV must contain a column named 'WORD'.")
    
# Convert WORD column to lowercase right away
df["WORD"] = df["WORD"].astype(str).str.strip().str.lower()

# === LENGTH FILTER (before all other filters) ===
print(f"Filtering words by length range ({MIN_LEN}-{MAX_LEN})...")
df = df[df["WORD"].str.len().between(MIN_LEN, MAX_LEN)]

# === FILTER SETTINGS ===
blacklist_patterns = [
    r'ium$', r'us$', r'ae$', r'aceae$', r'idae$', r'virus$',
    r'tech$', r'corp$', r'inc$', r'solutions$', r'pharma$', r'systems$',
    r'press$', r'media$', r'university$', r'institute$', r'studios$'
]

definition_banned_keywords = [
    # People / names
    "name", "given name", "surname", "nickname", "personal name", "forename", "philosopher", "historian",
    # Geographic
    "city", "town", "village", "country", "province", "continent", "region",
    "river", "lake", "mountain", "island", "ocean", "bay", "harbor", "Africa", 
    "africa", "China", "china", "Asia", "asia", "Asian", "asian", 
    "Australia", "australia", "Australian", "australian", "New Zealand", "new zealand",
    "India", "india", "Indian", "indian", "Texan", "texan", "Thai", "thai", "Rome", "rome", 
    "Uzbek", "uzbek", "Ireland", "ireland", "Balkans", "balkan", "Arab", "arab", "Croat", "croat"
    "Britain", "britain", "Persia", "persia", 
    # Plants / animals / biology
    "plant", "tree", "flower", "species", "genus", """"animal", "bird",""" "bacterium",
    "virus", "fungus", "insect", "mammal", """"fish",""" "reptile", "amphibian",
    # Companies / brands / organizations
    "brand", "company", "corporation", "organization", "manufacturer",
    "university", "school", "college", "institute", "foundation",
    # Mythology / religion / culture
    "mythology", "god", "goddess", "deity", "hero", "legend", "saint",
    "planet", "star", "constellation", "galaxy", "solar system", 
    "republic", "Islam", "Islamic", "dialect", "dialects", "phonetic",
    "Jewish", "jewish", "nomadic", "semi-nomadic", "Hindu", "hindu", 
    "Tajik", "tajik", "Islam", "islam", "Amish", "amish", "American", "american"
    "Irish", "irish", "Arabic", "arabic"
    # Science / chemistry
    "chemical", "element", "compound", "mineral", "exon", "nixtamalized", "alkaline", 
    "PHP", "php", "zoology", "biology", 
    #Others
    "stein", "brule", "Ouija", "ouija", 
]

# === FILTER FUNCTION ===
def is_clean_word(row):
    word = str(row["WORD"]).strip().lower()
    definition = str(row["DEFINITION"]).lower() if "DEFINITION" in row and isinstance(row["DEFINITION"], str) else ""

    # Basic validity
    if not re.match("^[a-z]+$", word):
        return False

    # Exclude names directly
    if word in name_words:
        return False

    # Exclude by pattern
    for pat in blacklist_patterns:
        if re.search(pat, word):
            return False

    # Exclude by definition keywords
    for kw in definition_banned_keywords:
        if kw in definition:
            return False

    # Frequency-based filter (modern common English words)
    freq = zipf_frequency(word, "en")
    if freq < FREQ_THRESHOLD:
        return False

    return True

# === APPLY FILTER WITH PROGRESS BAR ===
print("Filtering dataset...")
tqdm.pandas(desc="Processing words")
filtered_df = df[df.progress_apply(is_clean_word, axis=1)]

# === REMOVE DUPLICATES (already lowercased) ===
print("Removing duplicates...")
initial_count = len(filtered_df)
filtered_df = filtered_df.drop_duplicates(subset=["WORD"], keep="first")
duplicates_removed = initial_count - len(filtered_df)

# === SAVE RESULT ===
filtered_df.to_csv(OUTPUT_PATH, index=False)

# === SUMMARY ===
total = len(df)
kept = len(filtered_df)
removed = total - kept
percent = kept / total * 100 if total > 0 else 0

print(f"\n✅ Filtering complete!")
print(f"Total rows processed: {total:,}")
print(f"Rows kept: {kept:,} ({percent:.2f}%)")
print(f"Rows removed: {removed:,}")
print(f"Duplicates removed: {duplicates_removed:,}")
print(f"Clean file saved as: {OUTPUT_PATH}")
