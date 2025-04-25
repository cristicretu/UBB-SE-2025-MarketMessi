using Microsoft.AspNetCore.Mvc;
using server.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using MarketMinds.Repositories.HardwareSurveyRepository;

namespace MarketMinds.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HardwareSurveyController : ControllerBase
    {
        private readonly IHardwareSurveyRepository hardwareSurveyRepository;

        public HardwareSurveyController(IHardwareSurveyRepository hardwareSurveyRepository)
        {
            this.hardwareSurveyRepository = hardwareSurveyRepository ?? throw new ArgumentNullException(nameof(hardwareSurveyRepository));
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<HardwareSurvey>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult GetAllHardwareSurveys()
        {
            try
            {
                var surveys = hardwareSurveyRepository.GetAllHardwareSurveys();
                return Ok(surveys);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving hardware surveys: {ex}");
                return Ok(new List<HardwareSurvey>());
            }
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult SaveHardwareData([FromBody] HardwareSurvey hardwareData)
        {
            try 
            {
                if (hardwareData == null)
                {
                    Console.WriteLine("Hardware data is null");
                    return BadRequest("Hardware data is null");
                }

                // Debug logging
                Console.WriteLine($"Received hardware data: {JsonSerializer.Serialize(hardwareData)}");

                // Ensure we don't try to set the ID
                hardwareData.Id = 0;

                // Validate required fields
                if (string.IsNullOrEmpty(hardwareData.DeviceID))
                {
                    Console.WriteLine("Device ID is required");
                    return BadRequest("Device ID is required");
                }

                if (string.IsNullOrEmpty(hardwareData.DeviceType))
                {
                    Console.WriteLine("Device type is required");
                    return BadRequest("Device type is required");
                }

                if (string.IsNullOrEmpty(hardwareData.OperatingSystem))
                {
                    Console.WriteLine("Operating system is required");
                    return BadRequest("Operating system is required");
                }

                // Set default timestamp if not provided
                if (hardwareData.SurveyTimestamp == default)
                {
                    hardwareData.SurveyTimestamp = DateTime.UtcNow;
                    Console.WriteLine($"Using current timestamp: {hardwareData.SurveyTimestamp}");
                }

                hardwareSurveyRepository.SaveHardwareData(hardwareData);
                return Ok(new { message = "Hardware data saved successfully", id = hardwareData.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving hardware data: {ex}");
                return StatusCode((int)HttpStatusCode.InternalServerError, "An error occurred while saving hardware data");
            }
        }
    }
} 