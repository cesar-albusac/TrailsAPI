using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Trails.Data;
using Trails.Models;

namespace Trails.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class TrailsController : ControllerBase 
    {
        private readonly ITrailRepository _TrailRepository;
        private readonly ILogger _logger;

  

        public TrailsController(ILogger<TrailsController> logger, ITrailRepository TrailRepository)
        {
            this._TrailRepository = TrailRepository;
            this._logger = logger;
        
        }

        #region GET
        // Get all Trails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Trail>>> GetAllTrails(/*[FromQuery] int count*/)
        {
            try
            {
                var Trails = await _TrailRepository.GetAllTrailsAsync();
                return Ok(Trails);
            }
            catch(Exception e)
            {
                _logger.LogError($"Something went wrong inside the Get All Trails action: {e}");
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // Get Trail by id
        [HttpGet("{id}")]
        public async Task<ActionResult> GetTrailById(int id)
        {
            try
            {
                if (id < 1)
                    return BadRequest();

                var Trail = await _TrailRepository.GetTrailAsync(id.ToString());
                return Ok(Trail);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside the Get Trail by Id action: {e}");
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            }
            
        }



        #endregion

        #region POST

        // Create a new Trail
        [HttpPost]
        public async Task<ActionResult> CreateNewTrail([FromBody] Trail newTrail)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid model object");
                }

                if (newTrail != null)
                {
                    var result = await _TrailRepository.AddTrailAsync(newTrail);
                    return Created("", result);
                }

                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside the Create New Trail action: {e}");
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            }

         
        }

        #endregion

        #region PUT
       
        // Update a Trail
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTrail([FromBody] Trail Trail)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid model object");
                }

                if (Trail == null || Trail.Id == null)
                {
                    return BadRequest();
                }

                var result = await _TrailRepository.UpdateTrailAsync(Trail);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside the Update Trail action: {e}");
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            }

        }

        #endregion

        #region DELETE
        // Delete a Trail 
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTrail(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest();
                }

                await _TrailRepository.DeleteTrailAsync(id.ToString());
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside the Delete Trail action: {e}");
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            } 
        }

        // Delete all Trails
        [HttpDelete]
        public async Task<ActionResult> DeleteAllTrails()
        {
            try
            {
                await _TrailRepository.DeleteAllTrailsAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside the Delete All Trails action: {e}");
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            }   
           
        }   

        #endregion  
    }
}
