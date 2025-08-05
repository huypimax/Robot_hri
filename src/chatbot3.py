# Author: Fablab Team
# Date: 7/30/2025
# Voice-based receptionist assistant: AIko (Gemini-powered)

import datetime
import google.generativeai as genai
import playsound
import speech_recognition as sr
import asyncio
import edge_tts
import os

ASSISTANT_NAME = "AIko"

# === CẤU HÌNH GEMINI ===
api_key = "AIzaSyCsnVbGzLouYNPXIJxnYdmQFa2BrRo1uqA"  # ← thay bằng key thật
genai.configure(api_key=api_key)
model = genai.GenerativeModel("gemini-2.0-flash")

# === NGỮ CẢNH ===
initial_context = [
    {"role": "user", "parts": [{"text": "You are a helpful, concise virtual assistant named AIko, created by Fablab."}]},
    {"role": "model", "parts": [{"text": "Understood. I'm AIko, created by Fablab."}]},
    {"role": "user", "parts": [{"text": "When answering, use 1–2 full sentences, clear and friendly tone. No bullet points or keywords only."}]},
    {"role": "model", "parts": [{"text": "Okay. I will respond in short, clear sentences."}]}
]


async def speak(text):
    print(f"{ASSISTANT_NAME}: {text}")
    try:
        communicate = edge_tts.Communicate(text, voice="en-US-GuyNeural", rate="-10%")
        await communicate.save("tts.mp3")
        playsound.playsound("tts.mp3")
        os.remove("tts.mp3")
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
    asyncio.run(speak(f"{greet}! I'm AIko, your receptionist assistant."))
    asyncio.run(speak("How can I help you?"))


# === NHẬN LỆNH BẰNG GIỌNG NÓI ===
def get_command():
    r = sr.Recognizer()
    with sr.Microphone() as source:
        print("🎤 Calibrating noise (1.0s)...")
        r.adjust_for_ambient_noise(source, duration=1.0)
        print("🎤 Listening...")
        try:
            audio = r.listen(source, timeout=5, phrase_time_limit=10)
            query = r.recognize_google(audio, language="en-US")
            print("You:", query)
            return query
        except sr.UnknownValueError:
            asyncio.run(speak("Sorry, I didn't catch that."))
        except sr.RequestError:
            asyncio.run(speak("Speech service is unavailable."))
        except sr.WaitTimeoutError:
            asyncio.run(speak("Are you still there?"))
        except Exception as e:
            print("🎤 Error:", e)
            asyncio.run(speak("Something went wrong while listening."))
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
            asyncio.run(speak("You're welcome. Goodbye."))
            break

        elif "time" in query:
            current_time = datetime.datetime.now().strftime("%I:%M %p")
            asyncio.run(speak(f"It is {current_time}."))
        else:
            faq_answer = check_faq(query)
            if faq_answer:
                asyncio.run(speak(faq_answer))
            else:
                reply = ask_gemini(query)
                asyncio.run(speak(reply))

