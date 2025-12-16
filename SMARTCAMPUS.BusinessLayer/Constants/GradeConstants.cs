namespace SMARTCAMPUS.BusinessLayer.Constants
{
    public static class GradeConstants
    {
        // Grade calculation weights
        public const decimal MidtermWeight = 0.4m;
        public const decimal FinalWeight = 0.6m;

        // Late check-in grace period in minutes
        public const int LateCheckInGracePeriodMinutes = 15;

        // Grade thresholds
        public const decimal GradeA = 90;
        public const decimal GradeAMinus = 85;
        public const decimal GradeBPlus = 80;
        public const decimal GradeB = 75;
        public const decimal GradeBMinus = 70;
        public const decimal GradeCPlus = 65;
        public const decimal GradeC = 60;
        public const decimal GradeCMinus = 55;
        public const decimal GradeD = 50;
    }
}

