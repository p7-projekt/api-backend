using Core.Classrooms;
using Core.Classrooms.Contracts;
using Core.Classrooms.Models;
using Core.Exercises;
using Core.Sessions.Contracts;
using Core.Shared;
using FluentResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.Core.Classrooms;

public class ClassroomServiceTest
{
    [Fact]
    public async Task CreateClassroomTest_ShouldReturn_Ok()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.InsertClassroomAsync(Arg.Any<ClassroomDto>(), Arg.Any<int>(), Arg.Any<string>()).Returns(Result.Ok());

        var dto = new ClassroomDto("Title", "Description");
        var result = await classroomService.CreateClassroom(dto, 1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CreateClassroomTest_ClassroomRepositoryFails_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.InsertClassroomAsync(Arg.Any<ClassroomDto>(), Arg.Any<int>(), Arg.Any<string>()).Returns(Result.Fail("Failed to insert classroom"));

        var dto = new ClassroomDto("Title", "Description");
        var result = await classroomService.CreateClassroom(dto, 1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task AddSessionToClassroom_ShouldReturn_Ok()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.AddSessionToClassroomAsync(Arg.Any<ClassroomSessionDto>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Result.Ok());

        var dto = new ClassroomSessionDto("Title", "Description", new List<int> { 1, 2 }, new List<int> { 1 });
        var result = await classroomService.AddSessionToClassroom(dto, 1, 1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddSessionToClassroom_AuthorValidationFails_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var dto = new ClassroomSessionDto("Title", "Description", new List<int> { 1, 2 }, new List<int> { 1 });
        var result = await classroomService.AddSessionToClassroom(dto, 1, 1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task AddSessionToClassroom_ClassroomRepoFailsToInsert_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.AddSessionToClassroomAsync(Arg.Any<ClassroomSessionDto>(), Arg.Any<int>(), Arg.Any<int>()).Returns(Result.Fail("Failed to add session"));

        var dto = new ClassroomSessionDto("Title", "Description", new List<int> { 1, 2 }, new List<int> { 1 });
        var result = await classroomService.AddSessionToClassroom(dto, 1, 1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task DeleteClassroom_ShouldReturn_Ok()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.DeleteClassroomAsync(Arg.Any<int>()).Returns(Result.Ok());

        var result = await classroomService.DeleteClassroom(1, 1);

        Assert.True(result.IsSuccess);  
    }

    [Fact]
    public async Task DeleteClassroom_AuthorValidationFails_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var result = await classroomService.DeleteClassroom(1, 1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task DeleteClassroom_ClassroomRepoFailsDeletion_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.DeleteClassroomAsync(Arg.Any<int>()).Returns(Result.Fail("Failed to delete classroom"));

        var result = await classroomService.DeleteClassroom(1, 1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetClassroomById_GetStudentClassroom_ShouldReturn_ClassroomResponseDto()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyStudentInClassroom(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var responseDto = new GetClassroomResponseDto { Id = 10 };
        classroomRepoSub.GetClassroomByIdAsync(Arg.Any<int>()).Returns(responseDto);

        var role = Roles.Student;
        var result = await classroomService.GetClassroomById(10, 2, role);

        Assert.True(result.IsSuccess);
        Assert.IsType<GetClassroomResponseDto>(result.Value);
        Assert.Equal(10, result.Value.Id);
    }

    [Fact]
    public async Task GetClassroomById_GetInstructorClassroom_ShouldReturn_ClassroomResponseDto()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var responseDto = new GetClassroomResponseDto { Id = 10 };
        classroomRepoSub.GetClassroomByIdAsync(Arg.Any<int>()).Returns(responseDto);

        var role = Roles.Instructor;
        var result = await classroomService.GetClassroomById(10, 2, role);

        Assert.True(result.IsSuccess);
        Assert.IsType<GetClassroomResponseDto>(result.Value);
        Assert.Equal(10, result.Value.Id);
    }

    [Fact]
    public async Task GetClassroomById_GetStudentClassroom_StudentNotInClassroom_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyStudentInClassroom(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var role = Roles.Student;
        var result = await classroomService.GetClassroomById(10, 2, role);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetClassroomById_GetInstructorClassroom_InstrucorNotAuthor_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var role = Roles.Instructor;
        var result = await classroomService.GetClassroomById(10, 2, role);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetClassroomById_InvalidRole_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        var role = Roles.AnonymousUser;
        var result = await classroomService.GetClassroomById(10, 2, role);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetClassroomsByUserRole_StudentRole_ShouldReturn_ClassroomsResponseDtoList()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        var dto1 = new GetClassroomsResponseDto(1, "Title", "Description");
        var dto2 = new GetClassroomsResponseDto(2, "Name", "Desc");
        var dtoList = new List<GetClassroomsResponseDto> { dto1, dto2 };
        classroomRepoSub.GetStudentClassroomsById(Arg.Any<int>()).Returns(dtoList);

        var role = Roles.Student;
        var result = await classroomService.GetClassroomsByUserRole(1, role);

        Assert.True(result.IsSuccess);
        Assert.IsType<List<GetClassroomsResponseDto>>(result.Value);
        Assert.Equal(2, result.Value[1].Id);
    }

    [Fact]
    public async Task GetClassroomsByUserRole_InstructorRole_NoClassrooms_ShouldReturn_EmptyClassroomsResponseDtoList()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        var dtoList = new List<GetClassroomsResponseDto> { };
        classroomRepoSub.GetInstructorClassroomsById(Arg.Any<int>()).Returns(dtoList);

        var role = Roles.Instructor;
        var result = await classroomService.GetClassroomsByUserRole(1, role);

        Assert.True(result.IsSuccess);
        Assert.IsType<List<GetClassroomsResponseDto>>(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetClassroomsByUserRole_InvalidRole_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        var role = Roles.AnonymousUser;
        var result = await classroomService.GetClassroomsByUserRole(1, role);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task UpdateClassroomDetails_ShouldReturn_Ok()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.UpdateClassroomDetailsAsync(Arg.Any<UpdateClassroomDto>(), Arg.Any<int>()).Returns(Result.Ok());

        var dto = new UpdateClassroomDto("Title", "Description", true);
        var result = await classroomService.UpdateClassroomDetails(dto, 1, 1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateClassroomDetails_InvalidAuthor_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var dto = new UpdateClassroomDto("Title", "Description", true);
        var result = await classroomService.UpdateClassroomDetails(dto, 1, 1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task UpdateClassroomDetails_ClassroomRepoFailedUpdate_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.UpdateClassroomDetailsAsync(Arg.Any<UpdateClassroomDto>(), Arg.Any<int>()).Returns(Result.Fail("Failed to update classroom"));

        var dto = new UpdateClassroomDto("Title", "Description", true);
        var result = await classroomService.UpdateClassroomDetails(dto, 1, 1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task UpdateClassroomSession_ShouldReturn_Ok()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.UpdateClassroomSessionAsync(Arg.Any<UpdateClassroomSessionDto>()).Returns(Result.Ok());

        var dto = new UpdateClassroomSessionDto(1, "Title", "Description", true, new List<int> { 1, 2 }, new List<int> { 1 });
        var result = await classroomService.UpdateClassroomSession(dto, 1, 1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateClassroomSession_InvalidAuthor_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var dto = new UpdateClassroomSessionDto(1, "Title", "Description", true, new List<int> { 1, 2 }, new List<int> { 1 });
        var result = await classroomService.UpdateClassroomSession(dto, 1, 1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task UpdateClassroomSession_ClassroomRepoFailedUpdate_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyClassroomAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.UpdateClassroomSessionAsync(Arg.Any<UpdateClassroomSessionDto>()).Returns(Result.Fail("Failed to update classroom session"));

        var dto = new UpdateClassroomSessionDto(1, "Title", "Description", true, new List<int> { 1, 2 }, new List<int> { 1 });
        var result = await classroomService.UpdateClassroomSession(dto, 1, 1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task DeleteClassroomSession_ShouldReturn_Ok()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.DeleteClassroomSessionAsync(Arg.Any<int>()).Returns(Task.CompletedTask);

        var result = await classroomService.DeleteClassroomSession(1, 1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task DeleteClassroomSession_InvalidAuthor_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        sessionRepoSub.VerifyAuthor(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var result = await classroomService.DeleteClassroomSession(1, 1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task LeaveClassroom_ShouldReturn_Ok()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyStudentInClassroom(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        classroomRepoSub.LeaveClassroomAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(Result.Ok());

        var result = await classroomService.LeaveClassroom(1, 1);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task LeaveClassroom_StudentNotInClassroom_ShouldReturn_Fail()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        classroomRepoSub.VerifyStudentInClassroom(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

        var result = await classroomService.LeaveClassroom(1, 1);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task GetClassroomSessionById_ShouldReturn_ClassroomSessionResponseDto()
    {
        var logger = Substitute.For<ILogger<ClassroomService>>();
        var classroomRepoSub = Substitute.For<IClassroomRepository>();
        var sessionRepoSub = Substitute.For<ISessionRepository>();

        var classroomService = new ClassroomService(logger, classroomRepoSub, sessionRepoSub);

        var responseDto = new GetClassroomSessionResponseDto { Active = true };
        classroomRepoSub.GetClassroomSessionByIdAsync(Arg.Any<int>()).Returns(responseDto);

        var result = await classroomService.GetClassroomSessionById(1);

        Assert.IsType<GetClassroomSessionResponseDto>(result);
        Assert.True(result.Active);
    }
}
