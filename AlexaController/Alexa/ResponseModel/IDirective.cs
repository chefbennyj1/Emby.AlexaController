namespace AlexaController.Alexa.ResponseModel
{
    public interface IDirective
    {
        string type { get; }
        //Dictionary<string, IDataSource<T>> datasources { get; set; }
    }

}