using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace KerbalKonstructs.Core
{
    public enum EntryType : byte
    {
        File = 0,
        FileObsolete = 0x30,
        HardLink = 0x31,
        SymLink = 0x32,
        CharDevice = 0x33,
        BlockDevice = 0x34,
        Directory = 0x35,
        Fifo = 0x36,
    }


    //internal class TarHeader : ITarHeader
    internal class TarHeader
    {
        private readonly byte[] buffer = new byte[512];
        private long headerChecksum;

        public TarHeader()
        {
            // Default values
            Mode = 511; // 0777 dec
            UserId = 61; // 101 dec
            GroupId = 61; // 101 dec
        }

        private string fileName;
        public long SizeInBytes;
        public EntryType EntryType;
        public int Mode;
        public int UserId;
        public int GroupId;
        public DateTime LastModification;
        public readonly int HeaderSize = 512;


        protected readonly DateTime TheEpoch = new DateTime(1970, 1, 1, 0, 0, 0);

        private static readonly byte[] spaces = Encoding.ASCII.GetBytes("        ");

        public virtual string fileNameString
        {
            get
            {
                return fileName.Replace("\0", string.Empty);
            }
            set
            {
                if (value.Length > 100)
                {
                    Log.UserError("A file name can not be more than 100 chars long");
                }
                fileName = value;
            }
        }


        public string modeString
        {
            get
            {
                return Convert.ToString(Mode, 8).PadLeft(7, '0');
            }
        }

        public string userIdString
        {
            get
            {
                return Convert.ToString(UserId, 8).PadLeft(7, '0');
            }
        }

        public string groupIdString
        {
            get
            {
                return Convert.ToString(GroupId, 8).PadLeft(7, '0');
            }
        }

        public string sizeString
        {
            get
            {
                return Convert.ToString(SizeInBytes, 8).PadLeft(11, '0');
            }
        }

        public string lastModificationString
        {
            get
            {
                return Convert.ToString((long)(LastModification - TheEpoch).TotalSeconds, 8).PadLeft(11, '0');
            }
        }

        public string headerChecksumString
        {
            get
            {
                return Convert.ToString(headerChecksum, 8).PadLeft(6, '0');
            }
        }


        public virtual byte[] GetHeaderValue()
        {
            // Clean old values
            Array.Clear(buffer, 0, buffer.Length);

            if (string.IsNullOrEmpty(fileNameString))
            {
                Log.UserError("FileName can not be empty.");
            }

            if (fileNameString.Length >= 100)
            {
                Log.UserError("FileName is too long. It must be less than 100 bytes.");
            }

            // Fill header
            Encoding.ASCII.GetBytes(fileNameString.PadRight(100, '\0')).CopyTo(buffer, 0);
            Encoding.ASCII.GetBytes(modeString).CopyTo(buffer, 100);
            Encoding.ASCII.GetBytes(userIdString).CopyTo(buffer, 108);
            Encoding.ASCII.GetBytes(groupIdString).CopyTo(buffer, 116);
            Encoding.ASCII.GetBytes(sizeString).CopyTo(buffer, 124);
            Encoding.ASCII.GetBytes(lastModificationString).CopyTo(buffer, 136);

            //            buffer[156] = 20;
            buffer[156] = ((byte)EntryType);


            RecalculateChecksum(buffer);

            // Write checksum
            Encoding.ASCII.GetBytes(headerChecksumString).CopyTo(buffer, 148);

            return buffer;
        }

        protected virtual void RecalculateChecksum(byte[] buf)
        {
            // Set default value for checksum. That is 8 spaces.
            spaces.CopyTo(buf, 148);

            // Calculate checksum
            headerChecksum = 0;
            foreach (byte b in buf)
            {
                headerChecksum += b;
            }
        }
    }


    public class LegacyTarWriter : IDisposable
    {
        private readonly Stream outStreamObj;
        protected byte[] buffer = new byte[1024];
        private bool isClosed;
        public bool ReadOnZero = true;

        /// <summary>
        /// Writes tar (see GNU tar) archive to a stream
        /// </summary>
        /// <param name="writeStream">stream to write archive to</param>
        public LegacyTarWriter(Stream writeStream)
        {
            outStreamObj = writeStream;
        }

        protected virtual Stream outStream
        {
            get
            {
                return outStreamObj;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion


        public void WriteDirectoryEntry(string path)
        {
            WriteDirectoryEntry(path, 101, 101, 0777);
        }


        public void WriteDirectoryEntry(string path, int userId, int groupId, int mode)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (path[path.Length - 1] != '/')
            {
                path += '/';
            }
            DateTime lastWriteTime;
            if (Directory.Exists(path))
            {
                lastWriteTime = Directory.GetLastWriteTime(path);
            }
            else
            {
                lastWriteTime = DateTime.Now;
            }
            WriteHeader(path, lastWriteTime, 0, userId, groupId, mode, EntryType.Directory);
        }


        public static string GetRelativePath(string fullDestinationPath, string startPath)
        {
            int offsetlength = startPath.Length;

            var bla = fullDestinationPath.ToArray().Skip(offsetlength).ToArray();
            return new string(bla);
        }

        public virtual void Write(Stream data, long dataSizeInBytes, string name, int userId, int groupId, int mode, DateTime lastModificationTime)
        {
            if (isClosed)
            {
                Log.UserError("Can not write to the closed writer");
            }

            WriteHeader(name, lastModificationTime, dataSizeInBytes, userId, groupId, mode, EntryType.File);
            WriteContent(dataSizeInBytes, data);
            AlignTo512(dataSizeInBytes, false);
        }

        protected void WriteContent(long count, Stream data)
        {
            while (count > 0 && count > buffer.Length)
            {
                int bytesRead = data.Read(buffer, 0, buffer.Length);
                if (bytesRead < 0)
                {
                    throw new IOException("LegacyTarWriter unable to read from provided stream");
                }

                if (bytesRead == 0)
                {
                    if (ReadOnZero)
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        break;
                    }
                }
                outStream.Write(buffer, 0, bytesRead);
                count -= bytesRead;
            }
            if (count > 0)
            {
                int bytesRead = data.Read(buffer, 0, (int)count);
                if (bytesRead < 0)
                {
                    throw new IOException("LegacyTarWriter unable to read from provided stream");
                }

                if (bytesRead == 0)
                {
                    while (count > 0)
                    {
                        outStream.WriteByte(0);
                        --count;
                    }
                }
                else
                {
                    outStream.Write(buffer, 0, bytesRead);
                }
            }
        }

        protected virtual void WriteHeader(string name, DateTime lastModificationTime, long count, int userId, int groupId, int mode, EntryType entryType)
        {
            var header = new TarHeader
            {
                fileNameString = name,
                LastModification = lastModificationTime,
                SizeInBytes = count,
                UserId = userId,
                GroupId = groupId,
                Mode = mode,
                EntryType = entryType
            };
            outStream.Write(header.GetHeaderValue(), 0, header.HeaderSize);
        }


        public void AlignTo512(long size, bool acceptZero)
        {
            size %= 512;
            if (size == 0 && !acceptZero)
            {
                return;
            }

            while (size < 512)
            {
                outStream.WriteByte(0);
                size++;
            }
        }

        public virtual void Close()
        {
            if (isClosed)
            {
                return;
            }

            AlignTo512(0, true);
            AlignTo512(0, true);
            isClosed = true;
        }
    }
}
