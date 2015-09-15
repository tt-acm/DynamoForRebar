using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RevitTestServices;
using RTF.Framework;

namespace DynamoRebarTest
{

        [TestFixture]
        public class DynamoForRebar_SystemTesting : RevitSystemTestBase
        {
            [SetUp]
            public void Setup()
            {
                // Set the working directory. This will allow you to use the OpenAndRunDynamoDefinition method,
                // specifying a relative path to the .dyn file you want to test.

                var asmDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                workingDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(asmDirectory,@"..\..\..\TestFiles"));
                
            }


            [Test, TestModel(@"RevitProject.rvt")]
            public void Tag()
            {
                //open and run the example file
                OpenAndRunDynamoDefinition(workingDirectory + @"DynamoDefinitions\Tag.dyn");

                //check for errors and assert accordingly
                string errString = CompileErrorsIntoString();

                if (string.IsNullOrEmpty(errString))
                    Assert.Pass();              
                else
                    Assert.Fail(errString);

            }

            [Test, TestModel(@"RevitProject.rvt")]
            public void Morphed()
            {
                //open and run the example file
                OpenAndRunDynamoDefinition(@"DynamoDefinitions\Morphed.dyn");

                //check for errors and assert accordingly
                string errString = CompileErrorsIntoString();

                if (string.IsNullOrEmpty(errString))
                    Assert.Pass();
                else
                    Assert.Fail(errString);

            }

            [Test, TestModel(@"RevitProject.rvt")]
            public void CreateContainer()
            {
                //open and run the example file
                OpenAndRunDynamoDefinition(@"DynamoDefinitions\CreateContainer.dyn");

                //check for errors and assert accordingly
                string errString = CompileErrorsIntoString();

                if (string.IsNullOrEmpty(errString))
                    Assert.Pass();
                else
                    Assert.Fail(errString);

            }

            [Test, TestModel(@"RevitProject.rvt")]
            public void Cut()
            {
                //open and run the example file
                OpenAndRunDynamoDefinition(@"DynamoDefinitions\Cut.dyn");

                //check for errors and assert accordingly
                string errString = CompileErrorsIntoString();

                if (string.IsNullOrEmpty(errString))
                    Assert.Pass();
                else
                    Assert.Fail(errString);

            }

            [Test, TestModel(@"RevitProject.rvt")]
            public void FollowingSurface()
            {
                //open and run the example file
                OpenAndRunDynamoDefinition(@"DynamoDefinitions\FollowingSurface.dyn");

                //check for errors and assert accordingly
                string errString = CompileErrorsIntoString();

                if (string.IsNullOrEmpty(errString))
                    Assert.Pass();
                else
                    Assert.Fail(errString);

            }

            [Test, TestModel(@"RevitProject.rvt")]
            public void Perpendicular()
            {
                //open and run the example file
                OpenAndRunDynamoDefinition(@"DynamoDefinitions\Perpendicular.dyn");

                //check for errors and assert accordingly
                string errString = CompileErrorsIntoString();

                if (string.IsNullOrEmpty(errString))
                    Assert.Pass();
                else
                    Assert.Fail(errString);

            }

            [Test, TestModel(@"RevitProject.rvt")]
            public void Shorten()
            {
                //open and run the example file
                OpenAndRunDynamoDefinition(@"DynamoDefinitions\Shorten.dyn");

                //check for errors and assert accordingly
                string errString = CompileErrorsIntoString();

                if (string.IsNullOrEmpty(errString))
                    Assert.Pass();
                else
                    Assert.Fail(errString);

            }



            /// <summary>
            /// A utility function to loop over a sample file and list any nodes in error or warning state.
            /// </summary>
            /// <returns></returns>
            private string CompileErrorsIntoString()
            {
                //a string to return
                string errors = null;

                //loop over the active collection of nodes.
                foreach (var i in AllNodes)
                {
                    if (IsNodeInErrorOrWarningState(i.GUID.ToString()))
                    {
                        errors += "The node called '" + i.NickName + "' failed or threw a warning." + System.Environment.NewLine;
                    }
                }

                //return the errors string
                return errors;
            }

        }
    

}
