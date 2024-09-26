using System.Collections;
using CavrnusCore;
using CavrnusSdk.API;
using CavrnusSdk.Setup;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Cavrnus.Tests
{
    public class CavrnusSetupHelpersTests
    {
        [TearDown]
        public void Teardown()
        {
            TestHelpers.SceneTeardown();
        }
        
        /*[UnityTest]
        public IEnumerator SetupSceneForCavrnus__SetupNewScene()
        {
            CavrnusSpaceConnection spaceConn = null;
            yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

            var csc = GameObject.Find("Cavrnus Spatial Connector");
            Assert.IsNotNull(csc?.GetComponent<CavrnusSpatialConnector>());
            Assert.AreEqual(EditorPrefs.GetString("CavrnusServer"), csc.GetComponent<CavrnusSpatialConnector>().MyServer);
            Assert.AreEqual("SampleSpaceJoinConfig", csc.GetComponent<CavrnusSpatialConnector>().AutomaticSpaceJoinId);

            var mainCamera = GameObject.Find("Main Camera");
            Assert.NotNull(mainCamera.GetComponent<CavrnusLocalUserFlag>());
        }*/
    }
}