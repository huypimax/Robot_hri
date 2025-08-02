import paho.mqtt.client as mqtt

class MQTTSender:
    def __init__(self, broker_address="10.128.73.236", broker_port=1883, topic="robot/move"):
        self.topic = topic
        self.client = mqtt.Client()

        try:
            self.client.connect(broker_address, broker_port)
            self.client.loop_start()  # 🔁 giữ kết nối suốt phiên làm việc
            print("🔌 MQTT connected.")
        except Exception as e:
            print("❌ MQTT connection failed:", e)

    def send_destination(self, destination: str):
        if destination.upper() in ["A", "B", "C", "D"]:
            result = self.client.publish(self.topic, destination.upper())
            if result[0] == 0:
                print(f"✅ Sent command to move to {destination.upper()}")
            else:
                print("❌ Failed to send message")
        else:
            print("❌ Invalid destination")
