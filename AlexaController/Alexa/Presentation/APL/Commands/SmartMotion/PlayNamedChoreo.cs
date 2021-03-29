using System.Diagnostics.CodeAnalysis;

namespace AlexaController.Alexa.Presentation.APL.Commands.SmartMotion
{
    public class PlayNamedChoreo : ICommand
    {
        public Choreo name { get; set; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum Choreo
    {
        ScreenImpactCenter,
        ClockwiseMediumSweep,
        CounterClockwiseSlowSweep,
        MixedExpressiveShakes
    }
}
