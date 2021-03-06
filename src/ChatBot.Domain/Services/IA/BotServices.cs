﻿
using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Configuration;

namespace ChatBot.Domain.Services.IA
{
    public class BotServices
    {
        [Obsolete]
        public BotServices(BotConfiguration botConfiguration)
        {
            foreach (var service in botConfiguration.Services)
            {
                switch (service.Type)
                {
                    case ServiceTypes.Luis:
                        {
                            var luis = (LuisService)service ?? throw new InvalidOperationException("The LUIS service is not configured correctly in your '.bot' file."); 
                            var app = new LuisApplication(luis.AppId, luis.AuthoringKey, luis.GetEndpoint());
                            var recognizer = new LuisRecognizer(app);

                            LuisServices.Add(luis.Name, recognizer);

                            break;
                        }

                    case ServiceTypes.QnA:
                        {
                            var qna = (QnAMakerService)service ?? throw new InvalidOperationException("The Qna service is not configured correctly in your '.bot' file.");
                            var qnaMaker = new QnAMaker(
                                new QnAMakerEndpoint
                                {
                                    EndpointKey = qna.EndpointKey,
                                    Host = qna.Hostname,
                                    KnowledgeBaseId = qna.KbId
                                }, 
                                new QnAMakerOptions
                                {
                                    Top = 1,
                                    ScoreThreshold = 0.5f
                                });

                            QnAServices.Add(qna.Name, qnaMaker);

                            break;
                        }

                }
            }
        }

        public Dictionary<string, LuisRecognizer> LuisServices { get; } = new Dictionary<string, LuisRecognizer>();
        public Dictionary<string, QnAMaker> QnAServices { get; } = new Dictionary<string, QnAMaker>();

    }
}


