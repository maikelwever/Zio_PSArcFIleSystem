using System.IO;
using NUnit.Framework;
using Zio.FileSystems;

namespace Zio.PSArcFileSystem.Test
{
    public class Tests
    {
        private IFileSystem RootFS;
        private UPath ArcPath;
        
        [SetUp]
        public void Setup()
        {
            RootFS = new PhysicalFileSystem();
            ArcPath = RootFS.ConvertPathFromInternal(Path.GetFullPath("../../../../testarc.psarc"));
        }

        [Test]
        public void Test1()
        {
            var psarcFS = new PSArcFileSystem(RootFS, ArcPath);
            Assert.NotNull(psarcFS);
        }
    }
}