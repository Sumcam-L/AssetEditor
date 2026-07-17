using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.ATF;

internal static class Program
{
	private static int s_failures;

	private static int Main(string[] args)
	{
		if (args.Length == 8 && args[0] == "stage")
		{
			HotLoadFileStager hotLoadFileStager = new HotLoadFileStager(args[1], args[2], args[3], args[4], args[5]);
			hotLoadFileStager.Stage(new[] { args[6] }, new[] { args[7] }, (source, destination) => Console.WriteLine(source + " -> " + destination));
			return 0;
		}
		Run("copies ArtDef and BLP", TestCopiesArtDefAndBlp);
		Run("creates nested directories", TestCreatesNestedDirectories);
		Run("rejects mismatched GUID", TestRejectsMismatchedGuid);
		Run("rejects missing deployed Mod", TestRejectsMissingDeployedMod);
		Run("rejects rooted paths", TestRejectsRootedPath);
		Run("rejects parent traversal", TestRejectsParentTraversal);
		Run("rejects missing cooked source", TestRejectsMissingSource);
		Run("replaces existing destination", TestReplacesExistingDestination);
		Run("reports each staged file", TestReportsEachStagedFile);
		Run("staging failure suppresses send", TestStagingFailureSuppressesSend);
		Run("successful staging sends once", TestSuccessfulStagingSendsOnce);
		Run("send failures are not staging failures", TestSendFailureIsNotStagingFailure);
		Run("rejects missing project Guid", TestRejectsMissingProjectGuid);
		Run("rejects missing deployed Mod id", TestRejectsMissingModId);
		return s_failures == 0 ? 0 : 1;
	}

	private static void Run(string name, Action test)
	{
		try
		{
			test();
			Console.WriteLine("PASS: " + name);
		}
		catch (Exception ex)
		{
			s_failures++;
			Console.Error.WriteLine("FAIL: " + name + " - " + ex.Message);
		}
	}

	private static void TestCopiesArtDefAndBlp()
	{
		using (TestEnvironment environment = new TestEnvironment())
		{
			environment.WriteCookedArtDef("Units.artdef", "new artdef");
			environment.WriteCookedBlp("units/units.blp", "new blp");
			environment.CreateStager().Stage(new[] { "Units.artdef" }, new[] { "units/units.blp" });

			AssertEqual("new artdef", File.ReadAllText(environment.DeployedArtDef("Units.artdef")));
			AssertEqual("new blp", File.ReadAllText(environment.DeployedBlp("units/units.blp")));
		}
	}

	private static void TestCreatesNestedDirectories()
	{
		using (TestEnvironment environment = new TestEnvironment())
		{
			environment.WriteCookedArtDef("nested/Units.artdef", "nested");
			environment.CreateStager().Stage(new[] { "nested/Units.artdef" }, Array.Empty<string>());
			AssertTrue(File.Exists(environment.DeployedArtDef("nested/Units.artdef")), "nested ArtDef was not deployed");
		}
	}

	private static void TestRejectsMismatchedGuid()
	{
		using (TestEnvironment environment = new TestEnvironment(targetGuid: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"))
		{
			environment.WriteCookedArtDef("Units.artdef", "content");
			AssertThrows<InvalidOperationException>(() => environment.CreateStager().Stage(new[] { "Units.artdef" }, Array.Empty<string>()));
		}
	}

	private static void TestRejectsMissingDeployedMod()
	{
		using (TestEnvironment environment = new TestEnvironment(createTargetIdentity: false))
		{
			environment.WriteCookedArtDef("Units.artdef", "content");
			AssertThrows<FileNotFoundException>(() => environment.CreateStager().Stage(new[] { "Units.artdef" }, Array.Empty<string>()));
		}
	}

	private static void TestRejectsRootedPath()
	{
		using (TestEnvironment environment = new TestEnvironment())
		{
			AssertThrows<InvalidOperationException>(() => environment.CreateStager().Stage(new[] { Path.Combine(environment.Root, "Units.artdef") }, Array.Empty<string>()));
		}
	}

	private static void TestRejectsParentTraversal()
	{
		using (TestEnvironment environment = new TestEnvironment())
		{
			AssertThrows<InvalidOperationException>(() => environment.CreateStager().Stage(new[] { "../Units.artdef" }, Array.Empty<string>()));
		}
	}

	private static void TestRejectsMissingSource()
	{
		using (TestEnvironment environment = new TestEnvironment())
		{
			AssertThrows<FileNotFoundException>(() => environment.CreateStager().Stage(new[] { "Units.artdef" }, Array.Empty<string>()));
		}
	}

	private static void TestReplacesExistingDestination()
	{
		using (TestEnvironment environment = new TestEnvironment())
		{
			environment.WriteCookedBlp("units/units.blp", "new");
			environment.WriteDeployedBlp("units/units.blp", "old");
			environment.CreateStager().Stage(Array.Empty<string>(), new[] { "units/units.blp" });
			AssertEqual("new", File.ReadAllText(environment.DeployedBlp("units/units.blp")));
		}
	}

	private static void TestReportsEachStagedFile()
	{
		using (TestEnvironment environment = new TestEnvironment())
		{
			environment.WriteCookedArtDef("Units.artdef", "artdef");
			environment.WriteCookedBlp("units/units.blp", "blp");
			List<string> staged = new List<string>();
			environment.CreateStager().Stage(
				new[] { "Units.artdef" },
				new[] { "units/units.blp" },
				(source, destination) => staged.Add(source + " -> " + destination));
			AssertEqual(2, staged.Count);
		}
	}

	private static void TestStagingFailureSuppressesSend()
	{
		int sends = 0;
		int failures = 0;
		bool result = HotLoadStagingGate.StageThenSend(
			() => throw new IOException("copy failed"),
			() => sends++,
			ex => failures++);
		AssertTrue(!result, "failed staging reported success");
		AssertEqual(0, sends);
		AssertEqual(1, failures);
	}

	private static void TestSuccessfulStagingSendsOnce()
	{
		int sends = 0;
		int failures = 0;
		bool result = HotLoadStagingGate.StageThenSend(
			() => { },
			() => sends++,
			ex => failures++);
		AssertTrue(result, "successful staging reported failure");
		AssertEqual(1, sends);
		AssertEqual(0, failures);
	}

	private static void TestSendFailureIsNotStagingFailure()
	{
		int failures = 0;
		AssertThrows<IOException>(() => HotLoadStagingGate.StageThenSend(
			() => { },
			() => throw new IOException("send failed"),
			ex => failures++));
		AssertEqual(0, failures);
	}

	private static void TestRejectsMissingProjectGuid()
	{
		using (TestEnvironment environment = new TestEnvironment(sourceIdentityXml: "<Project />"))
		{
			environment.WriteCookedArtDef("Units.artdef", "content");
			AssertThrows<InvalidOperationException>(() => environment.CreateStager().Stage(new[] { "Units.artdef" }, Array.Empty<string>()));
		}
	}

	private static void TestRejectsMissingModId()
	{
		using (TestEnvironment environment = new TestEnvironment(targetIdentityXml: "<Mod />"))
		{
			environment.WriteCookedArtDef("Units.artdef", "content");
			AssertThrows<InvalidOperationException>(() => environment.CreateStager().Stage(new[] { "Units.artdef" }, Array.Empty<string>()));
		}
	}

	private static void AssertTrue(bool condition, string message)
	{
		if (!condition)
		{
			throw new InvalidOperationException(message);
		}
	}

	private static void AssertEqual<T>(T expected, T actual)
	{
		if (!EqualityComparer<T>.Default.Equals(expected, actual))
		{
			throw new InvalidOperationException("Expected '" + expected + "', got '" + actual + "'.");
		}
	}

	private static void AssertThrows<T>(Action action) where T : Exception
	{
		try
		{
			action();
		}
		catch (T)
		{
			return;
		}
		throw new InvalidOperationException("Expected " + typeof(T).Name + ".");
	}

	private sealed class TestEnvironment : IDisposable
	{
		private const string SourceGuid = "834e4a99-27c1-4315-a01b-7104056c0e38";
		private readonly string _personal;
		private readonly string _gamePantry;
		private readonly string _artDefOutput;
		private readonly string _blpOutput;
		private readonly string _deployedMod;

		public string Root { get; }

		public TestEnvironment(string targetGuid = SourceGuid, bool createTargetIdentity = true, string sourceIdentityXml = null, string targetIdentityXml = null)
		{
			Root = Path.Combine(Path.GetTempPath(), "hotload-stager-" + Guid.NewGuid().ToString("N"));
			_personal = Path.Combine(Root, "Personal");
			_gamePantry = Path.Combine(Root, "Workspace", "WuwaCore");
			_artDefOutput = Path.Combine(Root, "Workspace", "Cooked", "ArtDefs");
			_blpOutput = Path.Combine(Root, "Workspace", "Cooked", "BLPs");
			_deployedMod = Path.Combine(_personal, "My Games", "Sid Meier's Civilization VI", "Mods", "WuwaCore");
			Directory.CreateDirectory(_gamePantry);
			File.WriteAllText(Path.Combine(_gamePantry, "WuwaCore.civ6proj"), sourceIdentityXml ??
				"<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><PropertyGroup><Guid>" + SourceGuid + "</Guid></PropertyGroup></Project>");
			if (createTargetIdentity)
			{
				Directory.CreateDirectory(_deployedMod);
				File.WriteAllText(Path.Combine(_deployedMod, "WuwaCore.modinfo"), targetIdentityXml ?? "<Mod id=\"" + targetGuid + "\" />");
			}
		}

		public HotLoadFileStager CreateStager()
		{
			return new HotLoadFileStager("WuwaCore", _gamePantry, _artDefOutput, _blpOutput, _personal);
		}

		public void WriteCookedArtDef(string relativePath, string content)
		{
			Write(Path.Combine(_artDefOutput, Normalize(relativePath)), content);
		}

		public void WriteCookedBlp(string relativePath, string content)
		{
			Write(Path.Combine(_blpOutput, Normalize(relativePath)), content);
		}

		public void WriteDeployedBlp(string relativePath, string content)
		{
			Write(DeployedBlp(relativePath), content);
		}

		public string DeployedArtDef(string relativePath)
		{
			return Path.Combine(_deployedMod, "ArtDefs", Normalize(relativePath));
		}

		public string DeployedBlp(string relativePath)
		{
			return Path.Combine(_deployedMod, "Platforms", "Windows", "BLPs", Normalize(relativePath));
		}

		private static string Normalize(string path)
		{
			return path.Replace('/', Path.DirectorySeparatorChar);
		}

		private static void Write(string path, string content)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, content);
		}

		public void Dispose()
		{
			if (Directory.Exists(Root))
			{
				Directory.Delete(Root, recursive: true);
			}
		}
	}
}
