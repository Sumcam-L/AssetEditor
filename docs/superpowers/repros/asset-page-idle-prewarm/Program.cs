using System;
using System.Collections.Generic;
using Firaxis.AssetEditing;

internal static class Program
{
    private static int Main()
    {
        var calls = new List<string>();
        var coordinator = new AssetPageBindingCoordinator();
        object geometry = new object();
        object cook = new object();
        object attachments = new object();

        int firstGeneration = coordinator.BeginGeneration(new[]
        {
            new AssetPageBindingCoordinator.Page("Geometries", geometry, trigger => calls.Add("Geometry:" + trigger)),
            new AssetPageBindingCoordinator.Page("Cook Params", cook, trigger => calls.Add("Cook:" + trigger)),
            new AssetPageBindingCoordinator.Page("Attachments", attachments, trigger => calls.Add("Attachments:" + trigger))
        }, geometry);

        AssertSequence(calls, "Geometry:initial");
        Assert(coordinator.PendingCount == 2, "initial generation did not leave two hidden pages pending");

        Assert(coordinator.BindNextIdle(), "first idle did not bind one page");
        AssertSequence(calls, "Geometry:initial", "Cook:idle");
        Assert(coordinator.PendingCount == 1, "one idle callback bound more than one page");

        Assert(coordinator.BindForUser(attachments), "user did not claim pending page");
        AssertSequence(calls, "Geometry:initial", "Cook:idle", "Attachments:user");
        Assert(!coordinator.BindNextIdle(), "idle rebound a user-bound page");
        Assert(!coordinator.BindForUser(attachments), "user rebound an already-bound page");

        object failed = new object();
        bool fail = true;
        coordinator.BeginGeneration(new[]
        {
            new AssetPageBindingCoordinator.Page("Initial", geometry, _ => { }),
            new AssetPageBindingCoordinator.Page("Failed", failed, _ =>
            {
                if (fail) throw new InvalidOperationException("expected");
                calls.Add("Failed:user");
            })
        }, geometry);
        try
        {
            coordinator.BindNextIdle();
            return Fail("idle binding exception was swallowed");
        }
        catch (InvalidOperationException ex) when (ex.Message == "expected")
        {
        }
        Assert(coordinator.GetState(failed) == AssetPageBindingState.Failed, "failed idle page did not enter Failed state");
        fail = false;
        Assert(coordinator.BindForUser(failed), "user could not retry an idle-failed page");
        Assert(coordinator.GetState(failed) == AssetPageBindingState.Bound, "user retry did not bind failed page");

        object failedInitial = new object();
        try
        {
            coordinator.BeginGeneration(new[]
            {
                new AssetPageBindingCoordinator.Page("Failed Initial", failedInitial,
                    _ => throw new InvalidOperationException("initial expected")),
                new AssetPageBindingCoordinator.Page("Pending", cook, _ => { })
            }, failedInitial);
            return Fail("initial binding exception was swallowed");
        }
        catch (InvalidOperationException ex) when (ex.Message == "initial expected")
        {
        }
        Assert(coordinator.PendingCount == 0, "failed initial bind left pending page state");
        Assert(!coordinator.HasPending, "failed initial bind left pending work");
        Assert(coordinator.GetState(failedInitial) == null, "failed initial bind left page lookup state");
        Assert(!coordinator.BindNextIdle(), "failed initial bind left idle work");

        object duplicate = new object();
        try
        {
            coordinator.BeginGeneration(new[]
            {
                new AssetPageBindingCoordinator.Page("Duplicate One", duplicate, _ => { }),
                new AssetPageBindingCoordinator.Page("Duplicate Two", duplicate, _ => { })
            }, duplicate);
            return Fail("duplicate page key was accepted");
        }
        catch (ArgumentException)
        {
        }
        Assert(coordinator.PendingCount == 0, "duplicate page key left pending page state");
        Assert(!coordinator.HasPending, "duplicate page key left pending work");
        Assert(coordinator.GetState(duplicate) == null, "duplicate page key left page lookup state");
        Assert(!coordinator.BindNextIdle(), "duplicate page key left idle work");

        int staleGeneration = coordinator.Generation;
        coordinator.BeginGeneration(new[]
        {
            new AssetPageBindingCoordinator.Page("Replacement", cook, _ => calls.Add("Replacement:initial"))
        }, cook);
        Assert(coordinator.Generation > staleGeneration && coordinator.Generation > firstGeneration,
            "rebind did not advance generation");
        Assert(coordinator.GetState(geometry) == null, "old-generation page state survived rebind");

        coordinator.Cancel();
        Assert(!coordinator.HasPending && coordinator.PendingCount == 0, "cancel left pending work");
        Assert(!coordinator.BindNextIdle(), "canceled coordinator executed idle work");

        var oneStepCalls = new List<string>();
        object oneStepInitial = new object();
        var oneStep = new AssetPageBindingCoordinator();
        oneStep.BeginGeneration(new[]
        {
            new AssetPageBindingCoordinator.Page("Initial", oneStepInitial, _ => oneStepCalls.Add("Initial")),
            new AssetPageBindingCoordinator.Page("One", new object(), _ => oneStepCalls.Add("One")),
            new AssetPageBindingCoordinator.Page("Two", new object(), _ => oneStepCalls.Add("Two"))
        }, oneStepInitial);
        int beforeIdle = oneStepCalls.Count;
        Assert(oneStep.BindNextIdle(), "one-step scheduler did not bind pending work");
        Assert(oneStepCalls.Count == beforeIdle + 1, "one scheduler turn did not bind exactly one page");
        Assert(oneStep.PendingCount == 1, "one scheduler turn drained all pending pages");

        Console.WriteLine("PASS: AST page binding is ordered, single-step, user-prioritized, retryable, and cancellable.");
        return 0;
    }

    private static void AssertSequence(IList<string> actual, params string[] expected)
    {
        Assert(actual.Count == expected.Length, "unexpected bind count");
        for (int i = 0; i < expected.Length; i++)
            Assert(actual[i] == expected[i], "unexpected bind order at " + i + ": " + actual[i]);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine("FAIL: " + message);
        return 1;
    }
}
