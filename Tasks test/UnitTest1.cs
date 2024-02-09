using Crud_with_JWT_Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Moq;

namespace Tasks_test
{
    [Fact, Authorize(Roles = "Admin,User")]
    public async Task GetTasks_ReturnsListOfTasks()
    {
        // Arrange
        var mockContext = new Mock<AppDBContext>();
        var tasks = new List<Tasks>
    {
        new Tasks { Id = 1, Name = "Task 1" },
        new Tasks { Id = 2, Name = "Task 2" },
    };
        mockContext.Setup(m => m.Tasks.ToListAsync()).ReturnsAsync(tasks);
        var controller = new TasksController(mockContext.Object);

        // Act
        var result = await controller.GetTasks();

        // Assert
        Assert.Equal(tasks.Count, result.Count());
        Assert.Equal(tasks[0].Id, result.ElementAt(0).Id);
        Assert.Equal(tasks[0].Name, result.ElementAt(0).Name);
        Assert.Equal(tasks[1].Id, result.ElementAt(1).Id);
        Assert.Equal(tasks[1].Name, result.ElementAt(1).Name);
    }
}