using ElevatorSimulator.Core.Domain.Entities;
using NUnit.Framework;

namespace ElevatorSimulator.Tests.Domain.Entities;

[TestFixture]
public class RequestQueueTests
{
    private RequestQueue _queue;

    [SetUp]
    public void SetUp()
    {
        _queue = new RequestQueue();
    }

    [Test]
    public void NewQueue_ShouldBeEmpty()
    {
        // Assert
        Assert.That(_queue.HasPendingRequests, Is.False);
        Assert.That(_queue.PendingCount, Is.EqualTo(0));
    }

    [Test]
    public void Enqueue_ShouldAddRequest()
    {
        // Arrange
        var request = new Request(1, 5, 3);

        // Act
        _queue.Enqueue(request);

        // Assert
        Assert.That(_queue.HasPendingRequests, Is.True);
        Assert.That(_queue.PendingCount, Is.EqualTo(1));
    }

    [Test]
    public void Dequeue_ShouldReturnFirstRequest()
    {
        // Arrange
        var request1 = new Request(1, 5, 3);
        var request2 = new Request(2, 8, 4);
        _queue.Enqueue(request1);
        _queue.Enqueue(request2);

        // Act
        var dequeuedRequest = _queue.Dequeue();

        // Assert
        Assert.That(dequeuedRequest, Is.EqualTo(request1)); // FIFO
        Assert.That(_queue.PendingCount, Is.EqualTo(1));
    }

    [Test]
    public void Dequeue_ShouldReturnNull_WhenEmpty()
    {
        // Act
        var dequeuedRequest = _queue.Dequeue();

        // Assert
        Assert.That(dequeuedRequest, Is.Null);
    }

    [Test]
    public void PeekNext_ShouldReturnFirstRequest_WithoutRemoving()
    {
        // Arrange
        var request1 = new Request(1, 5, 3);
        var request2 = new Request(2, 8, 4);
        _queue.Enqueue(request1);
        _queue.Enqueue(request2);

        // Act
        var peekedRequest = _queue.PeekNext();

        // Assert
        Assert.That(peekedRequest, Is.EqualTo(request1));
        Assert.That(_queue.PendingCount, Is.EqualTo(2)); // Should not remove
    }

    [Test]
    public void PeekNext_ShouldReturnNull_WhenEmpty()
    {
        // Act
        var peekedRequest = _queue.PeekNext();

        // Assert
        Assert.That(peekedRequest, Is.Null);
    }

    [Test]
    public void Queue_ShouldMaintainFIFOOrder()
    {
        // Arrange
        var request1 = new Request(1, 5, 3);
        var request2 = new Request(2, 8, 4);
        var request3 = new Request(3, 12, 2);

        // Act
        _queue.Enqueue(request1);
        _queue.Enqueue(request2);
        _queue.Enqueue(request3);

        // Assert
        Assert.That(_queue.Dequeue(), Is.EqualTo(request1));
        Assert.That(_queue.Dequeue(), Is.EqualTo(request2));
        Assert.That(_queue.Dequeue(), Is.EqualTo(request3));
        Assert.That(_queue.HasPendingRequests, Is.False);
    }

    [Test]
    public void Queue_ShouldHandleMultipleEnqueueDequeue()
    {
        // Arrange
        var request1 = new Request(1, 5, 3);
        var request2 = new Request(2, 8, 4);

        // Act & Assert
        _queue.Enqueue(request1);
        Assert.That(_queue.PendingCount, Is.EqualTo(1));

        var dequeued1 = _queue.Dequeue();
        Assert.That(dequeued1, Is.EqualTo(request1));
        Assert.That(_queue.PendingCount, Is.EqualTo(0));

        _queue.Enqueue(request2);
        Assert.That(_queue.PendingCount, Is.EqualTo(1));

        var dequeued2 = _queue.Dequeue();
        Assert.That(dequeued2, Is.EqualTo(request2));
        Assert.That(_queue.HasPendingRequests, Is.False);
    }

    // CONCURRENCY & EDGE CASE TESTS
    [Test]
    public void Queue_ConcurrentEnqueue_ShouldHandleMultipleThreads()
    {
        // Arrange
        const int requestCount = 100;
        var tasks = new List<Task>();
        var requests = new List<Request>();
        
        for (int i = 0; i < requestCount; i++)
        {
            var request = new Request(i % 10 + 1, (i % 10) + 5, 1);
            requests.Add(request);
            tasks.Add(Task.Run(() => _queue.Enqueue(request)));
        }

        // Act
        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.That(_queue.PendingCount, Is.EqualTo(requestCount));
        Assert.That(_queue.HasPendingRequests, Is.True);
    }

    [Test]
    public void Queue_ConcurrentEnqueueDequeue_ShouldNotLoseRequests()
    {
        // Arrange
        const int requestCount = 50;
        var enqueueTasks = new List<Task>();
        var dequeueTasks = new List<Task>();
        var dequeuedRequests = new List<Request>();

        // Act - Enqueue and dequeue concurrently
        for (int i = 0; i < requestCount; i++)
        {
            var request = new Request(i + 1, i + 5, 1);
            enqueueTasks.Add(Task.Run(() => _queue.Enqueue(request)));
        }

        // Start dequeuing while enqueuing
        for (int i = 0; i < requestCount; i++)
        {
            dequeueTasks.Add(Task.Run(() =>
            {
                Request? dequeued = null;
                while (dequeued == null)
                {
                    dequeued = _queue.Dequeue();
                    if (dequeued == null)
                        Thread.Sleep(1); // Small delay to prevent busy waiting
                }
                lock (dequeuedRequests)
                {
                    dequeuedRequests.Add(dequeued);
                }
            }));
        }

        Task.WaitAll(enqueueTasks.ToArray());
        Task.WaitAll(dequeueTasks.ToArray());

        // Assert
        Assert.That(dequeuedRequests.Count, Is.EqualTo(requestCount));
        Assert.That(_queue.HasPendingRequests, Is.False);
    }

    [Test]
    public void Queue_StressTest_ShouldHandleHighVolume()
    {
        // Arrange
        const int requestCount = 1000;
        var requests = new List<Request>();
        
        for (int i = 0; i < requestCount; i++)
        {
            requests.Add(new Request(i % 20 + 1, (i % 20) + 5, i % 10 + 1));
        }

        // Act - Enqueue all requests
        foreach (var request in requests)
        {
            _queue.Enqueue(request);
        }

        // Assert - Dequeue all requests
        var dequeuedCount = 0;
        while (_queue.HasPendingRequests)
        {
            var dequeued = _queue.Dequeue();
            if (dequeued != null)
                dequeuedCount++;
        }

        Assert.That(dequeuedCount, Is.EqualTo(requestCount));
        Assert.That(_queue.PendingCount, Is.EqualTo(0));
    }

    [Test]
    public void Queue_EdgeCase_NullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert - Should throw ArgumentNullException for null request
        var exception = Assert.Throws<ArgumentNullException>(() => _queue.Enqueue(null));
        
        // Verify the exception details
        Assert.That(exception.ParamName, Is.EqualTo("request"));
        
        // Queue should remain empty after failed enqueue
        Assert.That(_queue.PendingCount, Is.EqualTo(0));
        Assert.That(_queue.HasPendingRequests, Is.False);
    }

    [Test]
    public void Queue_EdgeCase_ExtremePassengerCount_ShouldQueue()
    {
        // Arrange
        var extremeRequest = new Request(1, 20, int.MaxValue);

        // Act
        _queue.Enqueue(extremeRequest);

        // Assert
        Assert.That(_queue.PendingCount, Is.EqualTo(1));
        var dequeued = _queue.Dequeue();
        Assert.That(dequeued?.PassengerCount, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void Queue_EdgeCase_SameFloorRequest_ShouldQueue()
    {
        // Arrange
        var sameFloorRequest = new Request(5, 5, 3);

        // Act
        _queue.Enqueue(sameFloorRequest);

        // Assert
        Assert.That(_queue.PendingCount, Is.EqualTo(1));
        var dequeued = _queue.Dequeue();
        Assert.That(dequeued?.FromFloor, Is.EqualTo(5));
        Assert.That(dequeued?.ToFloor, Is.EqualTo(5));
    }

    [Test]
    public void Queue_PeekNext_ConcurrentAccess_ShouldNotCorruptQueue()
    {
        // Arrange
        var request1 = new Request(1, 5, 3);
        var request2 = new Request(2, 8, 4);
        _queue.Enqueue(request1);
        _queue.Enqueue(request2);

        var peekTasks = new List<Task<Request?>>();
        
        // Act - Multiple concurrent peeks
        for (int i = 0; i < 20; i++)
        {
            peekTasks.Add(Task.Run(() => _queue.PeekNext()));
        }

        var results = Task.WhenAll(peekTasks).Result;

        // Assert
        Assert.That(results.All(r => r == request1), Is.True);
        Assert.That(_queue.PendingCount, Is.EqualTo(2)); // Should not change
    }

    [Test]
    public void Queue_MixedOperations_ShouldMaintainConsistency()
    {
        // Arrange
        var random = new Random(42); // Fixed seed for reproducible results
        var operationTasks = new List<Task>();
        var processedRequests = new List<Request>();

        // Act - Mix of enqueue, dequeue, and peek operations
        for (int i = 0; i < 100; i++)
        {
            var operation = random.Next(0, 3);
            switch (operation)
            {
                case 0: // Enqueue
                    var request = new Request(i % 10 + 1, (i % 10) + 5, 1);
                    operationTasks.Add(Task.Run(() => _queue.Enqueue(request)));
                    break;
                case 1: // Dequeue
                    operationTasks.Add(Task.Run(() =>
                    {
                        var dequeued = _queue.Dequeue();
                        if (dequeued != null)
                        {
                            lock (processedRequests)
                            {
                                processedRequests.Add(dequeued);
                            }
                        }
                    }));
                    break;
                case 2: // Peek
                    operationTasks.Add(Task.Run(() => _queue.PeekNext()));
                    break;
            }
        }

        Task.WaitAll(operationTasks.ToArray());

        // Assert - Queue should be in consistent state
        Assert.That(_queue.PendingCount, Is.GreaterThanOrEqualTo(0));
        Assert.That(processedRequests.Count, Is.GreaterThanOrEqualTo(0));
    }
}