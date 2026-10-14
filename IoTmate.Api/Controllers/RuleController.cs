using IoTmate.Dto.Rule;
using IoTmate.Model.Entities;
using IoTmate.Services.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IoTmate.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/rooms/{roomId}/rules")]
    public class RuleController(IRuleService ruleService) : ControllerBase
    {
        private readonly IRuleService _ruleService = ruleService;
        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet]
        public async Task<ActionResult<List<RuleDto>>> GetRoomRules([FromRoute] Guid roomId)
        {
            var rules = await _ruleService.GetRulesByRoomId(roomId, CurrentUserId, HttpContext.RequestAborted);
            return Ok(rules);
        }

        [HttpPost]
        public async Task<ActionResult<RuleDto>> CreateRule([FromRoute] Guid roomId, [FromBody] CreateRuleInputDto input)
        {
            var newRule = await _ruleService.CreateRule(input, roomId, CurrentUserId, HttpContext.RequestAborted);
            return Ok(newRule);
        }

        [HttpPut("{ruleId}")]
        public async Task<ActionResult<RuleDto>> UpdateRule([FromRoute] Guid roomId, [FromRoute] Guid ruleId, [FromBody] UpdateRuleInputDto input)
        {
            var updatedRule = await _ruleService.UpdateRule(input, roomId, ruleId, CurrentUserId, HttpContext.RequestAborted);
            return Ok(updatedRule);
        }

        [HttpDelete("{ruleId}")]
        public async Task<IActionResult> DeleteRule([FromRoute] Guid roomId, [FromRoute] Guid ruleId)
        {
            await _ruleService.DeleteRule(ruleId, roomId, CurrentUserId, HttpContext.RequestAborted);
            return Ok();
        }
    }
}
