namespace RobotHri.Constants
{
    public static class MqttConstants
    {
        public const string BrokerHost = "45.117.177.157";
        public const int BrokerPort = 1883;
        public const string ClientId = "client";
        public const string Password = "viam1234";

        // Topics
        public const string TopicGoal = "robot/goal";
        public const string TopicWaypoints = "robot/waypoints";
        public const string TopicArrival = "robot/arrival";
        public const string TopicLocation = "robot/location";
        public const string TopicAttendance = "robot/attendance";
        public const string TopicStatus = "robot/status";
        /// <summary>Subscribe: UTF-8 payload is a single battery percentage value (0–100), e.g. <c>87</c> or <c>87.5</c>.</summary>
        public const string TopicBattery = "robot/status/battery";
        public const string TopicSpeedConfig = "robot/config/speed";
    }
}
