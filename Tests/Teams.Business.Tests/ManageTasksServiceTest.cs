﻿using MockQueryable.Moq;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teams.Business.Services;
using Teams.Data;
using Teams.Data.Models;
using Teams.Security;
using Task = Teams.Data.Models.Task;

namespace Teams.Business.Tests
{
    [TestFixture]
    class ManageTasksServiceTest
    {
        private Mock<IRepository<Task, int>> _tasksRepository;
        private ManageTasksService _manageTasksService;
        private Mock<ICurrentUser> _currentUser;

        [SetUp]
        public void Setup()
        {
            _currentUser = new Mock<ICurrentUser>();
            _tasksRepository = new Mock<IRepository<Task, int>>();
            _manageTasksService = new ManageTasksService(_tasksRepository.Object, _currentUser.Object);
            var mock = getFakeDbTasks().AsQueryable().BuildMock();
            _tasksRepository.Setup(x => x.GetAll()).Returns(mock.Object);
            _tasksRepository.Setup(t => t.DeleteAsync(It.IsAny<Task>())).ReturnsAsync(true);
        }

        [Test]
        public async System.Threading.Tasks.Task GetAllTasksInTeamAsync_RepositoryContainsTwoTasksForSelectedTeam_ReturnsTwoTasks()
        {
            //Arrange
            const int teamId = 1;

            //Act
            var result = new List<Task>(await _manageTasksService.GetAllTasksForTeamAsync(teamId, new DisplayOptions { }));

            //Assert
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async System.Threading.Tasks.Task GetAllTasksInTeamAsync_RepositoryContainsZeroTasksForSelectedTeam_ReturnsNull()
        {
            //Arrange
            const int teamId = 2;

            //Act
            var result = new List<Task>(await _manageTasksService.GetAllTasksForTeamAsync(teamId, new DisplayOptions { }));

            //Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public async System.Threading.Tasks.Task GetTaskByIdAsync_ManageTasksServiceReturnsTaskWithId1_ReturnTaskWithId1()
        {
            // Arrange
            const int taskId = 1;

            var task = new List<Data.Models.Task>()
            {
                new Data.Models.Task
                {
                    Id = 1,
                    Link = "google.com",
                    MemberId = 1,
                    Name = "Implement 4.5 issue",
                    StoryPoints = 3,
                    TeamMember = null,
                    Sprint = null,
                    SprintId = 9,
                    Team = null,
                    TeamId = 9
                }
            };

            var mock = task.AsQueryable().BuildMock();

            _tasksRepository.Setup(x => x.GetAll()).Returns(mock.Object);

            // Act
            var result = await _manageTasksService.GetTaskByIdAsync(taskId);

            // Assert
            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public async System.Threading.Tasks.Task RemoveAsync_ManageTasksServiceReturnsTrue_ReturnsTrue()
        {
            //Arrange
            const string ownerId = "1";
            const int taskId = 1;
            var user = new Mock<UserDetails>(null);
            user.Setup(x => x.Id()).Returns(ownerId);
            user.Setup(x => x.Name()).Returns("name");
            _currentUser.SetupGet(x => x.Current).Returns(user.Object);

            //Act
            bool result = await _manageTasksService.RemoveAsync(taskId);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async System.Threading.Tasks.Task RemoveAsync_ManageTasksServiceReturnsFalse_ReturnsFalse()
        {
            //Arrange
            const string ownerId = "2";
            const int taskId = 1;
            var user = new Mock<UserDetails>(null);
            user.Setup(x => x.Id()).Returns(ownerId);
            user.Setup(x => x.Name()).Returns("name");
            _currentUser.SetupGet(x => x.Current).Returns(user.Object);

            //Act
            bool result = await _manageTasksService.RemoveAsync(taskId);

            //Assert
            Assert.IsFalse(result);
        }

        private List<Task> getFakeDbTasks()
        {
            var tasks = new List<Task>()
            {
                new Task{Id = 1,SprintId = 1,TeamId = 1,Team = new Team{TeamOwner = "1"}},
                new Task{Id = 2,SprintId = 1,TeamId = 1,Team = new Team{TeamOwner = "2"}},
                new Task{Id = 3,SprintId = 3,TeamId = 3,Team = new Team{TeamOwner = "3"}},
                new Task{Id = 4,SprintId = 3,TeamId = 3,Team = new Team{TeamOwner = "4"}},
            };
            return tasks;
        }


        [Test]
        public async System.Threading.Tasks.Task EditTaskAsync_ManageTaskServiceReturns_True()
        {
            //Arrange
            var task = new Task { Id = 1, TeamId = 1, Name = "Task", MemberId = 1, Link = "vk.com", StoryPoints = 3, SprintId =3 };

            _tasksRepository.Setup(x => x.UpdateAsync(It.IsAny<Task>())).ReturnsAsync(true);
            _tasksRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(task);

            //Act
            var result = await _manageTasksService.EditTaskAsync(task);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async System.Threading.Tasks.Task EditTaskAsync_ManageSpiritsServiceReturns_False()
        {
            //Arrange
            var task1 = new Task { Id = 1, TeamId = 1, Name = "Ta sk", MemberId = 1, Link = "vk.com", StoryPoints = 3, SprintId = 3 };
            var task2 = new Task { Id = 1, TeamId = 1, Name = "Task", MemberId = 1, Link = "vkcom", StoryPoints = 3, SprintId = 3 };
            var task3 = new Task { Id = 1, TeamId = 1, Name = "Task", MemberId = 1, Link = "vk.com", StoryPoints = -3, SprintId = 3 };

            _tasksRepository.Setup(x => x.UpdateAsync(It.IsAny<Task>())).ReturnsAsync(true);
            _tasksRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(task2);

            //Act
            var result1 = await _manageTasksService.EditTaskAsync(task1); //Incorrect name
            var result2 = await _manageTasksService.EditTaskAsync(task2); //Incorrect link count
            var result3 = await _manageTasksService.EditTaskAsync(task3); //Incorrect SP count


            //Assert
            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
            Assert.IsFalse(result3);
        }

        [Test]
        public async System.Threading.Tasks.Task AddTaskAsync_ManageTaskServiceReturns_True()
        {
            //Arrange
            var task = new Task { Id = 1, TeamId = 1, Name = "Task1",Link = "https://github.com/",StoryPoints = 1};

            _tasksRepository.Setup(x => x.InsertAsync(It.IsAny<Task>())).ReturnsAsync(true);

            //Act
            var result = await _manageTasksService.AddTaskAsync(task);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async System.Threading.Tasks.Task AddTasksAsync_ManageTasksServiceReturns_False()
        {
            //Arrange
            var task1 = new Task { Id = 1, TeamId = 1, Name = "Task 1@@ ", Link = "https://github.com/", StoryPoints = 1 };
            var task2 = new Task { Id = 2, TeamId = 2, Name = "Task2", Link = "https://github.com/", StoryPoints = -5 };
            var task3 = new Task { Id = 3, TeamId = 3, Name = "Task3", Link = "https:/ qq/github.com/", StoryPoints = 3 };

            _tasksRepository.Setup(x => x.InsertAsync(It.IsAny<Task>())).ReturnsAsync(true);

            //Act
            var result1 = await _manageTasksService.AddTaskAsync(task1); //Incorrect name
            var result2 = await _manageTasksService.AddTaskAsync(task2); //Incorrect SP count
            var result3 = await _manageTasksService.AddTaskAsync(task3); //Incorrect link

            //Assert
            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
            Assert.IsFalse(result3);
        }
    }
}
