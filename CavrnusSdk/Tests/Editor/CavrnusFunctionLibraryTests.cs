using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using CavrnusSdk.API;
using CavrnusSdk.Setup;
using System;
using CavrnusCore;
using CavrnusSdk.PropertySynchronizers.CommonImplementations;
using UnityEngine.TestTools.Utils;

namespace Cavrnus.Tests
{
	#region Test Authentication
	public class Tests_Authentication
	{
		[SetUp]
		public void Setup() { }
		[TearDown]
		public void Teardown(){	TestHelpers.SceneTeardown(); }		

		[UnityTest]
		public IEnumerator CheckAuthentication_Guest()
		{
			TestHelpers.GenericGuestSceneSetup(TestHelpers.TestServer);

			Assert.That(CavrnusFunctionLibrary.IsLoggedIn, Is.False);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			Assert.That(CavrnusFunctionLibrary.IsLoggedIn, Is.True);
		}

		[UnityTest]
		public IEnumerator CheckAuthentication_Member()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			Assert.That(CavrnusFunctionLibrary.IsLoggedIn, Is.False);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			Assert.That(CavrnusFunctionLibrary.IsLoggedIn, Is.True);
		}

		[UnityTest]
		public IEnumerator CheckAuthentication_BadMemberEmail()
		{
			var csc = TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);
			csc.MemberEmail += "_mcFiddlesworth";

			Assert.That(CavrnusFunctionLibrary.IsLoggedIn, Is.False);

			LogAssert.Expect(LogType.Error, "401: InvalidCredentials: Invalid email or password");

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			Assert.That(CavrnusFunctionLibrary.IsLoggedIn, Is.False);
		}

		[UnityTest]
		public IEnumerator CheckAuthentication_BadMemberPwrd()
		{
			var csc = TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);
			csc.MemberPassword = "duckSauce";

			Assert.That(CavrnusFunctionLibrary.IsLoggedIn, Is.False);

			LogAssert.Expect(LogType.Error, "401: InvalidCredentials: Invalid email or password");

			CavrnusSpaceConnection spaceConn = null;

			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			Assert.That(CavrnusFunctionLibrary.IsLoggedIn, Is.False);
		}
	}
	#endregion


	#region Test Property Posts
	public class Tests_PostPropertyValues
	{
		[SetUp]
		public void Setup() { }
		[TearDown]
		public void Teardown() { TestHelpers.SceneTeardown(); }

		[UnityTest]
		public IEnumerator String()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "postTest_string";

			spaceConn.DefineStringPropertyDefaultValue(containerName, propName, "default");

			Assert.AreEqual("default", spaceConn.GetStringPropertyValue(containerName, propName));

			spaceConn.PostStringPropertyUpdate(containerName, propName, "change");

			Assert.AreEqual("change", spaceConn.GetStringPropertyValue(containerName, propName));
		}

		[UnityTest]
		public IEnumerator Bool()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "postTest_bool";

			spaceConn.DefineBoolPropertyDefaultValue(containerName, propName, true);

			Assert.AreEqual(true, spaceConn.GetBoolPropertyValue(containerName, propName));

			spaceConn.PostBoolPropertyUpdate(containerName, propName, false);

			Assert.AreEqual(false, spaceConn.GetBoolPropertyValue(containerName, propName));
		}

		[UnityTest]
		public IEnumerator Color()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "postTest_color";

			spaceConn.DefineColorPropertyDefaultValue(containerName, propName, new Color(0, .5f, 0));

			Assert.AreEqual(new Color(0, .5f, 0), spaceConn.GetColorPropertyValue(containerName, propName));

			spaceConn.PostColorPropertyUpdate(containerName, propName, new Color(.3f, .6f, 0));

			//Unity color comparison doesn't like these values, so need to use their helper
			Assert.True(ColorEqualityComparer.Instance.Equals(new Color(.3f, .6f, 0), spaceConn.GetColorPropertyValue(containerName, propName)));
		}

		[UnityTest]
		public IEnumerator Vector()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "postTest_vector";

			spaceConn.DefineVectorPropertyDefaultValue(containerName, propName, new Vector3(0, .5f, 0));

			Assert.AreEqual(new Vector3(0, .5f, 0), (Vector3)spaceConn.GetVectorPropertyValue(containerName, propName));

			spaceConn.PostVectorPropertyUpdate(containerName, propName, new Vector3(.3f, .6f, 0));

			Assert.AreEqual(new Vector3(.3f, .6f, 0), (Vector3)spaceConn.GetVectorPropertyValue(containerName, propName));
		}

		[UnityTest]
		public IEnumerator Transform()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "postTest_transform";

			spaceConn.DefineTransformPropertyDefaultValue(containerName, propName, new CavrnusTransformData(Vector3.up, Vector3.zero, Vector3.one));

			Assert.AreEqual(new CavrnusTransformData(Vector3.up, Vector3.zero, Vector3.one), spaceConn.GetTransformPropertyValue(containerName, propName));

			spaceConn.PostTransformPropertyUpdate(containerName, propName, new CavrnusTransformData(Vector3.down, Vector3.zero, Vector3.zero), new PropertyPostOptions() { smoothed = false });

			Assert.AreEqual(new CavrnusTransformData(Vector3.down, Vector3.zero, Vector3.zero), spaceConn.GetTransformPropertyValue(containerName, propName));
		}

		[UnityTest]
		public IEnumerator Float()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "postTest_float";

			spaceConn.DefineFloatPropertyDefaultValue(containerName, propName, 6.2f);

			Assert.AreEqual(6.2f, spaceConn.GetFloatPropertyValue(containerName, propName));

			spaceConn.PostFloatPropertyUpdate(containerName, propName, 7.3f);

			Assert.AreEqual(7.3f, spaceConn.GetFloatPropertyValue(containerName, propName));
		}
	}
	#endregion

	#region Test Transient Property Posts
	public class Tests_PostTransientPropertyValues
	{
		[SetUp]
		public void Setup() { }
		[TearDown]
		public void Teardown() { TestHelpers.SceneTeardown(); }

		[UnityTest]
		public IEnumerator String()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "transTest_string";

			spaceConn.DefineStringPropertyDefaultValue(containerName, propName, "default");

			Assert.AreEqual("default", spaceConn.GetStringPropertyValue(containerName, propName));

			var liveTransient = spaceConn.BeginTransientStringPropertyUpdate(containerName, propName, "change");

			Assert.AreEqual("change", spaceConn.GetStringPropertyValue(containerName, propName));

			liveTransient.UpdateWithNewData("updated");

			Assert.AreEqual("updated", spaceConn.GetStringPropertyValue(containerName, propName));

			liveTransient.Cancel();

			Assert.AreEqual("default", spaceConn.GetStringPropertyValue(containerName, propName));
		}

		[UnityTest]
		public IEnumerator Bool()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "transTest_bool";

			spaceConn.DefineBoolPropertyDefaultValue(containerName, propName, true);

			Assert.AreEqual(true, spaceConn.GetBoolPropertyValue(containerName, propName));

			var liveTransient = spaceConn.BeginTransientBoolPropertyUpdate(containerName, propName, false);

			Assert.AreEqual(false, spaceConn.GetBoolPropertyValue(containerName, propName));

			liveTransient.UpdateWithNewData(true);

			Assert.AreEqual(true, spaceConn.GetBoolPropertyValue(containerName, propName));

			liveTransient.Cancel();

			Assert.AreEqual(true, spaceConn.GetBoolPropertyValue(containerName, propName));
		}

		[UnityTest]
		public IEnumerator Float()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "transTest_float";

			spaceConn.DefineFloatPropertyDefaultValue(containerName, propName, 1f);

			Assert.AreEqual(1f, spaceConn.GetFloatPropertyValue(containerName, propName));

			var liveTransient = spaceConn.BeginTransientFloatPropertyUpdate(containerName, propName, 2f);

			Assert.AreEqual(2f, spaceConn.GetFloatPropertyValue(containerName, propName));

			liveTransient.UpdateWithNewData(3f);

			Assert.AreEqual(3f, spaceConn.GetFloatPropertyValue(containerName, propName));

			liveTransient.Cancel();

			Assert.AreEqual(1f, spaceConn.GetFloatPropertyValue(containerName, propName));
		}

		[UnityTest]
		public IEnumerator Vector()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "transTest_vector";

			spaceConn.DefineVectorPropertyDefaultValue(containerName, propName, Vector3.one);

			Assert.AreEqual(Vector3.one, (Vector3)spaceConn.GetVectorPropertyValue(containerName, propName));

			var liveTransient = spaceConn.BeginTransientVectorPropertyUpdate(containerName, propName, Vector3.zero);

			Assert.AreEqual(Vector3.zero, (Vector3)spaceConn.GetVectorPropertyValue(containerName, propName));

			liveTransient.UpdateWithNewData(Vector3.up);

			Assert.AreEqual(Vector3.up, (Vector3)spaceConn.GetVectorPropertyValue(containerName, propName));

			liveTransient.Cancel();

			Assert.AreEqual(Vector3.one, (Vector3)spaceConn.GetVectorPropertyValue(containerName, propName));
		}

		[UnityTest]
		public IEnumerator Color()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "transTest_color";

			spaceConn.DefineColorPropertyDefaultValue(containerName, propName, new Color(1, 1, 0));

			Assert.AreEqual(new Color(1, 1, 0), spaceConn.GetColorPropertyValue(containerName, propName));

			var liveTransient = spaceConn.BeginTransientColorPropertyUpdate(containerName, propName, new Color(1, 1, 1));

			Assert.AreEqual(new Color(1, 1, 1), spaceConn.GetColorPropertyValue(containerName, propName));

			liveTransient.UpdateWithNewData(new Color(0, 1, 1));

			Assert.AreEqual(new Color(0, 1, 1), spaceConn.GetColorPropertyValue(containerName, propName));

			liveTransient.Cancel();

			Assert.AreEqual(new Color(1, 1, 0), spaceConn.GetColorPropertyValue(containerName, propName));
		}

		[UnityTest]
		public IEnumerator Transform()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "transTest_transform";

			//TODO: should transient posts support no-smoothing?

			spaceConn.DefineTransformPropertyDefaultValue(containerName, propName, new CavrnusTransformData(Vector3.up, Vector3.zero, Vector3.one));

			Assert.AreEqual(new CavrnusTransformData(Vector3.up, Vector3.zero, Vector3.one), spaceConn.GetTransformPropertyValue(containerName, propName));

			var liveTransient = spaceConn.BeginTransientTransformPropertyUpdate(containerName, propName, new CavrnusTransformData(Vector3.zero, Vector3.zero, Vector3.one), new PropertyPostOptions() { smoothed = false });

			Assert.AreEqual(new CavrnusTransformData(Vector3.zero, Vector3.zero, Vector3.one), spaceConn.GetTransformPropertyValue(containerName, propName));

			liveTransient.UpdateWithNewData(new CavrnusTransformData(Vector3.down, Vector3.zero, Vector3.one));

			Assert.AreEqual(new CavrnusTransformData(Vector3.down, Vector3.zero, Vector3.one), spaceConn.GetTransformPropertyValue(containerName, propName));

			liveTransient.Cancel();

			Assert.AreEqual(new CavrnusTransformData(Vector3.up, Vector3.zero, Vector3.one), spaceConn.GetTransformPropertyValue(containerName, propName));
		}
	}
	#endregion

	#region Test Property Bindings
	public class Tests_BindPropertyValues
	{
		[SetUp]
		public void Setup() { }
		[TearDown]
		public void Teardown() { TestHelpers.SceneTeardown(); }

		[UnityTest]
		public IEnumerator String()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "bindTest_string";

			spaceConn.DefineStringPropertyDefaultValue(containerName, propName, "default");

			string boundVal = "";
			spaceConn.BindStringPropertyValue(containerName, propName, s => boundVal = s);

			Assert.AreEqual("default", boundVal);

			spaceConn.PostStringPropertyUpdate(containerName, propName, "change");

			Assert.AreEqual("change", boundVal);
		}

		[UnityTest]
		public IEnumerator Bool()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "bindTest_bool";

			spaceConn.DefineBoolPropertyDefaultValue(containerName, propName, true);

			bool boundVal = false;
			spaceConn.BindBoolPropertyValue(containerName, propName, s => boundVal = s);
			Assert.AreEqual(true, boundVal);

			spaceConn.PostBoolPropertyUpdate(containerName, propName, false);

			Assert.AreEqual(false, boundVal);
		}

		[UnityTest]
		public IEnumerator Color()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "bindTest_color";

			spaceConn.DefineColorPropertyDefaultValue(containerName, propName, new Color(0, .5f, 0));

			Color boundVal = new UnityEngine.Color();
			spaceConn.BindColorPropertyValue(containerName, propName, s => boundVal = s);
			Assert.AreEqual(new Color(0, .5f, 0), boundVal);

			spaceConn.PostColorPropertyUpdate(containerName, propName, new Color(.3f, .6f, 0));

			//Unity color comparison doesn't like these values, so need to use their helper
			Assert.True(ColorEqualityComparer.Instance.Equals(new Color(.3f, .6f, 0), boundVal));
		}

		[UnityTest]
		public IEnumerator Vector()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "bindTest_vector";

			spaceConn.DefineVectorPropertyDefaultValue(containerName, propName, new Vector3(0, .5f, 0));

			Vector3 boundVal = Vector3.zero;
			spaceConn.BindVectorPropertyValue(containerName, propName, s => boundVal = s);
			Assert.AreEqual(new Vector3(0, .5f, 0), boundVal);

			spaceConn.PostVectorPropertyUpdate(containerName, propName, new Vector3(.3f, .6f, 0));

			Assert.AreEqual(new Vector3(.3f, .6f, 0), boundVal);
		}

		[UnityTest]
		public IEnumerator Transform()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "bindTest_transform";

			spaceConn.DefineTransformPropertyDefaultValue(containerName, propName, new CavrnusTransformData(Vector3.up, Vector3.zero, Vector3.one));

			CavrnusTransformData boundVal = null;
			spaceConn.BindTransformPropertyValue(containerName, propName, s => boundVal = s);
			Assert.AreEqual(new CavrnusTransformData(Vector3.up, Vector3.zero, Vector3.one), boundVal);

			spaceConn.PostTransformPropertyUpdate(containerName, propName, new CavrnusTransformData(Vector3.down, Vector3.zero, Vector3.zero), new PropertyPostOptions() { smoothed = false });

			Assert.AreEqual(new CavrnusTransformData(Vector3.down, Vector3.zero, Vector3.zero), boundVal);
		}

		[UnityTest]
		public IEnumerator Float()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			string containerName = "test_" + DateTime.UtcNow.ToString();
			string propName = "bindTest_float";

			spaceConn.DefineFloatPropertyDefaultValue(containerName, propName, 6.2f);

			float boundVal = 0;
			spaceConn.BindFloatPropertyValue(containerName, propName, s => boundVal = s);
			Assert.AreEqual(6.2f, boundVal);

			spaceConn.PostFloatPropertyUpdate(containerName, propName, 7.3f);

			Assert.AreEqual(7.3f, boundVal);
		}
	}
	#endregion

	#region Test No-Code Components
	public class Tests_NoCodeComponents
	{
		[SetUp]
		public void Setup() { }
		[TearDown]
		public void Teardown() { TestHelpers.SceneTeardown(); }

		[UnityTest]
		public IEnumerator CheckNoCodeUserComponent_Transform()
		{
			TestHelpers.GenericMemberSceneSetup(TestHelpers.TestServer);

			CavrnusSpaceConnection spaceConn = null;
			yield return TestHelpers.AwaitSpaceConn(sc => spaceConn = sc);

			CavrnusUser localUser = null;
			spaceConn.AwaitLocalUser(lu => localUser = lu);

			yield return new WaitForSeconds(.1f);

			Assert.NotNull(localUser);

			var localUserOb = GameObject.Find("Main Camera");

			Assert.IsNotNull(localUserOb);
			Assert.IsNotNull(localUserOb.GetComponent<CavrnusLocalUserFlag>());
			Assert.IsNotNull(localUserOb.GetComponent<SyncLocalTransform>());

			Assert.AreEqual(Vector3.zero,
							spaceConn.GetTransformPropertyValue(localUser.ContainerId, "Transform").Position);

			localUserOb.transform.position = Vector3.one;

			yield return new WaitForSeconds(1f);

			Assert.AreEqual(Vector3.one, spaceConn.GetTransformPropertyValue(localUser.ContainerId, "Transform").Position);
		}
	}
	#endregion

	/*
	 
	 #region Test STUFF
	[TestFixture]
	public class Tests_STUFF
	{
		[SetUp]
		public void Setup() { }
		[TearDown]
		public void Teardown() { TestHelpers.SceneTeardown(); }
	}
	#endregion
	 
	 
	 */
}