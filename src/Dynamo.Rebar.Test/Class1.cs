using System.IO;
using System.Reflection;
using NUnit.Framework;
using RevitTestServices;
using RTF.Framework;
using Autodesk.Revit.DB;
using RevitServices.Persistence;



namespace EnergyAnalysisForDynamoTests
{
    [TestFixture]
    public class SystemTestExample : RevitSystemTestBase
    {
        [SetUp]
        public void Setup()
        {
            // Set the working directory. This will allow you to use the OpenAndRunDynamoDefinition method,
            // specifying a relative path to the .dyn file you want to test.

            var asmDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            workingDirectory = Path.GetFullPath(Path.Combine(asmDirectory,
                @"..\..\..\packages\EnergyAnalysisForDynamo\extra"));
        }

        /// <summary>
        /// Test for Example file 1a.  Set the Revit Project's energy settings, and check to make sure the settings were applied.
        /// </summary>
        [Test, TestModel(@".\EnergyAnalysisForDynamo_ex1_simpleRevitMass.rvt")]
        public void SetProjectEnergySettings()
        {
            //open and run the example file
            OpenAndRunDynamoDefinition(@".\EnergyAnalysisForDynamo_ex1a_SetProjectEnergySettings.dyn");


            //check to see that the value[s] that we set with Dynamo actually took in Revit.

            //glazing percentage
            var myTargetGlazingPercentage = GetPreviewValue("83f5eb3b-234f-4081-8461-bd1af9ae6708");
            var es = Autodesk.Revit.DB.Analysis.EnergyDataSettings.GetFromDocument(DocumentManager.Instance.CurrentUIDocument.Document);
            if ((double)myTargetGlazingPercentage != es.PercentageGlazing)
            {
                Assert.Fail();
            }


            //if we got here, nothing failed.
            Assert.Pass();

        }

        /// <summary>
        /// 
        /// </summary>
        [Test, TestModel(@".\EnergyAnalysisForDynamo_ex1_simpleRevitMass.rvt")]
        public void SetSurfaceParameters()
        {
            //open and run the example file
            OpenAndRunDynamoDefinition(@".\EnergyAnalysisForDynamo_ex1a_SetProjectEnergySettings.dyn");

            //get the ID of the surface we are trying to set

            //get the target glazing percentage

            //get the actual glazing percentage from the revit doc

            //do the target and the actual match?
        }
    }
}
