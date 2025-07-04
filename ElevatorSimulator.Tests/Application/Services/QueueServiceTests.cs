using ElevatorSimulator.Core.Application.Features.RequestProcessing.Services;
using ElevatorSimulator.Core.Domain.Entities;
using NUnit.Framework;

namespace ElevatorSimulator.Tests.Application.Services;

[TestFixture]
public class QueueServiceTests
{
    private QueueService _queueService;

    [SetUp]
    public void SetUp()
    {
        _queueService = new QueueService();
    }

    [Test]
    public void EnqueueRequest_ShouldAddRequestToQueue()
    {
        // Arrange
        var request = new Request(1, 5, 3);

        // Act
        _queueService.EnqueueRequest(request);

        // Assert
        Assert.That(_queueService.HasPendingRequests, Is.True);
        Assert.That(_queueService.PendingCount, Is.EqualTo(1));
    }

    [Test]
    public void EnqueueRequest_ShouldThrowException_WhenRequestIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _queueService.EnqueueRequest(null));
    }

    [Test]
    public void DequeueRequest_ShouldReturnFirstRequest()
    {
        // Arrange
        var request1 = new Request(1, 5, 3);
        var request2 = new Request(2, 8, 4);
        _queueService.EnqueueRequest(request1);
        _queueService.EnqueueRequest(request2);

        // Act
        var dequeuedRequest = _queueService.DequeueRequest();

        // Assert
        Assert.That(dequeuedRequest, Is.EqualTo(request1)); // FIFO
        Assert.That(_queueService.PendingCount, Is.EqualTo(1));
    }

    [Test]
    public void DequeueRequest_ShouldReturnNull_WhenQueueIsEmpty()
    {
        // Act
        var dequeuedRequest = _queueService.DequeueRequest();

        // Assert
        Assert.That(dequeuedRequest, Is.Null);
    }

    [Test]
    public void PeekNextRequest_ShouldReturnFirstRequest_WithoutRemoving()
    {
        // Arrange
        var request1 = new Request(1, 5, 3);
        var request2 = new Request(2, 8, 4);
        _queueService.EnqueueRequest(request1);
        _queueService.EnqueueRequest(request2);

        // Act
        var peekedRequest = _queueService.PeekNextRequest();

        // Assert
        Assert.That(peekedRequest, Is.EqualTo(request1));
        Assert.That(_queueService.PendingCount, Is.EqualTo(2)); // Should not remove
    }

    [Test]
    public void PeekNextRequest_ShouldReturnNull_WhenQueueIsEmpty()
    {
        // Act
        var peekedRequest = _queueService.PeekNextRequest();

        // Assert
        Assert.That(peekedRequest, Is.Null);
    }

    [Test]
    public void HasPendingRequests_ShouldReturnTrue_WhenRequestsExist()
    {
        // Arrange
        _queueService.EnqueueRequest(new Request(1, 5, 3));

        // Act & Assert
        Assert.That(_queueService.HasPendingRequests, Is.True);
    }

    [Test]
    public void HasPendingRequests_ShouldReturnFalse_WhenQueueIsEmpty()
    {
        // Act & Assert
        Assert.That(_queueService.HasPendingRequests, Is.False);
    }

    [Test]
    public void PendingCount_ShouldReturnCorrectCount()
    {
        // Act & Assert - Initially empty
        Assert.That(_queueService.PendingCount, Is.EqualTo(0));

        // Add requests
        _queueService.EnqueueRequest(new Request(1, 5, 3));
        _queueService.EnqueueRequest(new Request(2, 8, 4));
        Assert.That(_queueService.PendingCount, Is.EqualTo(2));

        // Remove one request
        _queueService.DequeueRequest();
        Assert.That(_queueService.PendingCount, Is.EqualTo(1));
    }

    [Test]
    public void GetQueue_ShouldReturnUnderlyingQueue()
    {
        // Arrange
        var request = new Request(1, 5, 3);
        _queueService.EnqueueRequest(request);

        // Act
        var queue = _queueService.GetQueue();

        // Assert
        Assert.That(queue, Is.Not.Null);
        Assert.That(queue.HasPendingRequests, Is.True);
        Assert.That(queue.PendingCount, Is.EqualTo(1));
    }

    [Test]
    public void QueueService_ShouldMaintainFIFOOrder()
    {
        // Arrange
        var request1 = new Request(1, 5, 3);
        var request2 = new Request(2, 8, 4);
        var request3 = new Request(3, 12, 2);

        // Act
        _queueService.EnqueueRequest(request1);
        _queueService.EnqueueRequest(request2);
        _queueService.EnqueueRequest(request3);

        // Assert
        Assert.That(_queueService.DequeueRequest(), Is.EqualTo(request1));
        Assert.That(_queueService.DequeueRequest(), Is.EqualTo(request2));
        Assert.That(_queueService.DequeueRequest(), Is.EqualTo(request3));
        Assert.That(_queueService.HasPendingRequests, Is.False);
    }
}