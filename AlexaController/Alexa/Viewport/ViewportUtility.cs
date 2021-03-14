/*
 *  Taken from Alexa SDK NodeJS code and wrapped for C#
 *
 */

using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming
// ReSharper disable ComplexConditionExpression

namespace AlexaController.Alexa.Viewport
{

    public enum ViewportProfile
    {
        HUB_ROUND_SMALL = 0,
        HUB_LANDSCAPE_SMALL = 1,
        HUB_LANDSCAPE_MEDIUM = 2,
        HUB_LANDSCAPE_LARGE = 3,
        MOBILE_LANDSCAPE_SMALL = 4,
        MOBILE_PORTRAIT_SMALL = 5,
        MOBILE_LANDSCAPE_MEDIUM = 6,
        MOBILE_PORTRAIT_MEDIUM = 7,
        TV_PORTRAIT_MEDIUM = 8,
        TV_LANDSCAPE_XLARGE = 9,
        TV_LANDSCAPE_MEDIUM = 10,
        UNKNOWN_VIEWPORT_PROFILE = 11
    }

    internal enum ViewportOrientation
    {
        EQUAL = 0,
        LANDSCAPE = 1,
        PORTRAIT = 2
    }

    internal enum ViewportSizeGroup
    {
        XSMALL = 0,
        SMALL = 1,
        MEDIUM = 2,
        LARGE = 3,
        XLARGE = 4
    }

    internal enum ViewportDpiGroup
    {
        XLOW = 0,
        LOW = 1,
        MEDIUM = 2,
        HIGH = 3,
        XHIGH = 4,
        XXHIGH = 5
    }


    public interface IViewportUtility
    {
        ViewportProfile GetViewportProfile(RequestModel.Viewport viewportState);
        bool ViewportSizeIsLessThen(ViewportProfile profile1, ViewportProfile profile2);
        bool ViewportSizeIsGreaterThen(ViewportProfile profile1, ViewportProfile profile2);
    }

    public class ViewportUtility : IViewportUtility
    {
        private ViewportSizeGroup GetViewportSizeGroup(int size)
        {
            if (isBetween(size, 0, 600))
            {
                return ViewportSizeGroup.XSMALL;
            }

            if (isBetween(size, 600, 960))
            {
                return ViewportSizeGroup.SMALL;
            }

            if (isBetween(size, 960, 1280))
            {
                return ViewportSizeGroup.MEDIUM;
            }

            if (isBetween(size, 1280, 1920))
            {
                return ViewportSizeGroup.LARGE;
            }

            if (isBetween(size, 1920, int.MaxValue))
            {
                return ViewportSizeGroup.XLARGE;
            }

            throw new Exception($"unknown size group value {size}");
        }

        private readonly List<ViewportSizeGroup> ViewportSizeGroupOrder = new List<ViewportSizeGroup>()
        {
            ViewportSizeGroup.XSMALL,
            ViewportSizeGroup.SMALL,
            ViewportSizeGroup.MEDIUM,
            ViewportSizeGroup.LARGE,
            ViewportSizeGroup.XLARGE
        };

        private ViewportDpiGroup GetViewportDpiGroup(int dpi)
        {
            if (isBetween(dpi, 0, 121))
            {
                return ViewportDpiGroup.XLOW;
            }

            if (isBetween(dpi, 121, 161))
            {
                return ViewportDpiGroup.LOW;
            }

            if (isBetween(dpi, 161, 241))
            {
                return ViewportDpiGroup.MEDIUM;
            }

            if (isBetween(dpi, 241, 321))
            {
                return ViewportDpiGroup.HIGH;
            }

            if (isBetween(dpi, 321, 481))
            {
                return ViewportDpiGroup.XHIGH;
            }

            if (isBetween(dpi, 481, int.MaxValue))
            {
                return ViewportDpiGroup.XXHIGH;
            }
            throw new Exception($"unknown dpi group value {dpi}");
        }

        private readonly List<ViewportDpiGroup> ViewportDpiGroupOrder = new List<ViewportDpiGroup>()
        {
            ViewportDpiGroup.XLOW,
            ViewportDpiGroup.LOW,
            ViewportDpiGroup.MEDIUM,
            ViewportDpiGroup.HIGH,
            ViewportDpiGroup.XHIGH,
            ViewportDpiGroup.XXHIGH
        };

        private ViewportOrientation GetViewportOrientation(int width, int height)
        {
            return width > height ? ViewportOrientation.LANDSCAPE : width < height ? ViewportOrientation.PORTRAIT : ViewportOrientation.EQUAL;
        }

        private readonly List<ViewportProfile> ViewportProfileOrder = new List<ViewportProfile>()
        {
            ViewportProfile.HUB_ROUND_SMALL,
            ViewportProfile.HUB_LANDSCAPE_SMALL,
            ViewportProfile.HUB_LANDSCAPE_MEDIUM,
            ViewportProfile.HUB_LANDSCAPE_LARGE,
            ViewportProfile.TV_LANDSCAPE_MEDIUM,
            ViewportProfile.TV_LANDSCAPE_XLARGE
        };

        public ViewportProfile GetViewportProfile(RequestModel.Viewport viewportState)
        {
            if (viewportState is null) return ViewportProfile.UNKNOWN_VIEWPORT_PROFILE;

            var currentPixelWidth = viewportState.pixelWidth;
            var currentPixelHeight = viewportState.pixelHeight;
            var dpi = viewportState.dpi;

            var shape = viewportState.shape;
            var viewportOrientation = GetViewportOrientation(currentPixelWidth, currentPixelHeight);
            var viewportDpiGroup = GetViewportDpiGroup(dpi);
            var pixelWidthSizeGroup = GetViewportSizeGroup(currentPixelWidth);
            var pixelHeightSizeGroup = GetViewportSizeGroup(currentPixelHeight);

            if (shape == "ROUND"
                && viewportOrientation == ViewportOrientation.EQUAL
                && viewportDpiGroup == ViewportDpiGroup.LOW
                && pixelWidthSizeGroup == ViewportSizeGroup.XSMALL
                && pixelHeightSizeGroup == ViewportSizeGroup.XSMALL)
            {
                return ViewportProfile.HUB_ROUND_SMALL;
            }

            if (shape == "RECTANGLE"
                && viewportOrientation == ViewportOrientation.LANDSCAPE
                && viewportDpiGroup == ViewportDpiGroup.LOW
                && ViewportSizeGroupOrder.IndexOf(pixelWidthSizeGroup) <= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.MEDIUM)
                && ViewportSizeGroupOrder.IndexOf(pixelHeightSizeGroup) <= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.XSMALL))
            {
                return ViewportProfile.HUB_LANDSCAPE_SMALL;
            }

            if (shape == "RECTANGLE"
                && viewportOrientation == ViewportOrientation.LANDSCAPE
                && viewportDpiGroup == ViewportDpiGroup.LOW
                && ViewportSizeGroupOrder.IndexOf(pixelWidthSizeGroup) <= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.MEDIUM)
                && ViewportSizeGroupOrder.IndexOf(pixelHeightSizeGroup) <= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.SMALL))
            {
                return ViewportProfile.HUB_LANDSCAPE_MEDIUM;
            }

            if (shape == "RECTANGLE"
                && viewportOrientation == ViewportOrientation.LANDSCAPE
                && viewportDpiGroup == ViewportDpiGroup.LOW
                && ViewportSizeGroupOrder.IndexOf(pixelWidthSizeGroup) >= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.LARGE)
                && ViewportSizeGroupOrder.IndexOf(pixelHeightSizeGroup) >= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.SMALL))
            {
                return ViewportProfile.HUB_LANDSCAPE_LARGE;
            }

            if (shape == "RECTANGLE"
                && viewportOrientation == ViewportOrientation.LANDSCAPE
                && viewportDpiGroup == ViewportDpiGroup.MEDIUM
                && ViewportSizeGroupOrder.IndexOf(pixelWidthSizeGroup) >= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.MEDIUM)
                && ViewportSizeGroupOrder.IndexOf(pixelHeightSizeGroup) >= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.SMALL))
            {
                return ViewportProfile.MOBILE_LANDSCAPE_MEDIUM;
            }

            if (shape == "RECTANGLE"
                && viewportOrientation == ViewportOrientation.PORTRAIT
                && viewportDpiGroup == ViewportDpiGroup.MEDIUM
                && ViewportSizeGroupOrder.IndexOf(pixelWidthSizeGroup) >= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.SMALL)
                && ViewportSizeGroupOrder.IndexOf(pixelHeightSizeGroup) >= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.MEDIUM))
            {
                return ViewportProfile.MOBILE_PORTRAIT_MEDIUM;
            }


            if (shape == "RECTANGLE"
                && viewportOrientation == ViewportOrientation.LANDSCAPE
                && viewportDpiGroup == ViewportDpiGroup.MEDIUM
                && ViewportSizeGroupOrder.IndexOf(pixelWidthSizeGroup) >= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.SMALL)
                && ViewportSizeGroupOrder.IndexOf(pixelHeightSizeGroup) >= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.XSMALL))
            {
                return ViewportProfile.MOBILE_LANDSCAPE_SMALL;
            }


            if (shape == "RECTANGLE"
                && viewportOrientation == ViewportOrientation.PORTRAIT
                && viewportDpiGroup == ViewportDpiGroup.MEDIUM
                && ViewportSizeGroupOrder.IndexOf(pixelWidthSizeGroup) <= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.XSMALL)
                && ViewportSizeGroupOrder.IndexOf(pixelHeightSizeGroup) <= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.SMALL))
            {
                return ViewportProfile.MOBILE_PORTRAIT_SMALL;
            }


            if (shape == "RECTANGLE"
                && viewportOrientation == ViewportOrientation.LANDSCAPE
                && ViewportDpiGroupOrder.IndexOf(viewportDpiGroup) >= ViewportDpiGroupOrder.IndexOf(ViewportDpiGroup.HIGH)
                && ViewportSizeGroupOrder.IndexOf(pixelWidthSizeGroup) >= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.XLARGE)
                && ViewportSizeGroupOrder.IndexOf(pixelHeightSizeGroup) >= ViewportSizeGroupOrder.IndexOf(ViewportSizeGroup.MEDIUM))
            {
                return ViewportProfile.TV_LANDSCAPE_XLARGE;
            }

            if (shape == "RECTANGLE"
                && viewportOrientation == ViewportOrientation.PORTRAIT
                && ViewportDpiGroupOrder.IndexOf(viewportDpiGroup) >= ViewportDpiGroupOrder.IndexOf(ViewportDpiGroup.HIGH)
                && pixelWidthSizeGroup == ViewportSizeGroup.XSMALL
                && pixelHeightSizeGroup == ViewportSizeGroup.XLARGE)
            {
                return ViewportProfile.TV_PORTRAIT_MEDIUM;
            }

            if (shape == "RECTANGLE"
                && viewportOrientation == ViewportOrientation.LANDSCAPE
                && ViewportDpiGroupOrder.IndexOf(viewportDpiGroup) >= ViewportDpiGroupOrder.IndexOf(ViewportDpiGroup.HIGH)
                && pixelWidthSizeGroup == ViewportSizeGroup.MEDIUM
                && pixelHeightSizeGroup == ViewportSizeGroup.SMALL)
            {
                return ViewportProfile.TV_LANDSCAPE_MEDIUM;
            }

            return ViewportProfile.UNKNOWN_VIEWPORT_PROFILE;

        }

        public bool ViewportSizeIsLessThen(ViewportProfile profile1, ViewportProfile profile2) => ViewportProfileOrder.IndexOf(profile1) < ViewportProfileOrder.IndexOf(profile2);

        public bool ViewportSizeIsGreaterThen(ViewportProfile profile1, ViewportProfile profile2) => ViewportProfileOrder.IndexOf(profile1) > ViewportProfileOrder.IndexOf(profile2);

        private bool isBetween(int target, int min, int max) => target >= min && target < max;

    }
}




