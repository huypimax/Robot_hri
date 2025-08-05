import sys
from ui.main_ui import Ui_MainWindow
from ui.resources.font_configurator import apply_custom_fonts
from PyQt6.QtWidgets import QApplication, QMainWindow
from PyQt6.QtCore import QTimer

from mqtt_client import MQTTHandler
import asyncio
import os
from welcome_thread import WelcomeThread
from speak_thread import SpeakThread
from listen_thread import ListenThread
from response_thread import ResponseThread
from call_chatbot import AIkoBot

ASSISTANT_NAME = "AIko"

class MainWindow(QMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.ui = Ui_MainWindow()
        self.ui.setupUi(self)
        apply_custom_fonts(self.ui)

        self.ui.stackedWidget.setCurrentWidget(self.ui.page_main)

        self.mqtt_handler = MQTTHandler(self.on_robot_status_update)
        self.bot = AIkoBot()

        self.btn_speaker_timer = QTimer()
        self.btn_speaker_timer.timeout.connect(lambda: self.SetStyleSheetForbtn("btn_speaker", "#ffffff")) 
        self.inactivity_timer = QTimer(self)
        self.inactivity_timer.setInterval(20000)  
        self.inactivity_timer.setSingleShot(True)
        self.inactivity_timer.timeout.connect(self.go_to_main_page)

        # --- Logic điều hướng ---

        self.ui.btn_qna.clicked.connect(lambda: [self.handle_btn_qna(), self.ui.stackedWidget.setCurrentWidget(self.ui.page_qna), self.reset_inactivity_timer()])
        self.ui.btn_navi.clicked.connect(lambda: [self.handle_btn_navi(), self.ui.stackedWidget.setCurrentWidget(self.ui.page_navi), self.reset_inactivity_timer()])
        self.ui.btn_home_qna.clicked.connect(lambda: self.go_to_main_page())
        self.ui.btn_home_navi.clicked.connect(lambda: self.go_to_main_page())

        self.ui.btn_micro.clicked.connect(lambda: [self.handle_micro()])
        #self.ui.btn_speaker.setEnabled(False)

        self.ui.btn_room_a.clicked.connect(lambda: [self.handle_go_to("A")])
        self.ui.btn_room_b.clicked.connect(lambda: [self.handle_go_to("B")])
        self.ui.btn_room_c.clicked.connect(lambda: [self.handle_go_to("C")])
        self.ui.btn_room_d.clicked.connect(lambda: [self.handle_go_to("D")])

    def handle_micro(self):
        self.SetStyleSheetForbtn("btn_micro", "#69ff3d")  
        self.SetStyleSheetForbtn("btn_speaker", "#ffffff")  
        self.ui.prompt_qna.setText("Listening...")
        self.listen_thread = ListenThread()
        self.listen_thread.finished.connect(self.get_response)
        self.listen_thread.finished.connect(lambda: self.cleanup_thread(self.listen_thread))
        self.listen_thread.start()

    def get_response(self, query: str):
        self.SetStyleSheetForbtn("btn_micro", "#ffffff")  
        if not query:
            self.continue_conversation()
        else:
            self.response_thread = ResponseThread(query)
            self.response_thread.finished.connect(self.answer)
            self.response_thread.finished.connect(lambda: self.cleanup_thread(self.response_thread))
            self.response_thread.start()

    def answer(self, text: str):
        self.ui.btn_speaker.setEnabled(True)
        self.SetStyleSheetForbtn("btn_speaker", "#69ff3d")
        self.ui.prompt_qna.setText("Answering...")
        self.reset_inactivity_timer()
        if text == "You're welcome. Goodbye.":
            print(f"AIko: {text}")
            self.speak_thread = SpeakThread(text)
            self.speak_thread.finished.connect(lambda: self.cleanup_thread(self.speak_thread))
            self.speak_thread.start()
            self.btn_speaker_timer.start(4000) 
        else:
            print(f"AIko: {text}")
            self.speak_thread = SpeakThread(text)
            self.speak_thread.finished.connect(self.continue_conversation)
            self.speak_thread.finished.connect(lambda: self.cleanup_thread(self.speak_thread))
            self.speak_thread.start()

    def continue_conversation(self):
        self.handle_micro()

    def start_navigation(self, room: str):
        if room == self.current_room:
            self.ui.prompt_navi.setText("You are already here!!!")
            return

        self._set_navigation_buttons_enabled(False)
        self._animate_prompt(
            base_text=f"Heading to room {room}",
            label_widget=self.ui.prompt_navi,
            duration_ms=5000,  # mô phỏng thời gian di chuyển
            callback_after=lambda: self._arrive_at(room)
        )

    def _arrive_at(self, room: str):
        self.current_room = room
        self._set_navigation_buttons_enabled(True)
        self.ui.prompt_navi.setText(f"Arrived at room {room}. Ready for next destination.")

        # Tự động quay về main sau 5 giây
        QTimer.singleShot(10000, self.go_home)

    def on_robot_status_update(self, location):
        if self.mqtt_handler.current_target == location:
            self.ui.prompt_navi.setText(f"Arrived at room {location}. Ready for next destination.")
            # Tự động quay về main sau 10 giây
            QTimer.singleShot(10000, self.go_home)

    def handle_go_to(self, room):
        if self.mqtt_handler.current_position == room:
            self.ui.prompt_navi.setText("You are already here!!!")
        else:
            self._set_navigation_buttons_enabled(False)
            self._animate_prompt(
                base_text=f"Heading to room {room}",
                label_widget=self.ui.prompt_navi,
                duration_ms=5000,  # mô phỏng thời gian di chuyển
        )
            self.mqtt_handler.send_destination(room)

    def _set_navigation_buttons_enabled(self, enabled: bool):
        # self.ui.btn_micro.setEnabled(enabled)
        self.ui.btn_room_a.setEnabled(enabled)
        self.ui.btn_room_b.setEnabled(enabled)
        self.ui.btn_room_c.setEnabled(enabled)
        self.ui.btn_room_d.setEnabled(enabled)

    def go_to_main_page(self):
        self.ui.stackedWidget.setCurrentWidget(self.ui.page_main)
        self.SetStyleSheetForbtn("btn_micro", "#ffffff")
        self.SetStyleSheetForbtn("btn_speaker", "#ffffff")

    def reset_inactivity_timer(self):
        self.inactivity_timer.stop()
        self.inactivity_timer.start()


    def _animate_prompt(self, base_text: str, label_widget, duration_ms, callback_after=None):
        """
        Hiệu ứng động "..." sau base_text
        """
        dots = [".", "..", "..."]
        index = 0
        timer = QTimer(self)

        def update():
            nonlocal index
            label_widget.setText(f"{base_text}{dots[index]}")
            index = (index + 1) % len(dots)

        timer.timeout.connect(update)
        timer.start(500)  # mỗi 500ms update chấm

        # Sau duration_ms thì stop hiệu ứng và gọi callback nếu có
        QTimer.singleShot(duration_ms, lambda: (timer.stop(), callback_after() if callback_after else None))

    def handle_btn_qna(self):
        self.SetStyleSheetForbtn("btn_speaker", "#69ff3d")
        self.btn_speaker_timer.start(7000)
        self.welcome_thread = WelcomeThread()
        self.welcome_thread.finished.connect(lambda: self.cleanup_thread(self.welcome_thread))
        self.welcome_thread.start()

    def handle_btn_navi(self):
        self.speak_thread = SpeakThread("Where do you want to go?")
        self.speak_thread.finished.connect(lambda: self.cleanup_thread(self.speak_thread))
        self.speak_thread.start()

    def SetStyleSheetForbtn(self, btn, background_color):
        #Style cho nút btnEmployeeWorking
        button = getattr(self.ui, btn)
        button.setStyleSheet(f"""
                QPushButton#{btn} {{
                    border-radius: 15px;
                    border-color: white;
                    background-color: {background_color};  /* Màu nền mới */
                    color: white;
                    text-align: center;
                    font-family: Inter, sans-serif;
                }}

                QPushButton#{btn}:hover {{
                    background-color: #ffffff;  /* Màu nền khi hover */
                }}

                QPushButton#{btn}:pressed {{
                    padding-left: 5px;
                    padding-top: 5px;
                }}
                """)                         

    def cleanup_thread(self, thread):
        if thread is not None:
            thread.quit()
            thread.wait()
            thread.deleteLater()
            thread = None

  

if __name__ == "__main__":
    app = QApplication(sys.argv)
    widget = MainWindow()
    # widget.resize(1920, 1080)
    screen = QApplication.primaryScreen()
    size = screen.availableGeometry().size()
    widget.resize(size.width(), size.height())
    widget.show()
    sys.exit(app.exec())