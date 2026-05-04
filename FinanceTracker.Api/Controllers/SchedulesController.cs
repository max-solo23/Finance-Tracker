using FinanceTracker.Api.Application;
using FinanceTracker.Api.Application.DTOs;
using FinanceTracker.Api.Domain;
using FinanceTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FinanceTracker.Api.Controllers;

[Route("api/[controller]")]
public class SchedulesController : BaseApiController
{
    private readonly ILogger<SchedulesController> _logger;
    private readonly IRecurringScheduleRepository _recurringScheduleRepository;
    private readonly IAccountService _accountService;

    public SchedulesController(
        ILogger<SchedulesController> logger,
        IRecurringScheduleRepository recurringScheduleRepository,
        IAccountService accountService
    )
    {
        _logger = logger;
        _recurringScheduleRepository = recurringScheduleRepository;
        _accountService = accountService;
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

    [HttpPost]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequest schedule)
    {
        if (GetUserIdFromClaims() is not int userId) 
            return Unauthorized(new ErrorResponse
        {
            Message = "Invalid token claims",
            StatusCode = 401
        });

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid schedule creation request: {Errors}", ModelState);

            return StatusCode(422, new ErrorResponse
                {
                    Message = "Invalid schedule creation request.",
                    StatusCode = 422,
                    Errors = ModelState
                        .Where(kvp => kvp.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
                        )
                }
            );  
        }

        var account = await _accountService.GetById(schedule.AccountId, userId);

        if (account == null)
        {
            _logger.LogWarning("Account id={Id} not found", schedule.AccountId);

            return StatusCode(404, new ErrorResponse
            {
                Message = "Account not found.",
                StatusCode = 404
            });
        }

        var newRecurringSchedule = new RecurringSchedule(
            userId, 
            schedule.AccountId, 
            schedule.Name, 
            schedule.Description, 
            schedule.Amount, 
            schedule.IntervalMonths,
            schedule.AnchorDate,
            schedule.Category
        );

        await _recurringScheduleRepository.Create(newRecurringSchedule);

        _logger.LogInformation("Schedule created with Id={Id}", newRecurringSchedule.Id);

        return CreatedAtAction(
            nameof(GetScheduleById),
            new { id = newRecurringSchedule.Id},
            newRecurringSchedule
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateScheduleRequest updateRequest)
    {
        if (GetUserIdFromClaims() is not int userId)
            return Unauthorized(new ErrorResponse
            {
                Message = "Invalid token claims",
                StatusCode = 401
            });

        if (id <= 0)
        {
            _logger.LogWarning("Invalid schedule id={Id}", id);

            return StatusCode(400, new ErrorResponse
            {
                Message = "Schedule ID must be a positive number.",
                StatusCode = 400
            });
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid update request for schedule id={Id}", id);

            return StatusCode(422, new ErrorResponse
            {
                Message = "Invalid schedule update request.",
                StatusCode = 422,
                Errors = ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList() ?? new List<string>()
                    )
            });
        }

        var existingSchedule = await _recurringScheduleRepository.GetById(id, userId);

        if (existingSchedule == null)
        {
            _logger.LogWarning("Schedule with id={Id} not found.", id);

            return StatusCode(404, new ErrorResponse
            {
                Message = "Schedule not found",
                StatusCode = 404
            });
        }

        if (updateRequest.AccountId != null) existingSchedule.AccountId = updateRequest.AccountId.Value;
        if (updateRequest.Name != null) existingSchedule.Name = updateRequest.Name;
        if (updateRequest.Description != null) existingSchedule.Description = updateRequest.Description;
        if (updateRequest.Amount != null) existingSchedule.Amount = updateRequest.Amount.Value;
        if (updateRequest.IntervalMonths != null) existingSchedule.IntervalMonths = updateRequest.IntervalMonths.Value;
        if (updateRequest.AnchorDate != null) existingSchedule.AnchorDate = DateTime.SpecifyKind(updateRequest.AnchorDate.Value, DateTimeKind.Utc);
        if (updateRequest.Category != null) existingSchedule.Category = updateRequest.Category;

        await _recurringScheduleRepository.Update(existingSchedule);

        return Ok(existingSchedule);
    }
}