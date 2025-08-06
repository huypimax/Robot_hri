from PyQt6.QtCore import QThread, pyqtSignal
import speech_recognition as sr

class ListenThread(QThread):
    finished = pyqtSignal(str)

    def __init__(self):
        super().__init__()
        self.r = sr.Recognizer()

#     def run(self):
#         print("In listen_thread")
#         if self.isInterruptionRequested():
#             return

#         with sr.Microphone() as source:
#             print("🎤 Calibrating noise (0.5s)...")
#             self.r.adjust_for_ambient_noise(source, duration=0.5)

#             if self.isInterruptionRequested():
#                 print("🛑 Interrupted before listening.")
#                 return

#             print("🎤 Listening...")
#             try:
#                 self.audio = self.r.listen(source, timeout=6, phrase_time_limit=10)

#                 if self.isInterruptionRequested():
#                     print("🛑 Interrupted after listening, before recognition.")
#                     return

#                 self.query = self.r.recognize_google(self.audio, language="en-US")
#                 print("You:", self.query)
#                 self.finished.emit(self.query)

#             except sr.UnknownValueError:
#                 self.finished.emit("Hmm, I didn't quite catch that. Could you please repeat?")
#             except sr.RequestError:
#                 self.finished.emit("Speech service is unavailable.")
#             except sr.WaitTimeoutError:
#                 self.finished.emit("Are you still there?")
#             except Exception as e:
#                 print("🎤 Error:", e)
#                 self.finished.emit("Something went wrong while listening.")


    def run(self):
        print("In listen_thread")
        if self.isInterruptionRequested():
            return

        with sr.Microphone() as source:
            print("🎤 Calibrating noise (0.5s)...")
            self.r.adjust_for_ambient_noise(source, duration=0.5)

            if self.isInterruptionRequested():
                print("🛑 Interrupted before listening.")
                return

            print("🎤 Listening...")
            try:
                self.audio = self.r.listen(source, timeout=6, phrase_time_limit=10)

                if self.isInterruptionRequested():
                    print("🛑 Interrupted after listening, before recognition.")
                    return

                self.query = self.r.recognize_google(self.audio, language="en-US")
                print("You:", self.query)
                self.finished.emit(self.query)

            except sr.UnknownValueError:
                self.finished.emit("Hmm, I didn't quite catch that. Could you please repeat?")
            except sr.RequestError:
                self.finished.emit("Speech service is unavailable.")
            except sr.WaitTimeoutError:
                self.finished.emit("Are you still there?")
            except Exception as e:
                print("🎤 Error:", e)
                self.finished.emit("Something went wrong while listening.")

        self.finished.emit("")




# from PyQt6.QtCore import QThread, pyqtSignal
# import asyncio
# import speech_recognition as sr

# class ListenThread(QThread):
#     finished = pyqtSignal(str)
#     def __init__(self):
#         super().__init__()

#     def run(self):
#         self.r = sr.Recognizer()
#         with sr.Microphone() as source:
#             print("🎤 Calibrating noise (1.0s)...")
#             self.r.adjust_for_ambient_noise(source, duration=0.5)
#             print("🎤 Listening...")
#             try:
#                 self.audio = self.r.listen(source, timeout=6, phrase_time_limit=10)
#                 self.query = self.r.recognize_google(self.audio, language="en-US")
#                 print("You:", self.query)
#                 self.finished.emit(self.query)
#                 return
#             except sr.UnknownValueError:
#                 self.finished.emit("Hmm, I didn't quite catch that. Could you please repeat?")
#                 return
#             except sr.RequestError:
#                 self.finished.emit("Speech service is unavailable.")
#                 return
#             except sr.WaitTimeoutError:
#                 self.finished.emit("Are you still there?")
#                 return
#             except Exception as e:
#                 print("🎤 Error:", e)
#                 self.finished.emit("Something went wrong while listening.")
#                 return
#         self.finished.emit("")
#         return

#     # async def speak(self, text):
#     #     from edge_tts import Communicate
#     #     import playsound, os

#     #     try:
#     #         communicate = Communicate(text, voice="en-US-GuyNeural", rate="-10%")
#     #         await communicate.save("tts.mp3")
#     #         playsound.playsound("tts.mp3")
#     #         os.remove("tts.mp3")
#     #     except Exception as e:
#     #         print("🔊 Error in thread speak:", e)