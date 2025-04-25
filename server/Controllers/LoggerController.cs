using Microsoft.AspNetCore.Mvc;
using server.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using MarketMinds.Repositories.LoggerRepository;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoggerController : ControllerBase
    {
        private readonly ILoggerRepository loggerRepository;

        public LoggerController(ILoggerRepository loggerRepository)
        {
            this.loggerRepository = loggerRepository ?? throw new ArgumentNullException(nameof(loggerRepository));
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Log>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetLogs([FromQuery] int count = 100)
        {
            try
            {
                var logs = loggerRepository.GetRecentLogs(count);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error retrieving logs: {ex.Message}";
                
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new { error = errorMsg, details = ex.ToString() });
            }
        }

        [HttpGet("level/{logLevel}")]
        [ProducesResponseType(typeof(List<Log>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetLogsByLevel(string logLevel, [FromQuery] int count = 100)
        {
            try
            {
                Console.WriteLine($"GetLogsByLevel called with logLevel={logLevel}, count={count}");
                
                if (string.IsNullOrEmpty(logLevel))
                {
                    Console.WriteLine("Bad request: Log level is required");
                    return BadRequest("Log level is required");
                }

                var logs = loggerRepository.GetLogsByLevel(logLevel.ToUpper(), count);
                Console.WriteLine($"Retrieved {logs.Count} logs with level {logLevel}");
                return Ok(logs);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error retrieving logs by level: {ex.Message}";
                Console.WriteLine(errorMsg);
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new { error = errorMsg, details = ex.ToString() });
            }
        }

        [HttpPost("info")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult LogInfo([FromBody] LogRequest request)
        {
            try
            {
                Console.WriteLine($"LogInfo called with request: {JsonSerializer.Serialize(request)}");
                
                if (request == null)
                {
                    Console.WriteLine("Bad request: Request body is null");
                    return BadRequest("Request body is required");
                }
                
                if (string.IsNullOrEmpty(request.Message))
                {
                    Console.WriteLine("Bad request: Message is required");
                    return BadRequest("Message is required");
                }

                loggerRepository.LogInfo(request.Message);
                Console.WriteLine("Info log saved successfully");
                return Ok(new { message = "Info log saved successfully" });
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error logging info message: {ex.Message}";
                Console.WriteLine(errorMsg);
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new { error = errorMsg, details = ex.ToString() });
            }
        }

        [HttpPost("error")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult LogError([FromBody] LogRequest request)
        {
            try
            {
                Console.WriteLine($"LogError called with request: {JsonSerializer.Serialize(request)}");
                
                if (request == null)
                {
                    Console.WriteLine("Bad request: Request body is null");
                    return BadRequest("Request body is required");
                }
                
                if (string.IsNullOrEmpty(request.Message))
                {
                    Console.WriteLine("Bad request: Message is required");
                    return BadRequest("Message is required");
                }

                loggerRepository.LogError(request.Message);
                Console.WriteLine("Error log saved successfully");
                return Ok(new { message = "Error log saved successfully" });
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error logging error message: {ex.Message}";
                Console.WriteLine(errorMsg);
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new { error = errorMsg, details = ex.ToString() });
            }
        }

        [HttpPost("warning")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult LogWarning([FromBody] LogRequest request)
        {
            try
            {
                Console.WriteLine($"LogWarning called with request: {JsonSerializer.Serialize(request)}");
                
                if (request == null)
                {
                    Console.WriteLine("Bad request: Request body is null");
                    return BadRequest("Request body is required");
                }
                
                if (string.IsNullOrEmpty(request.Message))
                {
                    Console.WriteLine("Bad request: Message is required");
                    return BadRequest("Message is required");
                }

                loggerRepository.LogWarning(request.Message);
                Console.WriteLine("Warning log saved successfully");
                return Ok(new { message = "Warning log saved successfully" });
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error logging warning message: {ex.Message}";
                Console.WriteLine(errorMsg);
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                return StatusCode((int)HttpStatusCode.InternalServerError, 
                    new { error = errorMsg, details = ex.ToString() });
            }
        }
    }

    public class LogRequest
    {
        public string Message { get; set; }
    }
}
