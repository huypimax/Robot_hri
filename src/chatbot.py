# Author: Fablab Team
# Date: 7/30/2025
# Voice-based receptionist assistant: AIko (Gemini-powered)

import datetime
import google.generativeai as genai
from gtts import gTTS
from playsound import playsound
import speech_recognition as sr
import os

ASSISTANT_NAME = "AIko"

# === CẤU HÌNH GEMINI ===
api_key = "AIzaSyCsnVbGzLouYNPXIJxnYdmQFa2BrRo1uqA"  # ← thay bằng key thật
genai.configure(api_key=api_key)
model = genai.GenerativeModel("gemini-1.5-flash")

# === NGỮ CẢNH ===
initial_context = [
    {"role": "user", "parts": [{"text": "You are a virtual assistant named AIko, created by Fablab."}]},
    {"role": "model", "parts": [{"text": "Okay. My name is AIko, created by Fablab."}]},
    {"role": "user", "parts": [{"text": "You give concise answers, max 30 words. Answer directly, no extra."}]},
    {"role": "model", "parts": [{"text": "Understood. I will be brief and direct."}]},
]


# === PHÁT ÂM THANH ===
def speak(text):
    print(f"{ASSISTANT_NAME}: {text}")
    try:
        tts = gTTS(text=text, lang="en", slow=False)
        filename = "temp.mp3"
        tts.save(filename)
        playsound(filename)
        os.remove(filename)
    except Exception as e:
        print("🔊 Error playing sound:", e)


# === CHÀO LÚC KHỞI ĐỘNG ===
def welcome():
    hour = datetime.datetime.now().hour
    if 6 <= hour < 12:
        greet = "Good morning"
    elif 12 <= hour < 18:
        greet = "Good afternoon"
    else:
        greet = "Good evening"
    speak(f"{greet}! I'm AIko, your receptionist assistant.")
    speak("How can I help you?")


# === NHẬN LỆNH BẰNG GIỌNG NÓI ===
def get_command():
    r = sr.Recognizer()
    with sr.Microphone() as source:
        print("🎤 Calibrating noise (1.0s)...")
        r.adjust_for_ambient_noise(source, duration=1.0)
        print("🎤 Listening...")
        try:
            audio = r.listen(source, timeout=5, phrase_time_limit=8)
            query = r.recognize_google(audio, language="en-US")
            print("You:", query)
            return query
        except sr.UnknownValueError:
            speak("Sorry, I didn't catch that.")
        except sr.RequestError:
            speak("Speech service is unavailable.")
        except sr.WaitTimeoutError:
            speak("Are you still there?")
        except Exception as e:
            print("🎤 Error:", e)
            speak("Something went wrong while listening.")
    return ""


# === GỬI ĐẾN GEMINI ===
def ask_gemini(prompt):
    try:
        context = initial_context + [{"role": "user", "parts": [{"text": prompt}]}]
        response = model.generate_content(context)
        return response.text.strip()
    except Exception as e:
        print("Gemini error:", e)
        return "Sorry, I have trouble getting an answer."
    

# === KIỂM TRA CÂU HỎI THƯỜNG GẶP ===
def check_faq(query: str):
    query = query.lower()

    if "ho chi minh university of Technology" in query or "bach khoa" in query or "bach khoa university" in query or "ho chi minh university" in query:
        return ("Ho Chi Minh City University of Technology, also known as Bách Khoa, "
            "is one of Vietnam’s top technical universities. It offers advanced training "
            "in engineering, technology, and innovation, and is part of the Vietnam National University system.")

    elif "fablab" in query or "innovation lab" in query or "robotics lab" in query or "fab lab" in query or "the lab" in query or "innovation laboratory" in query: 
        return "Fablab is an innovation lab at Ho Chi Minh University of Technology, supporting students in robotics, AI, and creative projects."

    elif "who created you" in query or "your creator" in query or "who made you" in query or "who built you" in query or "who developed you" in query or "who designed you" in query or "who creates you":
        return "I was created by a group of students from Fablab, including members from Electrical, Mechanical, and Computer Science departments."

    elif "your name" in query:
        return "My name is AIko, your friendly receptionist assistant."

    return None



# === MAIN LOOP ===
if __name__ == "__main__":
    welcome()
    while True:
        query = get_command().lower()

        if not query:
            continue

        if any(kw in query for kw in ["quit", "exit", "stop", "thank you", "bye"]):
            speak("You're welcome. Goodbye.")
            break

        elif "time" in query:
            current_time = datetime.datetime.now().strftime("%I:%M %p")
            speak(f"It is {current_time}.")
        else:
            faq_answer = check_faq(query)
            if faq_answer:
                speak(faq_answer)
            else:
                reply = ask_gemini(query)
                speak(reply)