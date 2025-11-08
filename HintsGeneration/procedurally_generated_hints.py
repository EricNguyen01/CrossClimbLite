# ============================================================
# procedurally_generated_hints_v34.py
# ------------------------------------------------------------
# Crossword-style hint generator (offline procedural)
# ------------------------------------------------------------
# Unified fallback-style generation with:
# ✅ Full-definition logic
# ✅ Blank-line cleanup
# ✅ Configurable MAX_SENTENCES and MAX_WORDS
# ✅ "Overall" sentence priority rule
# ✅ Smart trimming (drops final sentence if exceeding limit)
# ✅ Ignores colons and numeric dots (e.g., 3.5) as sentence ends
# 🚫 No meta-phrase exclusion (for more natural hints)
# ============================================================

import pandas as pd
import re
import string
from tqdm.auto import tqdm

# ---------- CONFIG ----------
INPUT_CSV = "Filtered_WordDB_ModernNoNames.csv"
OUTPUT_CSV = "Filtered_WordDB_Procedural_Hints.csv"
MAX_WORDS = 60
MAX_SENTENCES = 3   # Adjustable number of sentences per hint

# ---------- BASIC CLEANING ----------
def clean_definition(text):
    """Removes parentheses, formatting, and redundant punctuation."""
    if not isinstance(text, str) or not text.strip():
        return ""
    while re.search(r"\([^()]*\)", text):
        text = re.sub(r"\([^()]*\)", "", text)
    text = re.sub(r"\(\s*\)", "", text)

    # Normalize punctuation (keep colons, ignore decimals)
    text = re.sub(r"\s+[.,;]\s*", ". ", text)
    text = re.sub(r"[•→/\\\-;<>•]+", " ", text)
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
    - Colons as sentence boundaries
    """
    # Protect numeric decimals temporarily
    text = re.sub(r"(\d)\.(\d)", r"\1<DECIMAL>\2", text)

    # Split on '.', '!' or '?'
    sentences = re.split(r"(?<=[.!?])\s+", text)

    # Restore decimals
    sentences = [s.replace("<DECIMAL>", ".") for s in sentences]
    return sentences

def trim_to_sentence_or_limit(text, max_words=MAX_WORDS, max_sentences=MAX_SENTENCES):
    """
    Trims text to a maximum number of sentences or words.
    Removes the final sentence entirely if it pushes the total word count over the limit.
    """
    sentences = split_sentences(text)
    trimmed_sentences = []
    total_words = 0

    for s in sentences[:max_sentences]:
        words_in_s = len(s.split())
        if total_words + words_in_s > max_words:
            break
        trimmed_sentences.append(s)
        total_words += words_in_s

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
    """Unified fallback-style generation with 'Overall' priority rule."""
    if not isinstance(definition, str) or not definition.strip():
        return ""

    # 1️⃣ Remove internal blank lines
    lines = [ln.strip() for ln in definition.splitlines() if ln.strip()]
    full_def = " ".join(lines)

    # 2️⃣ Check for 'Overall' sentence priority
    sentences = split_sentences(full_def)
    for s in sentences:
        if re.match(r"^\s*Overall[,:\s]", s, flags=re.IGNORECASE):
            return finalize_hint(remove_word_from_text(word, s))

    # 3️⃣ Mask word, trim, and finalize
    masked = remove_word_from_text(word, full_def)
    limited = trim_to_sentence_or_limit(masked, MAX_WORDS, MAX_SENTENCES)

    return finalize_hint(limited)

# ---------- MAIN ----------
def main():
    tqdm.pandas()
    print(f"🔹 Loading: {INPUT_CSV}")
    df = pd.read_csv(INPUT_CSV)

    # Generate hints (no meta-phrase exclusion)
    df["HINT"] = df.progress_apply(lambda row: generate_hint(row["WORD"], row["DEFINITION"]), axis=1)

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

if __name__ == "__main__":
    main()
