namespace AlexaController.Alexa.Presentation.DataSources.Transformers
{
    public interface ITransformer
    {
        string inputPath { get; set; }
        string outputName { get; set; }
        string transformer { get; }
    }
}
