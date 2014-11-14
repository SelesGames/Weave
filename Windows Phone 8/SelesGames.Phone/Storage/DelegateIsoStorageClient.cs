using System;
using System.IO;

namespace SelesGames.IsoStorage
{
    public class DelegateIsoStorageClient<T> : IsoStorageClient<T>
    {
        Func<Stream, T> readAction;
        Action<T, Stream> writeAction;

        public DelegateIsoStorageClient(Func<Stream, T> readAction, Action<T, Stream> writeAction)
        {
            this.readAction = readAction;
            this.writeAction = writeAction;
        }
        
        protected override T ReadObject(Stream stream)
        {
            return readAction(stream);
        }

        protected override void WriteObject(T obj, Stream stream)
        {
            writeAction(obj, stream);
        }
    }
}
