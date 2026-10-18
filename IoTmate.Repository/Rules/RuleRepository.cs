using IoTmate.Model;
using IoTmate.Model.Enums;
using IoTmate.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace IoTmate.Repository.Rules
{
    public class RuleRepository(ApplicationDbContext dbContext) : BaseRepository(dbContext), IRuleRepository
    {
        public async Task<Rule?> GetRuleByIdAsync(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty) return null;
            return await DbContext.Rules
                .Include(r => r.TargetDevice)
                .Include(r => r.SourceDevice)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public async Task<List<Rule>> GetRulesByRoomIdAsync(Guid roomId, CancellationToken ct)
        {
            if (roomId == Guid.Empty) return [];
            return await DbContext.Rules
                .Where(r => r.RoomId == roomId)
                .Include(r => r.TargetDevice)
                .Include(r => r.SourceDevice)
                .ToListAsync(ct);
        }

        public async Task<List<Rule>> GetEnabledReadingRulesAsync(Guid sourceDeviceId, CancellationToken ct)
        {
            if (sourceDeviceId == Guid.Empty) return [];
            return await DbContext.Rules
                .Where(r => r.SourceDeviceId == sourceDeviceId && r.IsEnabled && r.RuleType == RuleType.Reading)
                .Include(x => x.TargetDevice)
                .ThenInclude(d => d!.RelayState)
                .ToListAsync(ct);
        }

        public async Task<List<Rule>> GetEnabledTimeRulesAsync(CancellationToken ct)
        {
            return await DbContext.Rules
                .Where(r => r.RuleType == RuleType.Time && r.IsEnabled)
                .Include(x => x.Room)
                .Include(x => x.TargetDevice)
                .ThenInclude(d => d!.RelayState)
                .ToListAsync(ct);
        }

        public async Task<Rule?> CreateRuleAsync(Rule newRule, CancellationToken ct)
        {
            DbContext.Rules.Add(newRule);
            await DbContext.SaveChangesAsync(ct);
            return newRule;
        }

        public async Task UpdateRuleAsync(Rule updatedRule, CancellationToken ct)
        {
            if (DbContext.Entry(updatedRule).State == EntityState.Detached)
                DbContext.Rules.Update(updatedRule);
            await DbContext.SaveChangesAsync(ct);
        }

        public async Task<bool> DeleteRuleAsync(Guid id, CancellationToken ct)
        {
            var rule = await DbContext.Rules.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (rule == null) return false;

            DbContext.Rules.Remove(rule);
            await DbContext.SaveChangesAsync(ct);

            return true;
        }
    }
}
