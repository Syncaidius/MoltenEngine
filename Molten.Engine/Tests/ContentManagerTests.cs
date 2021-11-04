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
        object _result;

        [TestInitialize]
        public void TestInit()
        {
            _engine = new Engine(null, false);
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

            Assert.AreNotEqual(TEST_STRING, _result);
        }

        const string TEST_STRING = "This is a test";

        [TestMethod]
        public void SaveLoad()
        {
            ContentRequest cr = _engine.Content.BeginRequest("tests");
            cr.Save<string>("binary_object.txt", TEST_STRING);
            cr.Load<string>("binary_object.txt");
            cr.OnCompleted += WriteRead_OnCompleted;
            cr.Commit();

            while (!_done)
                Thread.Sleep(5);

            Assert.AreEqual(TEST_STRING, _result);
        }

        [TestMethod]
        public void LoadDuplicate()
        {
            ContentRequest cr = _engine.Content.BeginRequest("tests");
            cr.Save<string>("dup_object.txt", TEST_STRING);
            cr.Load<string>("dup_object.txt");
            cr.Commit();

            cr = _engine.Content.BeginRequest("tests");
            cr.Load<string>("dup_object.txt");
            cr.OnCompleted += LoadDuplicate_OnCompleted;
            cr.Commit();

            while (!_done)
                Thread.Sleep(5);

            Assert.AreEqual(TEST_STRING, _result);
        }

        private void LoadDuplicate_OnCompleted(ContentRequest request)
        {
            _result = request.Get<string>("dup_object.txt");
            _done = true;
        }

        private void WriteRead_OnCompleted(ContentRequest request)
        {
            _result = request.Get<string>("binary_object.txt");
            _done = true;
        }

        private void SerializeDeserialize_OnCompleted(ContentRequest request)
        {
            _result = request.Get<TestObject>("test_object.txt");
            _done = true;
        }
    }
}
