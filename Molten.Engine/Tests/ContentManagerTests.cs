using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten.Tests
{
    [TestClass]
    public class ContentManagerTests
    {
        [DataContract]
        class TestObject
        {
            [DataMember]
            public int TestProperty1 { get; set; } = 5;

            [DataMember]
            public string TestProperty2 { get; set; } = "testing";
        }

        Engine _engine;
        bool _done;

        [TestInitialize]
        public void TestInit()
        {
            _engine = new Engine();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _engine.Dispose();
        }

        [TestMethod]
        public void SerializeDeserialize()
        {
            ContentRequest cr = _engine.Content.BeginRequest("tests");
            cr.Serialize("test_object.txt", new TestObject());
            cr.Deserialize<TestObject>("test_object.txt");
            cr.OnCompleted += SerializeDeserialize_OnCompleted;
            cr.Commit();

            while (!_done)
                Thread.Sleep(5);
        }

        const string TEST_STRING = "This is a test";

        [TestMethod]
        public void WriteRead()
        {
            ContentRequest cr = _engine.Content.BeginRequest("tests");
            cr.Save<string>("binary_object.txt", TEST_STRING);
            cr.Load<string>("binary_object.txt");
            cr.OnCompleted += WriteRead_OnCompleted;
            cr.Commit();

            while (!_done)
                Thread.Sleep(5);
        }

        private void WriteRead_OnCompleted(ContentRequest request)
        {
            string testString = request.Get<string>("binary_object.txt");
            _done = true;
            Assert.AreNotEqual(TEST_STRING, testString);
        }

        private void SerializeDeserialize_OnCompleted(ContentRequest request)
        {
            TestObject result = request.Get<TestObject>("test_object.txt");
            _done = true;
            Assert.AreNotEqual(null, result);
        }
    }
}
