from PyQt6.QtCore import QThread, pyqtSignal
import asyncio
import datetime
import playsound
import os
from edge_tts import Communicate


class WelcomeThread(QThread):
    def __init__(self, parent=None):
        super().__init__(parent)

    def run(self):
        self.hour = datetime.datetime.now().hour
        if 6 <= self.hour < 12:
            self.greet = "Good morning"
        elif 12 <= self.hour < 18:
            self.greet = "Good afternoon"
        else:
            self.greet = "Good evening"
        asyncio.run(self._speak(f"{self.greet}, I'm AIko, your receptionist assistant. How can I help you?"))

    async def _speak(self, text):
        try:
            communicate = Communicate(text, voice="en-US-GuyNeural")
            await communicate.save("tts.mp3")
            playsound.playsound("tts.mp3")
            os.remove("tts.mp3")
        except Exception as e:
            print("🔊 Error in thread speak:", e)