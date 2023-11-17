using Microsoft.AspNetCore.Mvc;
using Trails.Models;

namespace Trails.Controllers
{
    public interface ITrail
    {
        Task<ActionResult<IEnumerable<Trail>>> GetAllTrails(/*[FromQuery] int count*/);
    }
}