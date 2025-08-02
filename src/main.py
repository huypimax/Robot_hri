import sys
from ui.main_ui import Ui_MainWindow
from ui.resources.font_configurator import apply_custom_fonts

from PyQt6.QtWidgets import QApplication, QMainWindow, QWidget, QMessageBox
from PyQt6.QtCore import QTimer

class MainWindow(QMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.ui = Ui_MainWindow()
        self.ui.setupUi(self) 
        apply_custom_fonts(self.ui)
        self.ui.stackedWidget.setCurrentWidget(self.ui.page_main)

        self.ui.btn_qna.clicked.connect(lambda: self.ui.stackedWidget.setCurrentWidget(self.ui.page_qna))
        self.ui.btn_navi.clicked.connect(lambda: self.ui.stackedWidget.setCurrentWidget(self.ui.page_navi))
        self.ui.btn_home_qna.clicked.connect(lambda: self.ui.stackedWidget.setCurrentWidget(self.ui.page_main))
        self.ui.btn_home_navi.clicked.connect(lambda: self.ui.stackedWidget.setCurrentWidget(self.ui.page_main))

        self.ui.btn_micro.clicked.connect(self._handle_micro)
        self.ui.btn_room_a.clicked.connect(lambda: self.start_navigation("A"))
        self.ui.btn_room_b.clicked.connect(lambda: self.start_navigation("B"))
        self.ui.btn_room_c.clicked.connect(lambda: self.start_navigation("C"))
        self.ui.btn_room_d.clicked.connect(lambda: self.start_navigation("D"))

    def _handle_micro(self):
        self.ui.prompt_qna.setText("I'm hearing...")

    def start_navigation(self, room: str):
        self._set_navigation_buttons_enabled(False)
        self.ui.prompt_navi.setText(f"Heading to room {room}... Please follow me!")

        # Mô phỏng thời gian di chuyển (5 giây)
        QTimer.singleShot(5000, lambda: self.finish_navigation(room))

    def finish_navigation(self, room: str):
        self._set_navigation_buttons_enabled(True)
        self.ui.prompt_navi.setText(f"Arrived at room {room}. Ready for next destination.")

    def _set_navigation_buttons_enabled(self, enabled: bool):
        self.ui.btn_micro.setEnabled(enabled)
        self.ui.btn_room_a.setEnabled(enabled)
        self.ui.btn_room_b.setEnabled(enabled)
        self.ui.btn_room_c.setEnabled(enabled)
        self.ui.btn_room_d.setEnabled(enabled)



if __name__ == "__main__":
    app = QApplication(sys.argv)
    widget = MainWindow()
    widget.resize(1920, 1080)
    widget.show()
    sys.exit(app.exec())