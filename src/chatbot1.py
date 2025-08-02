# Author: Fablab Team
# Description: Voice-based assistant AIko using Vosk + Gemini

import os
import datetime
import json
from vosk import Model, KaldiRecognizer
import sounddevice as sd
from gtts import gTTS
from playsound import playsound
import google.generativeai as genai
import whisper
import numpy as np
import noisereduce as nr


# Load mô hình Whisper base
whisper_model = whisper.load_model("base")

# === AIko Settings ===
ASSISTANT_NAME = "AIko"
MODEL_PATH = "D:/Robot_hri/vosk-model-small-en-us-0.15"  # ← Đặt đúng tên folder đã giải nén
genai.configure(api_key="AIzaSyBZkrjR8VCVHcuzwZxiWWX0Rzx8OaYAADE")  # ← Thay bằng key của bạn
model = genai.GenerativeModel("gemini-1.5-flash")

# === Context khởi tạo AIko ===
initial_context = [
    {
        "role": "user",
        "parts": [{"text": "You are AIko, a friendly and concise receptionist robot assistant created by a student group from Fablab."}]
    },
    {
        "role": "model",
        "parts": [{"text": "Understood. I am AIko, your helpful receptionist. I will answer briefly and clearly."}]
    }
]

# === Voice Output ===
def speak(text):
    print(f"{ASSISTANT_NAME}: {text}")
    tts = gTTS(text=text, lang="en")
    filename = "aiko_voice.mp3"
    tts.save(filename)
    playsound(filename)
    os.remove(filename)

# === Greeting ===
def welcome():
    hour = datetime.datetime.now().hour
    if 6 <= hour < 12:
        speak("Good morning! I'm AIko, your receptionist assistant.")
    elif 12 <= hour < 18:
        speak("Good afternoon! I'm AIko, your receptionist assistant.")
    else:
        speak("Good evening! I'm AIko, your receptionist assistant.")
    speak("How can I help you?")

# === Voice Input (Vosk offline) ===
vosk_model = Model(MODEL_PATH)
rec = KaldiRecognizer(vosk_model, 16000)

def get_command():
    print("🎤 Listening (Whisper + Noise Reduction)...")
    
    duration = 5  # Thời gian ghi âm (giây)
    fs = 16000    # Tần số lấy mẫu
    print("Recording...")
    audio = sd.rec(int(duration * fs), samplerate=fs, channels=1, dtype='float32')
    sd.wait()

    audio_np = np.squeeze(audio)

    # === Noise reduction ===
    print("Reducing noise...")
    reduced_audio = nr.reduce_noise(y=audio_np, sr=fs)

    # === Whisper transcribe ===
    print("Transcribing...")
    result = whisper_model.transcribe(reduced_audio, fp16=False)
    text = result["text"].strip()

    print("You:", text)
    return text


# === Ask Gemini ===
def ask_gemini(prompt):
    try:
        full_prompt = initial_context + [{"role": "user", "parts": [{"text": prompt}]}]
        response = model.generate_content(full_prompt)
        return response.text.strip()
    except Exception as e:
        print("❌ Gemini error:", e)
        return "Sorry, I encountered an error."

# === Main Loop ===
if __name__ == "__main__":
    welcome()
    while True:
        query = get_command().lower()

        if query == "":
            continue

        if any(word in query for word in ["quit", "exit", "stop"]):
            speak("Goodbye! See you next time.")
            break

        elif "time" in query:
            current_time = datetime.datetime.now().strftime("%I:%M %p")
            speak(f"It is {current_time}.")
        else:
            reply = ask_gemini(query)
            speak(reply)
