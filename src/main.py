import sys
from ui.main_ui import Ui_MainWindow
from ui.resources.font_configurator import apply_custom_fonts
from PyQt6.QtWidgets import QApplication, QMainWindow
from PyQt6.QtCore import QTimer
from send_move_command import MQTTSender
from mqtt_client import MQTTHandler


class MainWindow(QMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.ui = Ui_MainWindow()
        self.ui.setupUi(self)
        apply_custom_fonts(self.ui)

        self.ui.stackedWidget.setCurrentWidget(self.ui.page_main)

        # --- Logic điều hướng ---
        self.current_room = ""
        self.mqtt_handler = MQTTHandler(self.on_robot_status_update)


        self.ui.btn_qna.clicked.connect(lambda: self.ui.stackedWidget.setCurrentWidget(self.ui.page_qna))
        self.ui.btn_navi.clicked.connect(lambda: self.ui.stackedWidget.setCurrentWidget(self.ui.page_navi))
        self.ui.btn_home_qna.clicked.connect(lambda: self.ui.stackedWidget.setCurrentWidget(self.ui.page_main))
        self.ui.btn_home_navi.clicked.connect(lambda: self.ui.stackedWidget.setCurrentWidget(self.ui.page_main))

        self.ui.btn_micro.clicked.connect(self._handle_micro)

        self.ui.btn_room_a.clicked.connect(lambda: self.handle_go_to("A"))
        self.ui.btn_room_b.clicked.connect(lambda: self.handle_go_to("B"))
        self.ui.btn_room_c.clicked.connect(lambda: self.handle_go_to("C"))
        self.ui.btn_room_d.clicked.connect(lambda: self.handle_go_to("D"))

    # ----------------------------
    # Micro - I'm hearing... hiệu ứng
    # ----------------------------

    def _handle_micro(self):
        self._animate_prompt("I'm hearing", self.ui.prompt_qna, duration_ms=5000, callback_after=self._stop_listening)

    def _stop_listening(self):
        self._animate_prompt("Answering", self.ui.prompt_qna, duration_ms=5000)


    # ----------------------------
    # Điều hướng robot tới phòng
    # ----------------------------

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

    # ----------------------------
    # Tiện ích chung
    # ----------------------------

    def _set_navigation_buttons_enabled(self, enabled: bool):
        self.ui.btn_micro.setEnabled(enabled)
        self.ui.btn_room_a.setEnabled(enabled)
        self.ui.btn_room_b.setEnabled(enabled)
        self.ui.btn_room_c.setEnabled(enabled)
        self.ui.btn_room_d.setEnabled(enabled)

    def go_home(self):
        self.ui.stackedWidget.setCurrentWidget(self.ui.page_main)

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

    def on_robot_status_update(self, location):
        if self.mqtt_handler.current_target == location:
            self.ui.prompt_navi.setText(f"Arrived at room {location}. Ready for next destination.")

    def handle_go_to(self, room):
        if self.mqtt_handler.current_position == room:
            self.ui.prompt_navi.setText("You are already here!!!")
        else:
            self.ui.prompt_navi.setText(f"Heading to room {room}...")
            self.mqtt_handler.send_destination(room)


if __name__ == "__main__":
    app = QApplication(sys.argv)
    widget = MainWindow()
    # widget.resize(1920, 1080)
    screen = QApplication.primaryScreen()
    size = screen.availableGeometry().size()
    widget.resize(size.width(), size.height())
    widget.show()
    sys.exit(app.exec())