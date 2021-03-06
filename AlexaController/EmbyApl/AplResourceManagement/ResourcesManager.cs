﻿using AlexaController.Alexa.Presentation.APL;
using System.Collections.Generic;

namespace AlexaController.EmbyApl.AplResourceManagement
{
    public static class ResourcesManager
    {
        public static List<IResource> RenderResourcesList => new List<IResource>()
        {
            new Resource()
            {
                description = "Stock color for the light theme",
                colors      = new Colors()
                {
                    colorTextPrimary = "#151920"
                }
            },
            new Resource()
            {
                description = "Stock color for the dark theme",
                when        = "${viewport.theme == 'dark'}",
                colors      = new Colors()
                {
                    colorTextPrimary = "#f0f1ef"
                }
            },
            new Resource()
            {
                description = "Standard font sizes",
                dimensions  = new Dimensions()
                {
                    textSizeBody          = 48,
                    textSizePrimary       = 27,
                    textSizeSecondary     = 23,
                    textSizeSecondaryHint = 25
                }
            },
            new Resource()
            {
                description = "Common spacing values",
                dimensions  = new Dimensions()
                {
                    spacingThin       = 6,
                    spacingSmall      = 12,
                    spacingMedium     = 24,
                    spacingLarge      = 48,
                    spacingExtraLarge = 72
                }
            },
            new Resource()
            {
                description = "Common margins and padding",
                dimensions  = new Dimensions()
                {
                    marginTop    = 40,
                    marginLeft   = 60,
                    marginRight  = 60,
                    marginBottom = 40
                }
            }
        };
    }
}
