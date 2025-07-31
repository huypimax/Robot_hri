# Author Tien Dat (Roy)
# Date: 7/30/2025
# MIT License
# Trợ lý ảo dùng Gemini (Sound-to-Sound)
import datetime

import google.generativeai as genai
import pyttsx3
import speech_recognition as sr

ASSISTANT_NAME = "DATV_Assistant"
# Gemini
api_key = "AIzaSyCsnVbGzLouYNPXIJxnYdmQFa2BrRo1uqA"
genai.configure(api_key=api_key)
model = genai.GenerativeModel("gemini-2.0-flash")
initial_context = [
    {
        "role": "user",
        "parts": [
            {
                "text": "You are a virtual assistant named DATV Assistant, created by Tien Dat."
            }
        ],
    },
    {
        "role": "model",
        "parts": [{"text": "Okay. My name is DATV Assistant, created by Tien Dat."}],
    },
    {
        "role": "user",
        "parts": [
            {
                "text": "You are an AI assistant designed to provide extremely concise and to-the-point answers. Get straight to the essence. Limit each answer to 1-2 sentences or a maximum of 20 words. Do not provide lengthy explanations or unrequested additional information. Only answer the question directly."
            }
        ],
    },
    {
        "role": "model",
        "parts": [
            {
                "text": "Understood. I will provide concise, brief, and direct answers as requested."
            }
        ],
    },
    {"role": "user", "parts": [{"text": "Example: What is the capital of France?"}]},
    {"role": "model", "parts": [{"text": "Paris."}]},
    {"role": "user", "parts": [{"text": "Example: What is gravity?"}]},
    {
        "role": "model",
        "parts": [
            {"text": "The natural force of attraction between objects with mass."}
        ],
    },
]


# Hàm để trợ lý nói
def speak(text):
    tiendat = pyttsx3.init()
    voices = tiendat.getProperty("voices")
    tiendat.setProperty("voice", voices[1].id)
    tiendat.setProperty("rate", 150)
    print("DATV_Assistant: ", text)
    tiendat.say(text)
    tiendat.runAndWait()  # Đảm bảo chờ đến khi phát xong


# Chào hỏi lúc khởi động
def welcome():
    hour = datetime.datetime.now().hour
    if 6 <= hour < 12:
        speak("Good morning sir!")
    elif 12 <= hour < 18:
        speak("Good afternoon sir!")
    elif 18 <= hour < 24:
        speak("Good evening sir!")
    else:
        speak("Hello sir!")
    speak("How can I help you today?")


# Thu âm và chuyển giọng nói thành văn bản
def get_command():
    r = sr.Recognizer()
    with sr.Microphone() as source:
        print("Listening...")
        r.pause_threshold = 2
        audio = r.listen(source)
    try:
        query = r.recognize_google(audio, language="en-US")
        print("You: ", query)
    except sr.UnknownValueError:
        speak("Sorry, I didn't catch that. Could you repeat?")
        return ""
    except sr.RequestError:
        speak("Sorry, my speech service is down.")
        return ""
    return query


# === Gửi câu lệnh lên Gemini ===
def ask_gemini(prompt):
    try:
        user_question = {"role": "user", "parts": [{"text": prompt}]}
        response = model.generate_content(initial_context + [user_question])
        return response.text
    except Exception as e:
        print("Error:", e)
        return "Sorry, I have some errors."


# === MAIN ===
if __name__ == "__main__":
    welcome()

    while True:
        query = get_command().lower()

        if query == "":
            continue

        if "quit" in query or "exit" in query or "stop" in query:
            speak("Goodbye sir!")  # Dừng engine trước khi thoát
            break
        elif "time" in query:
            current_time = datetime.datetime.now().strftime("%I:%M %p")
            speak("It is " + current_time)
        else:
            reply = ask_gemini(query)
            speak(reply)
