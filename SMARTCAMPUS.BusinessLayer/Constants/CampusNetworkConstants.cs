namespace SMARTCAMPUS.BusinessLayer.Constants
{
    public static class CampusNetworkConstants
    {
        // Campus IP ranges (example - should be configured per institution)
        public static readonly string[] CampusIpRanges = new[]
        {
            "192.168.1.0/24",  // Example campus network
            "192.168.2.0/24",
            "10.0.0.0/16",     // Another example range
            "172.16.0.0/12"    // VPN range
        };

        // Maximum allowed velocity for realistic travel (km/h)
        public const decimal MaxRealisticVelocity = 200; // km/h - impossible to travel faster on campus

        // Maximum time difference for velocity check (seconds)
        public const int MaxTimeDifferenceForVelocityCheck = 3600; // 1 hour

        // Fraud score thresholds
        public const int FraudScoreLow = 30;
        public const int FraudScoreMedium = 60;
        public const int FraudScoreHigh = 80;
    }
}

