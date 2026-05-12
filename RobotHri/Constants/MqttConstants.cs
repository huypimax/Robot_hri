namespace RobotHri.Constants
{
    public static class MqttConstants
    {
        public const string BrokerHost = "45.117.177.157";
        public const int BrokerPort = 1883;
        public const string ClientId = "client";
        public const string Password = "viam1234";

        // Topics
        public const string TopicDestinationPoint = "robot2/goal";
        public const string TopicGoal = TopicDestinationPoint;
        public const string TopicWaypoints = "robot2/waypoints";
        public const string TopicArrival = "robot2/arrival";
        public const string TopicLocation = "robot2/location";
        public const string TopicAttendance = "robot2/attendance";
        public const string TopicStatus = "robot2/status";
        public const string TopicSpeedConfig = "robot2/config/speed";
    }
}
