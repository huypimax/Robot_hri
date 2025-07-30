import paho.mqtt.client as mqtt

# --- Cấu hình MQTT Broker ---
BROKER_ADDRESS = "10.128.73.236"  # IP của laptop bạn
BROKER_PORT = 1883
TOPIC = "robot/move"

# --- Khởi tạo client và kết nối 1 lần ---
client = mqtt.Client()

try:
    client.connect(BROKER_ADDRESS, BROKER_PORT)
    client.loop_start()  # 🟢 Bắt đầu vòng lặp để giữ kết nối (rất quan trọng)
    print("🔌 MQTT connected.")
except Exception as e:
    print("❌ MQTT connection failed:", e)

# --- Gửi lệnh bất kỳ lúc nào ---
def send_destination(destination: str):
    if destination.upper() in ["A", "B", "C", "D"]:
        result = client.publish(TOPIC, destination.upper())
        status = result[0]
        if status == 0:
            print(f"✅ Sent command to move to {destination.upper()}")
        else:
            print("❌ Failed to send message")
    else:
        print("❌ Invalid destination")
