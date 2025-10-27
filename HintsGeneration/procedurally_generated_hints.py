# ============================================================
# procedurally_generated_hints.py
# ------------------------------------------------------------
# Generates crossword-style hints offline using templates.
# No internet or API key required.
# ============================================================

import pandas as pd
import re, random
from tqdm import tqdm

# ---------- CONFIG ----------
INPUT_CSV = "Filtered_WordDB_ModernNoNames.csv"
OUTPUT_CSV = "Filtered_WordDB_Procedural_Hints.csv"

# ---------- TEXT CLEANING ----------
def clean_definition(text):
    if not isinstance(text, str) or not text.strip():
        return ""
    text = re.sub(r"[•→/\\\-;<>•]+", " ", text)
    text = re.sub(r"\s+", " ", text.strip())
    return text

# ---------- TEMPLATES ----------
TEMPLATES = [
    "Something related to {core}.",
    "Often associated with {core}.",
    "You might think of {core} when you hear it.",
    "Found in places connected to {core}.",
    "A thing you’d encounter when dealing with {core}.",
    "Sometimes seen as {core}.",
    "Used when {core}.",
    "Commonly involved in {core}.",
    "It can be linked to {core}.",
    "A hint: think about {core}."
]

def extract_core(definition):
    """Extract a simplified topic or theme from a definition."""
    # Take the first 8–12 words that convey the concept
    words = definition.split()
    snippet = " ".join(words[:random.randint(8, 12)])
    return snippet

def procedural_hint(word, definition):
    if not isinstance(definition, str) or not definition.strip():
        return ""
    base = extract_core(definition)
    template = random.choice(TEMPLATES)
    hint = template.format(core=base)
    return hint.strip().capitalize()

# ---------- MAIN ----------
def main():
    print(f"🔹 Loading: {INPUT_CSV}")
    df = pd.read_csv(INPUT_CSV)
    
    # Ensure deterministic, original order
    df = df.reset_index(drop=True)
    print("Processing rows in original order.")

    tqdm.pandas(desc="Cleaning definitions")
    df["DEFINITION"] = df["DEFINITION"].progress_apply(clean_definition)

    tqdm.pandas(desc="Generating procedural hints")
    df["HINT"] = df.progress_apply(lambda row: procedural_hint(row.WORD, row.DEFINITION), axis=1)

    # Reorder columns
    cols = list(df.columns)
    if "WORD" in cols and "DEFINITION" in cols:
        cols.insert(cols.index("DEFINITION"), cols.pop(cols.index("HINT")))
        df = df[cols]

    df.to_csv(OUTPUT_CSV, index=False)
    print(f"\n✅ Done! Saved offline hints to {OUTPUT_CSV}")

if __name__ == "__main__":
    main()
