using System;
using EXPLORADORDEARCHIVOS_TERMINADO.Interfaces;

namespace EXPLORADORDEARCHIVOS_TERMINADO.Services
{
    public class MultiMediaPlayer : IMediaManager
    {
        private readonly Services.DefaultMediaFactory _factory;

        public MultiMediaPlayer(Services.DefaultMediaFactory factory)
        {
            _factory = factory;
        }

        public void OpenMedia(string filePath)
        {
            var handler = _factory.CreateHandlerFor(filePath);
            handler.Handle(filePath);
        }

        public void CloseAll()
        {
            // No-op: DefaultMediaFactory handlers mantienen cierre interno
        }
    }
}
