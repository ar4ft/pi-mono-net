using Pi.CodingAgent.Session;
using Xunit;

namespace Pi.CodingAgent.Tests;

public class SessionManagerTests
{
    [Fact]
    public async Task CreateSessionAsync_WithParentSessionId_SetsParentSession()
    {
        // Arrange
        var storage = new SessionStorage(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        var manager = new SessionManager(storage);
        
        // Create first session
        var parentSession = await manager.CreateSessionAsync("Parent Session");
        var parentId = parentSession.Id;
        
        // Act - Create child session with parent reference
        var childSession = await manager.CreateSessionAsync("Child Session", parentSessionId: parentId);
        
        // Assert
        Assert.NotNull(childSession.ParentSession);
        Assert.Equal(parentId, childSession.ParentSession);
        
        // Cleanup
        await manager.CloseSessionAsync();
    }
    
    [Fact]
    public async Task ForkSessionAsync_CreatesNewSessionWithParentReference()
    {
        // Arrange
        var storage = new SessionStorage(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        var manager = new SessionManager(storage);
        
        // Create initial session
        var originalSession = await manager.CreateSessionAsync("Original Session", "gpt-4");
        var originalId = originalSession.Id;
        
        // Act - Fork the session
        var forkedSession = await manager.ForkSessionAsync();
        
        // Assert
        Assert.NotNull(forkedSession);
        Assert.NotEqual(originalId, forkedSession.Id);
        Assert.NotNull(forkedSession.ParentSession);
        Assert.Equal(originalId, forkedSession.ParentSession);
        Assert.Contains("Fork of Original Session", forkedSession.Name);
        Assert.Equal("gpt-4", forkedSession.Model);
        
        // Cleanup
        await manager.CloseSessionAsync();
    }
    
    [Fact]
    public async Task ForkSessionAsync_PreservesParentSessionId()
    {
        // Arrange
        var storage = new SessionStorage(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        var manager = new SessionManager(storage);
        
        // Create first session
        var session1 = await manager.CreateSessionAsync("Session 1");
        var session1Id = session1.Id;
        
        // Fork to create session 2
        var session2 = await manager.ForkSessionAsync("Session 2");
        var session2Id = session2.Id;
        
        // Act & Assert - Verify the fork captured the parent ID correctly
        Assert.Equal(session1Id, session2.ParentSession);
        Assert.NotEqual(session1Id, session2Id);
        
        // Cleanup
        await manager.CloseSessionAsync();
    }
    
    [Fact]
    public async Task CreateSessionAsync_WithoutParentSessionId_HasNullParentSession()
    {
        // Arrange
        var storage = new SessionStorage(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        var manager = new SessionManager(storage);
        
        // Act
        var session = await manager.CreateSessionAsync("New Session");
        
        // Assert
        Assert.Null(session.ParentSession);
        
        // Cleanup
        await manager.CloseSessionAsync();
    }
    
    [Fact]
    public async Task SessionInfo_SerializesParentSession()
    {
        // Arrange
        var storage = new SessionStorage(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
        var manager = new SessionManager(storage);
        
        // Create parent and child sessions
        var parentSession = await manager.CreateSessionAsync("Parent");
        var parentId = parentSession.Id;
        
        var childSession = await manager.CreateSessionAsync("Child", parentSessionId: parentId);
        var childId = childSession.Id;
        
        await manager.CloseSessionAsync();
        
        // Act - Load the child session back
        await manager.LoadSessionAsync(childId);
        
        // Assert
        var loadedSession = manager.CurrentSession;
        Assert.NotNull(loadedSession);
        Assert.Equal(parentId, loadedSession.ParentSession);
        
        // Cleanup
        await manager.CloseSessionAsync();
    }
}
