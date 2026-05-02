using FinanceTracker.Api.Application.DTOs;
using FinanceTracker.Api.Domain;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers;

[Route("api/[controller]")]
public class SchedulesController : BaseApiController
{
    private readonly ILogger<SchedulesController> _logger;
    private readonly IRecurringScheduleRepository _recurringScheduleRepository;

    public SchedulesController(
        ILogger<SchedulesController> logger,
        IRecurringScheduleRepository recurringScheduleRepository
    )
    {
        _logger = logger;
        _recurringScheduleRepository = recurringScheduleRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetSchedules()
    {
        if (GetUserIdFromClaims() is not int userId) return Unauthorized(new ErrorResponse
        {
            Message = "Invalid token claims",
            StatusCode = 401
        });

        var schedules = await _recurringScheduleRepository.GetAll(userId);

        _logger.LogInformation("Found {Count} schedules for userId={UserId}.", schedules.Count, userId);

        return Ok(schedules);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetScheduleById(int id)
    {
        if (GetUserIdFromClaims() is not int userId) return Unauthorized(new ErrorResponse
        {
            Message = "Invalid token claims",
            StatusCode = 401
        });

        if (id <= 0)
        {
            _logger.LogWarning("Invalid recurring schedule id={Id} - must be positive", id);

            return StatusCode(400,
                new ErrorResponse
                {
                    Message = "Recurring schedule ID must be positive number.",
                    StatusCode = 400
                }
            );
        }

        var schedule = await _recurringScheduleRepository.GetById(id, userId);

        if (schedule == null)
        {
            _logger.LogWarning("Schedule with id={Id} for userId={UserId} is null.", id, userId);

            return NotFound(new ErrorResponse
            {
                Message = $"Schedule with id={id} not found.",
                StatusCode = 404
            });
        }

        _logger.LogInformation("Found schedule with id={Id} for userId={UserId}", id, userId);

        return Ok(schedule);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRecurringSchedule(int id)
    {
        if (GetUserIdFromClaims() is not int userId) return Unauthorized(new ErrorResponse
        {
            Message = "Invalid token claims",
            StatusCode = 401
        });

        if (id <= 0)
        {
            _logger.LogWarning("Invalid recurring schedule id={Id} for delete request.", id);

            return StatusCode(400, new ErrorResponse
            {
                Message = "Recurring schedule ID must be positive number.",
                StatusCode = 400
            });
        }

        var scheduleToDelete = await _recurringScheduleRepository.GetById(id, userId);

        if (scheduleToDelete == null)
        {
            _logger.LogWarning("Recurring schedule id={Id} not found.", id);

            return StatusCode(404, new ErrorResponse
            {
                Message = "Recurring schedule not found.",
                StatusCode = 404
            });
        }

        await _recurringScheduleRepository.Delete(scheduleToDelete);

        return NoContent();
    }
}