# ============================================================
# procedurally_generated_hints_v9.py
# ------------------------------------------------------------
# Offline crossword-style hint generator (definition-only version)
# Improvements:
# - Final repair pass for overly short hints
# - Ensures complete sentences by extending with next sentence if needed
# - Retains all previous cleanup (markdown, meta-phrases, numbering)
# ============================================================

import pandas as pd
import re
import string
from tqdm import tqdm
from pathlib import Path

# ---------- CONFIG ----------
INPUT_CSV = "Filtered_WordDB_ModernNoNames.csv"
OUTPUT_CSV = "Filtered_WordDB_Procedural_Hints.csv"
META_FILE = "meta_phrases.txt"
MAX_WORDS = 25
MIN_HINT_WORDS = 4  # If fewer words than this, try to repair

# ---------- BASIC CLEANING ----------
def clean_definition(text):
    if not isinstance(text, str) or not text.strip():
        return ""
    text = re.sub(r"[•→/\\\-;<>•]+", " ", text)
    text = re.sub(r"\s+", " ", text.strip())

    # Remove markdown bold/italic markers and underscores
    text = re.sub(r"\*{1,2}", "", text)
    text = re.sub(r"_+", "", text)

    # Remove parts of speech labels (noun, verb, etc.)
    text = re.sub(
        r"\b(?:noun|verb|adjective|adverb|preposition|conjunction|interjection|pronoun)\b[:\s]*",
        "",
        text,
        flags=re.IGNORECASE,
    )

    text = re.sub(r"\*+", "", text)
    return text.strip(string.punctuation + " ")

# ---------- META-PHRASE HANDLING ----------
def load_meta_phrases(filepath=META_FILE):
    defaults = [
        r"\bhas (a couple of|several|multiple|a few) meanings\b.*?(?=[.:\n]|$)",
        r"\bcan have (several|multiple) meanings\b.*?(?=[.:\n]|$)",
        r"\bhas (several|multiple) definitions\b.*?(?=[.:\n]|$)",
        r"\bhas (a couple of|several|multiple) meanings in English\b.*?(?=[.:\n]|$)",
        r"\bdepending on the context\b.*?(?=[.:\n]|$)",
        r"\band it has several related meanings\b.*?(?=[.:\n]|$)",
        r"\bas a noun\b.*?(?=[.:\n]|$)",
        r"\bsee also\b.*?(?=[.:\n]|$)",
    ]
    path = Path(filepath)
    if path.exists():
        try:
            with open(path, "r", encoding="utf-8") as f:
                patterns = [line.strip() for line in f if line.strip() and not line.startswith("#")]
            print(f"✅ Loaded {len(patterns)} meta-phrase patterns from {path}")
            return patterns
        except Exception as e:
            print(f"⚠️ Could not read {path}: {e}. Using defaults.")
            return defaults
    else:
        print(f"⚠️ Meta-phrase file not found ({path}). Using default patterns.")
        return defaults

def remove_meta_phrases(text, patterns):
    for pattern in patterns:
        text = re.sub(pattern, "", text, flags=re.IGNORECASE)
    return text.strip()

# ---------- UTILITIES ----------
def remove_word_from_text(word, text):
    pattern = re.compile(rf"\b{re.escape(word)}\w*\b", re.IGNORECASE)
    return pattern.sub("____", text)

def extract_first_numbered_definition(defn):
    defn = defn.strip()
    numbered_parts = re.split(r"(?:\b\d+\s*[\.\)])\s*", defn)
    if len(numbered_parts) > 1:
        first_def = numbered_parts[1]
    else:
        first_def = re.split(r"\b[2-9]\s*[\.\)]", defn)[0]
    first_def = re.split(r"\b[2-9]\s*[\.\)]", first_def)[0]
    return first_def.strip()

def extract_relevant_sentence(defn):
    first_def = extract_first_numbered_definition(defn)
    sentences = re.split(r"(?<=[.!?])\s+", first_def)
    for s in sentences:
        s = s.strip()
        if s and not re.search(r"example|e\.g\.", s, re.IGNORECASE):
            return s
    return first_def.strip()

def trim_to_sentence_or_limit(text, max_words=MAX_WORDS):
    words = text.split()
    if len(words) <= max_words:
        return text.strip()
    after_limit = " ".join(words[max_words:])
    match = re.search(r"([.!?])", after_limit)
    if match:
        end_index = max_words + len(after_limit[:match.start()].split())
        return " ".join(words[:end_index + 1]).strip()
    else:
        return " ".join(words[:max_words]).strip()

def finalize_hint(text):
    text = text.strip()
    text = re.sub(r"\s+", " ", text)
    text = text.strip(string.punctuation + " ")
    if not text:
        return ""
    if not text.endswith("."):
        text += "."
    text = text[0].upper() + text[1:]
    return text

# ---------- HINT GENERATOR ----------
def generate_hint(word, definition, meta_patterns):
    if not isinstance(definition, str) or not definition.strip():
        return ""
    defn = clean_definition(definition)
    defn = remove_meta_phrases(defn, meta_patterns)
    defn = remove_word_from_text(word, defn)
    if not defn:
        return ""
    main_sentence = extract_relevant_sentence(defn)
    hint = trim_to_sentence_or_limit(main_sentence, MAX_WORDS)
    return finalize_hint(hint)

# ---------- REPAIR SHORT HINTS ----------
def repair_short_hints(df, min_words=MIN_HINT_WORDS):
    """Fix short or incomplete hints by extending them with next sentence if available."""
    repaired = 0
    for idx, row in df.iterrows():
        hint = row["HINT"]
        definition = str(row["DEFINITION"])
        if not isinstance(hint, str) or len(hint.split()) < min_words:
            sentences = re.split(r"(?<=[.!?])\s+", definition)
            if len(sentences) > 1:
                extended_hint = sentences[0].strip() + " " + sentences[1].strip()
            else:
                extended_hint = definition.strip()
            df.at[idx, "HINT"] = finalize_hint(extended_hint)
            repaired += 1
    print(f"🔧 Repaired {repaired} short hints.")
    return df

# ---------- MAIN ----------
def main():
    meta_patterns = load_meta_phrases()

    print(f"🔹 Loading: {INPUT_CSV}")
    df = pd.read_csv(INPUT_CSV)

    tqdm.pandas(desc="Cleaning definitions")
    df["DEFINITION"] = df["DEFINITION"].progress_apply(clean_definition)

    tqdm.pandas(desc="Generating crossword-style hints")
    df["HINT"] = df.progress_apply(
        lambda row: generate_hint(row.WORD, row.DEFINITION, meta_patterns), axis=1
    )

    tqdm.pandas(desc="Repairing short hints")
    df = repair_short_hints(df)

    cols = list(df.columns)
    if "WORD" in cols and "DEFINITION" in cols and "HINT" in cols:
        cols.insert(cols.index("DEFINITION"), cols.pop(cols.index("HINT")))
        df = df[cols]

    df.to_csv(OUTPUT_CSV, index=False)
    print(f"\n✅ Done! Saved refined crossword-style hints to {OUTPUT_CSV}")

if __name__ == "__main__":
    main()

