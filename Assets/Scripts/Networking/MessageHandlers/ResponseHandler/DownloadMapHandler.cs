using System;
using System.Collections.Generic;
using System.IO;
using Infrastructure.Services.StaticData;
using MapLogic;
using Networking.Messages.Responses;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class DownloadMapHandler : ResponseHandler<DownloadMapResponse>
    {
        private readonly MapLoadingProgress _mapLoadProgress;

        public DownloadMapHandler(MapLoadingProgress mapLoadingProgress)
        {
            _mapLoadProgress = mapLoadingProgress;
        }

        protected override void OnResponseReceived(DownloadMapResponse message)
        {
            _mapLoadProgress.AddBytes(message.ByteChunk);
            _mapLoadProgress.UpdateProgress(message.Progress);
        }
    }
}