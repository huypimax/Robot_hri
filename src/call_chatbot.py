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

class AIkoBot:
    def __init__(self, voice="en-US-GuyNeural", rate="-10%"):
        self.api_key = "AIzaSyCsnVbGzLouYNPXIJxnYdmQFa2BrRo1uqA"  # ← thay bằng key thật
        genai.configure(api_key=self.api_key)
        self.model = genai.GenerativeModel("gemini-2.0-flash")

        self.initial_context = [
            {"role": "user", "parts": [{"text": "You are a helpful, concise virtual assistant named AIko, created by Fablab."}]},
            {"role": "model", "parts": [{"text": "Understood. I'm AIko, created by Fablab."}]},
            {"role": "user", "parts": [{"text": "When answering, use 1–2 full sentences, clear and friendly tone. No bullet points or keywords only."}]},
            {"role": "model", "parts": [{"text": "Okay. I will respond in short, clear sentences."}]}
        ]   


    async def speak(self, text):
        print(f"{ASSISTANT_NAME}: {text}")
        try:
            self.communicate = edge_tts.Communicate(text, voice="en-US-GuyNeural", rate="-10%")
            await self.communicate.save("tts.mp3")
            playsound.playsound("tts.mp3")
            os.remove("tts.mp3")
        except Exception as e:
            print("🔊 Error playing sound:", e)


    # === GỬI ĐẾN GEMINI ===
    def ask_gemini(self, prompt):
        try:
            self.context = self.initial_context + [{"role": "user", "parts": [{"text": prompt}]}]
            self.response = self.model.generate_content(self.context)
            return self.response.text.strip()
        except Exception as e:
            print("Gemini error:", e)
            return "Sorry, I have trouble getting an answer."
    

    # === KIỂM TRA CÂU HỎI THƯỜNG GẶP ===
    def check_faq(self, query: str):
        self.query = query.lower()

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
        elif any(kw in query for kw in ["help", "what can you do", "what are your functions"]):
            return ("I can answer questions, tell the time, and guide you with basic information. Just ask me anything.")
        else:
            return None

    async def handle_conversation(self, query):
        self.query = query
        while True:
            self.last_reply = ""

            if any(kw in self.query for kw in ["quit", "exit", "stop", "thank you", "bye"]):
                return "You're welcome. Goodbye."

            elif "time" in self.query:
                self.current_time = datetime.datetime.now().strftime("%I:%M %p")
                return f"It is {self.current_time}."
                
            elif "date" in self.query or "day" in self.query:
                self.now = datetime.datetime.now()
                self.date_str = self.now.strftime("%A, %B %d, %Y")  # Friday, August 02, 2025
                return f"Today is {self.date_str}."

            elif any(kw in self.query for kw in ["repeat", "repeat that", "say it again"]):
                if self.last_reply:
                    return self.last_reply
                else:
                    return "I haven't said anything yet."
                
            else:
                self.faq_answer = self.check_faq(self.query)
                if self.faq_answer:
                    return self.faq_answer
                else:
                    self.reply = self.ask_gemini(self.query)
                    self.last_reply = self.reply
                    return self.reply