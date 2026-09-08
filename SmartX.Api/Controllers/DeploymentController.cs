using Microsoft.AspNetCore.Mvc;
using SmartX.Api.Models;
using SmartX.Api.Services;

namespace SmartX.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeploymentController : ControllerBase
    {
        private readonly DeploymentValidator _validator;

        public DeploymentController(
            DeploymentValidator validator)
        {
            _validator = validator;
        }

        [HttpGet("validate/{nodeName}")]
        public IActionResult Validate(string nodeName)
        {
            DeploymentNode hierarchy = new()
            {
                Name = "Hydroponic Farm A",
                Type = "Facility",
                Children =
                {
                    new DeploymentNode
                    {
                        Name = "Zone A",
                        Type = "Zone",
                        Children =
                        {
                            new DeploymentNode
                            {
                                Name = "Grow Room 1",
                                Type = "Room",
                                Children =
                                {
                                    new DeploymentNode
                                    {
                                        Name = "NODE-001",
                                        Type = "Node"
                                    },
                                    new DeploymentNode
                                    {
                                        Name = "NODE-002",
                                        Type = "Node"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            bool exists =
                _validator.ContainsNode(hierarchy, nodeName);

            return Ok(new
            {
                node = nodeName,
                valid = exists
            });
        }
    }
}