from PyQt6.QtCore import QThread, pyqtSignal
import asyncio
from edge_tts import Communicate
import playsound, os


class SpeakThread(QThread):
    def __init__(self, text):
        super().__init__()
        self.text = text

    def run(self):
        asyncio.run(self.speak(self.text))

    async def speak(self, text):
        try:
            communicate = Communicate(text, voice="en-US-GuyNeural", rate="-10%")
            await communicate.save("tts.mp3")
            playsound.playsound("tts.mp3")
            os.remove("tts.mp3")
        except Exception as e:
            print("🔊 Error in thread speak:", e)     
        
