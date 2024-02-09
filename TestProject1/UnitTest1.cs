using Crud_with_JWT_Auth.Controllers;
using Crud_with_JWT_Auth.Context;
using Crud_with_JWT_Auth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public async Task GetTasks_ReturnsListOfTasks()
        {
            // Arrange
            var dbContextMock = new Mock<AppDBContext>();
            var controller = new TasksController(dbContextMock.Object);
            var tasksList = new List<Tasks> { new Tasks { Id = 1, Name = "Task 1", Description = "Description 1" } };
            var dbSetMock = new Mock<DbSet<Tasks>>();

            dbSetMock.As<IQueryable<Tasks>>().Setup(m => m.Provider).Returns(tasksList.AsQueryable().Provider);
            dbSetMock.As<IQueryable<Tasks>>().Setup(m => m.Expression).Returns(tasksList.AsQueryable().Expression);
            dbSetMock.As<IQueryable<Tasks>>().Setup(m => m.ElementType).Returns(tasksList.AsQueryable().ElementType);
            dbSetMock.As<IQueryable<Tasks>>().Setup(m => m.GetEnumerator()).Returns(tasksList.AsQueryable().GetEnumerator());

            dbContextMock.Setup(c => c.Tasks).Returns(dbSetMock.Object);

            // Act
            var result = await controller.GetTasks();

            // Assert
            Assert.IsType<ActionResult<IEnumerable<Tasks>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var tasksResult = Assert.IsAssignableFrom<IEnumerable<Tasks>>(okResult.Value);
            Assert.Equal(tasksList.Count, tasksResult.Count());
        }
    }
}