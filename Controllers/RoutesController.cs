using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Recipes.Data;
using Routes.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Xml;

namespace Routes.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteRepository _routeRepository;
        private readonly ILogger _logger;
        public RoutesController(ILogger logerManager, IRouteRepository routeRepository)
        {
            this._routeRepository = routeRepository;
            this._logger = logerManager;
        
        }

        #region GET
        // Get all routes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HikingRoute>>> GetAllRoutes(/*[FromQuery] int count*/)
        {
            try
            {
                var routes = await _routeRepository.GetAllRoutesAsync();
                return Ok(routes);
            }
            catch(Exception e)
            {
                _logger.LogError($"Something went wrong inside the Get All Routes action: {e}");
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // Get route by id
        [HttpGet("{id}")]
        public async Task<ActionResult> GetRouteById(int id)
        {
            try
            {
                if (id < 1)
                    return BadRequest();

                var route = await _routeRepository.GetRouteAsync(id.ToString());
                return Ok(route);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside the Get Route by Id action: {e}");
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            }
            
        }



        #endregion

        #region POST

        // Create a new route
        [HttpPost]
        public async Task<ActionResult> CreateNewRoute([FromBody] HikingRoute newRoute)
        {
            try
            {
                if (newRoute != null)
                {
                    var result = await _routeRepository.AddRouteAsync(newRoute);
                    return Ok(result);
                }

                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside the Create New Route action: {e}");
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            }

         
        }

        #endregion

        #region PUT
       
        // Update a route
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRoute([FromBody] HikingRoute route)
        {
            try
            {
                if (route == null || route.Id == null)
                {
                    return BadRequest();
                }

                var result = await _routeRepository.UpdateRouteAsync(route);
                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside the Update Route action: {e}");
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            }

        }

        #endregion

        #region DELETE
        // Delete a route 
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRoute(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest();
                }

                await _routeRepository.DeleteRouteAsync(id.ToString());
                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside the Delete Route action: {e}");
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            } 
        }

        // Delete all routes
        [HttpDelete]
        public async Task<ActionResult> DeleteAllRoutes()
        {
            try
            {
                await _routeRepository.DeleteAllRoutesAsync();

                return NoContent();
            }
            catch (Exception e)
            {
                _logger.LogError($"Something went wrong inside the Delete All Routes action: {e}");
                Console.WriteLine(e.Message);
                return StatusCode(500, "Internal server error");
            }   
           
        }   

        #endregion  
    }
}
