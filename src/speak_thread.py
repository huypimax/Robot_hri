from PyQt6.QtCore import QThread
from edge_tts import Communicate
import asyncio
import pygame
import time
import os

class SpeakThread(QThread):
    def __init__(self, text):
        super().__init__()
        self.text = text

    def run(self):
        print("In speak_thread")
        try:
            if self.isInterruptionRequested():
                return

            # Generate TTS
            communicate = Communicate(self.text, voice="en-US-GuyNeural")
            self.loop = asyncio.new_event_loop()
            asyncio.set_event_loop(self.loop)
            self.loop.run_until_complete(communicate.save("tts.mp3"))

            if not pygame.mixer.get_init():
                pygame.mixer.init()

            pygame.mixer.music.load("tts.mp3")
            pygame.mixer.music.play()

            # Wait until playback finishes or interrupted
            while pygame.mixer.music.get_busy():
                if self.isInterruptionRequested():
                    print("🛑 Interruption requested, stopping audio.")
                    pygame.mixer.music.stop()
                    break
                time.sleep(0.1)

            # Clean up
            pygame.mixer.music.stop()
            pygame.mixer.music.unload()
            time.sleep(0.1)
            os.remove("tts.mp3")

        except Exception as e:
            print("🔊 Error in SpeakThread:", e)


# from PyQt6.QtCore import QThread, pyqtSignal
# import asyncio
# from edge_tts import Communicate
# import playsound, os

# class SpeakThread(QThread):
#     def __init__(self, text):
#         super().__init__()
#         self.text = text

#     def run(self):
#         asyncio.run(self.speak(self.text))

#     async def speak(self, text):
#         try:
#             communicate = Communicate(text, voice="en-US-GuyNeural", rate="-10%")
#             await communicate.save("tts.mp3")
#             playsound.playsound("tts.mp3")
#             os.remove("tts.mp3")
#         except Exception as e:
#             print("🔊 Error in thread speak:", e)     