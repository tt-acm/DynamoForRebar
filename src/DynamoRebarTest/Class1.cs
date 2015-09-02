using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RevitTestServices;
using RTF.Framework;

namespace DynamoRebarTest
{
    //[SetUp]
    //public void Setup()
    //{
        // Set the working directory. This will allow you to use the OpenAndRunDynamoDefinition method,
        // specifying a relative path to the .dyn file you want to test.

     //   var asmDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    //    workingDirectory = Path.GetFullPath(Path.Combine(asmDirectory, @"..\..\..\exampleFiles"));
   // }  

    [TestFixture]
    public class SystemTestExample : RevitSystemTestBase
    {
        [Test, TestModel(@"..\..\..\..\TextFiles\RevitProject\RevitProject.rvt")]
        public void Location()
        {
            OpenAndRunDynamoDefinition(@"..\..\..\..\TextFiles\DynamoDefinitions\Tag.dyn");

            // Your test logic goes here.
        }
    }
}
