using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace HoaWebApplication.Extensions.Streams
{
    public static class StreamExtensions
    {
        private static int DefaultBufferSizeOnRead { get; } = 1024;

        /// <summary>
        /// Read from the stream and deserialize into an object of type T (assuming Json content).
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="stream">The stream</param>
        /// <returns>An object of type T</returns>
        public static T ReadAndDeserializeFromJson<T>(
            this Stream stream)
        {
            return ReadAndDeserializeFromJson<T>(stream, new UTF8Encoding(), true,
                DefaultBufferSizeOnRead, false);
        }

        /// <summary>
        /// Read from the stream and deserialize into an object of type T (assuming Json content).
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="stream">The stream</param>
        /// <param name="encoding">The encoding to use</param>
        /// <returns>An object of type T</returns>
        public static T ReadAndDeserializeFromJson<T>(
            this Stream stream,
            Encoding encoding)
        {
            return ReadAndDeserializeFromJson<T>(stream, encoding, true,
                DefaultBufferSizeOnRead, false);
        }

        /// <summary>
        /// Read from the stream and deserialize into an object of type T (assuming Json content).
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="stream">The stream</param>
        /// <param name="detectEncodingFromByteOrderMarks">True to detect encoding from byte order marks, false otherwise</param>
        /// <returns>An object of type T</returns>
        public static T ReadAndDeserializeFromJson<T>(
            this Stream stream,
            bool detectEncodingFromByteOrderMarks)
        {
            return ReadAndDeserializeFromJson<T>(stream, new UTF8Encoding(),
                detectEncodingFromByteOrderMarks, DefaultBufferSizeOnRead, false);
        }

        /// <summary>
        /// Read from the stream and deserialize into an object of type T (assuming Json content).
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="stream">The stream</param>
        /// <param name="encoding">The encoding to use</param>
        /// <param name="detectEncodingFromByteOrderMarks">True to detect encoding from byte order marks, false otherwise</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <returns>An object of type T</returns>
        public static T ReadAndDeserializeFromJson<T>(
            this Stream stream,
            Encoding encoding,
            bool detectEncodingFromByteOrderMarks,
            int bufferSize)
        {
            return ReadAndDeserializeFromJson<T>(stream, encoding,
                detectEncodingFromByteOrderMarks, bufferSize, false);
        }

        /// <summary>
        /// Read from the stream and deserialize into an object of type T (assuming Json content).
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="stream">The stream</param>
        /// <param name="encoding">The encoding to use</param>
        /// <param name="detectEncodingFromByteOrderMarks">True to detect encoding from byte order marks, false otherwise</param>
        /// <param name="bufferSize">The size of the buffer</param>
        /// <param name="leaveOpen">True to leave the stream open after the (internally used) StreamReader object is disposed</param>
        /// <returns>An object of type T</returns>
        public static T ReadAndDeserializeFromJson<T>(
            this Stream stream,
            Encoding encoding,
            bool detectEncodingFromByteOrderMarks,
            int bufferSize,
            bool leaveOpen)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanRead)
            {
                throw new NotSupportedException("Can't read from this stream.");
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            using (var streamReader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var jsonSerializer = new JsonSerializer();
                    return jsonSerializer.Deserialize<T>(jsonTextReader);
                }
            }
        }      
    }
}
