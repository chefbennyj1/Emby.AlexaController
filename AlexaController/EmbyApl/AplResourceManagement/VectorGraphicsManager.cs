using AlexaController.Alexa.Presentation.APL.VectorGraphics;
using System.Collections.Generic;

namespace AlexaController.EmbyApl.AplResourceManagement
{
    public static class VectorGraphicsManager
    {
        public static Dictionary<string, IAlexaVectorGraphic> RenderVectorGraphicsDictionary => new Dictionary<string, IAlexaVectorGraphic>
        {
            {
                "CheckMark", new AlexaVectorGraphic()
                {
                    height         = 35,
                    width          = 35,
                    viewportHeight = 25,
                    viewportWidth  = 25,
                    items          = new List<IVectorGraphic>()
                    {
                        new VectorPath()
                        {
                            pathData    = MaterialVectorIcons.CheckMark,
                            stroke      = "none",
                            strokeWidth = 1,
                            fill        = "rgba(255,0,0,1)"
                        }
                    }
                }
            },
            {
                "Audio", new AlexaVectorGraphic()
                {
                    height         = 25,
                    width          = 25,
                    viewportHeight = 28,
                    viewportWidth  = 28,
                    items          = new List<IVectorGraphic>()
                    {
                        new VectorPath()
                        {
                            pathData    = MaterialVectorIcons.Audio,
                            stroke      = "none",
                            strokeWidth = 1,
                            fill        = "white"
                        }
                    }
                }
            },
            {
                "Carousel", new AlexaVectorGraphic()
                {
                    height         = 35,
                    width          = 35,
                    viewportHeight = 25,
                    viewportWidth  = 25,
                    items          = new List<IVectorGraphic>()
                    {
                        new VectorPath()
                        {
                            pathData    =  MaterialVectorIcons.Carousel,
                            stroke      = "none",
                            strokeWidth = 1,
                            fill        = "rgba(255,250,0,1)"
                        }
                    }
                }
            },
            {
                "ArrayIcon", new AlexaVectorGraphic()
                {
                    height         = 35,
                    width          = 35,
                    viewportHeight = 25,
                    viewportWidth  = 25,
                    items          = new List<IVectorGraphic>()
                    {
                        new VectorPath()
                        {
                            pathData    =  MaterialVectorIcons.ArrayIcon,
                            stroke      = "none",
                            strokeWidth = 1,
                            fill        = "rgba(255,250,0,1)"
                        }
                    }
                }
            },
            {
                "AlexaLarge", new AlexaVectorGraphic()
                {
                    parameters = new List<string>()
                    {
                        "strokeDashOffset",
                        "fill",
                        "stroke"
                    },
                    height         = 235,
                    width          = 235,
                    viewportHeight = 25,
                    viewportWidth  = 25,
                    items          = new List<IVectorGraphic>()
                    {
                        new VectorPath()
                        {
                            pathData    = MaterialVectorIcons.Alexa,
                            stroke      = "${stroke}",
                            strokeWidth = 0.5,
                            fill        = "${fill}",
                            strokeDashArray = new List<string>()
                            {
                                "${strokeDashOffset}",
                                "100"
                            },
                            strokeDashOffset = 0,
                            filters = new List<VectorFilter>()
                            {
                                new VectorFilter()
                                {
                                    type             = VectorFilterType.DropShadow,
                                    color            = "rgba(0,0,0,0.375)",
                                    horizontalOffset = 0.005,
                                    verticalOffset   = 0.005,
                                    radius           = 1
                                }
                            }
                        }
                    }
                }
            },
            {
                "EmbyLarge", new AlexaVectorGraphic()
                {
                    parameters = new List<string>()
                    {
                        "strokeDashOffset",
                        "fill",
                        "stroke"
                    },
                    height         = 240,
                    width          = 240,
                    viewportHeight = 25,
                    viewportWidth  = 25,
                    items          = new List<IVectorGraphic>()
                    {
                        new VectorPath()
                        {
                            pathData    = MaterialVectorIcons.EmbyIcon,
                            stroke      = "${stroke}",
                            strokeWidth = 0.5,
                            fill        = "${fill}",
                            strokeDashArray = new List<string>()
                            {
                                "${strokeDashOffset}",
                                "100"
                            },
                            strokeDashOffset = 0,
                            filters = new List<VectorFilter>()
                            {
                                new VectorFilter()
                                {
                                    type             = VectorFilterType.DropShadow,
                                    color            = "rgba(0,0,0,0.375)",
                                    horizontalOffset = 0.005,
                                    verticalOffset   = 0.005,
                                    radius           = 1
                                }
                            }
                        }
                    }
                }
            },
            {
                "EmbySmall", new AlexaVectorGraphic()
                {
                    height         = 75,
                    width          = 75,
                    viewportHeight = 25,
                    viewportWidth  = 25,
                    items          = new List<IVectorGraphic>()
                    {
                        new VectorPath()
                        {
                            pathData    = MaterialVectorIcons.EmbyIcon,
                            stroke      = "none",
                            strokeWidth = 0,
                            fill        = "white"
                        }
                    }
                }
            },
            {
                "Line", new AlexaVectorGraphic()
                {
                    height         = 55,
                    width          = 500,
                    viewportWidth  = 50,
                    viewportHeight = 50,
                    items          = new List<IVectorGraphic>()
                    {
                        new VectorPath()
                        {
                            pathData    = "M0 0 l1120 0",
                            stroke      = "rgba(255,255,255)",
                            strokeWidth = 1
                        }
                    }
                }
            }
        };
    }
}
