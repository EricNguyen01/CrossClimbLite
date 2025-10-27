# ============================================================
# generate_hints_ai.py
# ------------------------------------------------------------
# Generates crossword-style hints using OpenAI's GPT-4o-mini.
# Requires an OpenAI API key in your environment:
#    export OPENAI_API_KEY="your_api_key_here"
# ============================================================

import pandas as pd
import re, os, random, time
from tqdm import tqdm
from openai import OpenAI

# ---------- CONFIG ----------
INPUT_CSV = "Filtered_WordDB_ModernNoNames.csv"
OUTPUT_CSV = "Filtered_WordDB_AIGen_Hints.csv"
MODEL = "gpt-4o-mini"
BATCH_SIZE = 10   # Adjust for speed vs. cost
TEMPERATURE = 0.8 # Adds slight natural variation

# ---------- INIT ----------
client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))
if not client.api_key:
    raise EnvironmentError("❌ OPENAI_API_KEY not found. Please set your API key.")

# ---------- CLEANING ----------
def clean_definition(text):
    if not isinstance(text, str) or not text.strip():
        return ""
    text = re.sub(r"[•→/\\\-;<>•]+", " ", text)
    text = re.sub(r"\s+", " ", text.strip())
    return text

# ---------- AI PROMPT ----------
def build_prompt(word, definition):
    return (
        f"Generate a concise, crossword-style hint for the English word '{word}'. "
        f"The hint should be playful but not too easy. Avoid using the word itself or direct synonyms. "
        f"Base your clue on this definition: '{definition}'. "
        f"Write 1–4 short sentences, natural and engaging."
    )

# ---------- GENERATION ----------
def generate_hints(df):
    hints = []
    for i in tqdm(range(0, len(df), BATCH_SIZE), desc="Generating hints (AI)"):
        batch = df.iloc[i:i+BATCH_SIZE]
        prompts = [build_prompt(row.WORD, row.DEFINITION) for _, row in batch.iterrows()]

        try:
            responses = client.chat.completions.create(
                model=MODEL,
                messages=[{"role": "system", "content": "You are a crossword puzzle clue writer."}] +
                         [{"role": "user", "content": p} for p in prompts],
                temperature=TEMPERATURE,
                n=1
            )
            # Extract hints
            for r in responses.choices:
                hints.append(r.message.content.strip() if r.message.content else "")
        except Exception as e:
            print(f"⚠️ API error: {e}")
            # Fill with blanks on failure
            hints.extend([""] * len(batch))
            time.sleep(5)

    return hints[:len(df)]

# ---------- MAIN ----------
def main():
    print(f"🔹 Loading: {INPUT_CSV}")
    df = pd.read_csv(INPUT_CSV)
    df["DEFINITION"] = df["DEFINITION"].apply(clean_definition)

    tqdm.pandas(desc="Cleaning definitions")
    
    # Ensure deterministic, original order
    df = df.reset_index(drop=True)
    print("Processing rows in original order.")

    df["HINT"] = generate_hints(df)

    # Reorder columns
    cols = list(df.columns)
    if "WORD" in cols and "DEFINITION" in cols:
        cols.insert(cols.index("DEFINITION"), cols.pop(cols.index("HINT")))
        df = df[cols]

    df.to_csv(OUTPUT_CSV, index=False)
    print(f"\n✅ Done! Saved AI-generated hints to {OUTPUT_CSV}")

if __name__ == "__main__":
    main()
