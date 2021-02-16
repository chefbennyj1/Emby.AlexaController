﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APL.Commands;
using Parallel = AlexaController.Alexa.Presentation.APL.Commands.Parallel;

namespace AlexaController.Alexa.Presentation.APL
{
    public class Animations
    {
        public static async Task<ICommand> FadeOutItem(string componentId, int duration, int? delay = null)
        {
            return await Task.FromResult(new AnimateItem()
            {
                componentId = componentId,
                easing = "ease-in",
                duration = duration,
                delay = delay ?? 0,
                value = new List<IValue>()
                {
                    new OpacityValue()
                    {
                        @from = 1,
                        to = 0
                    }
                }
            });
        }

        public static async Task<ICommand> FadeIn(string componentId, int duration, int? delay = null)
        {
            return await Task.FromResult(new AnimateItem()
            {
                componentId = componentId,
                easing = "ease-in",
                duration = duration,
                delay = delay ?? 0,
                value = new List<IValue>()
                {
                    new OpacityValue()
                    {
                        @from = 0,
                        to = 1
                    }
                }
            });
        }

        public static async Task<ICommand> ScaleFadeInItem(string componentId, int duration, int? delay = null)
        {
            // ReSharper disable once ComplexConditionExpression
            return await Task.FromResult(new Parallel()
            {
                commands = new List<ICommand>()
                {
                    new AnimateItem()
                    {
                        componentId = componentId,
                        duration = duration,
                        delay = delay ?? 0,
                        value = new List<IValue>()
                        {
                            new OpacityValue()
                            {
                                @from = 0,
                                to = 1
                            }
                        }
                    },
                    new AnimateItem()
                    {
                        componentId = componentId,
                        duration = duration,
                        delay = delay ?? 0,
                        value = new List<IValue>()
                        {
                            new TransitionValue()
                            {
                                @from = new List<From>()
                                {
                                    new From()
                                    {
                                        scaleX = 1.2,
                                        scaleY = 1.2
                                    }
                                },
                                to = new List<To>()
                                {
                                    new To()
                                    {
                                        scaleX = 1,
                                        scaleY = 1
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        public static async Task<ICommand> ScaleInOutOnPress()
        {
            return await Task.FromResult(new Sequential()
            {
                commands = new List<ICommand>()
                {
                    new AnimateItem()
                    {
                        easing = "ease",
                        duration = 250,
                        value = new List<IValue>()
                        {
                            new TransitionValue()
                            {
                                from = new List<From>() {new From() {scaleX = 1, scaleY = 1}},
                                to = new List<To>() {new To() {scaleX = 0.9, scaleY = 0.9}}
                            }
                        }
                    },
                    new AnimateItem()
                    {
                        easing = "ease",
                        duration = 250,
                        value = new List<IValue>()
                        {
                            new TransitionValue()
                            {
                                from = new List<From>() {new From() {scaleX = 0.9, scaleY = 0.9}},
                                to = new List<To>() {new To() {scaleX = 1, scaleY = 1}}
                            }
                        }
                    }
                }
            });
        }

        public static async Task<ICommand> FadeInUp(string componentId, int duration, string distance, int? delay = null)
        {
            return await Task.FromResult(new AnimateItem()
            {
                componentId = componentId,
                easing = "ease-in",
                duration = duration,
                delay = delay ?? 0,
                value = new List<IValue>()
                {
                    new OpacityValue()
                    {
                        @from = 0,
                        to = 1
                    },
                    new TransitionValue()
                    {
                        from = new List<From>()
                        {
                            new From()
                            {
                                translateY = distance
                            }
                        },
                        to = new List<To>()
                        {
                            new To()
                            {
                                translateY = "0"
                            }
                        }
                    }
                }
            });
        }
        
    }
}