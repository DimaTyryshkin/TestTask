using System;
using System.IO;
using System.Text;

namespace TestTask
{
    public class ReadOnlyStream : IReadOnlyStream
    {
        private readonly Stream _localStream;
        private StreamReader _textStreamReader;

        /// <summary>
        /// Конструктор класса. 
        /// Т.к. происходит прямая работа с файлом, необходимо 
        /// обеспечить ГАРАНТИРОВАННОЕ закрытие файла после окончания работы с таковым!
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        public ReadOnlyStream(string fileFullPath)
        {
            _localStream = File.OpenRead(fileFullPath);
            if (_localStream == null)
                throw new Exception($"Cant open file '{fileFullPath}'");
            
            _textStreamReader = new StreamReader(_localStream); 
        }
 
        ~ReadOnlyStream()
        {
            Dispose();
        }
        
        public void Dispose()
        { 
            _localStream.Dispose();
        }

        /// <summary>
        /// Флаг окончания файла.
        /// </summary>
        public bool IsEof => _textStreamReader.EndOfStream;

        /// <summary>
        /// Ф-ция чтения следующего символа из потока.
        /// Если произведена попытка прочитать символ после достижения конца файла, метод 
        /// должен бросать соответствующее исключение
        /// </summary>
        /// <returns>Считанный символ.</returns>
        public char ReadNextChar()
        {
            if (IsEof)
                throw new EndOfStreamException();
            
            return (char)_textStreamReader.Read();
        }

        /// <summary>
        /// Сбрасывает текущую позицию потока на начало.
        /// </summary>
        public void ResetPositionToStart()
        {
            _localStream.Seek(0, SeekOrigin.Begin);
            _textStreamReader = new StreamReader(_localStream, Encoding.Unicode);
        }
    }
}
