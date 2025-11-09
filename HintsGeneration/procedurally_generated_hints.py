# ============================================================
# procedurally_generated_hints_v41.py
# ------------------------------------------------------------
# Crossword-style hint generator (offline procedural)
# ------------------------------------------------------------
# Unified fallback-style generation with:
# ✅ Full-definition logic
# ✅ Blank-line cleanup
# ✅ Configurable MAX_SENTENCES and MAX_WORDS
# ✅ Smart trimming (finish last sentence even if exceeding limit)
# ✅ Ignores colons (:), semicolons (;), numeric dots (3.5), list numbers (1.), and asterisks (*)
# ✅ Counts "quoted." endings as sentence boundaries
# ✅ Detects and logs blank hints
# 🚫 No meta-phrase exclusion
# 🚫 No "Overall" rule
# ============================================================

import pandas as pd
import re
import string
from tqdm.auto import tqdm

# ---------- CONFIG ----------
INPUT_CSV = "Filtered_WordDB_ModernNoNames.csv"
OUTPUT_CSV = "Filtered_WordDB_Procedural_Hints.csv"
MAX_WORDS = 100
MAX_SENTENCES = 2   # Adjustable number of sentences per hint

# ---------- BASIC CLEANING ----------
def clean_definition(text):
    """Removes parentheses, formatting, and redundant punctuation."""
    if not isinstance(text, str) or not text.strip():
        return ""
    # Remove parentheses and nested content
    while re.search(r"\([^()]*\)", text):
        text = re.sub(r"\([^()]*\)", "", text)
    text = re.sub(r"\(\s*\)", "", text)

    # Normalize punctuation (keep colons and semicolons)
    text = re.sub(r"\s+[.,]\s*", ". ", text)
    text = re.sub(r"[•→/\\\-<>\*]+", " ", text)
    text = re.sub(r"\*{1,2}", "", text)
    text = re.sub(r"_+", "", text)
    text = re.sub(r"\b\d+\s*[\.\)]\s*", "", text)
    text = re.sub(r"\s+", " ", text.strip())
    text = re.sub(
        r"\b(?:noun|verb|adjective|adverb|preposition|conjunction|interjection|pronoun)\b[:\s]*",
        "",
        text,
        flags=re.IGNORECASE,
    )
    return text.strip(string.punctuation + " ")

# ---------- UTILITIES ----------
def remove_word_from_text(word, text):
    """Replaces occurrences of the target word with underscores."""
    pattern = re.compile(rf"\b{re.escape(word)}\w*\b", re.IGNORECASE)
    return pattern.sub("____", text)

def split_sentences(text):
    """
    Splits sentences while ignoring:
    - Dots inside numbers (e.g., 3.5, 2.0)
    - Dots after numeric list markers (e.g., 1., 2.)
    - Colons (:), semicolons (;), and asterisks (*) as sentence boundaries
    - ✅ Counts dots after closing quotes ("word." or “word.”) as sentence ends
    """
    # Protect decimals (3.5 → 3<DECIMAL>5)
    text = re.sub(r"(\d)\.(\d)", r"\1<DECIMAL>\2", text)

    # Protect list markers (e.g., "1.", "23.") → "1<ENUM>"
    text = re.sub(r"\b(\d+)\.(\s)", r"\1<ENUM>\2", text)

    # Protect asterisks, colons, and semicolons
    text = (
        text.replace("*", "<ASTERISK>")
        .replace(";", "<SEMICOLON>")
        .replace(":", "<COLON>")
    )

    # ✅ Split sentences, including quotes before punctuation
    sentences = re.split(
        r'(?:(?<=[.!?])["\'“”’])\s+|(?<=[.!?])\s+', text
    )

    # Restore protected markers
    sentences = [
        s.replace("<DECIMAL>", ".")
         .replace("<ENUM>", ".")
         .replace("<ASTERISK>", "*")
         .replace("<SEMICOLON>", ";")
         .replace("<COLON>", ":")
        for s in sentences
    ]

    return [s.strip() for s in sentences if s.strip()]

def trim_to_sentence_or_limit(text, max_words=MAX_WORDS, max_sentences=MAX_SENTENCES):
    """
    Trims text to a maximum number of sentences or words.
    If final sentence exceeds word limit, finish it instead of dropping.
    """
    sentences = split_sentences(text)
    trimmed_sentences = []
    total_words = 0

    for s in sentences[:max_sentences]:
        words_in_s = len(s.split())
        trimmed_sentences.append(s)
        total_words += words_in_s
        if total_words >= max_words:
            break  # stop adding further sentences, but keep current one

    return " ".join(trimmed_sentences).strip()

def _strip_leading_the_word_mask(text):
    """Removes leading 'The word ____' if followed by punctuation."""
    quotes = r"[\"'“”‘’]?"
    mask = r"\s*_+\s*"
    pattern = rf"^\s*(the\s+word\s+{quotes}{mask}{quotes})(?=\s*[.:;,\-–—!?…]+)"
    return re.sub(pattern, "", text, flags=re.IGNORECASE)

def finalize_hint(text):
    """Final cleanup and punctuation normalization."""
    text = re.sub(r"\.{2,}", ".", text)
    text = re.sub(r"\b\d+\s*[\.\)]\s*", "", text)
    text = _strip_leading_the_word_mask(text)
    text = re.sub(r"^(this\s+word\s+means\s+|it\s+means\s+)", "", text, flags=re.IGNORECASE)
    text = re.sub(r"\s+", " ", text.strip())
    text = text.strip(string.punctuation + " ")
    if not text:
        return ""
    if not text.endswith("."):
        text += "."
    return text[0].upper() + text[1:]

# ---------- MAIN GENERATOR ----------
def generate_hint(word, definition):
    """Generates a crossword-style hint without meta-phrase or 'Overall' filters."""
    if not isinstance(definition, str) or not definition.strip():
        return ""

    # 1️⃣ Remove internal blank lines
    lines = [ln.strip() for ln in definition.splitlines() if ln.strip()]
    full_def = " ".join(lines)

    # 2️⃣ Mask, trim, and finalize
    masked = remove_word_from_text(word, full_def)
    limited = trim_to_sentence_or_limit(masked, MAX_WORDS, MAX_SENTENCES)

    return finalize_hint(limited)

# ---------- MAIN ----------
def main():
    tqdm.pandas()
    print(f"🔹 Loading: {INPUT_CSV}")
    df = pd.read_csv(INPUT_CSV)

    # Generate hints
    df["HINT"] = df.progress_apply(lambda row: generate_hint(row["WORD"], row["DEFINITION"]), axis=1)

    # Identify blank hints and record the corresponding word
    df["Blank Hint Word"] = df.apply(
        lambda row: row["WORD"] if not str(row["HINT"]).strip() else "", axis=1
    )
    blank_count = df["Blank Hint Word"].astype(bool).sum()

    # Clean DEFINITION column (remove internal blanks)
    df["DEFINITION"] = df["DEFINITION"].apply(
        lambda d: " ".join([ln.strip() for ln in str(d).splitlines() if ln.strip()])
    )

    # Reorder columns
    if {"WORD", "DEFINITION", "HINT"}.issubset(df.columns):
        cols = list(df.columns)
        cols.insert(cols.index("DEFINITION"), cols.pop(cols.index("HINT")))
        df = df[cols]

    df.to_csv(OUTPUT_CSV, index=False)

    print(
        f"\n✅ Done! Saved crossword-style hints to {OUTPUT_CSV}\n"
        f"   (Max Sentences: {MAX_SENTENCES}, Max Words: {MAX_WORDS})"
    )
    print(f"⚠️ Blank hints detected: {blank_count}")

if __name__ == "__main__":
    main()
