from PyQt6.QtCore import QThread, pyqtSignal
import asyncio
import datetime
import google.generativeai as genai


ASSISTANT_NAME = "AIko"

class ResponseThread(QThread):
    finished = pyqtSignal(str)  # Signal to emit the response text
    def __init__(self, query):
        super().__init__()
        self.query = query.lower()  # Normalize the query to lowercase
        self.api_key = "AIzaSyCsnVbGzLouYNPXIJxnYdmQFa2BrRo1uqA"  # ← thay bằng key thật
        genai.configure(api_key=self.api_key)
        self.model = genai.GenerativeModel("gemini-2.0-flash")

        self.initial_context = [
            {"role": "user", "parts": [{"text": "You are a helpful, concise virtual assistant named AIko, created by Fablab."}]},
            {"role": "model", "parts": [{"text": "Understood. I'm AIko, created by Fablab."}]},
            {"role": "user", "parts": [{"text": "When answering, use 1–2 full sentences, clear and friendly tone. No bullet points or keywords only."}]},
            {"role": "model", "parts": [{"text": "Okay. I will respond in short, clear sentences."}]}
        ]

    def run(self):
        self.last_reply = ""

        if any(kw in self.query for kw in ["stop", "thank you", "bye"]):
            self.finished.emit("You're welcome. Goodbye.")
            return

        elif "time" in self.query:
            self.current_time = datetime.datetime.now().strftime("%I:%M %p")
            self.finished.emit(f"It is {self.current_time}.")
            return
                
        elif "date" in self.query or "day" in self.query:
            self.now = datetime.datetime.now()
            self.date_str = self.now.strftime("%A, %B %d, %Y")  # Friday, August 02, 2025
            self.finished.emit(f"Today is {self.date_str}.")
            return

        elif any(kw in self.query for kw in ["repeat", "repeat that", "say it again"]):
            if self.last_reply:
                self.finished.emit(self.last_reply)
                return
            else:
                self.finished.emit("I haven't said anything yet.")
                return
                
        else:
            self.faq_answer = self.check_faq(self.query)
            if self.faq_answer:
                self.finished.emit(self.faq_answer)
                return
            else:
                self.reply = asyncio.run(self.ask_gemini(self.query))
                self.last_reply = self.reply
                self.finished.emit(self.reply)
                return

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
        
    async def ask_gemini(self, prompt):
        try:
            self.context = self.initial_context + [{"role": "user", "parts": [{"text": prompt}]}]
            self.response = self.model.generate_content(self.context)
            return self.response.text.strip()
        except Exception as e:
            print("Gemini error:", e)
            return "Sorry, I have trouble getting an answer."
