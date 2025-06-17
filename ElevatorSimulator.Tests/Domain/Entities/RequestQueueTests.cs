using ElevatorSimulator.Domain.Entities;
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
}