import paho.mqtt.client as mqtt

# --- Cấu hình MQTT Broker ---
BROKER_ADDRESS = "10.128.73.236"   # 👉 Đổi IP này thành IP của Xavier
BROKER_PORT = 1883
TOPIC = "robot/move"

# --- MQTT Client ---
client = mqtt.Client()
client.connect(BROKER_ADDRESS, BROKER_PORT)

def send_destination(destination: str):
    """Publish điểm đến (A, B, C, D) tới robot."""
    if destination.upper() in ["A", "B", "C", "D"]:
        client.publish(TOPIC, destination.upper())
        print(f"✅ Sent command to move to {destination.upper()}")
    else:
        print("❌ Invalid destination")
