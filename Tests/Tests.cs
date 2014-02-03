using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using NUnit.Framework;
using NUnit;

using SysconCommon;
using SysconCommon.Common.Environment;
using SysconCommon.Algebras.Graphs;
using SysconCommon.Algebras.DataTables;

namespace JobDetailAnalysis
{
    [TestFixture]
    public class Tests
    {
        [TestFixtureSetUp]
        public void Setup()
        {
           Env.SetConfigFile("../../../JobDetailAnalysis/config.xml", false); // so that we can update the config and re-run without recompilation or copying
        }

        [Test]
        public void TestDbConnection()
        {
            var con = Connections.Connection;
            Assert.IsTrue(con.State == System.Data.ConnectionState.Open);
            Console.WriteLine("Odbc connection opened without problems");
        }

        [Test]
        public void EnsureAllDependanciesExist()
        {
            var grph = Env.SQLGraph;
            foreach (var n in grph.Nodes)
            {
                var neighbors = grph.NeighborFinder(n);
                foreach (var ne in neighbors)
                {
                    Assert.IsTrue(grph.Nodes.Contains(ne), string.Format("Node {0} is referenced but does not exist", ne));
                }
            }

            Console.WriteLine("Good.  There are no missing nodes");
        }

        class JsonTestClass
        {
            public class Inner
            {
                public string a = "a";
                public string b = "b";
            }

            public enum EnumTest
            {
                EnumOne,
                EnumTwo
            }

            public Inner innerTest = new Inner();
            public decimal decimalTest = 42.0m;
            public double doubleTest = 43.0;
            public int intTest = 44;
            public string stringTest = "This is a string";
            public int[] arrayTest = new int[] { 1, 2 };
            public EnumTest enumTest = EnumTest.EnumOne;

            private string privateTest = "This should not show up in JSON";

            private string avoidWarning() {
                return privateTest;
            }
        }
    }
}