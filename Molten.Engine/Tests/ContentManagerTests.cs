using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Tests
{
    [TestClass]
    public class ContentManagerTests
    {
        [DataContract]
        [DeploymentItem("../../../Molten.Input.Windows/bin/x64/debug/Molten.Input.Windows.dll", "Molten.Input.Windows.dll")]
        class TestObject
        {
            [DataMember]
            public int TestProperty1 { get; set; } = 5;

            [DataMember]
            public string TestProperty2 { get; set; } = "testing";
        }

        Engine _engine;
        [TestInitialize]
        public void TestInit()
        {
            bool fileExists = File.Exists("../../../Molten.Input.Windows/bin/x64/debug/Molten.Input.Windows.dll");
            string test = Path.GetFullPath("../../../Molten.Input.Windows/bin/x64/debug/Molten.Input.Windows.dll");
            _engine = new Engine();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _engine.Dispose();
        }

        [TestMethod]
        public void Serialize()
        {
            ContentRequest cr = _engine.Content.BeginRequest("tests");
            cr.Serialize("test_object.txt", new TestObject());
            cr.Commit();
        }
    }
}
