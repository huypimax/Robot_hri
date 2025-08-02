import paho.mqtt.client as mqtt

# --- Cấu hình MQTT Broker ---
BROKER_ADDRESS = "10.128.73.236"  # IP của Xavier hoặc của MQTT broker
BROKER_PORT = 1883
TOPIC = "robot/status/location"

# --- Callback khi nhận được tin nhắn ---
def on_message(client, userdata, msg):
    location = msg.payload.decode()
    print(f"📍 Robot has arrived at: {location}")
    # 🧠 Bạn có thể gọi hàm cập nhật UI ở đây
    # e.g., window.update_arrival_status(location)

# --- Khởi tạo MQTT Client ---
client = mqtt.Client()
client.on_message = on_message

try:
    client.connect(BROKER_ADDRESS, BROKER_PORT)
    client.subscribe(TOPIC)
    client.loop_start()
    print(f"🟢 Listening for robot arrival on topic: {TOPIC}")
except Exception as e:
    print("❌ MQTT connection failed:", e)

# --- Giữ chương trình chạy ---
try:
    while True:
        pass  # hoặc tích hợp với PyQt6 event loop nếu bạn dùng GUI
except KeyboardInterrupt:
    print("🛑 Stopped by user.")
    client.loop_stop()
    client.disconnect()
