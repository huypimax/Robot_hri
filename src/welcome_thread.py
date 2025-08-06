from PyQt6.QtCore import QThread
import asyncio
import datetime
import pygame
import os
from edge_tts import Communicate


class WelcomeThread(QThread):
    def __init__(self, parent=None):
        super().__init__(parent)

    def run(self):
        print("In welcome_thread")
        if self.isInterruptionRequested():
            return

        hour = datetime.datetime.now().hour
        if 6 <= hour < 12:
            greet = "Good morning"
        elif 12 <= hour < 18:
            greet = "Good afternoon"
        else:
            greet = "Good evening"

        text = f"{greet}, I'm AIko. How can I help you?"
        asyncio.run(self.speak(text))

    async def speak(self, text):
        try:
            # Generate TTS
            communicate = Communicate(text, voice="en-US-GuyNeural")
            await communicate.save("tts.mp3")

            if not pygame.mixer.get_init():
                pygame.mixer.init()

            pygame.mixer.music.load("tts.mp3")
            pygame.mixer.music.play()

            # Wait until playback is finished or interrupted
            while pygame.mixer.music.get_busy():
                if self.isInterruptionRequested():
                    print("🛑 Interruption requested, stopping audio.")
                    pygame.mixer.music.stop()
                    break
                await asyncio.sleep(0.1)

            # Safely unload and remove file
            pygame.mixer.music.unload()
            await asyncio.sleep(0.1)  # Give system time to release file
            os.remove("tts.mp3")

        except Exception as e:
            print("🔊 Error in WelcomeThread:", e)
