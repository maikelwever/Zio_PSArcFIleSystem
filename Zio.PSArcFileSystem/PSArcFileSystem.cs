using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Be.IO;
using Zio.FileSystems;

namespace Zio.PSArcFileSystem
{
    public class PSArcFileSystem : FileSystem
    {
        protected const string FileSystemIsReadOnly = "PSArc FileSystem is read-only";
        
        private Stream PSArcStream;

        public ushort MinorVersion;
        public ushort MajorVersion;
        public uint CompressionType;
        public uint TocLength;
        public uint TocEntrySize;
        public uint TocEntryCount;
        public uint BlockSize;
        public uint ArchiveFlags;
        private List<FileEntry> Entries;
        private List<ulong> BlockSizes;

        public PSArcFileSystem(IFileSystem parentFileSystem, UPath psArcPath)
        {
            if (!parentFileSystem.FileExists(psArcPath))
            {
                throw new IOException($"Could not find {psArcPath} on the given filesystem.");
            }
            
            PSArcStream = parentFileSystem.OpenFile(psArcPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            ParseArcHeader();
            ParseArcManifest();
        }

        private void ParseArcHeader()
        {
            PSArcStream.Seek(0, SeekOrigin.Begin);
            var binaryStream = new BeBinaryReader(PSArcStream);
            var magic = binaryStream.ReadUInt32();
            if (magic != 0x50534152)
            {
                throw new IOException(
                    $"Did not encounter correct magic value during PSArc parsing. Instead, got '{magic:X}'.");
            }

            MajorVersion = binaryStream.ReadUInt16();
            MinorVersion = binaryStream.ReadUInt16();
            CompressionType = binaryStream.ReadUInt32();
            TocLength = binaryStream.ReadUInt32();
            TocEntrySize = binaryStream.ReadUInt32();
            TocEntryCount = binaryStream.ReadUInt32();
            BlockSize = binaryStream.ReadUInt32();
            ArchiveFlags = binaryStream.ReadUInt32();

            Entries = new List<FileEntry>();
            for (var i = 0; i < TocEntryCount; i++)
            {
                Entries.Add(new FileEntry(
                    System.Text.Encoding.ASCII.GetChars(binaryStream.ReadBytes(16)).ToString(),
                    binaryStream.ReadUInt32(),
                    ParseUInt40(binaryStream.ReadBytes(5)),
                    ParseUInt40(binaryStream.ReadBytes(5))
                ));
            }

            var positionAfterEntries = PSArcStream.Position;

            BlockSizes = new List<ulong>();
            long numberOfBlocks = 0;
            switch (BlockSize)
            {
                case 65536:
                    numberOfBlocks = (TocLength - positionAfterEntries) / 2;
                    for (long i = 0; i < numberOfBlocks; i++) BlockSizes.Add(binaryStream.ReadUInt16());
                    break;
                case 16777216:
                    numberOfBlocks = (TocLength - positionAfterEntries) / 3;
                    for (long i = 0; i < numberOfBlocks; i++) BlockSizes.Add(ParseUInt24(binaryStream.ReadBytes(3)));
                    break;
                case 4294967295:
                    numberOfBlocks = (TocLength - positionAfterEntries) / 4;
                    for (long i = 0; i < numberOfBlocks; i++) BlockSizes.Add(binaryStream.ReadUInt32());
                    break;
                default:
                    throw new IOException($"Invalid block size for PSArc: {BlockSize}");
            };

        }

        private void ParseArcManifest()
        {
            
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ParseUInt24(byte[] p)
        {
            var array = new byte[4];
            p.CopyTo(array, 1);
            Array.Reverse(array);
            return BitConverter.ToUInt32(array, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ParseUInt40(byte[] p)
        {
            var array = new byte[8];
            p.CopyTo(array, 3);
            Array.Reverse(array);
            return BitConverter.ToUInt64(array, 0);
        }

        protected override void CreateDirectoryImpl(UPath path)
        {
            throw new IOException(FileSystemIsReadOnly);
        }

        protected override bool DirectoryExistsImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void MoveDirectoryImpl(UPath srcPath, UPath destPath)
        {
            throw new IOException(FileSystemIsReadOnly);
        }

        protected override void DeleteDirectoryImpl(UPath path, bool isRecursive)
        {
            throw new IOException(FileSystemIsReadOnly);
        }

        protected override void CopyFileImpl(UPath srcPath, UPath destPath, bool overwrite)
        {
            throw new IOException(FileSystemIsReadOnly);
        }

        protected override void ReplaceFileImpl(UPath srcPath, UPath destPath, UPath destBackupPath, bool ignoreMetadataErrors)
        {
            throw new IOException(FileSystemIsReadOnly);
        }

        protected override long GetFileLengthImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override bool FileExistsImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void MoveFileImpl(UPath srcPath, UPath destPath)
        {
            throw new IOException(FileSystemIsReadOnly);
        }

        protected override void DeleteFileImpl(UPath path)
        {
            throw new IOException(FileSystemIsReadOnly);
        }

        protected override Stream OpenFileImpl(UPath path, FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        protected override FileAttributes GetAttributesImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetAttributesImpl(UPath path, FileAttributes attributes)
        {
            throw new IOException(FileSystemIsReadOnly);
        }

        protected override DateTime GetCreationTimeImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetCreationTimeImpl(UPath path, DateTime time)
        {
            throw new IOException(FileSystemIsReadOnly);
        }

        protected override DateTime GetLastAccessTimeImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetLastAccessTimeImpl(UPath path, DateTime time)
        {
            throw new IOException(FileSystemIsReadOnly);
        }

        protected override DateTime GetLastWriteTimeImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override void SetLastWriteTimeImpl(UPath path, DateTime time)
        {
            throw new IOException(FileSystemIsReadOnly);
        }

        protected override IEnumerable<UPath> EnumeratePathsImpl(UPath path, string searchPattern, SearchOption searchOption, SearchTarget searchTarget)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<FileSystemItem> EnumerateItemsImpl(UPath path, SearchOption searchOption, SearchPredicate searchPredicate)
        {
            throw new NotImplementedException();
        }

        protected override IFileSystemWatcher WatchImpl(UPath path)
        {
            throw new NotImplementedException();
        }

        protected override string ConvertPathToInternalImpl(UPath path)
        {
            return path.ToAbsolute().ToString();
        }

        protected override UPath ConvertPathFromInternalImpl(string innerPath)
        {
            return new UPath(innerPath);
        }

        private class FileEntry
        {
            public string Name;
            public string NameDigest;
            public uint IndexListSize;
            public ulong Length;
            public ulong Offset;

            public FileEntry(string nameDigest, uint indexListSize, ulong length, ulong offset)
            {
                Name = "";
                NameDigest = nameDigest;
                IndexListSize = indexListSize;
                Length = length;
                Offset = offset;
            }

            public void SetName(string name)
            {
                Name = name;
            }
        }
    }
}