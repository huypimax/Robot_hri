# Author: Fablab Team
# Date: 7/30/2025
# Voice-based receptionist assistant: AIko (Openai-powered)

import datetime
import openai
from openai import OpenAI
from gtts import gTTS
from playsound import playsound
import speech_recognition as sr
import os

ASSISTANT_NAME = "AIko"

client = OpenAI(api_key="OPENAI_API_KEY")  # ← thay bằng key thật

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
        print("🎤 Calibrating background noise (1s)...")
        r.adjust_for_ambient_noise(source, duration=1.0)
        print("🎤 Listening for your command...")

        try:
            audio = r.listen(source, timeout=10, phrase_time_limit=10)
            query = r.recognize_google(audio, language="en-US")
            print("You said:", query)
            return query
        except sr.UnknownValueError:
            speak("Sorry, I didn't understand.")
        except sr.RequestError:
            speak("Speech service error. Please check your internet.")
        except Exception as e:
            print("❌ Error:", e)
            speak("Something went wrong.")
        return ""       



# === GỬI ĐẾN CHATGPT ===
def ask_chatgpt(prompt: str) -> str:
    try:
        response = client.chat.completions.create(
            model="gpt-3.5-turbo",  # hoặc "gpt-4" nếu bạn có quyền
            messages=[
                {
                    "role": "system",
                    "content": "You are AIko, a friendly receptionist assistant created by Fablab. Keep answers short and relevant (max 2 sentences)."
                },
                {
                    "role": "user",
                    "content": prompt
                }
            ],
            max_tokens=100,
            temperature=0.7
        )
        return response.choices[0].message.content.strip()
    except Exception as e:
        print("❌ OpenAI error:", e)
        return "Sorry, I can't get a response right now."
    

# === KIỂM TRA CÂU HỎI THƯỜNG GẶP ===
def check_faq(query: str):
    query = query.lower()

    if any(kw in query for kw in [
        "ho chi minh university of technology", "bach khoa", 
        "bach khoa university", "ho chi minh university"
    ]):
        return ("Ho Chi Minh City University of Technology, also known as Bách Khoa, "
                "is one of Vietnam’s top technical universities. It offers advanced training "
                "in engineering, technology, and innovation, and is part of the Vietnam National University system.")

    elif any(kw in query for kw in [
        "fablab", "innovation lab", "robotics lab", 
        "fab lab", "the lab", "innovation laboratory"
    ]):
        return "Fablab is an innovation lab at Ho Chi Minh University of Technology, supporting students in robotics, AI, and creative projects."

    elif any(kw in query for kw in [
        "who created you", "your creator", "who made you", "who built you", 
        "who developed you", "who designed you", "who creates you"
    ]):
        return "I was created by a group of students from Fablab laboratory, including members from Electrical, Mechanical, and Computer Science departments."

    elif any(kw in query for kw in [
        "what's your name", "what is your name", "your name", "who are you"
    ]):
        return "My name is AIko, your friendly receptionist assistant."
    else:
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
                reply = ask_chatgpt(query)
                speak(reply)