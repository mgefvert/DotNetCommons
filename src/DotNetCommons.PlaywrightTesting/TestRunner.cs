using System.Reflection;
using Dasync.Collections;
using DotNetCommons.Sys;

namespace DotNetCommons.PlaywrightTesting;

public class TestRunner
{
    private readonly PlaywrightSession _session;
    private readonly Uri _root;
    private readonly Assembly _assembly;

    public List<TestResult> Results { get; } = new();
    
    public TestRunner(PlaywrightSession session, Uri root, Assembly assembly)
    {
        _session = session;
        _root = root;
        _assembly = assembly;
    }

    public async Task<bool> Run()
    {
        Results.Clear();
        
        var types = FindPlaywrightTestClasses(_assembly);
        if (!types.Any())
            return true;

        foreach (var type in types)
        {
            Console.WriteLine($"# {type.Name}");
            try
            {
                await RunTests(type);
            }
            catch (Exception ex)
            {
                using (new SetConsoleColor(ConsoleColor.Red))
                    Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine();
            }
        }

        return !Results.Any();
    }

    private List<Type> FindPlaywrightTestClasses(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(x => x.IsClass && x.IsPublic && x.GetCustomAttribute<PlaywrightTestClassAttribute>() != null)
            .ToList();
    }

    private async Task RunTests(Type testClass)
    {
        var instance = Activator.CreateInstance(testClass);
        if (instance == null)
            throw new PlaywrightTestingException($"Unable to instantiate test class {testClass.Name}");

        await using var context = await _session.NewContext(_root);

        MethodInfo? FindMethod<T>() where T : Attribute => testClass
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(x => x.GetCustomAttribute<T>() != null);
        
        var classSetup = FindMethod<PlaywrightClassSetupAttribute>();
        var classTeardown = FindMethod<PlaywrightClassTeardownAttribute>();
        var testSetup = FindMethod<PlaywrightSetupAttribute>();
        var testTeardown = FindMethod<PlaywrightTeardownAttribute>();

        await CallMethod(instance, classSetup, _session, context);
        try
        {
            await RunSingleTests(context, instance, Parallelism.First, testSetup, testTeardown);
            await RunSingleTests(context, instance, Parallelism.Single, testSetup, testTeardown);
            await RunParallelTests(context, instance, testSetup, testTeardown);
            await RunSingleTests(context, instance, Parallelism.Last, testSetup, testTeardown);
        }
        finally
        {
            await CallMethod(instance, classTeardown);
        }
    }

    private async Task RunParallelTests(PlaywrightContext context, object instance, MethodInfo? testSetup, MethodInfo? testTeardown)
    {
        var methods = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.GetCustomAttribute<PlaywrightTestAttribute>()?.Parallelism == Parallelism.Parallel)
            .ToList();

        await methods.ParallelForEachAsync(async method => await RunIndividualTest(context, instance, method, testSetup, testTeardown),
            maxDegreeOfParallelism: 3);
    }

    private async Task RunSingleTests(PlaywrightContext context, object instance, Parallelism parallelism, MethodInfo? testSetup, MethodInfo? testTeardown)
    {
        var methods = instance.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => m.GetCustomAttribute<PlaywrightTestAttribute>()?.Parallelism == parallelism)
            .ToList();

        foreach (var method in methods)
            await RunIndividualTest(context, instance, method, testSetup, testTeardown);
    }

    private async Task RunIndividualTest(PlaywrightContext context, object instance, MethodInfo method, MethodInfo? testSetup, MethodInfo? testTeardown)
    {
        Console.WriteLine($"## {instance.GetType().Name}.{method.Name}");
        
        await using var page = await context.NewPage(method.Name);
        await CallMethod(instance, testSetup, page);
        try
        {
            await CallMethod(instance, method, page);
            Results.Add(new TestResult(instance.GetType().Name, method.Name, true, null));
        }
        catch (Exception e)
        {
            using (new SetConsoleColor(ConsoleColor.Red))
                Console.WriteLine(e);

            Results.Add(new TestResult(instance.GetType().Name, method.Name, false, e.Message));
        }
        await CallMethod(instance, testTeardown);
    }

    private async Task CallMethod(object instance, MethodInfo? method, params object[] parameters)
    {
        try
        {
            if (method == null)
                return;

            var result = method.Invoke(instance, parameters);
            if (result is Task task)
                await task;
        }
        catch (TargetParameterCountException e)
        {
            throw new Exception($"{instance.GetType().Name}.{method?.Name}: {e.Message}");
        }
        catch (ArgumentException e)
        {
            throw new Exception($"{instance.GetType().Name}.{method?.Name}: {e.Message}");
        }
    }
}