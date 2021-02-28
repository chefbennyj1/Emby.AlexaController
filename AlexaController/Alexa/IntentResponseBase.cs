namespace AlexaController.Alexa
{
    // ReSharper disable once InconsistentNaming
    public abstract class IntentResponseBase<IAlexaRequest, IAlexaSession>
    {
        public IntentResponseBase(IAlexaRequest alexaRequest, IAlexaSession session) { }
    }
}
