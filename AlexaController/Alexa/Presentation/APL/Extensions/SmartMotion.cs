namespace AlexaController.Alexa.Presentation.APL.Extensions
{
    public class SmartMotion 
    {
        public WakeWordResponse wakeWordResponse { get; set; } = WakeWordResponse.followOnWakeWord;
        public string deviceStateName { get; set; }
    }

    public enum WakeWordResponse
    {
        doNotMoveOnWakeWord,
        followOnWakeWord,
        turnToWakeWord
    }
}
