import sys
from ui.main_ui import Ui_MainWindow

from PyQt6.QtWidgets import QApplication, QMainWindow, QWidget, QMessageBox

class MainWindow(QMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.ui = Ui_MainWindow()
        self.ui.setupUi(self) 
        self.ui.stackedWidget.setCurrentWidget(self.ui.page_main)

        self.ui.btn_qna.clicked.connect(lambda: self.ui.stackedWidget.setCurrentWidget(self.ui.page_qna))
        self.ui.btn_navi.clicked.connect(lambda: self.ui.stackedWidget.setCurrentWidget(self.ui.page_navi))


if __name__ == "__main__":
    app = QApplication(sys.argv)
    widget = MainWindow()
    widget.resize(1920, 1080)
    widget.show 
    sys.exit(app.exec())