// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Benchmarks.Glob.Api;

/// <summary>
/// Stores the reusable file system context needed to create a fresh <see cref="GlobEnumerator"/> for each benchmark
/// invocation.
/// </summary>
public readonly record struct GlobContext(IFileSystem FileSystem, string FromDirectory = ".");

/// <summary>
/// Base class for all glob benchmarks providing common setup and teardown functionality.
/// </summary>
#if SHORT_RUN || DEBUG
[ShortRunJob]
#else
[SimpleJob(RuntimeMoniker.HostProcess)]
#endif
public abstract class BenchmarkBase
{
    // these must be initialized in GlobalSetup(), so we use the old dirty hack - the null-forgiving operator:
    protected GlobContext _glob;
    protected string _realFSRootsPath = "";
    protected const string FsStandardJsonModelFileName = "standard-test-tree.json";
    protected string _fsStandardJsonModelPath = null!;
    protected bool _createdTempDirectory = false;

    public void SetupFakeStandardFileSystem()
    {
        BmConfiguration.BindOptions();
        _fsStandardJsonModelPath = Path.Combine(
                                            BmConfiguration.Options.FsJsonModelsDirectory,
                                            FsStandardJsonModelFileName);
        _glob = SetupFakeFileSystem(_fsStandardJsonModelPath);
    }

    protected virtual string FSJsonModelExist(string fsJsonModelPath)
        => File.Exists(fsJsonModelPath)
                    ? fsJsonModelPath
                    : throw new FileNotFoundException($"Did not find the test file system structure file {fsJsonModelPath} (CWD: {Directory.GetCurrentDirectory()}).", fsJsonModelPath);

    protected virtual GlobContext SetupFakeFileSystem(string fsJsonModelPath)
        => new(
            new FakeFS(
                FSJsonModelExist(fsJsonModelPath),
                DataType.Json));

    protected virtual GlobContext SetupRealFileSystems(string fsJsonModelPath)
    {
        FSJsonModelExist(fsJsonModelPath);

        // all real FS will be tested under the root specified in the configuration or in a temp directory.
        // Each glob enumerator will have its own subdirectory with a name - the name of the JSON model file (without the extension).

        _realFSRootsPath = BmConfiguration.Options.TestsRootPath;

        // figure out where is the root of all the file systems in the current environment:
        if (string.IsNullOrWhiteSpace(_realFSRootsPath))
        {
            // not specified - create the file system root in a temp directory:
            var info = Directory.CreateTempSubdirectory($"GlobBm_");
            _realFSRootsPath = info.FullName;
            _createdTempDirectory = true;
        }

        // the directory for this specific file system:
        var realDirectoryPath = Path.Combine(_realFSRootsPath, Path.GetFileNameWithoutExtension(fsJsonModelPath));

        // use the specified directory
        if (Directory.Exists(realDirectoryPath))
        {
            // it exists - verify it first:
            var errors = string.Join("\n  ", TestFileStructure.VerifyTestFileStructure(fsJsonModelPath, realDirectoryPath));

            if (errors.Length > 0)
                throw new InvalidOperationException($"Test file structure verification failed:\n{errors}");
        }
        else
        {
            // it does not exist - create it:
            Directory.CreateDirectory(realDirectoryPath);
            TestFileStructure.CreateTestFileStructure(fsJsonModelPath, realDirectoryPath);
        }

        return new(new FileSystem(), realDirectoryPath);
    }

    protected virtual void CleanupRealFileSystems()
    {
        if (!_createdTempDirectory || !Directory.Exists(_realFSRootsPath))
            return;

        try
        {
            Directory.Delete(_realFSRootsPath, true);
            _createdTempDirectory = false;
        }
        // ignore any errors during cleanup of the temp directory.
        catch (UnauthorizedAccessException) { }
        catch (ArgumentNullException) { }
        catch (PathTooLongException) { }
        catch (DirectoryNotFoundException) { }
        catch (ArgumentException) { }
        catch (IOException) { }
    }

    /// <summary>
    /// Executes the glob enumeration and consumes all results.
    /// </summary>
    protected static int EnumerateAll(GlobEnumerator enumerator)
    {
        var count = 0;

        foreach (var _ in enumerator.Enumerate())
            count++;

        return count;
    }

    /// <summary>
    /// Creates a fresh <see cref="GlobEnumerator"/> from the default benchmark file system context.
    /// </summary>
    /// <remarks>
    /// Benchmarks must start from a fresh enumerator every invocation because <see cref="GlobEnumerator"/> becomes permanently
    /// frozen after the first <see cref="GlobEnumerator.Enumerate"/> call.
    /// </remarks>
    protected GlobEnumerator CreateGlob() => CreateGlob(_glob);

    /// <summary>
    /// Creates a fresh <see cref="GlobEnumerator"/> from the specified benchmark file system context.
    /// </summary>
    protected static GlobEnumerator CreateGlob(GlobContext context)
        => new(context.FileSystem) { FromDirectory = context.FromDirectory };
}
