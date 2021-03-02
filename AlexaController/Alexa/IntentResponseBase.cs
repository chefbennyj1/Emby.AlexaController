namespace AlexaController.Alexa
{
    // ReSharper disable twice InconsistentNaming
    public abstract class IntentResponseBase<IAlexaRequest, IAlexaSession>
    {
        protected IntentResponseBase(IAlexaRequest alexaRequest, IAlexaSession session) { }
    }
}
