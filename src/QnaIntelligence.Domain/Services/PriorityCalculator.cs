namespace QnaIntelligence.Domain.Services;

public static class PriorityCalculator
{
    public static int Calculate(bool hasAnswers, double confidence, int ageHours)
    {
        var score = 100;

        if (hasAnswers)
        {
            score -= 30;
        }

        score -= ageHours;
        score += (int)(confidence * 10);

        return Math.Max(score, 0);
    }
}
