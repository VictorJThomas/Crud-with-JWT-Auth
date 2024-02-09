using Crud_with_JWT_Auth.Controllers;
using Crud_with_JWT_Auth.Context;

using Xunit;
using Crud_with_JWT_Auth.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;

namespace TestProject1
{
    public class TaskTesting
    {
        [Fact]
        public async Task PostTasks_InvalidModelState_ReturnsViewResult()
        {
            // Arrange
            var mockContext = new Mock<AppDBContext>();
            var controller = new TasksController(mockContext.Object);

            // Set up the mocked context behavior
            mockContext.Setup(context => context.Tasks.Add(It.IsAny<Tasks>()));
            mockContext.Setup(context => context.SaveChangesAsync(default)).ReturnsAsync(1);

            // Create an invalid model state
            controller.ModelState.AddModelError("Description", "Description is required");

            // Act
            var result = await controller.PostTasks(new Tasks { Description = "Sample Description" });

            // Assert
            Assert.IsType<ActionResult<Tasks>>(result); 
            Assert.Null((result as ActionResult<Tasks>).Value);
        }

        [Fact]
        public async Task GetTasks_ReturnsListOfTasks()
        {
            // Arrange
            var mockContext = new Mock<AppDBContext>();
            var controller = new TasksController(mockContext.Object);

            // Set up the mocked context behavior
            var tasksData = new List<Tasks>
            {
                new() { Id = 1, Name = "Task 1", Description = "Description 1" },
                new() { Id = 2, Name = "Task 2", Description = "Description 2" },
           
            };

            var mockDbSet = new Mock<DbSet<Tasks>>();

            mockDbSet.As<IAsyncEnumerable<Tasks>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Tasks>(tasksData.GetEnumerator()));

            mockDbSet.As<IQueryable<Tasks>>().Setup(m => m.Provider).Returns(tasksData.AsQueryable().Provider);
            mockDbSet.As<IQueryable<Tasks>>().Setup(m => m.Expression).Returns(tasksData.AsQueryable().Expression);
            mockDbSet.As<IQueryable<Tasks>>().Setup(m => m.ElementType).Returns(tasksData.AsQueryable().ElementType);
            mockDbSet.As<IQueryable<Tasks>>().Setup(m => m.GetEnumerator()).Returns(() => tasksData.GetEnumerator());

            mockContext.Setup(context => context.Tasks).Returns(mockDbSet.Object);

            // Act
            var result = await controller.GetTasks();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Tasks>>>(result);
            var tasks = Assert.IsType<List<Tasks>>(actionResult.Value);

            Assert.Equal(2, tasks.Count); 
            Assert.Equal("Task 1", tasks[0].Name);
            Assert.Equal("Task 2", tasks[1].Name);
        }


        [Fact]
        public async Task DeleteTasks_ExistingTask_ReturnsNoContentResult()
        {
            // Arrange
            var mockContext = new Mock<AppDBContext>();
            var controller = new TasksController(mockContext.Object);

            var existingTask = new Tasks { Id = 1, Name = "Existing Task", Description = "Existing Description" };

            mockContext.Setup(context => context.Tasks.FindAsync(It.IsAny<int>())).ReturnsAsync(existingTask);

            // Act
            var result = await controller.DeleteTasks(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }

    public class AuthTesting
    {
        [Fact]
        public void Register_ReturnsOkResult()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            var controller = new AuthController(mockConfiguration.Object);

            var newUserRequest = new Users
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                Password = "testpassword"
            };

            // Act
            var result = controller.Register(newUserRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Users>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

            var returnedUser = Assert.IsType<Users>(okResult.Value);

            Assert.Equal(newUserRequest.Email, returnedUser.Email);
            // UserName comparison is not needed because it's now hashed
            // Assert.Equal(newUserRequest.UserName, returnedUser.UserName);
            Assert.NotNull(returnedUser.Password); // Check that Password is not null
                                                   // Optionally, you can assert other properties as needed.
        }
    }


    // Helper classes for asynchronous operations
    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());

        public ValueTask DisposeAsync() => default;
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression) =>
            new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
            new TestAsyncEnumerable<TElement>(expression);

        public object Execute(Expression expression) =>
            _inner.Execute(expression);

        public TResult Execute<TResult>(Expression expression) =>
            _inner.Execute<TResult>(expression);

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression) =>
            new TestAsyncEnumerable<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken) =>
            Execute<TResult>(expression);
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }
}